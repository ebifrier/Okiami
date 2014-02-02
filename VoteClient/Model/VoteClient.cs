using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

using Ragnarok;
using Ragnarok.Net;
using Ragnarok.Net.ProtoBuf;
using Ragnarok.NicoNico.Live;
using Ragnarok.Shogi;
using Ragnarok.ObjectModel;
using Ragnarok.Utility;

namespace VoteSystem.Client.Model
{
    using Protocol;
    using Protocol.Vote;
    using Protocol.Commenter;

    /// <summary>
    /// 各リクエストのリスポンスを受け取ったときに使われます。
    /// </summary>
    public delegate void ResponseHandler<T>(object sender,
                                            PbResponseEventArgs<T> response);

    /// <summary>
    /// ネットワークの投票を行います。
    /// </summary>
    public class VoteClient : NotifyObject, IDisposable
    {
        private readonly ReentrancyLock leaveTimeTimerLock = new ReentrancyLock();
        private Timer leaveTimeTimer;
        private readonly CommenterManager commenterManager;
        private PbConnection conn;
        private bool showErrorMessage = true;        
        private VoteRoomInfo voteRoomInfo;
        private int participantNo = -1;
        private VoteResult voteResult = new VoteResult();
        private DateTime roomInfoLastUpdated = DateTime.MinValue;
        private int oldVoteLeaveSeconds;
        private int oldTotalVoteLeaveSeconds;
        private int oldThinkTimeSeconds;
        private bool disposed = false;

        /// <summary>
        /// 通投票サーバーからの通知受信時に呼ばれます。
        /// </summary>
        public event EventHandler<NotificationEventArgs> NotificationReceived;

        /// <summary>
        /// 投票サーバーからの通知受信時に呼ばれます。
        /// </summary>
        public void OnNotificationReceived(Notification notification)
        {
            var handler = this.NotificationReceived;

            if (handler != null)
            {
                var e = new NotificationEventArgs(notification);

                Util.CallEvent(() => handler(this, e));
            }
        }

        /// <summary>
        /// コメンターの管理用オブジェクトを取得します。
        /// </summary>
        public CommenterManager CommenterManager
        {
            get
            {
                return this.commenterManager;
            }
        }

        /// <summary>
        /// エラーメッセージを表示するかどうかを取得または設定します。
        /// </summary>
        public bool IsShowErrorMessage
        {
            get
            {
                return this.showErrorMessage;
            }
            set
            {
                using (LazyLock())
                {
                    if (this.showErrorMessage != value)
                    {
                        this.showErrorMessage = value;

                        this.RaisePropertyChanged("IsShowErrorMessage");
                    }
                }
            }
        }

        /// <summary>
        /// サーバーに接続しているかどうかを取得します。
        /// </summary>
        public bool IsConnected
        {
            get
            {
                using (LazyLock())
                {
                    if (this.conn == null)
                    {
                        return false;
                    }

                    return this.conn.IsConnected;
                }
            }
        }

        /// <summary>
        /// ログイン中か取得します。
        /// </summary>
        [DependOnProperty("IsConnected")]
        [DependOnProperty("VoteRoomInfo")]
        public bool IsLogined
        {
            get
            {
                using (LazyLock())
                {
                    if (this.conn == null || !this.conn.IsConnected)
                    {
                        return false;
                    }

                    return (this.voteRoomInfo != null);
                }
            }
        }

        /// <summary>
        /// 投票参加者としての番号を取得します。
        /// </summary>
        public int VoteParticipantNo
        {
            get
            {
                return this.participantNo;
            }
            private set
            {
                using (LazyLock())
                {
                    this.participantNo = value;

                    this.RaisePropertyChanged("VoteParticipantNo");
                }
            }
        }

        /// <summary>
        /// 投票ルームのオーナーであるか調べます。
        /// </summary>
        [DependOnProperty("VoteParticipantNo")]
        [DependOnProperty(typeof(VoteRoomInfo), "OwnerNo")]
        public bool IsVoteRoomOwner
        {
            get
            {
                using (LazyLock())
                {
                    if (this.voteRoomInfo == null)
                    {
                        return false;
                    }

                    return (this.voteRoomInfo.OwnerNo == this.participantNo);
                }
            }
        }

        /// <summary>
        /// 入室している投票部屋の情報を取得します。
        /// </summary>
        public VoteRoomInfo VoteRoomInfo
        {
            get
            {
                return this.voteRoomInfo;
            }
            private set
            {
                using (LazyLock())
                {
                    if (!ReferenceEquals(this.voteRoomInfo, value))
                    {
                        this.RemoveDependModel(this.voteRoomInfo);
                        this.voteRoomInfo = value;
                        this.AddDependModel(this.voteRoomInfo);

                        this.RaisePropertyChanged("VoteRoomInfo");
                    }
                }
            }
        }

        /// <summary>
        /// 入室中の投票部屋のIDを取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteRoomInfo), "Id")]
        public int VoteRoomId
        {
            get
            {
                using (LazyLock())
                {
                    if (this.voteRoomInfo == null)
                    {
                        return -1;
                    }

                    return this.voteRoomInfo.Id;
                }
            }
        }

