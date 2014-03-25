using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;

using Ragnarok;
using Ragnarok.Utility;
using Ragnarok.NicoNico;
using Ragnarok.NicoNico.Live;
using Ragnarok.ObjectModel;

namespace VoteSystem.Client.Model
{
    using Protocol;
    using Protocol.Vote;
    using Live;

    /// <summary>
    /// 放送主用のモデルオブジェクトです。
    /// </summary>
    /// <remarks>
    /// このクラスの行うことは主に以下です。
    /// １、ニコニコやUstreamなどの生放送への接続。
    /// ２、放送主の画像や作成する投票ルームの名前、入室する投票ルーム
    /// 　　などを管理します。
    /// ３、投票サーバーとの接続を管理します。
    /// </remarks>
    public class MainModel : NotifyObject
    {
        private readonly VoteClient voteClient;
        private readonly NicoClient nicoClient;
        //private readonly CommenterManager commenterManager;

        /// <summary>
        /// 投票用のクライアントを取得します。
        /// </summary>
        public VoteClient VoteClient
        {
            get { return this.voteClient; }
        }

        /// <summary>
        /// ニコニコ用のクライアントオブジェクトを取得します。
        /// </summary>
        public NicoClient NicoClient
        {
            get { return this.nicoClient; }
        }

        /*/// <summary>
        /// コメンター管理用のオブジェクトを取得します。
        /// </summary>
        public CommenterManager CommenterManager
        {
            get { return this.commenterManager; }
        }*/

        /// <summary>
        /// 投票ルームで使われるIDを取得または設定します。
        /// </summary>
        public Guid Id
        {
            get
            {
                return Global.Settings.AS_UserId;
            }
            set
            {
                using (LazyLock())
                {
                    if (Global.Settings.AS_UserId != value)
                    {
                        Global.Settings.AS_UserId = value;

                        this.RaisePropertyChanged("Id");
                    }
                }
            }
        }

        /// <summary>
        /// 投票ルームで使われるニックネームを取得または設定します。
        /// </summary>
        public string NickName
        {
            get
            {
                return Global.Settings.AS_LoginName;
            }
            set
            {
                using (LazyLock())
                {
                    if (Global.Settings.AS_LoginName != value)
                    {
                        Global.Settings.AS_LoginName = value;

                        this.RaisePropertyChanged("NickName");
                    }
                }
            }
        }

        /// <summary>
        /// 投票ルームで画像URLを取得または設定します。
        /// </summary>
        public string ImageUrl
        {
            get
            {
                return Global.Settings.AS_LoginImageUrl;
            }
            set
            {
                using (LazyLock())
                {
                    if (Global.Settings.AS_LoginImageUrl != value)
                    {
                        Global.Settings.AS_LoginImageUrl = value;

                        this.RaisePropertyChanged("ImageUrl");
                    }
                }
            }
        }

        /// <summary>
        /// 投票ルーム作成時/入室時に使うパスワードを取得または設定します。
        /// </summary>
        public string VoteRoomPassword
        {
            get
            {
                return Global.Settings.AS_VoteRoomPassword;
            }
            set
            {
                using (LazyLock())
                {
                    if (Global.Settings.AS_VoteRoomPassword != value)
                    {
                        Global.Settings.AS_VoteRoomPassword = value;

                        this.RaisePropertyChanged("VoteRoomPassword");
                    }
                }
            }
        }

        /// <summary>
        /// 投票ルーム作成時に使うルーム名を取得または設定します。
        /// </summary>
        public string VoteRoomName
        {
            get
            {
                return Global.Settings.AS_VoteRoomName;
            }
            set
            {
                using (LazyLock())
                {
                    if (Global.Settings.AS_VoteRoomName != value)
                    {
                        Global.Settings.AS_VoteRoomName = value;

                        this.RaisePropertyChanged("VoteRoomName");
                    }
                }
            }
        }

        /// <summary>
        /// 投票モードを取得または設定します。
        /// </summary>
        public VoteMode CurrentVoteMode
        {
            get { return GetValue<VoteMode>("CurrentVoteMode"); }
            set { SetValue("CurrentVoteMode", value); }
        }

