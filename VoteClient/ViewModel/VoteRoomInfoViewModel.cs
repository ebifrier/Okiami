using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;

using Ragnarok;
using Ragnarok.ObjectModel;
using Ragnarok.Net.ProtoBuf;

namespace VoteSystem.Client.ViewModel
{
    using Protocol;
    using Protocol.Vote;
    using VoteSystem.Client.Model;

    /// <summary>
    /// 投票ルーム一覧を表示するためのモデルオブジェクトです。
    /// </summary>
    public class VoteRoomInfoViewModel : DynamicViewModel, ILogObject
    {
        private readonly VoteClient voteClient;
        private int selectedVoteRoomId = -1;
        private ObservableCollection<VoteRoomInfo> voteRoomInfoList =
            new ObservableCollection<VoteRoomInfo>();
        private readonly Thread thread;

        /// <summary>
        /// プロパティの変更を通知します。
        /// </summary>
        public override event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// プロパティ値の変更を通知します。
        /// </summary>
        public override void NotifyPropertyChanged(PropertyChangedEventArgs e)
        {
            Ragnarok.Presentation.WPFUtil.CallPropertyChanged(
                PropertyChanged, this, e);
        }

        /// <summary>
        /// ログ出力用の名前を取得します。
        /// </summary>
        public string LogName
        {
            get
            {
                return "投票ルームモデル";
            }
        }

        /// <summary>
        /// 選択された投票ルームのIdを取得または設定します。
        /// </summary>
        public int SelectedVoteRoomId
        {
            get
            {
                return this.selectedVoteRoomId;
            }
            set
            {
                if (this.selectedVoteRoomId != value)
                {
                    this.selectedVoteRoomId = value;

                    RaisePropertyChanged("SelectedVoteRoomId");
                }
            }
        }

        /// <summary>
        /// 選択された投票ルームを取得または設定します。
        /// </summary>
        public VoteRoomInfo SelectedVoteRoomInfo
        {
            get
            {
                var roomInfo = this.voteRoomInfoList.FirstOrDefault(
                    room => room.Id == this.selectedVoteRoomId);

                if (roomInfo == null)
                {
                    this.selectedVoteRoomId = -1;
                }

                return roomInfo;
            }
        }

        /// <summary>
        /// 投票ルームのリストを取得します。
        /// </summary>
        /// <remarks>
        /// マルチスレッド非対応なので、かならずGUIスレッドから呼んでください。
        /// </remarks>
        public ObservableCollection<VoteRoomInfo> VoteRoomInfoList
        {
            get
            {
                return this.voteRoomInfoList;
            }
            private set
            {
                this.voteRoomInfoList = value;

                this.RaisePropertyChanged("VoteRoomInfoList");
            }
        }

        /// <summary>
        /// サーバー上からの投票ルーム情報の取得を行います。
        /// </summary>
        /// <remarks>
        /// サーバーが起動していなかったり、エラーがあったりすると
        /// レスポンスが返るまでに異常な時間がかかることがあります。
        /// 
        /// 以前は、更新処理に<see cref="System.Threading.Timer"/>を使って
        /// いたのですが、余り長い時間を使うコールバックでは
        /// 他のタイマーとの兼ね合いで渋滞を引き起こすことがあるので、
        /// 安全のためスレッドを使うように変更しました。
        /// </remarks>
        private void UpdateVoteRoomInfoLoop(object state)
        {
            while (true)
            {
                try
                {
                    // ログインチェックにはグローバルのVoteClientを使います。
                    if (!Global.VoteClient.IsLogined)
                    {
                        if (!this.voteClient.IsConnected)
                        {
                            // 未接続なら投票サーバーに接続しに行きます。
                            this.voteClient.Connect(
                                Protocol.ServerSettings.VoteAddress,
                                Protocol.ServerSettings.VotePort);

                            continue;
                        }

                        // 投票ルーム情報を取得します。
                        this.voteClient.GetVoteRoomList(
                            0, -1,
                            GetVoteRoomListDone);
                    }
                    else
                    {
                        // サーバー負荷を減らすため、
                        // 非ログイン時はコネクションを切断します。
                        this.voteClient.Disconnect();
                    }
                }
                catch (VersionUnmatchedException)
                {
                    // アプリのバージョンが低いため、接続できません。
                    Global.UIProcess(() =>
                    {
                        //MessageUtil.ErrorMessage(ex.Message);

                        VoteRoomInfoList.Clear();
                    });
                    break;
                }
                catch (Exception)
                {
                    /*Log.ErrorException(this, ex,
                        "投票ルーム情報の取得に失敗しました。");*/

                    // エラー時に変なゴミが残ると大変なので、
                    // ルーム情報はすべて初期化しておきます。
                    Global.UIProcess(() =>
                        VoteRoomInfoList.Clear());
                }

                Thread.Sleep(TimeSpan.FromSeconds(5.0));
            }
        }

        /// <summary>
        /// 投票ルーム一覧の取得時に呼ばれます。
        /// </summary>
        private void GetVoteRoomListDone(
            object sender,
            PbResponseEventArgs<GetVoteRoomListResponse> e)
        {
            if (e.ErrorCode != Protocol.ErrorCode.None)
            {
                Log.Error(this,
                    "投票ルームの取得に失敗しました。(理由: {0})",
                    ErrorCodeUtil.GetDescription(e.ErrorCode));
                return;
            }

            Global.UIProcess(() =>
            {
                // 投票ルームリストをUIスレッド上で更新します。
                VoteRoomInfoList =
                    new ObservableCollection<VoteRoomInfo>(
                        e.Response.RoomInfoList);
            });
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VoteRoomInfoViewModel()
        {
            this.voteClient = new VoteClient(false)
            {
                IsShowErrorMessage = false,
            };

            this.AddDependModel(this.voteClient);

            this.thread = new Thread(UpdateVoteRoomInfoLoop)
            {
                Name = "GetVoteRoomInfo",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
            };
            this.thread.Start();
        }
    }
}
