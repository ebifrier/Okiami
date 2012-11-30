﻿using System;
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
using Ragnarok.Utility;
using Ragnarok.NicoNico;
using Ragnarok.NicoNico.Live;
using Ragnarok.ObjectModel;
using Ragnarok.Presentation;

namespace VoteSystem.Client.Model
{
    using VoteSystem.Protocol;
    using VoteSystem.Protocol.Vote;
    using VoteSystem.Client.Model.Live;

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
        private readonly List<LiveClient> liveClientList =
            new List<LiveClient>();
        private readonly ConcurrentQueue<Notification> notificationQueue =
            new ConcurrentQueue<Notification>();
        private Timer leaveTimeTimer;
        private int oldVoteLeaveSeconds = -1;
        private int oldTotalVoteLeaveSeconds = -1;        

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
        /// 投票モードごとに固有の評価値を取得または設定します。
        /// </summary>
        /// <remarks>
        /// この評価値はプラグインから設定されることも想定しています。
        /// そのため、これはグローバル変数である必要がありますが、
        /// プロパティ変更通知が送れるグローバル変数を定義する適切な場所が
        /// 今のシステムにはありません。
        /// 
        /// そのため、評価値をこのクラスで定義しています。
        /// </remarks>
        public double ModeCustomPoint
        {
            get { return GetValue<double>("ModeCustomPoint"); }
            set { SetValue("ModeCustomPoint", value); }
        }

        /// <summary>
        /// 投票用のクライアントを取得します。
        /// </summary>
        public VoteClient VoteClient
        {
            get { return this.voteClient; }
        }

        /// <summary>
        /// 投票サーバーから得られたメッセージのリストを取得します。
        /// </summary>
        public ConcurrentQueue<Notification> NotificationQueue
        {
            get { return this.notificationQueue; }
        }

        /// <summary>
        /// 放送クライアントのリストのコピーを取得します。
        /// </summary>
        public List<LiveClient> LiveClientList
        {
            get { return new List<LiveClient>(this.liveClientList); }
        }

        /// <summary>
        /// ニコニコ用のクライアントオブジェクトを取得します。
        /// </summary>
        public NicoClient NicoClient
        {
            get { return this.nicoClient; }
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
        /// 接続待ちの放送一覧を取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteClient), "CommenterClientList")]
        public NotifyCollection<CommenterCommentClient> CommenterClientList
        {
            get { return this.voteClient.CommenterClientList; }
        }

        /// <summary>
        /// 中継したコメント一覧を取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteClient), "PostCommentList")]
        public NotifyCollection<PostCommentData> PostCommentList
        {
            get { return this.voteClient.PostCommentList; }
        }

        /// <summary>
        /// 投票状態を取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteClient), "VoteState")]
        public VoteState VoteState
        {
            get { return this.voteClient.VoteState; }
        }

        /// <summary>
        /// 投票の残り時間を取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteClient), "VoteState")]
        [DependOnProperty(typeof(VoteClient), "VoteSpan")]
        public TimeSpan VoteLeaveTime
        {
            get
            {
                using (LazyLock())
                {
                    // 未入室の場合
                    if (this.voteClient.VoteRoomInfo == null)
                    {
                        return TimeSpan.Zero;
                    }

                    // 時間無制限
                    if (this.voteClient.VoteSpan == TimeSpan.MaxValue)
                    {
                        return TimeSpan.MaxValue;
                    }

                    // 残り時間を計算します。
                    var baseTimeNtp = Ragnarok.Net.NtpClient.GetTime();
                    switch (voteClient.VoteState)
                    {
                        case VoteState.Voting:
                            // 終了時刻から現在時刻を減算し、残り時間を出します。
                            var endTimeNtp =
                                this.voteClient.VoteRoomInfo.BaseTimeNtp +
                                this.voteClient.VoteSpan;
                            return (endTimeNtp - baseTimeNtp);
                        case VoteState.Pause:
                            return this.voteClient.VoteSpan;
                        case VoteState.Stop:
                        case VoteState.End:
                            return TimeSpan.Zero;
                    }

                    return TimeSpan.Zero;
                }
            }
        }

        /// <summary>
        /// 投票の全残り時間を取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteClient), "VoteState")]
        [DependOnProperty(typeof(VoteClient), "TotalVoteSpan")]
        public TimeSpan TotalVoteLeaveTime
        {
            get
            {
                using (LazyLock())
                {
                    // 未入室の場合
                    if (this.voteClient.VoteRoomInfo == null)
                    {
                        return TimeSpan.Zero;
                    }

                    // 時間無制限
                    if (this.voteClient.TotalVoteSpan == TimeSpan.MaxValue)
                    {
                        return TimeSpan.MaxValue;
                    }

                    // 残り時間を計算します。
                    var baseTimeNtp = Ragnarok.Net.NtpClient.GetTime();
                    switch (voteClient.VoteState)
                    {
                        case VoteState.Voting:
                            // 終了時刻から現在時刻を減算し、残り時間を出します。
                            var endTimeNtp =
                                this.voteClient.VoteRoomInfo.BaseTimeNtp +
                                this.voteClient.TotalVoteSpan;
                            return (endTimeNtp - baseTimeNtp);
                        case VoteState.Pause:
                        case VoteState.Stop:
                        case VoteState.End:
                            return this.voteClient.TotalVoteSpan;
                    }

                    return TimeSpan.Zero;
                }
            }
        }

        /// <summary>
        /// 投票時間が無制限かどうかを取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteClient), "VoteSpan")]
        public bool IsVoteLeaveTimeNoLimit
        {
            get
            {
                // 未入室の場合
                if (this.voteClient.VoteRoomInfo == null)
                {
                    return false;
                }

                return (this.voteClient.VoteSpan == TimeSpan.MaxValue);
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
            this.notificationQueue.Enqueue(notification);

            // 各放送に通知を処理させます。
            foreach (var liveClient in this.liveClientList)
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
                this.voteClient.ChangeVoteMode(CurrentVoteMode);
            }
        }

        /// <summary>
        /// 各放送ルームの属性を更新します。
        /// </summary>
        private void UpdateLiveRoomAttribute()
        {
            if (this.voteClient.IsLogined)
            {
                foreach (var liveClient in this.liveClientList)
                {
                    liveClient.VoteLogined();
                }
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
        }

        void nicoClient_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // ニコニコにログインしたら、状態を通知します。
            if (e.PropertyName == "IsLogined")
            {
                UpdateNicoLoginStatus();
            }
        }

        void MainModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentVoteMode")
            {
                UpdateVoteMode();
            }
        }

