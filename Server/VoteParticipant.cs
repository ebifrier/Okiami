using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.ComponentModel;

using Ragnarok;
using Ragnarok.Net;
using Ragnarok.Net.ProtoBuf;
using Ragnarok.ObjectModel;

namespace VoteSystem.Server
{
    using VoteSystem.Protocol;
    using VoteSystem.Protocol.Vote;
    using VoteSystem.Protocol.Commenter;

    /// <summary>
    /// クライアント側との通信を担当します。
    /// </summary>
    /// <remarks>
    /// 投票や参加希望などを処理します。
    /// 
    /// 放送主が放送を始めた場合、それ用のLiveRoomオブジェクトが作成され
    /// それはこのクラスで管理されます。
    /// </remarks>
    public sealed class VoteParticipant : NotifyObject, ILogObject
    {
        private PbConnection connection;
        private VoteRoom voteRoom;
        private readonly LiveRoomManager liveRoomManager;

        /// <summary>
        /// 切断時に呼ばれるイベントです。
        /// </summary>
        public event EventHandler<DisconnectEventArgs> Disconnected;

        /// <summary>
        /// ログ出力用の名前を取得します。
        /// </summary>
        [DependOnProperty("Name")]
        public string LogName
        {
            get { return string.Format("参加者[{0}]", Name); }
        }

        /// <summary>
        /// ハンドラの追加や投票オーナーの判定に使います。
        /// </summary>
        internal PbConnection Connection
        {
            get { return this.connection; }
        }

        /// <summary>
        /// 参加者のIDを取得します。これは参加者側が設定します。
        /// </summary>
        public Guid Id
        {
            get { return GetValue<Guid>("Id"); }
            private set { SetValue("Id", value); }
        }

        /// <summary>
        /// 投票ルーム内の番号を取得します。
        /// </summary>
        public int No
        {
            get { return GetValue<int>("No"); }
            private set { SetValue("No", value); }
        }

        /// <summary>
        /// 設定名を取得します。
        /// </summary>
        public string Name
        {
            get { return GetValue<string>("Name"); }
            private set { SetValue("Name", value); }
        }

        /// <summary>
        /// サムネイル画像URLを取得します。
        /// </summary>
        public string ImageUrl
        {
            get { return GetValue<string>("ImageUrl"); }
            private set { SetValue("ImageUrl", value); }
        }

        /// <summary>
        /// 一言メッセージを取得または設定します。
        /// </summary>
        public string Message
        {
            get { return GetValue<string>("Message"); }
            set { SetValue("Message", value); }
        }

        /// <summary>
        /// このオブジェクトをニコ生のコメンターとして使うかどうかを取得します。
        /// </summary>
        public bool IsUseAsNicoCommenter
        {
            get { return GetValue<bool>("IsUseAsNicoCommenter"); }
            private set { SetValue("IsUseAsNicoCommenter", value); }
        }

        /// <summary>
        /// ニコ生へのログイン状況を取得します。
        /// </summary>
        public NicoLoginType NicoLoginType
        {
            get { return GetValue<NicoLoginType>("NicoLoginType"); }
            private set { SetValue("NicoLoginType", value); }
        }

        /// <summary>
        /// 参加者情報を取得します。
        /// </summary>
        [DependOnProperty("Id")]
        [DependOnProperty("No")]
        [DependOnProperty("Name")]
        [DependOnProperty("ImageUrl")]
        [DependOnProperty("Message")]
        [DependOnProperty("IsUseAsNicoCommenter")]
        [DependOnProperty("NicoLoginType")]
        [DependOnProperty(typeof(LiveRoomManager), "LiveDataList")]
        public VoteParticipantInfo Info
        {
            get
            {
                return new VoteParticipantInfo()
                {
                    Id = Id.ToString(),
                    No = No,
                    Name = Name,
                    ImageUrl = ImageUrl,
                    Message = Message,
                    IsUseAsNicoCommenter = IsUseAsNicoCommenter,
                    NicoLoginType = NicoLoginType,
                    LiveDataList = this.liveRoomManager.LiveDataList,
                };
            }
        }

        /// <summary>
        /// 入室している投票ルームを取得します。
        /// </summary>
        public VoteRoom VoteRoom
        {
            get { return this.voteRoom; }
            private set { SetValue("VoteRoom", value, ref this.voteRoom); }
        }

