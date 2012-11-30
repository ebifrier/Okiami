using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Ragnarok;
using Ragnarok.NicoNico;
using Ragnarok.NicoNico.Live;
using Ragnarok.ObjectModel;

namespace VoteSystem.Client.Model
{
    using VoteSystem.Protocol;
    using VoteSystem.Protocol.Vote;
    using VoteSystem.Protocol.Commenter;

    /// <summary>
    /// コメンターの各接続オブジェクトの状態です。
    /// </summary>
    public enum CommentClientState
    {
        /// <summary>
        /// 接続待ち状態(ユーザーの許可待ち)
        /// </summary>
        Wait,
        /// <summary>
        /// 接続中
        /// </summary>
        Connect,
        /// <summary>
        /// エラー状態
        /// </summary>
        Error,
        /// <summary>
        /// 放送接続待ちリストから削除された
        /// </summary>
        Deleted,
    }

    /// <summary>
    /// 中継コメント投稿時に呼ばれるイベントです。
    /// </summary>
    public sealed class CommentPostEvent : EventArgs
    {
        /// <summary>
        /// 中継されたコメントを取得または設定します。
        /// </summary>
        public PostCommentData PostComment
        {
            get;
            set;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CommentPostEvent(PostCommentData postComment)
        {
            PostComment = postComment;
        }
    }

    /// <summary>
    /// 各放送への接続情報などを保持します。
    /// </summary>
    public class CommenterCommentClient : NotifyObject
    {
        private static readonly TimeSpan ErrorInternal = TimeSpan.FromSeconds(120);
        private CommentClientState state;
        private CommentClient commentClient;
        private PlayerStatus playerStatus;
        private bool isAllowToConnect = false;
        private bool isWatching = false;
        private bool isPostCommentEnabled = false;
        private DateTime lastErrorDateTime = DateTime.Now;
        private DateTime lastCheckTimeToWatch = DateTime.Now;

        /// <summary>
        /// コメント中継時に呼ばれるイベントです。
        /// </summary>
        public event EventHandler<CommentPostEvent> CommentPost;

        /// <summary>
        /// 放送ＩＤを取得します。
        /// </summary>
        public string LiveId
        {
            get;
            private set;
        }

        /// <summary>
        /// このオブジェクトの状態を取得します。
        /// </summary>
        public CommentClientState State
        {
            get
            {
                return this.state;
            }
            private set
            {
                SetValue("State", value, ref this.state);
            }
        }

        /// <summary>
        /// 放送接続用のオブジェクトを取得します。
        /// </summary>
        public CommentClient CommentClient
        {
            get
            {
                return this.commentClient;
            }
            private set
            {
                SetValue("CommentClient", value, ref this.commentClient);
            }
        }

        /// <summary>
        /// 放送説明などを取得します。
        /// </summary>
        public PlayerStatus PlayerStatus
        {
            get
            {
                return this.playerStatus;
            }
            private set
            {
                SetValue("PlayerStatus", value, ref this.playerStatus);
            }
        }

        /// <summary>
        /// 接続を許可するかを取得または設定します。
        /// </summary>
        public bool IsAllowToConnect
        {
            get
            {
                return this.isAllowToConnect;
            }
            set
            {
                SetValue("IsAllowToConnect", value, ref this.isAllowToConnect);
            }
        }

        /// <summary>
        /// この放送を今見ているかどうかを取得します。
        /// </summary>
        public bool IsWatching
        {
            get
            {
                return this.isWatching;
            }
            private set
            {
                SetValue("IsWatching", value, ref this.isWatching);
            }
        }

        /// <summary>
        /// コメントが投稿可能かどうかを取得または設定します。
        /// </summary>
        public bool IsPostCommentEnabled
        {
            get
            {
                return this.isPostCommentEnabled;
            }
            private set
            {
                SetValue("IsPostCommentEnabled", value, ref this.isPostCommentEnabled);
            }
        }

        /// <summary>
        /// 今すぐコメント投稿が可能か調べます。
        /// </summary>
        public bool CanPostCommentNow
        {
            get
            {
                var commentClient = CommentClient;
                if (commentClient == null)
                {
                    return false;
                }

                var currentIndex = commentClient.CurrentRoomIndex;
                return (
                    commentClient.CanPostCommentNow(currentIndex) &&
                    commentClient.LeaveCommentCount == 0);
            }
        }