        /// <summary>
        /// 全コメントをミラーするモードかどうかを取得または設定します。
        /// </summary>
        public bool IsMirrorMode
        {
            get { return GetValue<bool>("IsMirrorMode"); }
            set { SetValue("IsMirrorMode", value); }
        }

        /// <summary>
        /// 投票サーバーから得られたメッセージのリストを取得します。
        /// </summary>
        public ConcurrentQueue<Notification> NotificationQueue
        {
            get { return GetValue<ConcurrentQueue<Notification>>("NotificationQueue"); }
            set { SetValue("NotificationQueue", value); }
        }

        /// <summary>
        /// 放送クライアントのリストを取得します。
        /// </summary>
        public List<LiveClient> LiveClientList
        {
            get { return GetValue<List<LiveClient>>("LiveClientList"); }
            private set { SetValue("LiveClientList", value); }
        }
        
        /// <summary>
        /// 一つでも放送に接続していれば真を返します。
        /// </summary>
        [DependOnProperty(typeof(LiveClient), "IsConnected")]
        public bool IsConnectedToLive
        {
            get { return LiveClientList.Any(_ => _.IsConnected); }
        }

        /// <summary>
        /// 同じ放送IDを持っているかどうか調べます。
        /// </summary>
        public bool HasLiveRoom(LiveData liveData)
        {
            if (liveData == null || !liveData.Validate())
            {
                return false;
            }

            return LiveClientList.Any(_ => _.LiveData == liveData);
        }

        /// <summary>
        /// このニコニコアカウントをコメンターとして扱うかどうかを
        /// 取得または設定します。
        /// </summary>
        public bool IsUseAsNicoCommenter
        {
            get
            {
                return Global.Settings.AS_IsUseAsNicoCommenter;
            }
            set
            {
                using (LazyLock())
                {
                    if (Global.Settings.AS_IsUseAsNicoCommenter != value)
                    {
                        Global.Settings.AS_IsUseAsNicoCommenter = value;

                        UpdateNicoLoginStatus();

                        this.RaisePropertyChanged("IsUseAsNicoCommenter");
                    }
                }
            }
        }

        /// <summary>
        /// コメント中継を自動的に開始します。
        /// </summary>
        public bool IsNicoCommenterAutoStart
        {
            get
            {
                return Global.Settings.AS_IsNicoCommenterAutoStart;
            }
            set
            {
                using (LazyLock())
                {
                    if (Global.Settings.AS_IsNicoCommenterAutoStart != value)
                    {
                        Global.Settings.AS_IsNicoCommenterAutoStart = value;

                        this.RaisePropertyChanged("IsNicoCommenterAutoStart");
                    }
                }
            }
        }

        /// <summary>
        /// 投票サーバーからメッセージが届いたときに呼ばれます。
        /// </summary>
        private void HandleNotification(object sender, NotificationEventArgs e)
        {
            var notification = e.Notification;

            if (notification == null || !notification.Validate())
            {
                return;
            }

            // 並列実行可能なコレクションを使っています。
            NotificationQueue.Enqueue(notification);

            // 各放送に通知を処理させます。
            foreach (var liveClient in LiveClientList)
            {
                liveClient.HandleNotification(notification);
            }

            // 各プラグインに処理させます。
            foreach(var plugin in Global.PluginList)
            {
                Util.SafeCall(() =>
                    plugin.HandleNotification(notification));
            }
        }

        /// <summary>
        /// ニコ生へのログイン状況を更新します。
        /// </summary>
        private void UpdateNicoLoginStatus()
        {
            // 投票ルームに接続していなければ無視します。
            if (!this.voteClient.IsConnected)
            {
                return;
            }

            // ログイン状況を更新します。
            var loginType = NicoLoginType.NotLogined;
            if (this.nicoClient.IsLogin)
            {
                loginType =
                    (this.nicoClient.AccountInfo.IsPremium
                         ? NicoLoginType.Premium
                         : NicoLoginType.Normal);
            }

            this.voteClient.SetParticipantAttribute(
                IsUseAsNicoCommenter, loginType, null, null);
        }

