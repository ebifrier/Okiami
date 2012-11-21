using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;

using Ragnarok;
using Ragnarok.Net;
using Ragnarok.Net.ProtoBuf;
using Ragnarok.Utility;
using Ragnarok.ObjectModel;

namespace VoteSystem.Server
{
    using Protocol;
    using Protocol.Vote;

    /// <summary>
    /// 投票部屋ごとに用意されるオブジェクトです。
    /// </summary>
    /// <remarks>
    /// 各投票部屋ごとに用意され、それに参加している放送主が集約されています。
    /// 各放送主から投票や参加希望などのメソッドが実行されます。
    /// </remarks>
    public sealed class VoteRoom : NotifyObject, ILogObject
    {
        private int liveOwnerIdCounter = 0;
        private readonly VoteModel voteModel;
        private readonly VoteTimeKeeper voteTimeKeeper;
        private readonly VoterListManager voterListManager;
        private readonly List<VoteParticipant> participantList =
            new List<VoteParticipant>();
        private readonly int id;
        private readonly string name;
        private readonly string password;
        private VoteParticipant voteRoomOwner;
        private Thread voteResultThread;
        private volatile bool isClosed;

        /// <summary>
        /// ログ出力用の名前を取得します。
        /// </summary>
        public string LogName
        {
            get
            {
                return string.Format(
                    "投票部屋[{0}]",
                    this.id);
            }
        }

        /// <summary>
        /// 投票管理オブジェクトを取得します。
        /// </summary>
        public VoteModel VoteModel
        {
            get { return this.voteModel; }
        }

        /// <summary>
        /// 投票の時間管理オブジェクトを取得します。
        /// </summary>
        public VoteTimeKeeper VoteTimeKeeper
        {
            get { return this.voteTimeKeeper; }
        }
        
        /// <summary>
        /// 投票の参加者管理オブジェクトを取得します。
        /// </summary>
        public VoterListManager VoterListManager
        {
            get { return this.voterListManager; }
        }

        /// <summary>
        /// 投票ルームのオーナーを取得します。
        /// </summary>
        public VoteParticipant VoteRoomOwner
        {
            get { return this.voteRoomOwner; }
            set { SetValue("VoteRoomOwner", value, ref this.voteRoomOwner); }
        }

        /// <summary>
        /// 部屋IDを取得します。
        /// </summary>
        public int Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// 名前を取得または設定します。
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// パスワードを使用するか取得します。
        /// </summary>
        public bool HasPassword
        {
            get { return (this.password != null); }
        }

        /// <summary>
        /// PropertyChanged用のプロパティです。使わないで下さい。
        /// </summary>
        private List<VoteParticipant> ParticipantList
        {
            get { return this.participantList; }
        }

        /// <summary>
        /// 部屋に接続されている参加者の数を取得します。
        /// </summary>
        public int ParticipantCount
        {
            get
            {
                using (LazyLock())
                {
                    return this.participantList.Count;
                }
            }
        }

        /// <summary>
        /// 投票部屋の情報を取得します。
        /// </summary>
        public VoteRoomInfo Info
        {
            get
            {
                using (LazyLock())
                {
                    var ownerId = (
                        this.voteRoomOwner != null ?
                        this.voteRoomOwner.No :
                        -1);

                    // パスワードは渡しません。
                    var voteRoomInfo = new VoteRoomInfo()
                    {
                        Id = this.Id,
                        Name = this.Name,
                        HasPassword = this.HasPassword,
                        OwnerNo = ownerId,
                        State = this.voteTimeKeeper.VoteState,
                        Mode = this.voteModel.VoteMode,
                        BaseTimeNtp = this.voteTimeKeeper.VoteStartTimeNtp,
                        VoteSpan = this.voteTimeKeeper.VoteSpan,
                        TotalVoteSpan = this.voteTimeKeeper.TotalVoteSpan,

                        // 投票ルームに参加している各参加者の情報を設定します。
                        // 放送主を先に表示します。
                        ParticipantList = this.participantList
                            .Select(participant => participant.Info)
                            .OrderBy(info => info.No +
                                (info.LiveDataList.Any() ? 0 : int.MaxValue / 2))
                            .ToArray(),
                    };

                    return voteRoomInfo;
                }
            }
        }