        /// <summary>
        /// 入室中の投票部屋の名前を取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteRoomInfo), "AuthorName")]
        public string VoteRoomName
        {
            get
            {
                using (LazyLock())
                {
                    if (this.voteRoomInfo == null)
                    {
                        return string.Empty;
                    }

                    return this.voteRoomInfo.Name;
                }
            }
        }

        /// <summary>
        /// 投票ルームのオーナーNoを取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteRoomInfo), "OwnerNo")]
        public int VoteRoomOwnerNo
        {
            get
            {
                using (LazyLock())
                {
                    if (this.voteRoomInfo == null)
                    {
                        return -1;
                    }

                    return this.voteRoomInfo.OwnerNo;
                }
            }
        }

        /// <summary>
        /// 投票状態を取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteRoomInfo), "State")]
        public VoteState VoteState
        {
            get
            {
                using (LazyLock())
                {
                    if (this.voteRoomInfo == null)
                    {
                        return VoteState.Stop;
                    }

                    return this.voteRoomInfo.State;
                }
            }
        }

        /// <summary>
        /// 投票モードを取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteRoomInfo), "Mode")]
        public VoteMode VoteMode
        {
            get
            {
                using (LazyLock())
                {
                    if (this.voteRoomInfo == null)
                    {
                        return VoteMode.Mirror;
                    }

                    return this.voteRoomInfo.Mode;
                }
            }
        }

        /// <summary>
        /// 全コメントをミラーするモードか取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteRoomInfo), "IsMirrorMode")]
        public bool IsMirrorMode
        {
            get
            {
                using (LazyLock())
                {
                    if (this.voteRoomInfo == null)
                    {
                        return false;
                    }

                    return this.voteRoomInfo.IsMirrorMode;
                }
            }
        }

        /// <summary>
        /// 投票ルームにログインしているメンバー一覧を取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteRoomInfo), "ParticipantList")]
        public NotifyCollection<VoteParticipantInfo> ParticipantList
        {
            get
            {
                if (VoteRoomInfo == null)
                {
                    return new NotifyCollection<VoteParticipantInfo>();
                }

                return VoteRoomInfo.ParticipantList;
            }
        }

        /// <summary>
        /// 全投票時間の全期間を取得します。
        /// </summary>
        /// <remarks>
        /// 投票中であれば、現在の残り時間ではなく、
        /// 投票開始時の全投票時間を返します。
        /// </remarks>
        [DependOnProperty(typeof(VoteRoomInfo), "TotalVoteSpan")]
        public TimeSpan TotalVoteSpan
        {
            get
            {
                using (LazyLock())
                {
                    if (this.voteRoomInfo == null)
                    {
                        return TimeSpan.Zero;
                    }

                    return this.voteRoomInfo.TotalVoteSpan;
                }
            }
        }

        /// <summary>
        /// 投票期間を取得します。
        /// </summary>
        /// <remarks>
        /// 投票中であれば、現在の残り時間ではなく、
        /// 投票開始時に設定された投票時間を返します。
        /// </remarks>
        [DependOnProperty(typeof(VoteRoomInfo), "VoteSpan")]
        public TimeSpan VoteSpan
        {
            get
            {
                using (LazyLock())
                {
                    if (this.voteRoomInfo == null)
                    {
                        return TimeSpan.Zero;
                    }

                    return this.voteRoomInfo.VoteSpan;
                }
            }
        }

        /// <summary>
        /// 投票開始時刻（NTP）を取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteRoomInfo), "BaseTimeNtp")]
        public DateTime BaseTimeNtp
        {
            get
            {
                using (LazyLock())
                {
                    if (this.voteRoomInfo == null)
                    {
                        return DateTime.MinValue;
                    }

                    return this.voteRoomInfo.BaseTimeNtp;
                }
            }
        }

        /// <summary>
        /// 投票期間が無制限がどうか取得します。
        /// </summary>
        [DependOnProperty("VoteSpan")]
        public bool IsVoteSpanNolimit
        {
            get
            {
                if (VoteRoomInfo == null)
                {
                    return false;
                }

                return (VoteSpan == TimeSpan.MaxValue);
            }
        }

        /// <summary>
        /// 東京結果を取得または設定します。
        /// </summary>
        public VoteResult VoteResult
        {
            get
            {
                return this.voteResult;
                /*return new VoteResult()
                {
                    CandidateList = new []
                    {
                        new VoteCandidatePair() {Candidate = "１二銀", Point = 10},
                        new VoteCandidatePair() {Candidate = "５五角", Point = 1000},
                        new VoteCandidatePair() {Candidate = "２六成香", Point = 100},
                        new VoteCandidatePair() {Candidate = "３四成銀直", Point = 3450},
                        new VoteCandidatePair() {Candidate = "４四桂直不成", Point = 4512},
                    },
                    TimeExtendPoint = 0,
                    TimeStablePoint = 100,
                };*/
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                using (LazyLock())
                {
                    // 候補手のリストはnullにしないようにします。
                    if (value.CandidateList == null)
                    {
                        value.CandidateList = new VoteCandidatePair[0];
                    }

                    this.voteResult = value;

                    this.RaisePropertyChanged("VoteResult");
                }
            }
        }

