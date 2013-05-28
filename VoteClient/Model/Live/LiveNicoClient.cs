﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public sealed class LiveNicoClient : LiveClient
    {
        private readonly object SyncObject = new object();
        private readonly NicoClient nicoClient;
        private readonly CommentClient commentClient;
        private readonly Timer timer;

        /// <summary>
        /// 放送への自動接続機能があるため、
        /// それを使用するかどうかを取得または設定します。
        /// </summary>
        /// <remarks>
        /// 人為的な接続／切断ボタンによりスイッチされます。
        /// </remarks>
        public bool IsWantToConnect
        {
            get { return GetValue<bool>("IsWantToConnect"); }
            private set { SetValue("IsWantToConnect", value); }
        }

        /// <summary>
        /// 放送URLにコミュニティURLが指定された場合は、
        /// それを放送URLに変換します。
        /// </summary>
        private string ConvertUrl(string url, out bool isCommunityUrl)
        {
            try
            {
                isCommunityUrl = false;

                if (string.IsNullOrEmpty(url) ||
                    !this.nicoClient.IsLogin)
                {
                    return null;
                }

                var communityId = LiveUtil.GetCommunityId(LiveUrlText);
                if (communityId < 0)
                {
                    // URLにコミュニティが指定されていない。
                    return null;
                }

                isCommunityUrl = true;

                var liveUrl = LiveUtil.GetCurrentLiveUrl(
                    communityId,
                    this.nicoClient.CookieContainer);
                if (string.IsNullOrEmpty(liveUrl))
                {
                    // 放送中じゃない
                    return null;
                }

                return liveUrl;
            }
            catch (Exception ex)
            {
                Util.ThrowIfFatal(ex);
                Log.ErrorException(ex,
                    "放送URL変換中にエラーが発生しました。");
            }

            isCommunityUrl = false;
            return null;
        }

        /// <summary>
        /// ニコ生に接続します。
        /// </summary>
        public override void ConnectCommand()
        {
            bool isCommunityUrl;

            // コミュニティのURLの可能性があります。
            var liveUrl = ConvertUrl(LiveUrlText, out isCommunityUrl);
            if (isCommunityUrl && string.IsNullOrEmpty(liveUrl))
            {
                throw new VoteClientException(
                    "コミュニティの放送が確認できませんでした。");
            }

            liveUrl = liveUrl ?? LiveUrlText;
            Connect(liveUrl);

            IsWantToConnect = true;
        }

        /// <summary>
        /// ニコ生に接続します。
        /// </summary>
        private void Connect(string liveUrl)
        {
            lock (SyncObject)
            {
                if (!this.nicoClient.IsLogin)
                {
                    throw new InvalidOperationException(
                        "ログインしていません (-ω-｡)");
                }

                // コメントサーバーに接続する前に、それが自分の放送か確認し、
                // もしそうならサーバーに接続します。
                var playerStatus = PlayerStatus.Create(
                    liveUrl,
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
                    TimeSpan.FromSeconds(30));

                // メッセージの受信を開始します。
                this.commentClient.StartReceiveMessage(1);
            }
        }

        /// <summary>
        /// ニコ生から切断します。
        /// </summary>
        public override void DisconnectCommand()
        {
            lock (SyncObject)
            {
                if (!this.commentClient.IsConnected)
                {
                    // 接続してません(^^;
                    return;
                }

                IsWantToConnect = false;
                this.commentClient.Disconnect();
            }
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
        /// 放送URLにコミュニティURLが指定された場合は、
        /// 放送開始後に自動的に再接続に行きます。
        /// </summary>
        private void OnTimerCallback(object state)
        {
            lock (SyncObject)
            {
                if (this.commentClient.IsConnected ||
                    !IsWantToConnect)
                {
                    return;
                }

                bool isCommunityUrl;
                var liveUrl = ConvertUrl(LiveUrlText, out isCommunityUrl);
                if (liveUrl != null)
                {
                    Connect(liveUrl);
                }
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LiveNicoClient(MainModel participant, NicoClient nicoClient)
            : base(participant, LiveSite.NicoNama)
        {
            this.nicoClient = nicoClient;

            // このコンストラクタはアプリ起動時に一度しか呼ばれないため、
            // グローバル変数にハンドラを設定しています。
            // 本来はDisposeなどでハンドラを外す必要があります。
            Attribute =
                Global.Settings.AS_NicoLiveAttribute ??
                new LiveAttribute();
            Attribute.PropertyChanged +=
                (_, __) => Global.Settings.Save();
            Global.Settings.AS_NicoLiveAttribute = Attribute;

            LiveUrlText = Global.Settings.AS_NicoLiveUrl;
            AddPropertyChangedHandler(
                "LiveUrlText",
                (_, __) => Global.Settings.AS_NicoLiveUrl = LiveUrlText);

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

            this.timer = new Timer(
                OnTimerCallback,
                null,
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(10));
        }
    }
}