        /// <summary>
        /// 放送接続後のメッセージを送ります。
        /// </summary>
        private void SendConnectedCommand()
        {
            var commentClient = CommentClient;
            if (commentClient == null)
            {
                // 何で！？
                return;
            }

            Global.VoteClient.SendCommand(new LiveConnectedCommand()
            {
                Live = new LiveData(LiveSite.NicoNama, LiveId),
                LiveRoom = commentClient.CurrentRoomIndex,
            });
        }

        /// <summary>
        /// 放送切断後のメッセージを送ります。
        /// </summary>
        private void SendDisconnectedCommand()
        {
            Global.VoteClient.SendCommand(new LiveDisconnectedCommand()
            {
                Live = new LiveData(LiveSite.NicoNama, LiveId),
            });
        }

        /// <summary>
        /// 投稿可能状態になったことを通知します。
        /// </summary>
        private void SendCommenterStateChangedCommand()
        {
            Global.VoteClient.SendCommand(new CommenterStateChangedCommand()
            {
                Live = new LiveData(LiveSite.NicoNama, LiveId),
                CanPostComment = IsPostCommentEnabled,
                IsWatching = IsWatching,
            });
        }

        /// <summary>
        /// このコメンターの放送切断時に呼ばれます。
        /// </summary>
        private void commentClient_Disconnected(object sender, EventArgs e)
        {
            var commentClient = sender as CommentClient;

            if (commentClient != null)
            {
                using (LazyLock())
                {
                    State = CommentClientState.Wait;
                    CommentClient = null;
                    IsPostCommentEnabled = false;

                    SendCommenterStateChangedCommand();
                }
            }
        }

        /// <summary>
        /// オブジェクトの状態を更新し、必要なフィールドも一緒に変更します。
        /// </summary>
        private void SetState(CommentClientState state)
        {
            using (LazyLock())
            {
                this.lastErrorDateTime =
                    (state == CommentClientState.Error
                         ? DateTime.Now
                         : DateTime.MinValue);

                State = state;
                CommentClient = null;
                IsPostCommentEnabled = false;

                SendCommenterStateChangedCommand();
            }
        }

        /// <summary>
        /// 放送に接続します。
        /// </summary>
        private bool Connect()
        {
            using (LazyLock())
            {
                if (State == CommentClientState.Connect ||
                    State == CommentClientState.Deleted)
                {
                    // すでに接続中か、削除状態では無視されます。
                    return false;
                }

                var nicoClient = Global.MainModel.NicoClient;
                if (!nicoClient.IsLogin)
                {
                    return false;
                }

                try
                {
                    // 指定の放送に接続します。
                    var commentClient = Global.CreateCommentClient();
                    var cc = Global.MainModel.NicoClient.CookieContainer;

                    commentClient.IsSupressLog = true;
                    commentClient.Connect(LiveId, cc, true);
                    commentClient.StartReceiveMessage(0);
                    commentClient.Disconnected += commentClient_Disconnected;

                    // 状態の更新を行います。
                    SetState(CommentClientState.Connect);
                    CommentClient = commentClient;

                    SendConnectedCommand();
                }
                catch (Exception ex)
                {
                    Log.ErrorException(ex,
                        "CommenterCommentClient: {0}放送への接続に失敗しました。",
                        LiveId);

                    SetState(CommentClientState.Error);
                }

                return true;
            }
        }

        /// <summary>
        /// 放送から切断します。
        /// </summary>
        public void Disconnect()
        {
            using (LazyLock())
            {
                if (State != CommentClientState.Connect &&
                    State != CommentClientState.Error)
                {
                    return;
                }

                // マルチスレッドのことも一応考えて、ローカル変数にコピーしています。
                var commentClient = CommentClient;
                if (commentClient != null)
                {
                    commentClient.Disconnected -= commentClient_Disconnected;
                    commentClient.Disconnect();
                }

                // 接続待ち状態に移行します。
                SetState(CommentClientState.Wait);

                SendDisconnectedCommand();
            }
        }

        /// <summary>
        /// 放送の接続要求から削除します。
        /// </summary>
        public void Delete()
        {
            using (LazyLock())
            {
                Disconnect();

                SetState(CommentClientState.Deleted);
            }
        }