        /// <summary>
        /// 投票の残り時間を取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteRoomInfo), "State")]
        [DependOnProperty(typeof(VoteRoomInfo), "BaseTimeNtp")]
        [DependOnProperty(typeof(VoteRoomInfo), "VoteSpan")]
        public TimeSpan VoteLeaveTime
        {
            get
            {
                using (LazyLock())
                {
                    // 未入室の場合
                    if (VoteRoomInfo == null)
                    {
                        return TimeSpan.Zero;
                    }

                    return ProtocolUtil.CalcVoteLeaveTime(
                        VoteRoomInfo.State,
                        VoteRoomInfo.BaseTimeNtp,
                        VoteRoomInfo.VoteSpan);
                }
            }
        }

        /// <summary>
        /// 投票の全残り時間を取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteRoomInfo), "State")]
        [DependOnProperty(typeof(VoteRoomInfo), "BaseTimeNtp")]
        [DependOnProperty(typeof(VoteRoomInfo), "TotalVoteSpan")]
        public TimeSpan TotalVoteLeaveTime
        {
            get
            {
                using (LazyLock())
                {
                    // 未入室の場合
                    if (VoteRoomInfo == null)
                    {
                        return TimeSpan.Zero;
                    }

                    return ProtocolUtil.CalcTotalVoteLeaveTime(
                        VoteRoomInfo.State,
                        VoteRoomInfo.BaseTimeNtp,
                        VoteRoomInfo.TotalVoteSpan);
                }
            }
        }

        /// <summary>
        /// 思考時間を取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteRoomInfo), "State")]
        [DependOnProperty(typeof(VoteRoomInfo), "BaseTimeNtp")]
        [DependOnProperty(typeof(VoteRoomInfo), "VoteSpan")]
        [DependOnProperty(typeof(VoteRoomInfo), "ProgressSpan")]
        public TimeSpan ThinkTime
        {
            get
            {
                using (LazyLock())
                {
                    // 未入室の場合
                    if (VoteRoomInfo == null)
                    {
                        return TimeSpan.Zero;
                    }

                    return ProtocolUtil.CalcThinkTime(
                        VoteRoomInfo.State,
                        VoteRoomInfo.BaseTimeNtp,
                        VoteRoomInfo.ProgressSpan,
                        VoteRoomInfo.VoteSpan);
                }
            }
        }