        /// <summary>
        /// 投票ルームに入室中かどうかを取得します。
        /// </summary>
        [DependOnProperty("VoteRoom")]
        public bool IsEnteringVoteRoom
        {
            get
            {
                return (VoteRoom != null);
            }
        }

        /// <summary>
        /// 投票ルームを設定します。
        /// </summary>
        /// <remarks>
        /// VoteRoomのParticipantListと整合性を取るため、
        /// VoteRoomのみから呼ばれます。
        /// </remarks>
        internal void SetVoteRoom(VoteRoom voteRoom, int no)
        {
            using (LazyLock())
            {
                if (this.VoteRoom != null)
                {
                    this.VoteRoom.DisconnectHandlers(this.connection);
                }

                this.VoteRoom = voteRoom;
                this.No = no;

                // 処理ハンドラを設定します。
                if (this.VoteRoom != null)
                {
                    this.VoteRoom.ConnectHandlers(this.connection);
                }

                // 投票ルームが変わったときは、投票ルーム内の参加者から
                // 選ばれているコメンターをすべて削除します。
                this.liveRoomManager.OnVoteRoomChanged(voteRoom);

                // 放送主として参加者リストに追加します。
                AddLiveOwnerVoter();
            }
        }

        /// <summary>
        /// 与えられた放送IDを持つ放送を返します。
        /// </summary>
        public LiveRoom GetLiveRoom(LiveData liveData)
        {
            return this.liveRoomManager.GetLiveRoom(liveData);
        }

        /// <summary>
        /// 放送主として投票者を追加します。
        /// </summary>
        internal void AddLiveOwnerVoter()
        {
            using (LazyLock())
            {
                // "投票ルームに入室している and 放送が設定されている"
                // 状態でないと放送主として登録できません。
                if (!IsEnteringVoteRoom || !this.liveRoomManager.HasLiveRoom)
                {
                    return;
                }

                VoteRoom.VoterListManager.AddLiveOwnerVoter(
                    new VoterInfo()
                    {
                        LiveSite = LiveSite.Unknown,
                        Id = Id.ToString(),
                        Name = Name,
                    });
            }
        }

        /// <summary>
        /// 投票ルームへの入室/未入室確認を行います。
        /// </summary>
        private int CheckEnteringVoteRoom(bool value)
        {
            if (IsEnteringVoteRoom != value)
            {
                if (value)
                {
                    Log.Error(this,
                        "まだ投票ルームに入室していません。");

                    return ErrorCode.NotEnteringVoteRoom;
                }
                else
                {
                    Log.Error(this,
                        "すでに投票ルームに入室しています。");

                    return ErrorCode.AlreadyEnteredVoteRoom;
                }
            }

            return ErrorCode.None;
        }

        #region コマンド送信
        /// <summary>
        /// 投票結果を送信します。
        /// </summary>
        public void SendCommand<T>(T command)
            where T: class
        {
            if (command == null)
            {
                return;
            }

            if (this.connection == null)
            {
                return;
            }

            this.connection.SendCommand(command);
        }

        /// <summary>
        /// 投票結果を送信します。
        /// </summary>
        public void SendVoteResultCommand(VoteResult voteResult)
        {
            var command = new SendVoteResultCommand()
            {
                Result = voteResult,
            };

            this.connection.SendCommand(command);
        }

        /// <summary>
        /// 通知をこの参加者や放送ルームに対して送信します。
        /// </summary>
        public void SendNotificationCommand(Notification notification,
                                            bool sendAsNotification,
                                            bool sendToLiveRoom)
        {
            // 放送ルームにもコメントを投稿するなら、
            // それ用の通知を送ります。
            if (sendToLiveRoom)
            {
                this.liveRoomManager.BroadcastNotificationForPost(
                    notification);
            }

            if (sendAsNotification)
            {
                var command = new NotificationCommand()
                {
                    Notification = notification,
                };

                this.connection.SendCommand(command);
            }
        }

        /// <summary>
        /// オブジェクトのプロパティ変更通知を送信します。
        /// </summary>
        public void SendObjectChangedCommand(string objectName,
                                             string propertyName,
                                             Type propertyType,
                                             object propertyValue)
        {
            var command = new PbPropertyChanged()
            {
                ObjectId = objectName,
                PropertyName = propertyName,
                PropertyType = propertyType,
                PropertyValue = propertyValue,
            };

