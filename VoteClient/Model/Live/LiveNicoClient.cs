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
        private readonly AlertClient alert;
        private readonly Timer heartbeatTimer;

        /// <summary>
        /// 放送に接続しているかどうかを取得します。
        /// </summary>
        [DependOnProperty(typeof(CommentClient), "IsConnected")]
        public override bool IsConnected
        {
            get { return this.commentClient.IsConnected; }
        }

        /// <summary>
        /// 放送への自動接続機能に使うコミュニティ番号を取得します。
        /// </summary>
        /// <remarks>
        /// URLにコミュニティURLを指定すると、
        /// 自動接続機能がオンになり、
        /// 新しい放送に自動的に接続するようになります。
        /// </remarks>
        public int CommunityId
        {
            get { return GetValue<int>("CommunityId"); }
            private set { SetValue("CommunityId", value); }
        }

        /// <summary>
        /// 放送URLにコミュニティURLが指定された場合は、
        /// それを放送URLに変換します。
        /// </summary>
        private string ConvertUrl(string url, out int communityId)
        {
            try
            {
                if (string.IsNullOrEmpty(url) || !this.nicoClient.IsLogin)
                {
                    communityId = -1;
                    return null;
                }

                communityId = LiveUtil.GetCommunityId(LiveUrlText);
                if (communityId < 0)
                {
                    // URLにコミュニティが指定されていない。
                    return null;
                }

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

            communityId = -1;
            return null;
        }

        /// <summary>
        /// ニコ生に接続します。
        /// </summary>
        public override void ConnectCommand()
        {
            int communityId;

            // 指定されたURLがコミュニティURLで
            // 現在放送中の放送が見つからない場合は失敗とします。
            var liveUrl = ConvertUrl(LiveUrlText, out communityId);
            if (communityId > 0 && string.IsNullOrEmpty(liveUrl))
            {
                throw new VoteClientException(
                    "コミュニティの放送が確認できませんでした。");
            }

            // コミュニティから得られた放送URLか
            // 指定されたURLそのままを使用して接続に行きます。
            liveUrl = liveUrl ?? LiveUrlText;
            Connect(liveUrl);

            CommunityId = communityId;
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

#if !OFFICIAL
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
                    this.nicoClient.CookieContainer);
#else // !OFFICIAL
                this.commentClient.ConnectToOfficial(
                    liveUrl, 
                    this.nicoClient.CookieContainer);
#endif

                // メッセージの受信を開始します。
                this.commentClient.StartReceiveMessage(100);
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

                CommunityId = -1;
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
            try
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
            catch (Exception ex)
            {
                Util.ThrowIfFatal(ex);

                Log.ErrorException(ex,
                    "放送主コメントの投稿に失敗しました。");
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
            if ((comment.IsOwnerComment || comment.IsManagementComment) &&
                roomIndex != 0)
            {
                return;
            }

            // リレーコメントなら無視します。
            if (comment.IsRelayComment)
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

        private void Heartbeat_Callback(object state)
        {
            if (LiveData == null || !VoteClient.IsLogined)
            {
                return;
            }

            try
            {
                var heartbeat = Heartbeat.Create(
                    this.commentClient.LiveId,
                    this.nicoClient.CookieContainer);

                // 来場者数・コメント数を投票サーバーに送ります。
                VoteClient.SendCommand(
                    new SetLiveHeartbeatCommand
                    {
                        LiveData = LiveData,
                        CommentCount = heartbeat.CommentCount,
                        VisitorCount = heartbeat.WatchCount,
                    });
            }
            catch (NicoLiveException)
            {
                // ログは出しません。
            }
            catch (Exception ex)
            {
                Util.ThrowIfFatal(ex);
                Log.ErrorException(ex,
                    "heartbeatの取得に失敗しました。");
            }
        }

        /// <summary>
        /// ニコ生アラートへの接続を行います。
        /// </summary>
        /// <remarks>
        /// ネットワーク制限がある場合などは、
        /// 接続に失敗することがあります。
        /// </remarks>
        private AlertClient ConnectAlert()
        {
            try
            {
                var alert = new AlertClient();
                alert.LiveAlerted += alert_LiveAlerted;
                alert.Connect();
                return alert;
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "アラートへの接続に失敗しました。");
            }

            return null;
        }

        void alert_LiveAlerted(object sender, LiveAlertedEventArgs e)
        {
            if (e.ProviderData.ProviderType != ProviderType.Community ||
                e.ProviderData.Id != CommunityId)
            {
                return;
            }

            Connect(e.LiveIdString);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LiveNicoClient(MainModel participant, NicoClient nicoClient)
            : base(participant, LiveSite.NicoNama)
        {
            this.nicoClient = nicoClient;

            Attribute = Global.Settings.AS_NicoLiveAttribute;
            LiveUrlText = Global.Settings.AS_NicoLiveUrl;

            AddPropertyChangedHandler(
                "Attribute",
                (_, __) => Global.Settings.AS_NicoLiveAttribute = Attribute);
            AddPropertyChangedHandler(
                "LiveUrlText",
                (_, __) => Global.Settings.AS_NicoLiveUrl = LiveUrlText);

            this.commentClient = Global.CreateCommentClient();
            this.commentClient.Connected += (sender, e) =>
            {
                var cc = (CommentClient)sender;

#if !OFFICIAL
                var liveIdString = cc.LiveIdString;
                var title = (
                    cc.LiveInfo != null ?
                    cc.LiveInfo.Title :
                    string.Empty);
#else
                var liveIdString = "lv2525";
                var title = "公式生放送";
#endif

                var ownerId = (
                    cc.PlayerStatus != null ?
                    cc.PlayerStatus.Stream.OwnerId :
                    -1);

                // 接続後に放送IDなどを設定します。
                LiveConnected(
                    new LiveData(
                        LiveSite.NicoNama,
                        liveIdString,
                        title,
                        ownerId.ToString()));
            };
            this.commentClient.Disconnected +=
                (sender, e) => LiveDisconnected();
            this.commentClient.CommentReceived +=
                (sender, e) => HandleComment(e.RoomIndex, e.Comment);
            this.AddDependModel(this.commentClient);

#if !OFFICIAL
            this.alert = ConnectAlert();
#endif

            this.heartbeatTimer = new Timer(
                Heartbeat_Callback,
                null,
                TimeSpan.FromSeconds(10),
                TimeSpan.FromMinutes(3.0));
        }
    }
}
