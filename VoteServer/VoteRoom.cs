using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

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
        private int isIntClosed;

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
        /// ルームが閉じられたかどうか取得します。
        /// </summary>
        public bool IsClosed
        {
            get { return (this.isIntClosed != 0); }
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
        public VoteRoomInfo GetInfo(bool needList)
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
                    Id = Id,
                    Name = Name,
                    HasPassword = HasPassword,
                    OwnerNo = ownerId,
                    State = this.voteTimeKeeper.VoteState,
                    Mode = this.voteModel.VoteMode,
                    IsMirrorMode = this.voteModel.IsMirrorMode,
                    BaseTimeNtp = this.voteTimeKeeper.VoteStartTimeNtp,
                    VoteSpan = this.voteTimeKeeper.VoteSpan,
                    TotalVoteSpan = this.voteTimeKeeper.TotalVoteSpan,
                };

                if (needList)
                {
                    // 投票ルームに参加している各参加者の情報を設定します。
                    var list = new NotifyCollection<VoteParticipantInfo>();
                    this.participantList
                        .Select(_ => _.Info)
                        .ForEach(_ => list.Add(_));

                    voteRoomInfo.ParticipantList = list;
                }

                return voteRoomInfo;
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
            if (Interlocked.Exchange(ref this.isIntClosed, 1) != 0)
            {
                // すでに閉じています。
                return;
            }

            if (this.voteResultThread != null)
            {
                // Joinを呼ばなくても、スレッド終了時にはリソースが破棄されます。
                //this.voteResultThread.Join();

                this.voteResultThread = null;
            }

            // 参加者が０になったら、投票ルームを削除します。
            GlobalControl.Instance.RemoveVoteRoom(Id);

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

            BroadcastCommand(e.Command);
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
                // TODO: これは遅いかも
                //this.RaisePropertyChanged("ParticipantList");
                ParticipantUpdated(
                    CollectionOperation.CollectionReplace,
                    participant, false);
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
                // このメソッドでプロパティが変更されます。
                // その時参加者一覧を送ることを避けるため、
                // PropertyChangedなどのイベントは後で設定します。
                participant.SetVoteRoom(this, NextLiveOwnerId());

                participant.PropertyChanged += participant_PropertyChanged;
                participant.Disconnected += participant_Disconnected;

                this.participantList.Add(participant);

                // 参加者の変化を通知します。
                ParticipantUpdated(
                    CollectionOperation.CollectionAdd,
                    participant, true);
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

                Log.Info(this,
                   "参加者を削除しました。");

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

                // 参加者の変化やオーナーの変更を通知します。
                ParticipantUpdated(
                    CollectionOperation.CollectionRemove,
                    participant, true);
            }

            if (ParticipantCount == 0)
            {
                // 参加者が０になったら、投票ルームを削除します。
                Close();
            }
        }
        #endregion

        #region 通知などのブロードキャスト
        /// <summary>
        /// 参加者の状態が変わった時に呼ばれます。
        /// </summary>
        /// <remarks>
        /// 参加者の状態をすべての参加者に通知します。
        /// </remarks>
        public void ParticipantUpdated(CollectionOperation op,
                                       VoteParticipant participant,
                                       bool exceptSelf)
        {
            if (participant == null)
            {
                return;
            }

            using (LazyLock())
            {
                var command = new ChangeParticipantInfoCommand
                {
                    Operation = op,
                    Info = participant.Info,
                    ListCount = this.participantList.Count(),
                };

                var sendData = new PbSendData(command);
                foreach (var p in this.participantList)
                {
                    if (exceptSelf && ReferenceEquals(participant, p))
                    {
                        continue;
                    }

                    p.SendData(sendData, false);
                }

                Log.Info(this,
                    "VoteParticipant(No.{0})の状態変化をすべての参加者({1})に送信しました。",
                    command.Info.No,
                    this.participantList.Count());
            }
        }

        /// <summary>
        /// 投票ルームの状態が変わった時に呼ばれます。
        /// </summary>
        /// <remarks>
        /// ルームの状態をすべての参加者に通知します。
        /// </remarks>
        public void Updated()
        {
            using (LazyLock())
            {
                // 参加者一覧は送りません。
                var command = new SendVoteRoomInfoCommand
                {
                    RoomInfo = GetInfo(false),
                };

                var sendData = new PbSendData(command);
                foreach (var participant in this.participantList)
                {
                    participant.SendData(sendData, false);
                }

                Log.Info(this,
                    "VoteRoomの状態をすべての参加者({0})に送信しました。",
                    this.participantList.Count());
            }
        }

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
            return source.Clone().Apply(_ => _.Color = voter.Color);
        }

        /// <summary>
        /// 通知を全参加者に送ります。
        /// </summary>
        public void BroadcastNotification(Notification notification,
                                          bool sendAsNotification,
                                          bool sendToLiveRoom,
                                          VoteParticipant except = null)
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
                    if (ReferenceEquals(participant, except))
                    {
                        continue;
                    }

                    participant.SendNotificationCommand(
                        newNotification,
                        sendAsNotification,
                        sendToLiveRoom,
                        false);
                }

                Log.Info(this,
                    "通知({0})を全ユーザー({1})に送信しました。",
                    notification.Text,
                    this.participantList.Count());
            }
        }

        /// <summary>
        /// 通知を各ルームの参加者に送信します。
        /// </summary>
        public void BroadcastNotification(NotificationType type,
                                          Notification source,
                                          bool sendAsNotification,
                                          bool sendToLiveRoom,
                                          VoteParticipant except = null)
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
                sendToLiveRoom,
                except);
        }

        /// <summary>
        /// 通知を各ルームの参加者に送信します。
        /// </summary>
        public void BroadcastNotification(string text, NotificationType type,
                                          Notification source,
                                          bool sendAsNotification,
                                          bool sendToLiveRoom,
                                          VoteParticipant except = null)
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
                sendToLiveRoom,
                except);
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
                // 各参加者にメッセージを送信します。
                foreach (var participant in this.participantList)
                {
                    participant.SendCommand(command, false);
                }

                Log.Info(this,
                    "コマンド({0})を全ユーザー({1})に送信しました。",
                    command.GetType(),
                    this.participantList.Count());
            }
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
                var command = new SendVoteResultCommand()
                {
                    Result = voteResult,
                };

                // 各参加者にメッセージを送信します。
                foreach (var participant in this.participantList)
                {
                    participant.SendCommand(command, false);
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
            while (!IsClosed)
            {
                try
                {
                    UpdateVoteResult();                    
                }
                catch (Exception ex)
                {
                    Log.ErrorException(ex,
                        "投票結果の送信に失敗しました。");
                }

                // 1秒おきにチェックします。
                Thread.Sleep(TimeSpan.FromSeconds(1.0));
            }
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VoteRoom(VoteParticipant voteRoomOwner, int id,
                        string name, string password)
        {
            // VoteModelのプロパティ値は変更は、VoteRoom側で行います。
            this.voteModel = new VoteModel(this);

            // TimeKeeperの方は手動で更新します。
            // そうしないと遅くなるためです。
            this.voteTimeKeeper = new VoteTimeKeeper(this);
            
            this.voterListManager = new VoterListManager();

            this.voteRoomOwner = voteRoomOwner;
            this.id = id;
            this.name = name;

            // passwordはnull or '\0'のときに、
            // パスワード不要という意味になります。
            this.password = (string.IsNullOrEmpty(password) ? null : password);

            AddParticipant(voteRoomOwner);
            this.voteModel.ChangeMode(VoteMode.Shogi, false);

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