        /// <summary>
        /// 指定のIDの放送に指定の内容のコメントを投稿します。
        /// </summary>
        public void PostComment(Notification notification)
        {
            using (LazyLock())
            {
                if (notification == null || !notification.Validate())
                {
                    return;
                }

                if (State != CommentClientState.Connect)
                {
                    return;
                }

                // コメント投稿
                var text = notification.Text;
                var colorString = ColorConverter.ToNicoColorString(
                    notification.Color, false);
                CommentClient.SendComment(text, "184 " + colorString);

                // 投稿可能フラグはサーバー側と同期させるため、
                // とりあえず偽に設定しておきます。
                IsPostCommentEnabled = false;
            }

            // 中継したコメントの情報です。
            var data = new PostCommentData
            {
                LiveId = LiveId,
                Text = notification.Text,

                // Notificationの時刻はサーバー時刻なので
                // ＰＣのローカル時間とずれる可能性がある。
                Timestamp = DateTime.Now,
            };
            CommentPost.SafeRaiseEvent(this, new CommentPostEvent(data));
        }

        /// <summary>
        /// 定期的にheartbeatを取得し、現在放送を視聴中かどうか調べます。
        /// </summary>
        private void UpdateWatchingStatus()
        {
            if (State != CommentClientState.Wait &&
                State != CommentClientState.Connect)
            {
                IsWatching = false;
                return;
            }

            try
            {
                var now = DateTime.Now;
                if (now < lastCheckTimeToWatch + TimeSpan.FromSeconds(10))
                {
                    return;
                }

                var playerStatus = PlayerStatus;
                if (playerStatus == null)
                {
                    IsWatching = false;
                    return;
                }

                // heartbeatを取得します。(失敗時は例外発生)
                var cc = Global.MainModel.NicoClient.CookieContainer;
                var heartbeat = Heartbeat.Create(this.LiveId, cc);
                // ここまで例外なく来たら、視聴中の可能性があります。

                // 
                var count = Global.ConnectionCounter.GetCount(
                    playerStatus.MS.Address,
                    playerStatus.MS.Port);
                IsWatching = (count > 0);
            }
            catch (NicoLiveException)
            {
                // 失敗したと言うことは視聴中では無いということです。
                IsWatching = false;
            }

            this.lastCheckTimeToWatch = DateTime.Now;
        }

        /// <summary>
        /// 必要なら放送への接続処理を行います。
        /// </summary>
        public bool Update()
        {
            // もし投票ルームにログインしていなければ、
            // コメンターとしては機能しません。
            if (!Global.VoteClient.IsLogined)
            {
                Disconnect();
                return false;
            }

            if (this.playerStatus == null)
            {
                var cc = Global.MainModel.NicoClient.CookieContainer;
                PlayerStatus = PlayerStatus.Create(LiveId, cc);
            }

            // コメントの投稿可能状態を更新します。
            if (!IsPostCommentEnabled && CanPostCommentNow)
            {
                IsPostCommentEnabled = true;
                SendCommenterStateChangedCommand();
            }

            // 放送を視聴中か調べます。
            UpdateWatchingStatus();

            using (LazyLock())
            {
                // アカウントをニコ生のコメンターとして使って良い場合のみ
                // 接続状態を保ちます。
                var isAllowToConnect = (
                    Global.MainModel.IsUseAsNicoCommenter &&
                    IsAllowToConnect);

                switch (State)
                {
                    case CommentClientState.Wait:
                        // 待ち状態で接続許可が出た場合は、
                        // 放送に接続に行きます。
                        if (isAllowToConnect)
                        {
                            return Connect();
                        }
                        break;

                    case CommentClientState.Connect:
                        // 接続許可が無い場合は放送から切断します。
                        if (!isAllowToConnect)
                        {
                            Disconnect();
                        }
                        break;

                    case CommentClientState.Error:
                        // エラー状態なら、もう一度接続/切断処理を行います。
                        if (isAllowToConnect)
                        {
                            // 最後にエラーが起こった時から、指定の時間が経って
                            // いなければ、何もしません。
                            // そうでなければ、再接続に行きます。
                            var now = DateTime.Now;
                            if (now < this.lastErrorDateTime + ErrorInternal)
                            {
                                return false;
                            }
                            
                            return Connect();
                        }
                        else
                        {
                            Disconnect();
                        }
                        break;

                    case CommentClientState.Deleted:
                        // 放送リストから切断された場合は、何もしません。
                        break;
                }
            }

            return false;
        }

        void this_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsWatching")
            {
                SendCommenterStateChangedCommand();
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CommenterCommentClient(ConnectionCounter connectionCounter,
                                      string liveId)
        {
            if (string.IsNullOrEmpty(liveId))
            {
                throw new ArgumentNullException("liveId");
            }

            this.PropertyChanged += this_PropertyChanged;

            LiveId = liveId;
            State = CommentClientState.Wait;
        }
    }
}