        /// <summary>
        /// 指定の位置にいる参加者を取得します。
        /// </summary>
        public VoteParticipant GetParticipant(int index)
        {
            using (LazyLock())
            {
                if (index < 0 || this.participantList.Count <= index)
                {
                    return null;
                }

                return this.participantList[index];
            }
        }

        /// <summary>
        /// 指定のIDを持つ参加者を探します。
        /// </summary>
        public VoteParticipant GetParticipant(Guid guid)
        {
            using (LazyLock())
            {
                return this.participantList.FirstOrDefault(
                    p => p.Id == guid);
            }
        }

        /// <summary>
        /// パスワードにマッチするか調べます。
        /// </summary>
        public bool MatchPassword(string password)
        {
            return (this.password == password);
        }

        /// <summary>
        /// 投票ルーム主のコネクションか調べます。
        /// </summary>
        /// <remarks>
        /// 通信プロトコルのコールバックとして処理が行われるので、
        /// コネクションからオーナーか判断する必要があります。
        /// </remarks>
        public bool IsRoomOwnerConnection(PbConnection connection)
        {
            using (LazyLock())
            {
                if (this.voteRoomOwner == null || connection == null)
                {
                    return false;
                }

                return ReferenceEquals(
                    this.voteRoomOwner.Connection,
                    connection);
            }
        }