            command.SerializePropertyValue();

            this.connection.SendCommand(command);
        }
        #endregion

        #region リクエスト処理
        /// <summary>
        /// ルーム数取得リクエストを処理します。
        /// </summary>
        private void HandleGetVoteRoomCountRequest(
            object sender,
            PbRequestEventArgs<GetVoteRoomCountRequest,
                               GetVoteRoomCountResponse> e)
        {
            e.Response = new GetVoteRoomCountResponse()
            {
                Count = GlobalControl.Instance.VoteRoomCount,
            };
        }

        /// <summary>
        /// ルーム情報取得リクエストを処理します。
        /// </summary>
        private void HandleGetVoteRoomListRequest(
            object sender,
            PbRequestEventArgs<GetVoteRoomListRequest,
                               GetVoteRoomListResponse> e)
        {
            var roomCount = GlobalControl.Instance.VoteRoomCount;
            var fromIndex = e.Request.FromIndex;
            var toIndex = e.Request.ToIndex;

            if (roomCount == 0)
            {
                // ルームがない場合は、どんな引数でも何も返しません。
                // TODO: いいのかこれで？
                fromIndex = 0;
                toIndex = 0;
            }
            else
            {
                if (fromIndex < 0 || fromIndex >= roomCount || toIndex >= roomCount)
                {
                    Log.Error(this,
                        "渡されたインデックスが正しくありません。" +
                        "(FromIndex = {0}, ToIndex = {1})",
                        fromIndex, toIndex);

                    e.ErrorCode = ErrorCode.Argument;
                    return;
                }

                // 最終インデックスが負の場合は、全部屋情報を取得します。
                if (toIndex < 0)
                {
                    toIndex = roomCount;
                }
            }

            // 部屋情報をコピーします。
            var roomInfoList = GlobalControl.Instance.VoteRoomInfoList;
            var response = new GetVoteRoomListResponse();

            for (var i = fromIndex; i < toIndex; ++i)
            {
                // nullデータは送信しません。
                if (roomInfoList[i] == null)
                {
                    continue;
                }

                response.RoomInfoList.Add(roomInfoList[i]);
            }

            e.Response = response;
        }

        /// <summary>
        /// 参加者の属性情報の設定を行います。
        /// </summary>
        private void HandleSetParticipantAttributeRequest(
             object sender,
             PbRequestEventArgs<SetParticipantAttributeRequest,
                                SetParticipantAttributeResponse> e)
        {
            // 必要な属性情報を設定します。
            using (LazyLock())
            {
                var req = e.Request;

                // ニコ生のコメンターとして使うかどうかです。
                if (req.IsUseAsNicoCommenter != null)
                {
                    this.IsUseAsNicoCommenter =
                        req.IsUseAsNicoCommenter.Value;
                }

                // ニコ生へのログイン状況を設定します。
                if (req.IsSetLoginType)
                {
                    this.NicoLoginType = req.LoginType;
                }

                if (!string.IsNullOrEmpty(req.Message))
                {
                    this.Message = req.Message;
                }
            }

            e.Response = new SetParticipantAttributeResponse()
            {
                Info = Info,
            };
        }
        #endregion

        #region 入出前のみ有効なリクエスト
        /// <summary>
        /// 投票ルーム作成リクエストを処理します。
        /// </summary>
        private void HandleCreateVoteRoomRequest(
            object sender,
            PbRequestEventArgs<CreateVoteRoomRequest,
                               CreateVoteRoomResponse> e)
        {
            using (LazyLock())
            {
                // 投票ルームへの未入室確認を行います。
                var error = CheckEnteringVoteRoom(false);
                if (error != ErrorCode.None)
                {
                    e.ErrorCode = error;
                    return;
                }

                // IDを事前にパースします。
                var guid = ProtocolUtil.ParseId(e.Request.OwnerId);
                if (guid == null)
                {
                    Log.Error(this,
                        "渡されたIDが正しくありません。('{0}')",
                        e.Request.OwnerId);

                    e.ErrorCode = ErrorCode.Argument;
                    return;
                }

                if (!ProtocolUtil.CheckVoteRoomOwnerName(e.Request.OwnerName))
                {
                    Log.Error(this,
                        "渡されたオーナー名が正しくありません。('{0}')",
                        e.Request.OwnerName);

                    e.ErrorCode = ErrorCode.Argument;
                    return;
                }

                if (!ProtocolUtil.CheckVoteRoomName(e.Request.RoomName))
                {
                    Log.Error(this,
                        "渡されたルーム名が正しくありません。('{0}')",
                        e.Request.RoomName);

                    e.ErrorCode = ErrorCode.Argument;
                    return;
                }

                // 新たに部屋を作成します。
                var voteRoom = GlobalControl.Instance.CreateVoteRoom(
                    this, e.Request.RoomName, e.Request.Password);
                if (voteRoom == null)
                {
                    Log.Error(this,
                        "ルーム作成に失敗しました。");

                    e.ErrorCode = ErrorCode.CreateVoteRoom;
                    return;
                }

                Log.Info(voteRoom,
                    "投票ルームを作成しました。");

                // 名前と画像URLを設定します。
                this.Id = guid.Value;
                this.Name = e.Request.OwnerName;
                this.ImageUrl = e.Request.ImageUrl;

                // レスポンスを返します。
                e.Response = new CreateVoteRoomResponse()
                {
                    RoomInfo = voteRoom.Info,
                    ParticipantNo = No,
                };
            }
        }