        /// <summary>
        /// 投票時間の更新を行います。
        /// </summary>
        private void LeaveTimeTimer_Callback(object state)
        {
            var leaveSeconds = (int)VoteLeaveTime.TotalSeconds;
            if (leaveSeconds != this.oldVoteLeaveSeconds)
            {
                this.oldVoteLeaveSeconds = leaveSeconds;
                this.RaisePropertyChanged("VoteLeaveTime");
            }

            leaveSeconds = (int)TotalVoteLeaveTime.TotalSeconds;
            if (leaveSeconds != this.oldTotalVoteLeaveSeconds)
            {
                this.oldTotalVoteLeaveSeconds = leaveSeconds;
                this.RaisePropertyChanged("TotalVoteLeaveTime");
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainModel()
        {
            PropertyChanged += MainModel_PropertyChanged;

            this.voteClient = new VoteClient(true);
            this.voteClient.NotificationReceived += HandleNotification;
            this.voteClient.PropertyChanged += voteClient_PropertyChanged;

            // ニコニコへのログインを裏で非同期的に行います。
            this.nicoClient = new NicoClient();
            this.nicoClient.LoginSucceeded += (sender, e) =>
            {
                Global.Settings.AS_OwnerNicoLoginData = this.nicoClient.LoginData;
                UpdateNicoLoginStatus();
            };
            this.nicoClient.PropertyChanged += nicoClient_PropertyChanged;

            var loginData = Global.Settings.AS_OwnerNicoLoginData;
            if (loginData != null)
            {
                this.nicoClient.LoginAsync(loginData);
            }

            // 放送サイトごとのリストを作成します。
            this.liveClientList = new List<LiveClient>()
            {
                new LiveNicoClient(this, this.nicoClient),
            };

            CurrentVoteMode = VoteMode.Shogi;

            this.AddDependModel(this.nicoClient);
            this.AddDependModel(this.voteClient);

            // 投票残り時間を更新するために使います。
            this.leaveTimeTimer = new Timer(
                LeaveTimeTimer_Callback,
                null,
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromMilliseconds(1000));
        }
    }
}