        /// <summary>
        /// 指定のIDを持つ放送ルームを取得します。
        /// </summary>
        public LiveRoom GetLiveRoom(LiveData liveData)
        {
            if (liveData == null || !liveData.Validate())
            {
                return null;
            }

            if (liveData.Site != LiveSite.NicoNama)
            {
                return null;
            }

            using (LazyLock())
            {
                // 全参加者の中から指定のIDを持つ放送ルームを探します。
                foreach (var participant in this.participantList)
                {
                    var liveRoom = participant.GetLiveRoom(liveData);
                    if (liveRoom != null)
                    {
                        return liveRoom;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// シグナル受信時に呼ばれます。
        /// </summary>
        public void SignalReceived(int signum)
        {
            var voteModel = this.voteModel;
            if (voteModel != null)
            {
                voteModel.SignalReceived(signum);
            }
        }

        /// <summary>
        /// 投票ルームを閉じます。
        /// </summary>
        public void Close()
        {
            this.isClosed = true;

            if (this.voteResultThread != null)
            {
                // Joinを呼ばなくても、スレッド終了時にはリソースが破棄されます。
                //this.voteResultThread.Join();

                this.voteResultThread = null;
            }

            if (this.voteTimeKeeper != null)
            {
                this.voteTimeKeeper.PropertyChanged -= voteModel_PropertyChanged;
            }

            // 参加者が０になったら、投票ルームを削除します。
            GlobalControl.Instance.RemoveVoteRoom(this.Id);

            Log.Info(this,
                "投票ルームを削除しました。");
        }

        #region Participant
        /// <summary>
        /// 投票に参加する放送主の新規ＩＤを取得します。
        /// </summary>
        private int NextLiveOwnerId()
        {
            return Interlocked.Increment(ref this.liveOwnerIdCounter);
        }

        /// <summary>
        /// 処理ハンドラを追加します。
        /// </summary>
        public void ConnectHandlers(PbConnection connection)
        {
            this.voteModel.ConnectHandlers(connection);
            this.voteTimeKeeper.ConnectHandlers(connection);

            connection.AddCommandHandler<StartEndRollCommand>(
                HandleStartEndRollCommand);
            connection.AddCommandHandler<StopEndRollCommand>(
                HandleStopEndRollCommand);
        }

        /// <summary>
        /// 処理ハンドラを削除します。
        /// </summary>
        public void DisconnectHandlers(PbConnection connection)
        {
            this.voteModel.DisconnectHandlers(connection);
            this.voteTimeKeeper.DisconnectHandlers(connection);

            connection.RemoveHandler<StartEndRollCommand>();
            connection.RemoveHandler<StopEndRollCommand>();
        }

        /// <summary>
        /// エンドロールを開始します。
        /// </summary>
        private void HandleStartEndRollCommand(
            object sender,
            PbCommandEventArgs<StartEndRollCommand> e)
        {
            var isRoomOwner = IsRoomOwnerConnection(sender as PbConnection);
            if (!isRoomOwner)
            {
                throw new InvalidOperationException(
                    "投票ルームオーナーではありません。");
            }

            var seconds = e.Command.RollTimeSeconds;
            BroadcastCommand(new StartEndRollCommand
            {
                RollTimeSeconds = seconds,
            });
        }

        /// <summary>
        /// エンドロールを停止します。
        /// </summary>
        private void HandleStopEndRollCommand(
            object sender,
            PbCommandEventArgs<StopEndRollCommand> e)
        {
            var isRoomOwner = IsRoomOwnerConnection(sender as PbConnection);
            if (!isRoomOwner)
            {
                throw new InvalidOperationException(
                    "投票ルームオーナーではありません。");
            }

            BroadcastCommand(new StopEndRollCommand());
        }

        /// <summary>
        /// 参加者のプロパティが変更されたときに呼ばれます。
        /// </summary>
        private void participant_PropertyChanged(object sender,
                                                 PropertyChangedEventArgs e)
        {
            var participant = sender as VoteParticipant;

            if (participant != null && e.PropertyName == "Info")
            {
                this.RaisePropertyChanged("ParticipantList");
            }
        }

        /// <summary>
        /// 参加者のコネクションが切断されたときに呼ばれます。
        /// </summary>
        private void participant_Disconnected(object sender,
                                              DisconnectEventArgs e)
        {
            var participant = sender as VoteParticipant;

            if (participant != null)
            {
                RemoveParticipant(participant);
            }
        }

        /// <summary>
        /// 参加者を追加します。
        /// </summary>
        public void AddParticipant(VoteParticipant participant)
        {
            if (participant == null)
            {
                return;
            }

            if (participant.VoteRoom != null)
            {
                throw new InvalidOperationException(
                    "すでに投票ルームにログインしています。");
            }

            using (LazyLock())
            {
                participant.PropertyChanged += participant_PropertyChanged;
                participant.Disconnected += participant_Disconnected;
                participant.SetVoteRoom(this, NextLiveOwnerId());

                this.participantList.Add(participant);

                this.RaisePropertyChanged("ParticipantList");
            }

            Log.Info(this,
                "新規参加者を追加しました。");
        }

        /// <summary>
        /// 参加者を削除します。
        /// </summary>
        public void RemoveParticipant(VoteParticipant participant)
        {
            if (participant == null)
            {
                return;
            }

            using (LazyLock())
            {
                // このルームの参加者じゃなければ帰ります。
                if (!this.participantList.Remove(participant))
                {
                    return;
                }

                // もし投票ルームのオーナーが退出した場合は、
                // 別の人をオーナーに設定します。
                if (participant == this.voteRoomOwner)
                {
                    // リストの先頭にいる放送主をルームオーナーにします。
                    // もし参加者がいない場合、オーナーは不在となります。
                    VoteRoomOwner = this.participantList.FirstOrDefault();
                }

                // 投票ルームなどを初期化します。
                participant.PropertyChanged -= participant_PropertyChanged;
                participant.Disconnected -= participant_Disconnected;
                participant.SetVoteRoom(null, -1);

                this.RaisePropertyChanged("ParticipantList");
            }

            Log.Info(this,
                "参加者を削除しました。");

            if (this.ParticipantCount == 0)
            {
                // 参加者が０になったら、投票ルームを削除します。
                Close();
            }
        }
        #endregion

        #region 通知などのブロードキャスト
        /// <summary>
        /// メッセージを登録情報に応じて修正します。
        /// </summary>
        private Notification ModifyNotification(Notification source)
        {
            // 投票通知と参加通知の場合のみ、
            // 投稿者のスキルによって色を変えます。
            if (source.Type != NotificationType.Vote &&
                source.Type != NotificationType.Join)
            {
                return source;
            }

            var voter = this.voterListManager.GetJoinedVoter(source.VoterId);
            if (voter == null)
            {
                return source;
            }

            // 色を変えます。
            var newNotification = source.Clone();
            newNotification.Color = voter.Color;
            return newNotification;
        }

        /// <summary>
        /// 通知を全参加者に送ります。
        /// </summary>
        public void BroadcastNotification(Notification notification,
                                          bool sendAsNotification,
                                          bool sendToLiveRoom)
        {
            if (notification == null || !notification.Validate())
            {
                throw new ArgumentException(
                    "送信する通知が正しくありません。", "e");
            }

            // モードによっては参加者のスキルによって
            // 色などを変える場合があります。
            var newNotification = ModifyNotification(notification);

            using (LazyLock())
            {
                // 各放送主にメッセージを送信します。
                foreach (var participant in this.participantList)
                {
                    participant.SendNotificationCommand(
                        newNotification,
                        sendAsNotification,
                        sendToLiveRoom);
                }
            }

            Log.Info(this,
                "通知({0})を全ユーザーに送信しました。",
                notification.Text);
        }

        /// <summary>
        /// 通知を各ルームの参加者に送信します。
        /// </summary>
        public void BroadcastNotification(NotificationType type,
                                          Notification source,
                                          bool sendAsNotification,
                                          bool sendToLiveRoom)
        {
            if (source == null || !source.Validate())
            {
                return;
            }

            var newNotification = source.Clone();
            newNotification.Type = type;

            BroadcastNotification(
                newNotification,
                sendAsNotification,
                sendToLiveRoom);
        }

        /// <summary>
        /// 通知を各ルームの参加者に送信します。
        /// </summary>
        public void BroadcastNotification(string text, NotificationType type,
                                          Notification source,
                                          bool sendAsNotification,
                                          bool sendToLiveRoom)
        {
            if (source == null || !source.Validate())
            {
                return;
            }

            var newNotification = source.Clone();
            newNotification.Text = text;
            newNotification.Type = type;

            BroadcastNotification(
                newNotification,
                sendAsNotification,
                sendToLiveRoom);
        }

        /// <summary>
        /// システム通知を全参加者に送ります。
        /// </summary>
        public void BroadcastSystemNotification(SystemNotificationType systemType)
        {
            if (systemType == SystemNotificationType.Unknown)
            {
                throw new ArgumentException(
                    "与えられた通知種別が正しくありません。",
                    "systemType");
            }

            var notification = new Notification()
            {
                Color = NotificationColor.Default,
                Type = NotificationType.System,
                SystemType = systemType,
                Text = "SystemNotification",
                FromLiveRoom = null,
                VoterId = "$owner",
                Timestamp = Ragnarok.Net.NtpClient.GetTime(),
            };
            BroadcastNotification(notification, true, false);
        }

        /// <summary>
        /// コマンドを全参加者に送ります。
        /// </summary>
        public void BroadcastCommand<T>(T command)
            where T: class
        {
            if (command == null)
            {
                throw new ArgumentException(
                    "送信するコマンドが正しくありません。", "e");
            }

            using (LazyLock())
            {
                // 各放送主にメッセージを送信します。
                foreach (var participant in this.participantList)
                {
                    participant.SendCommand(command);
                }
            }

            Log.Info(this,
                "コマンド({0})を全ユーザーに送信しました。",
                command.GetType());
        }
        #endregion

        #region 投票結果通知
        /// <summary>
        /// 投票結果を各クライアントに通知します。
        /// </summary>
        private void UpdateVoteResult()
        {
            // 投票結果が変わっているときのみ、それを送ります。
            var voteResult = this.voteModel.GetVoteResultIfChanged();
            if (voteResult == null || !voteResult.Validate())
            {
                return;
            }

            using (LazyLock())
            {
                // 各放送主にメッセージを送信します。
                foreach (var participant in this.participantList)
                {
                    participant.SendVoteResultCommand(voteResult);
                }
            }

            Log.Info(this,
                "投票結果を全ユーザーに送信しました。");
        }

        /// <summary>
        /// 投票結果を各クライアントに通知します。
        /// </summary>
        /// <remarks>
        /// 定期的にクライアントに投票結果を送るためにスレッドを使います。
        /// 
        /// タイマーでは処理の遅延が起こったときに、タイマーに設定された
        /// 他のコールバックが呼ばれなくなってしまうことがあります。
        /// このため、少し無駄ですが一応スレッドを使っています。
        /// </remarks>
        private void UpdateVoteResultLoop()
        {
            while (!this.isClosed)
            {
                try
                {
                    UpdateVoteResult();                    
                }
                catch (Exception)
                {
                    //Log.ErrorException(ex,
                    //    "投票結果の送信に失敗しました。");
                }

                // 1秒おきにチェックします。
                Thread.Sleep(TimeSpan.FromSeconds(1.0));
            }
        }
        #endregion

        #region プロパティ値変更通知
        private void voteModel_PropertyChanged(object sender,
                                               PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "VoteState":
                    VoteRoomInfoChanged("State");
                    break;
                case "VoteMode":
                    VoteRoomInfoChanged("Mode");
                    break;
                case "VoteStartTimeNtp":
                    VoteRoomInfoChanged("BaseTimeNtp");
                    break;
                case "VoteSpan":
                    VoteRoomInfoChanged("VoteSpan");
                    break;
                case "TotalVoteSpan":
                    VoteRoomInfoChanged("TotalVoteSpan");
                    break;
            }

            Log.Trace(this,
                "VoteModel.{0}: 変更されました。",
                e.PropertyName);
        }

        private void VoteRoom_PropertyChanged(object sender,
                                              PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Id":
                    VoteRoomInfoChanged("Id");
                    break;
                case "Name":
                    VoteRoomInfoChanged("Name");
                    break;
                case "VoteRoomOwner":
                    VoteRoomInfoChanged("OwnerNo");
                    break;
                case "ParticipantList":
                    VoteRoomInfoChanged("ParticipantList");
                    break;
            }

            Log.Trace(this,
                "VoteRoom.{0}: 変更されました。",
                e.PropertyName);
        }

        /// <summary>
        /// 投票ルームの状態が変更されたときに呼ばれます。
        /// </summary>
        private void VoteRoomInfoChanged(string propertyName)
        {
            // 全参加者に投票ルームの状態変化を通知します。
            using (LazyLock())
            {
                var info = Info;

                // 指定のプロパティと、その値を取得します。
                var property = MethodUtil.GetPropertyInfo(
                    typeof(VoteRoomInfo), propertyName);
                if (property == null)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "'{0}': 指定のプロパティ名は存在しません。",
                            propertyName));
                }

                var value = property.GetValue(info, null);

                // 全参加者にプロパティ値の変更を通知します。
                foreach (var participant in this.participantList)
                {
                    participant.SendObjectChangedCommand(
                        "VoteRoomInfo",
                        propertyName,
                        property.PropertyType,
                        value);
                }
            }

            Log.Trace(this,
                "Info.{0}: 変更されました。",
                propertyName);
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VoteRoom(VoteParticipant voteRoomOwner, int id,
                        string name, string password)
        {
            this.voteModel = new VoteModel(this);

            this.voteTimeKeeper = new VoteTimeKeeper(this);
            this.voteTimeKeeper.PropertyChanged += voteModel_PropertyChanged;
            
            this.voterListManager = new VoterListManager();

            this.PropertyChanged += VoteRoom_PropertyChanged;
            this.voteRoomOwner = voteRoomOwner;
            this.id = id;
            this.name = name;

            // passwordはnull or '\0'のときに、
            // パスワード不要という意味になります。
            this.password = (string.IsNullOrEmpty(password) ? null : password);

            AddParticipant(voteRoomOwner);
            this.voteModel.ChangeMode(VoteMode.Shogi);

            // 投票結果を送るためのスレッドを初期化します。
            voteResultThread = new Thread(UpdateVoteResultLoop)
            {
                Name = "VoteResultThread",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
            };
            voteResultThread.Start();
        }
    }
}
