using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.ComponentModel;

using Ragnarok;
using Ragnarok.NicoNico;
using Ragnarok.NicoNico.Live;
using Ragnarok.ObjectModel;

namespace VoteSystem.Client.Model.Live
{
    using Protocol;
    using Protocol.Vote;

    /// <summary>
    /// 放送主用のモデルオブジェクトです。
    /// </summary>
    public class LiveNicoClient : LiveClient
    {
        private readonly NicoClient nicoClient;
        private readonly CommentClient commentClient;

        /// <summary>
        /// ニコニコ用のクライアントオブジェクトを取得します。
        /// </summary>
        public NicoClient NicoClient
        {
            get { return this.nicoClient; }
        }

        /// <summary>
        /// ニコニコ用のコメントクライアントを取得します。
        /// </summary>
        public CommentClient NicoCommentClient
        {
            get { return this.commentClient; }
        }

        /// <summary>
        /// ニコ生に接続します。
        /// </summary>
        public override void ConnectCommand()
        {
            if (!this.nicoClient.IsLogin)
            {
                throw new InvalidOperationException(
                    "ログインしていません (-ω-｡)");
            }

            // コメントサーバーに接続する前に、それが自分の放送か確認し、
            // もしそうならサーバーに接続します。
            var playerStatus = PlayerStatus.Create(
                this.LiveUrlText,
                this.nicoClient.CookieContainer);

#if PUBLISHED
            if (playerStatus == null || !playerStatus.Stream.IsOwner)
            {
                throw new VoteClientException(
                    "放送主ではありません。m(-_-)m");
            }
#endif

            this.commentClient.Connect(
                playerStatus,
                this.nicoClient.CookieContainer,
                TimeSpan.FromSeconds(10));

            // メッセージの受信を開始します。
            this.commentClient.StartReceiveMessage(1);
        }

        /// <summary>
        /// ニコ生から切断します。
        /// </summary>
        public override void DisconnectCommand()
        {
            if (!this.commentClient.IsConnected)
            {
                // 接続してません(^^;
                return;
            }

            this.commentClient.Disconnect();
        }

        /// <summary>
        /// オーナーコメント投稿用に改行文字をタグに置き換えます。
        /// </summary>
        private static string ModifyOwnerComment(string rowstr)
        {
            if (string.IsNullOrEmpty(rowstr))
            {
                return rowstr;
            }

            rowstr = rowstr.Replace("\r\n", "<br />");
            rowstr = rowstr.Replace("\n", "<br />");
            rowstr = rowstr.Replace("\r", "<br />");
            return rowstr;
        }

        /// <summary>
        /// 受信した通知を処理します。
        /// </summary>
        public override void HandleNotification(Notification notification)
        {
            // 受信したメッセージを放送に再投稿します。
            // システムメッセージのみ。
            if (this.commentClient.IsConnected &&
                ProtocolUtil.IsPostComment(notification, false, Attribute, LiveData))
            {
                var message = ModifyOwnerComment(notification.Text);
                var nicoColorStr = ColorConverter.ToNicoColorString(
                    notification.Color, false);

                if (notification.Type == NotificationType.Important)
                {
                    // 重要メッセージ
                    this.commentClient.SendOwnerComment(
                        ProtocolUtil.MakeMirrorComment(message),
                        "184 " + nicoColorStr);
                }
                else if (notification.Type == NotificationType.System)
                {
                    // システムメッセージ
                    message = GetVoteSystemMessage(notification.SystemType);

                    if (!string.IsNullOrEmpty(message))
                    {
                        this.commentClient.SendOwnerComment(
                            ProtocolUtil.MakeMirrorComment(message),
                            "184 " + nicoColorStr);
                    }
                }
                else
                {
                    return;
                }

                Log.Info("コメント投稿: {0}", message);
            }
        }

        /// <summary>
        /// コメントサーバーから受信したコメントを処理します。
        /// </summary>
        private void HandleComment(int roomIndex, Comment comment)
        {
            if (comment == null || string.IsNullOrEmpty(comment.Text))
            {
                return;
            }

            // 投票可能でなければ何もしません。
            if (!this.VoteClient.IsLogined)
            {
                return;
            }

            // 主コメや運コメはアリーナのものしか処理しません。
            if ((comment.CommentType == CommentType.Owner ||
                 comment.IsManagementComment) &&
                roomIndex != 0)
            {
                return;
            }

            // 指定の文字が先頭にある場合は、投票サーバーから
            // 送られたメッセージです。
            if (ProtocolUtil.IsMirrorComment(comment.Text))
            {
                return;
            }

            // 通知を作成します。
            var notification = new Notification()
            {
                Text = comment.Text,
                Color = ColorConverter.ToMessageColor(comment.Color),
                FromLiveRoom = new LiveRoomData(LiveData, roomIndex),
                VoterId = comment.UserId,
                Timestamp = comment.Date,
            };

            // 投稿者が放送主かどうかを判断します。
            var selfId = this.commentClient.UserId.ToString();
            var isFromLiveOwner = (comment.UserId == selfId);

            VoteClient.SendNotification(notification, isFromLiveOwner);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LiveNicoClient(MainModel participant, NicoClient nicoClient)
            : base(participant, LiveSite.NicoNama)
        {
            this.commentClient = Global.CreateCommentClient();
            this.commentClient.Connected += (sender, e) =>
            {
                var cc = (CommentClient)sender;
                var title = (
                    cc.LiveInfo != null ?
                    cc.LiveInfo.Title :
                    "");
                var ownerId = (
                    cc.PlayerStatus != null ?
                    cc.PlayerStatus.Stream.OwnerId :
                    -1);

                // 接続後に放送IDなどを設定します。
                LiveConnected(
                    new LiveData(
                        LiveSite.NicoNama,
                        cc.LiveIdString,
                        title,
                        ownerId.ToString()));
            };
            this.commentClient.Disconnected +=
                (sender, e) => LiveDisconnected();
            this.commentClient.CommentReceived +=
                (sender, e) => HandleComment(e.RoomIndex, e.Comment);

            this.nicoClient = nicoClient;
        }
    }
}