        /// <summary>
        /// 投票ルームへの接続要求を処理します。
        /// </summary>
        private void HandleEnterVoteRoomRequest(
            object sender,
            PbRequestEventArgs<EnterVoteRoomRequest,
                               EnterVoteRoomResponse> e)
        {
            // IDを事前にパースします。
            var guid = ProtocolUtil.ParseId(e.Request.ParticipantId);
            if (guid == null)
            {
                Log.Error(this,
                    "渡されたIDが正しくありません。('{0}')",
                    e.Request.ParticipantId);

                e.ErrorCode = ErrorCode.Argument;
                return;
            }

            if (!ProtocolUtil.CheckVoteRoomOwnerName(e.Request.ParticipantName))
            {
                Log.Error(this,
                    "渡された参加者名が正しくありません。('{0}')",
                    e.Request.ParticipantName);

                e.ErrorCode = ErrorCode.Argument;
                return;
            }

            using (LazyLock())
            {
                // 投票ルームへの未入室確認を行います。
                var error = CheckEnteringVoteRoom(false);
                if (error != ErrorCode.None)
                {
                    e.ErrorCode = error;
                    return;
                }

                // 指定のＩＤを持つ部屋を探します。
                var voteRoom = GlobalControl.Instance.FindVoteRoom(e.Request.RoomId);
                if (voteRoom == null)
                {
                    Log.Error(this,
                        "指定の投票ルームが見つかりませんでした。(Index = {0})",
                        e.Request.RoomId);

                    e.ErrorCode = ErrorCode.VoteRoomNotFound;
                    return;
                }

#if false
            // 接続切れなどの問題に対応するため、IDチェックは行いません。
            // 同じルームに同じIDのメンバーが入室することもありえます。
            if (voteRoom.GetParticipant(guid.Value) != null)
            {
                Log.Error(this,
                    "指定の投票ルームには既に参加しています。(Index = {0})",
                    e.Request.RoomId);

                e.ErrorCode = ErrorCode.AlreadyEnteredVoteRoom;
                return;
            }
#endif

                // パスワードが一致するか調べます。
                if (voteRoom.HasPassword)
                {
                    if (!voteRoom.MatchPassword(e.Request.Password))
                    {
                        Log.Error(this,
                            "パスワードが一致しませんでした。(Index = {0})",
                            e.Request.RoomId);

                        e.ErrorCode = ErrorCode.PasswordUnmatched;
                        return;
                    }
                }

                // 名前と画像URLを設定します。
                this.Id = guid.Value;
                this.Name = e.Request.ParticipantName;
                this.ImageUrl = e.Request.ImageUrl;

                // 所属ルームを設定し、Noなどを設定してもらいます。
                voteRoom.AddParticipant(this);

                e.Response = new EnterVoteRoomResponse()
                {
                    RoomInfo = voteRoom.Info,
                    ParticipantNo = No,
                };
            }
        }
        #endregion