        #region リクエスト送信
        /// <summary>
        /// 投票ルームに入室中かどうかを調べます。
        /// </summary>
        private void CheckEnteringVoteRoom(bool? mustEnterRoom)
        {
            using (LazyLock())
            {
                if (!IsConnected)
                {
                    throw new InvalidOperationException(
                        "サーバーに接続していません。");
                }

                if (mustEnterRoom != null)
                {
                    if (mustEnterRoom.Value)
                    {
                        // 入室していないとだめ。
                        if (!IsLogined)
                        {
                            throw new InvalidOperationException(
                                "投票ルームに入室していません。");
                        }
                    }
                    else
                    {
                        // 入室していたらだめ。
                        if (IsLogined)
                        {
                            throw new InvalidOperationException(
                                "すでに投票ルームに入室しています。");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// リクエストを送信します。
        /// </summary>
        public void SendRequest<TReq, TRes>(TReq request,
                                            PbResponseHandler<TRes> handler)
            where TReq: class
        {
            if (request == null)
            {
                return;
            }

            using (LazyLock())
            {
                if (!IsConnected)
                {
                    return;
                }

                this.conn.SendRequest(request, handler);
            }
        }

        /// <summary>
        /// コマンドを送信します。
        /// </summary>
        public void SendCommand<TCmd>(TCmd command)
            where TCmd: class
        {
            if (command == null)
            {
                return;
            }

            using (LazyLock())
            {
                if (!IsConnected)
                {
                    return;
                }

                this.conn.SendCommand(command);
            }
        }

        #region いつでも送れるリクエスト
        /// <summary>
        /// 投票ルーム数の取得要求を出します。
        /// </summary>
        public void GetVoteRoomCount(
            ResponseHandler<GetVoteRoomCountResponse> callback)
        {
            using (LazyLock())
            {
                CheckEnteringVoteRoom(null);

                var request = new GetVoteRoomCountRequest();

                this.conn.SendRequest(
                    request,
                    (object sender,
                     PbResponseEventArgs<GetVoteRoomCountResponse> e) =>
                    {
                        if (callback != null)
                        {
                            callback(this, e);
                        }
                    });
            }
        }

        /// <summary>
        /// 投票ルームの情報取得要求を出します。
        /// </summary>
        public void GetVoteRoomList(
            int fromIndex, int toIndex,
            ResponseHandler<GetVoteRoomListResponse> callback)
        {
            using (LazyLock())
            {
                CheckEnteringVoteRoom(null);

                var request = new GetVoteRoomListRequest()
                {
                    FromIndex = fromIndex,
                    ToIndex = toIndex,
                };

                this.conn.SendRequest(
                    request,
                    (object sender,
                     PbResponseEventArgs<GetVoteRoomListResponse> e) =>
                    {
                        if (callback != null)
                        {
                            callback(this, e);
                        }
                    },
                    false);
            }
        }

        /// <summary>
        /// 投票参加者の属性情報の更新要求を送ります。
        /// </summary>
        public void SetParticipantAttribute(
            bool? isUseAsNicoCommenter,
            NicoLoginType? nicoLoginType,
            string message,
            ResponseHandler<SetParticipantAttributeResponse> callback)
        {
            using (LazyLock())
            {
                CheckEnteringVoteRoom(null);

                // 属性更新要求を送ります。
                var request = new SetParticipantAttributeRequest()
                {
                    IsUseAsNicoCommenter =
                        ProtocolUtil.ToBoolObject(isUseAsNicoCommenter),
                    IsSetLoginType = (nicoLoginType != null),
                    LoginType = nicoLoginType ?? NicoLoginType.NotLogined,
                    HasMessage = (message != null),
                    Message = message,
                };

                this.conn.SendRequest(
                    request,
                    TimeSpan.FromSeconds(20),
                    (object sender,
                     PbResponseEventArgs<SetParticipantAttributeResponse> e) =>
                    {
                        if (callback != null)
                        {
                            callback(this, e);
                        }
                    });
            }
        }

        /// <summary>
        /// 放送ルームの各種操作を行います。
        /// </summary>
        public void OperateLive(
            LiveOperation operation,
            LiveData liveData,
            LiveAttribute attribute,
            ResponseHandler<LiveOperationResponse> callback)
        {
            using (LazyLock())
            {
                CheckEnteringVoteRoom(null);

                var request = new LiveOperationRequest()
                {
                    Operation = operation,
                    LiveData = liveData,
                    Attribute = attribute,
                };

                this.conn.SendRequest(
                    request,
                    (object sender,
                     PbResponseEventArgs<LiveOperationResponse> e) =>
                    {
                        if (callback != null)
                        {
                            callback(this, e);
                        }
                    });
            }
        }
        #endregion

        #region 投票ルーム未入室時のみ送れるリクエスト
        /// <summary>
        /// 投票ルームの作成要求を出します。
        /// </summary>
        public void CreateVoteRoom(
            string roomName, string password,
            Guid id, string nickname, string imageUrl,
            ResponseHandler<CreateVoteRoomResponse> callback)
        {
            using (LazyLock())
            {
                CheckEnteringVoteRoom(false);

                var request = new CreateVoteRoomRequest()
                {
                    RoomName = roomName,
                    Password = password,
                    OwnerId = id.ToString(),
                    OwnerName = nickname,
                    ImageUrl = imageUrl,
                };

                this.conn.SendRequest(
                    request,
                    (object sender,
                     PbResponseEventArgs<CreateVoteRoomResponse> e) =>
                    {
                        // 部屋に接続したので
                        if (e.ErrorCode == ErrorCode.None)
                        {
                            VoteRoomInfo = e.Response.RoomInfo;
                            VoteParticipantNo = e.Response.ParticipantNo;

                            // ルーム作成時に時間に関する設定を行います。
                            SetTimeExtendSetting(
                                Global.Settings.VoteEndCount,
                                Global.Settings.VoteExtendTime);
                        }

                        if (callback != null)
                        {
                            callback(this, e);
                        }
                    });
            }
        }

        /// <summary>
        /// 投票ルームへの接続要求を出します。
        /// </summary>
        public void EnterVoteRoom(
            int roomId, string password,
            Guid id, string nickname, string imageUrl,
            ResponseHandler<EnterVoteRoomResponse> callback)
        {
            using (LazyLock())
            {
                CheckEnteringVoteRoom(false);

                var request = new EnterVoteRoomRequest()
                {
                    // 参加者の情報を設定します。
                    RoomId = roomId,
                    Password = password,
                    ParticipantId = id.ToString(),
                    ParticipantName = nickname,
                    ImageUrl = imageUrl,
                };

                this.conn.SendRequest(
                    request,
                    (object sender,
                     PbResponseEventArgs<EnterVoteRoomResponse> e) =>
                    {
                        // 部屋に接続したので
                        if (e.ErrorCode == ErrorCode.None)
                        {
                            VoteRoomInfo = e.Response.RoomInfo;
                            VoteParticipantNo = e.Response.ParticipantNo;
                        }

                        if (callback != null)
                        {
                            callback(this, e);
                        }
                    });
            }
        }
        #endregion

        #region 投票ルーム入室時のみ送れるリクエスト
        /// <summary>
        /// 投票者リストを更新します。
        /// </summary>
        public VoterList GetVoterList()
        {
            CheckEnteringVoteRoom(true);

            using (var ev = new ManualResetEvent(false))
            {
                VoterList voterList = null;

                GetVoterList(
                    (sender, response) =>
                    {
                        voterList = GetVoterListDone(response);
                        ev.Set();
                    });
                if (!ev.WaitOne(TimeSpan.FromSeconds(10.0)))
                {
                    throw new TimeoutException(
                        "参加者リストの取得がタイムアウトしました。(~∇~;)");
                }

                // 理由不明だが･･････
                if (voterList == null)
                {
                    throw new VoteClientException(
                        "参加者リストがありませんでした。(~∇~;)");
                }

                return voterList;
            }
        }

        /// <summary>
        /// 投票者リストの設定を行います。
        /// </summary>
        private static VoterList GetVoterListDone(
            PbResponseEventArgs<GetVoterListResponse> response)
        {
            if (response.ErrorCode != 0)
            {
                return null;
            }

            if (response.Response == null)
            {
                return null;
            }

            return response.Response.VoterList;
        }

        /// <summary>
        /// 投票者リストを取得します。
        /// </summary>
        public void GetVoterList(
            ResponseHandler<GetVoterListResponse> callback)
        {
            using (LazyLock())
            {
                CheckEnteringVoteRoom(true);

                var request = new GetVoterListRequest();

                this.conn.SendRequest(
                    request,
                    (object sender,
                     PbResponseEventArgs<GetVoterListResponse> e) =>
                    {
                        if (callback != null)
                        {
                            callback(this, e);
                        }
                    });
            }
        }

        /// <summary>
        /// 投票部屋からの退出リクエストを送信します。
        /// </summary>
        public void LeaveVoteRoom(
            ResponseHandler<LeaveVoteRoomResponse> callback)
        {
            using (LazyLock())
            {
                CheckEnteringVoteRoom(true);

                var request = new LeaveVoteRoomRequest();

                this.conn.SendRequest(
                    request,
                    (object sender,
                     PbResponseEventArgs<LeaveVoteRoomResponse> e) =>
                    {
                        // 部屋から退出したので
                        if (e.ErrorCode == ErrorCode.None)
                        {
                            VoteRoomInfo = null;
                            Global.InvalidateCommand();
                        }

                        if (callback != null)
                        {
                            callback(this, e);
                        }

                        // ルーム退出時にコネクションを切断しておきます。
                        var conn = this.conn;
                        if (conn != null)
                        {
                            conn.Shutdown();
                        }
                    });
            }
        }
        #endregion
        #endregion

        #region コマンド送信
        /// <summary>
        /// 入室中投票ルームの情報取得要求を出します。
        /// </summary>
        /// <remarks>
        /// 投票ルーム情報は差分で更新されますが、
        /// それに失敗した場合に呼ばれます。
        /// </remarks>
        private void GetVoteRoomInfoFromServer()
        {
            using (LazyLock())
            {
                CheckEnteringVoteRoom(null);

                // ルーム情報の連続取得を避けるため、
                // "結果が返ってきていない and 一定時間立っていない"
                // 間は取得コマンドの送信を行わないようにします。
                // 
                // roomInfoLastUpdatedは受信待ちの時は要求送信時刻、
                // そうでない場合はMinValueに設定されます。
                // このため、roomInfoLastUpdatedが時刻で
                // かつ一定時間立っていないときにはそのまま帰ります。
                var baseTime = DateTime.Now - TimeSpan.FromSeconds(30);
                if (baseTime < this.roomInfoLastUpdated)
                {
                    // roomInfoLastUpdatedがMinValueの時は常にfalse
                    return;
                }

                this.conn.SendCommand(new GetVoteRoomInfoCommand());
                this.roomInfoLastUpdated = DateTime.Now;
            }
        }

        /// <summary>
        /// 投票開始コマンドを送信します。
        /// </summary>
        public void StartVote(TimeSpan voteSpan)
        {
            using (LazyLock())
            {
                CheckEnteringVoteRoom(true);

                this.conn.SendCommand(new StartVoteCommand()
                {
                    Seconds = voteSpan.TotalSeconds,
                });
            }
        }

        /// <summary>
        /// 投票一時停止コマンドを送信します。
        /// </summary>
        public void PauseVote()
        {
            using (LazyLock())
            {
                CheckEnteringVoteRoom(true);

                this.conn.SendCommand(new PauseVoteCommand());
            }
        }

        /// <summary>
        /// 投票停止コマンドを送信します。
        /// </summary>
        public void StopVote(TimeSpan addTotalTime)
        {
            using (LazyLock())
            {
                CheckEnteringVoteRoom(true);

                this.conn.SendCommand(new StopVoteCommand()
                {
                    AddTotalTimeSeconds = addTotalTime.TotalSeconds
                });
            }
        }

        /// <summary>
        /// 投票停止コマンドを送信します。
        /// </summary>
        public void StopVote()
        {
            StopVote(TimeSpan.Zero);
        }

        /// <summary>
        /// 時間設定コマンドを送信します。
        /// </summary>
        public void SetVoteSpan(TimeSpan span)
        {
            using (LazyLock())
            {
                CheckEnteringVoteRoom(true);

                this.conn.SendCommand(new SetVoteSpanCommand()
                {
                    Seconds = span.TotalSeconds,
                });
            }
        }

        /// <summary>
        /// 時間変更コマンドを送信します。
        /// </summary>
        public void AddVoteSpan(TimeSpan diff)
        {
            using (LazyLock())
            {
                CheckEnteringVoteRoom(true);

                this.conn.SendCommand(new AddVoteSpanCommand()
                {
                    DiffSeconds = diff.TotalSeconds,
                });
            }
        }

        /// <summary>
        /// 全投票時間設定コマンドを送信します。
        /// </summary>
        public void SetTotalVoteSpan(TimeSpan span)
        {
            using (LazyLock())
            {
                CheckEnteringVoteRoom(true);

                this.conn.SendCommand(new SetTotalVoteSpanCommand()
                {
                    Seconds = span.TotalSeconds,
                });
            }
        }

        /// <summary>
        /// 全投票時間変更コマンドを送信します。
        /// </summary>
        public void AddTotalVoteSpan(TimeSpan diff)
        {
            using (LazyLock())
            {
                CheckEnteringVoteRoom(true);

                this.conn.SendCommand(new AddTotalVoteSpanCommand()
                {
                    DiffSeconds = diff.TotalSeconds,
                });
            }
        }

        /// <summary>
        /// 時間延長・短縮に関する設定を行います。
        /// </summary>
        public void SetTimeExtendSetting(int? voteEndCount,
                                         TimeSpan voteExtendTimeSpan)
        {
            using (LazyLock())
            {
                if (!IsVoteRoomOwner)
                {
                    return;
                }

                CheckEnteringVoteRoom(true);                

                this.conn.SendCommand(new SetTimeExtendSettingCommand()
                {
                    VoteEndCount = (voteEndCount ?? -1),
                    VoteExtendTimeSeconds =
                        (voteExtendTimeSpan != TimeSpan.MinValue &&
                         voteExtendTimeSpan != TimeSpan.MaxValue ?
                         (int)voteExtendTimeSpan.TotalSeconds : -1),
                });
            }
        }

        /// <summary>
        /// 投票モード変更コマンドを送信します。
        /// </summary>
        public void ChangeVoteMode(VoteMode mode, bool isMirrorMode)
        {
            using (LazyLock())
            {
                CheckEnteringVoteRoom(true);

                this.conn.SendCommand(new ChangeVoteModeCommand()
                {
                    VoteMode = mode,
                    IsMirrorMode = isMirrorMode,
                });
            }
        }

        /// <summary>
        /// 投票結果クリアコマンドを送信します。
        /// </summary>
        public void ClearVote()
        {
            using (LazyLock())
            {
                CheckEnteringVoteRoom(true);

                this.conn.SendCommand(new ClearVoteCommand());
            }
        }

        /// <summary>
        /// 延長要求結果クリアコマンドを送信します。
        /// </summary>
        public void ClearTimeExtendDemand()
        {
            using (LazyLock())
            {
                CheckEnteringVoteRoom(true);

                var notification = new Notification()
                {
                    Text = "/cleartimeextenddemand",
                    FromLiveRoom = null,
                    Timestamp = Ragnarok.Net.NtpClient.GetTime(),
                    VoterId = "$owner",
                };

                SendNotification(notification, true);
            }
        }

        /// <summary>
        /// 評価値クリアコマンドを送信します。
        /// </summary>
        public void ClearEvaluationPoint()
        {
            using (LazyLock())
            {
                CheckEnteringVoteRoom(true);

                var notification = new Notification()
                {
                    Text = "/clearevaluationpoints",
                    FromLiveRoom = null,
                    Timestamp = Ragnarok.Net.NtpClient.GetTime(),
                    VoterId = "$owner",
                };

                SendNotification(notification, true);
            }
        }

        /// <summary>
        /// プレイヤーからの操作コマンド(コメント)を送信します。
        /// </summary>
        public void SendNotification(Notification notification,
                                     bool isFromLiveOwner)
        {
            if (notification == null || !notification.Validate())
            {
                return;
            }

            using (LazyLock())
            {
                CheckEnteringVoteRoom(true);

                // テスト用
                //notification.VoterId = Guid.NewGuid().ToString();

                this.conn.SendCommand(new NotificationCommand()
                {
                    Notification = notification,
                    IsFromLiveOwner = isFromLiveOwner,
                });
            }
        }

        /// <summary>
        /// エンドロールの開始コマンドを送信します。
        /// </summary>
        public void SendStartEndRoll(DateTime startTimeNtp)
        {
            using (LazyLock())
            {
                CheckEnteringVoteRoom(true);

                this.conn.SendCommand(new StartEndRollCommand()
                {
                    StartTimeNtpTicks = startTimeNtp.Ticks,
                });
            }
        }

        /// <summary>
        /// エンドロールの停止コマンドを送信します。
        /// </summary>
        public void SendStopEndRoll()
        {
            using (LazyLock())
            {
                CheckEnteringVoteRoom(true);

                this.conn.SendCommand(new StopEndRollCommand());
            }
        }
        #endregion

        #region コマンド受信
        /// <summary>
        /// ParticipantListはサイズが大きいため、設定されていないことがあります。
        /// </summary>
        private static bool IgnoreProperty(IPropertyObject property,
                                           object value)
        {
            if (property.Name == "ParticipantList")
            {
                var list = value as NotifyCollection<VoteParticipantInfo>;
                if (list == null || !list.Any())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 入室中のルーム情報をすべて更新します。
        /// </summary>
        private void HandleVoteRoomInfoCommand(
            object sender, PbCommandEventArgs<SendVoteRoomInfoCommand> e)
        {
            using (LazyLock())
            {
                // ルーム情報の更新時間はエラーに関係なく設定します。
                // MinValue => ルーム情報受信待ちではない
                this.roomInfoLastUpdated = DateTime.MinValue;

                var roomInfo = e.Command.RoomInfo;
                if (roomInfo == null || !roomInfo.Validate())
                {
                    return;
                }
            
                if (VoteRoomInfo == null)
                {
                    VoteRoomInfo = roomInfo;
                }
                else
                {
                    // VoteRoomInfoオブジェクトを直接変更すると
                    // すべてのプロパティが変更されたことになってしまい、
                    // サーバーへの通知を含めた無駄な処理がいくつも行われます。
                    // それを避けるため、ここではプロパティごとに値の設定を行っています。
                    var updatePropList =
                        from pair in MethodUtil.GetPropertyDic(typeof(VoteRoomInfo))
                        let property = pair.Value
                        where property.CanRead && property.CanWrite
                        let value = property.GetValue(roomInfo)
                        // ParticipantListはサイズが大きいため、無視することがあります。
                        where !IgnoreProperty(property, value)
                        select new { property, value };

                    updatePropList.ForEach(
                        _ => _.property.SetValue(this.voteRoomInfo, _.value));
                }
            }

            Global.InvalidateCommand();
            if (VoteRoomInfo == null)
            {
                VoteRoomInfo.RaisePropertyChanged("ParticipantList");
            }
        }

        /// <summary>
        /// 参加者情報の差分を受け取ります。
        /// </summary>
        private void HandleChangeParticipantInfoCommand(
            object sender, PbCommandEventArgs<ChangeParticipantInfoCommand> e)
        {
            var op = e.Command.Operation;
            var info = e.Command.Info;
            if (info == null || !info.Validate())
            {
                Log.Error("ChangeParticipantInfoCommand.Infoが正しくありません。");
                return;
            }

            Global.UIProcess(() =>
                ParticipantListChanged(op, info, e.Command.ListCount));
        }

        /// <summary>
        /// 参加者リストの一部を更新します。
        /// </summary>
        private void ParticipantListChanged(CollectionOperation op,
                                            VoteParticipantInfo info,
                                            int listCount)
        {
            using (LazyLock())
            {
                var voteRoomInfo = this.voteRoomInfo;
                if (voteRoomInfo == null)
                {
                    // ルーム未入室の場合
                    return;
                }

                var participantList = voteRoomInfo.ParticipantList;
                if (participantList == null)
                {
                    return;
                }

                bool error = false;
                int index;
                switch (op)
                {
                    case CollectionOperation.CollectionAdd:
                        participantList.Add(info);
                        break;
                    case CollectionOperation.CollectionRemove:
                        if (!participantList.RemoveIf(_ => _.No == info.No))
                        {
                            Log.Error(
                                "No.{0}の参加者情報の削除に失敗しました。",
                                info.No);
                            error = true;
                        }
                        break;
                    case CollectionOperation.CollectionReplace:
                        index = participantList.FindIndex(_ => _.No == info.No);
                        if (index < 0)
                        {
                            Log.Error(
                                "No.{0}の参加者情報の置換に失敗しました。",
                                info.No);
                            error = true;
                        }
                        else
                        {
                            // 参加者情報を置き換えます。
                            participantList[index] = info;
                        }
                        break;
                }

                // 参加者リストの更新が上手くいかなかった場合は、
                // リストをすべて更新します。
                if (error || participantList.Count() != listCount)
                {
                    GetVoteRoomInfoFromServer();
                }
                else
                {
                    this.voteRoomInfo.RaisePropertyChanged("ParticipantList");
                }
            }
        }

        /// <summary>
        /// 投票結果を受信します。
        /// </summary>
        private void HandleVoteResultCommand(
            object sender, PbCommandEventArgs<SendVoteResultCommand> e)
        {
            var voteResult = e.Command.Result;

            // 受信したメッセージの妥当性を確認します。
            if (voteResult == null || !voteResult.Validate())
            {
                return;
            }

            VoteResult = voteResult;
        }

        /// <summary>
        /// メッセージコマンドを処理します。
        /// </summary>
        private void HandleNotificationCommand(
            object sender, PbCommandEventArgs<NotificationCommand> e)
        {
            var notification = e.Command.Notification;

            // 受信したメッセージの妥当性を確認します。
            if (notification == null || !notification.Validate())
            {
                return;
            }

            OnNotificationReceived(notification);
        }
        #endregion

        /// <summary>
        /// 投票時間の更新を行います。
        /// </summary>
        private void LeaveTimeTimer_Callback(object state)
        {
            // Timerオブジェクトはコールバックを同時に呼ぶことがあるため、
            // その対策を行っています。
            using (var result = this.leaveTimeTimerLock.Lock())
            {
                if (result == null) return;

                var seconds = (int)VoteLeaveTime.TotalSeconds;
                if (seconds != this.oldVoteLeaveSeconds)
                {
                    this.oldVoteLeaveSeconds = seconds;
                    this.RaisePropertyChanged("VoteLeaveTime");
                }

                seconds = (int)TotalVoteLeaveTime.TotalSeconds;
                if (seconds != this.oldTotalVoteLeaveSeconds)
                {
                    this.oldTotalVoteLeaveSeconds = seconds;
                    this.RaisePropertyChanged("TotalVoteLeaveTime");
                }

                seconds = (int)ThinkTime.TotalSeconds;
                if (seconds != this.oldThinkTimeSeconds)
                {
                    this.oldThinkTimeSeconds = seconds;
                    this.RaisePropertyChanged("ThinkTime");
                }
            }
        }

        /// <summary>
        /// 投票時間の更新を開始します。
        /// </summary>
        public void StartLeaveTimeTimer()
        {
            using (LazyLock())
            {
                // 投票残り時間を更新するために使います。
                this.leaveTimeTimer = new Timer(
                    LeaveTimeTimer_Callback,
                    null,
                    TimeSpan.FromMilliseconds(1000),
                    TimeSpan.FromMilliseconds(200));
            }
        }

        /// <summary>
        /// 投票サーバーに接続します。
        /// </summary>
        public void Connect(string address, int port)
        {
            using (LazyLock())
            {
                if (IsLogined)
                {
                    throw new InvalidOperationException(
                        "投票ルーム入室中です。");
                }

                var conn = CreateConnection();

                // サーバーに接続し、プロトコルバージョンなどを確認します。
                conn.Connect(address, port);

                var result = conn.CheckProtocolVersion(
                    TimeSpan.FromSeconds(30));
                if (result != PbVersionCheckResult.Ok)
                {
                    conn.Shutdown();

                    throw new VersionUnmatchedException(
                        result,
                        string.Format(
                            "投票サーバーと通信プロトコルのバージョンが合いません。{0}" +
                            "アプリを最新版にしてみてください (T◇To){0}{0}" +
                            "理由：{1}",
                            Environment.NewLine,
                            EnumEx.GetDescription(result)));
                }

                // 既存の接続を切断し、新たな接続を設定します。
                Disconnect();

                // 切断イベントはここで設定します。
                // でないとバージョンミスマッチの時も呼ばれてしまいます。
                this.conn = conn;
                this.conn.Disconnected += connection_Disconnected;

                VoteRoomInfo = null;
                VoteParticipantNo = -1;
            }

            this.RaisePropertyChanged("IsConnected");
        }

        /// <summary>
        /// 接続を切断します。
        /// </summary>
        public void Disconnect()
        {
            using (LazyLock())
            {
                if (this.conn != null)
                {
                    this.conn.Disconnect();
                    this.conn = null;
                }
            }
        }

        /// <summary>
        /// 新規のコネクションを作成します。
        /// </summary>
        private PbConnection CreateConnection()
        {
            var conn = new PbConnection()
            {
                Name = "投票コネクション",
                ProtocolVersion = ServerSettings.ProtocolVersion,
            };

            conn.AddCommandHandler<SendVoteRoomInfoCommand>(HandleVoteRoomInfoCommand);
            conn.AddCommandHandler<ChangeParticipantInfoCommand>(HandleChangeParticipantInfoCommand);
            conn.AddCommandHandler<SendVoteResultCommand>(HandleVoteResultCommand);
            conn.AddCommandHandler<NotificationCommand>(HandleNotificationCommand);

            if (this.commenterManager != null)
            {
                this.commenterManager.Attach(conn);
            }

            Plugin_ConnectHandlers(conn);
            return conn;
        }

        /// <summary>
        /// コネクションの切断時に呼ばれます。
        /// </summary>
        private void connection_Disconnected(object sender,
                                             DisconnectEventArgs e)
        {
            VoteRoomInfo = null;
            VoteParticipantNo = -1;

            if (e.Reason == DisconnectReason.DisconnectedByOpposite)
            {
                if (this.showErrorMessage)
                {
                    MessageUtil.ErrorMessage(
                        "投票サーバーとの接続が切れました (T◇To)");
                }
            }
            else if (e.Reason == DisconnectReason.Error)
            {
                if (this.showErrorMessage)
                {
                    MessageUtil.ErrorMessage(
                        "投票サーバーとの接続にエラーが発生しました (T◇To)");
                }
            }

            // このメソッドはUIとは違うスレッドから呼ばれることがあるため、
            // コマンドの有効/無効をここで強制的に更新します。
            Global.InvalidateCommand();

            this.RaisePropertyChanged("IsConnected");
        }

        /// <summary>
        /// 各プラグインにprotobufのカスタム処理ハンドラを追加します。
        /// </summary>
        private void Plugin_ConnectHandlers(PbConnection conn)
        {
            if (Global.PluginList == null || conn == null)
            {
                return;
            }

            foreach (var plugin in Global.PluginList)
            {
                Util.SafeCall(() =>
                    plugin.ConnectHandlers(conn));
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VoteClient(CommenterManager commenterManager)
        {
            Global.PluginLoaded +=
                (sender, e) => Plugin_ConnectHandlers(this.conn);
            
            this.commenterManager = commenterManager;
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~VoteClient()
        {
            Dispose(false);
        }

        /// <summary>
        /// オブジェクトを破棄します。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// オブジェクトを破棄します。
        /// </summary>
        protected void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    Disconnect();
                }

                this.disposed = true;
            }
        }
    }
}