        /// <summary>
        /// 投票モードの更新を行います。
        /// </summary>
        private void UpdateVoteMode()
        {
            if (this.voteClient.IsLogined &&
                this.voteClient.IsVoteRoomOwner)
            {
                this.voteClient.ChangeVoteMode(CurrentVoteMode, IsMirrorMode);
            }
        }

        /// <summary>
        /// 各放送ルームの属性を更新します。
        /// </summary>
        private void UpdateLiveRoomAttribute()
        {
            if (this.voteClient.IsLogined)
            {
                foreach (var liveClient in LiveClientList)
                {
                    liveClient.VoteLogined();
                }
            }
        }

        /// <summary>
        /// 他ツールと連携するために必要です。
        /// </summary>
        private void UpdateTotalVoteSpan()
        {
            var roomInfo = this.voteClient.VoteRoomInfo;

            if (roomInfo == null)
            {
                ProtocolUtil.RemoveTotalVoteSpan();
            }
            else
            {
                ProtocolUtil.WriteTotalVoteSpan(
                    roomInfo.State,
                    roomInfo.BaseTimeNtp,
                    roomInfo.TotalVoteSpan,
                    roomInfo.ProgressSpan);
            }
        }

        void voteClient_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // 接続したら投票ルームに入っていなくてもログイン状態を送信します。
            // ログイン状態は投票ルームとは関係無く設定できます。
            if (e.PropertyName == "IsConnected")
            {
                UpdateNicoLoginStatus();
            }

            // 投票ルーム入出時に、各放送オブジェクトにそれを通知します。
            // 放送情報は投票ルームごとに通知する必要があります。
            if (e.PropertyName == "IsLogined")
            {
                UpdateVoteMode();
                UpdateLiveRoomAttribute();
            }

            if (e.PropertyName == "IsLogined" ||
                e.PropertyName == "VoteState" ||
                e.PropertyName == "TotalVoteSpan" ||
                e.PropertyName == "BaseTimeNtp")
            {
                UpdateTotalVoteSpan();
            }

            if (e.PropertyName == "VoteMode")
            {
                CurrentVoteMode = this.voteClient.VoteMode;
            }

            if (e.PropertyName == "IsMirrorMode")
            {
                IsMirrorMode = this.voteClient.IsMirrorMode;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainModel()
        {
            ProtocolUtil.RemoveTotalVoteSpan();

            AddPropertyChangedHandler(
                "CurrentVoteMode",
                (_, __) => UpdateVoteMode());
            AddPropertyChangedHandler(
                "IsMirrorMode",
                (_, __) => UpdateVoteMode());

            this.voteClient = new VoteClient();
            this.voteClient.NotificationReceived += HandleNotification;
            this.voteClient.PropertyChanged += voteClient_PropertyChanged;
            this.voteClient.StartLeaveTimeTimer();

            // ニコニコへのログインを裏で非同期的に行います。
            this.nicoClient = new NicoClient();
            this.nicoClient.LoginSucceeded += (sender, e) =>
            {
                Global.Settings.AS_OwnerNicoLoginData = this.nicoClient.LoginData;
                UpdateNicoLoginStatus();
            };
            this.nicoClient.AddPropertyChangedHandler(
                "IsLogined",
                (_, __) => UpdateNicoLoginStatus());

            var loginData = Global.Settings.AS_OwnerNicoLoginData;
            if (loginData != null)
            {
                this.nicoClient.LoginAsync(loginData);
            }

            // 放送サイトごとのリストを作成します。
            LiveClientList = new List<LiveClient>()
            {
                new LiveNicoClient(this, this.nicoClient),
            };

            // 放送接続状態が変わったら、このオブジェクトのプロパティも更新します。
            LiveClientList.ForEach(_ => 
                _.AddPropertyChangedHandler(
                    "IsConnected",
                    (__, ___) => this.RaisePropertyChanged("IsConnectedToLive")));

            NotificationQueue = new ConcurrentQueue<Notification>();
            CurrentVoteMode = VoteMode.Shogi;

            this.AddDependModel(this.nicoClient);
            this.AddDependModel(this.voteClient);
            LiveClientList.ForEach(_ => this.AddDependModel(_));
        }
    }
}