        #region 入室後のみ有効なリクエスト
        /// <summary>
        /// 投票者一覧を取得します。
        /// </summary>
        private void HandleVoterListRequest(
            object sender,
            PbRequestEventArgs<GetVoterListRequest,
                               GetVoterListResponse> e)
        {
            using (LazyLock())
            {
                var error = CheckEnteringVoteRoom(true);
                if (error != ErrorCode.None)
                {
                    e.ErrorCode = error;
                    return;
                }

                e.Response = new GetVoterListResponse()
                {
                    VoterList = this.voteRoom.VoterListManager.VoterList,
                };
            }
        }

        /// <summary>
        /// 投票ルームからの退出要求を取得します。
        /// </summary>
        private void HandleLeaveVoteRoomRequest(
            object sender,
            PbRequestEventArgs<LeaveVoteRoomRequest,
                               LeaveVoteRoomResponse> e)
        {
            using (LazyLock())
            {
                // 投票ルームへの入室確認を行います。
                var error = CheckEnteringVoteRoom(true);
                if (error != ErrorCode.None)
                {
                    e.ErrorCode = error;
                    return;
                }

                // 投票ルームから削除します。
                VoteRoom.RemoveParticipant(this);

                e.Response = new LeaveVoteRoomResponse();
            }
        }
        #endregion

        /// <summary>
        /// 新しいコネクションを作成します。
        /// </summary>
        private PbConnection CreateConnection()
        {
            var connection = new PbConnection()
            {
                Name = "投票参加者",
                ProtocolVersion = ServerSettings.ProtocolVersion,
            };

            connection.Disconnected += Connection_OnDisconnected;

            // リクエスト
            connection.AddRequestHandler<
                GetVoteRoomCountRequest, GetVoteRoomCountResponse>(
                    HandleGetVoteRoomCountRequest);
            connection.AddRequestHandler<
                GetVoteRoomListRequest, GetVoteRoomListResponse>(
                    HandleGetVoteRoomListRequest);
            connection.AddRequestHandler<
                SetParticipantAttributeRequest,
                SetParticipantAttributeResponse>(
                    HandleSetParticipantAttributeRequest);
            connection.AddRequestHandler<
                LiveOperationRequest, LiveOperationResponse>(
                    this.liveRoomManager.HandleLiveOperationRequest);

            // 入室前のみに有効なリクエスト
            connection.AddRequestHandler<
                CreateVoteRoomRequest, CreateVoteRoomResponse>(
                    HandleCreateVoteRoomRequest);
            connection.AddRequestHandler<
                EnterVoteRoomRequest, EnterVoteRoomResponse>(
                    HandleEnterVoteRoomRequest);

            // 入室後のみに有効なリクエスト
            connection.AddRequestHandler<
                GetVoterListRequest, GetVoterListResponse>(
                    HandleVoterListRequest);
            connection.AddRequestHandler<
                LeaveVoteRoomRequest, LeaveVoteRoomResponse>(
                    HandleLeaveVoteRoomRequest);

            // コマンド
            connection.AddCommandHandler<LiveConnectedCommand>(
                this.liveRoomManager.HandleLiveConnectedAsCommenterCommand);
            connection.AddCommandHandler<LiveDisconnectedCommand>(
                this.liveRoomManager.HandleLiveDisconnectedAsCommenterCommand);
            connection.AddCommandHandler<CommenterStateChangedCommand>(
                this.liveRoomManager.HandleCommenterStateChangedCommand);

            return connection;
        }

        /// <summary>
        /// 接続切断時に呼ばれます。
        /// </summary>
        private void Connection_OnDisconnected(object sender,
                                               DisconnectEventArgs e)
        {
            var handler = this.Disconnected;

            if (handler != null)
            {
                Util.SafeCall(() =>
                    handler(this, e));
            }

            // もし放送ルームがある場合は、それをすべて削除します。
            this.liveRoomManager.ClearLiveRoom();

            this.connection = null;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VoteParticipant(Socket socket)
        {
            if (socket == null || !socket.Connected)
            {
                throw new ArgumentException(
                    "ソケットが接続されていません。", "socket");
            }

            No = -1;
            Id = Guid.Empty;
            Name = string.Empty;
            ImageUrl = string.Empty;
            Message = string.Empty;
            IsUseAsNicoCommenter = false;
            NicoLoginType = NicoLoginType.NotLogined;

            this.liveRoomManager = new LiveRoomManager(this);
            this.AddDependModel(this.liveRoomManager);

            // ハンドラを設定してから、ソケットの設定を行います。
            this.connection = CreateConnection();            
            this.connection.SetSocket(socket);
        }
    }
}
