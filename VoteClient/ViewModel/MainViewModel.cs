using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Ragnarok;
using Ragnarok.NicoNico;
using Ragnarok.NicoNico.Live;
using Ragnarok.ObjectModel;
using Ragnarok.Presentation;

namespace VoteSystem.Client.ViewModel
{
    using VoteSystem.Protocol;
    using VoteSystem.Protocol.Vote;
    using VoteSystem.Client.Model;

    /// <summary>
    /// GUI関連のプロパティを持つモデルオブジェクトです。
    /// </summary>
    /// <remarks>
    /// シングルスレッドオブジェクトです。
    /// GUIスレッド以外からはアクセスしないでください。
    /// </remarks>
    public class MainViewModel : DynamicViewModel, ILogObject
    {
        private readonly MainModel baseModel;
        private List<MenuItem> pluginMenuList;
        private string messageString = string.Empty;
        private string notificationString = string.Empty;
        private VoteState oldVoteState = VoteState.Stop;

        /// <summary>
        /// ログ出力用の名前を取得します。
        /// </summary>
        public string LogName
        {
            get
            {
                return "MainViewModel";
            }
        }

        /// <summary>
        /// 各プラグインのメニュー一覧を取得します。
        /// </summary>
        public List<MenuItem> PluginMenuList
        {
            get
            {
                return this.pluginMenuList;
            }
            private set
            {
                this.pluginMenuList = value;

                this.RaisePropertyChanged("PluginMenuList");
            }
        }

        /// <summary>
        /// 投票状態を示すテキストを取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteClient), "VoteState")]
        public string VoteStateText
        {
            get
            {
                var voteClient = this.baseModel.VoteClient;
                var label = EnumEx.GetEnumLabel(voteClient.VoteState);
                if (label == null)
                {
                    return "不明な状態";
                }

                return label;
            }
        }

        /// <summary>
        /// 投票状態を示すアイコン画像のURLを取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteClient), "VoteState")]
        public Uri VoteStateImageUri
        {
            get
            {
                switch (this.baseModel.VoteClient.VoteState)
                {
                    case VoteState.Voting:
                        return Global.MakeImageUri("vote_state_voting.png");
                    case VoteState.Pause:
                        return Global.MakeImageUri("vote_state_pause.png");
                    case VoteState.Stop:
                        return Global.MakeImageUri("vote_state_stop.png");
                    case VoteState.End:
                        return Global.MakeImageUri("vote_state_end.png");
                }

                return null;
            }
        }

        /// <summary>
        /// 投票の残り時間を表示する文字列を取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteClient), "VoteState")]
        [DependOnProperty(typeof(MainModel), "VoteLeaveTime")]
        public string VoteLeaveTimeString
        {
            get
            {
                var leaveTime = this.baseModel.VoteLeaveTime;
                if (this.baseModel.VoteClient.VoteState == VoteState.Stop)
                {
                    return "停止中";
                }
                else if (leaveTime == TimeSpan.MaxValue)
                {
                    return "無制限";
                }
                else
                {
                    var time = (
                        leaveTime >= TimeSpan.Zero ?
                        leaveTime :
                        TimeSpan.Zero);

                    return string.Format("{0:D2}:{1:D2}",
                        (int)time.TotalMinutes,
                        time.Seconds);
                }
            }
        }

        /// <summary>
        /// 投票の全残り時間を表示する文字列を取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteClient), "VoteState")]
        [DependOnProperty(typeof(MainModel), "TotalVoteLeaveTime")]
        public string TotalVoteLeaveTimeString
        {
            get
            {
                var leaveTime = this.baseModel.TotalVoteLeaveTime;
                if (leaveTime == TimeSpan.MaxValue)
                {
                    return "無制限";
                }
                else
                {
                    var time = (
                        leaveTime >= TimeSpan.Zero ?
                        leaveTime :
                        TimeSpan.Zero);

                    return string.Format("{0:D2}:{1:D2}",
                        (int)time.TotalMinutes,
                        time.Seconds);
                }
            }
        }

        /// <summary>
        /// 投票の残り時間を示すラベルの色を取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteClient), "VoteState")]
        [DependOnProperty(typeof(MainModel), "VoteLeaveTime")]
        public Color VoteLeaveTimeBackgroundColor
        {
            get
            {
                var voteLeaveTime = this.baseModel.VoteLeaveTime;

                switch (this.baseModel.VoteClient.VoteState)
                {
                    case VoteState.Voting:
                        if (voteLeaveTime < TimeSpan.FromSeconds(60))
                        {
                            return Color.FromArgb(160, 230, 0, 0);
                        }
                        else if (voteLeaveTime < TimeSpan.FromMinutes(3))
                        {
                            return WPFUtil.MakeColor(180, Colors.DarkOrange);
                        }
                        return WPFUtil.MakeColor(200, Colors.DarkGray);
                    case VoteState.End:
                        //return WpfUtil.MakeColor(160, Colors.DarkViolet);
                        return Colors.Transparent;
                    case VoteState.Pause:
                        return WPFUtil.MakeColor(127, Colors.Goldenrod);
                    case VoteState.Stop:
                        //return WpfUtil.MakeColor(200, Colors.DarkGray);
                        return Colors.Transparent;
                }

                throw new InvalidOperationException(
                    "VoteStateの値が正しくありません。");
            }
        }

        /// <summary>
        /// 一言メッセージを取得または設定します。
        /// </summary>
        public string MessageString
        {
            get
            {
                return this.messageString;
            }
            set
            {
                if (this.messageString != value)
                {
                    this.messageString = value;

                    RaisePropertyChanged("MessageString");
                }
            }
        }

        /// <summary>
        /// 通知メッセージを取得または設定します。
        /// </summary>
        public string NotificationString
        {
            get
            {
                return this.notificationString;
            }
            set
            {
                if (this.notificationString != value)
                {
                    this.notificationString = value;

                    RaisePropertyChanged("NotificationString");
                }
            }
        }

        /// <summary>
        /// 通知メッセージを送るときのパラメーターを取得します。
        /// </summary>
        [DependOnProperty("NotificationString")]
        public Command.SendNotificationParameter SendNotificationParameter
        {
            get
            {
                return new Command.SendNotificationParameter()
                {
                    Notification = new Notification()
                    {
                        Text = this.notificationString,
                        FromLiveRoom = null,
                        VoterId = "$self",
                        VoterName = this.baseModel.NickName,
                        Timestamp = DateTime.Now,
                    },
                    IsFromLiveOwner = true,
                };
            }
        }

        #region ニコニコ関連
        /// <summary>
        /// ニコニコへのログイン名を取得します。
        /// </summary>
        [DependOnProperty(typeof(NicoClient), "LoginName")]
        public string NicoLoginName
        {
            get
            {
                return this.baseModel.NicoClient.LoginName;
            }
        }

        /// <summary>
        /// 放送タイトルを取得します。
        /// </summary>
        [DependOnProperty(typeof(NicoClient), "LoginId")]
        public string NicoLoginUserUrl
        {
            get
            {
                var id = this.baseModel.NicoClient.LoginId;
                if (id <= 0)
                {
                    return "";
                }

                return NicoString.UserInfoUrl(id);
            }
        }
        #endregion

        private void this_PropertyChanged(object sender,
                                          PropertyChangedEventArgs e)
        {
            if (this.baseModel == null)
            {
                return;
            }

            // 投票状態の変更に応じてSEを再生します。
            if (e.PropertyName == "VoteState")
            {
                var state = this.baseModel.VoteState;
                if (state != this.oldVoteState)
                {
                    Global.SoundManager.PlayVoteSE(state);
                    this.oldVoteState = state;
                }
            }

            // 秒読みSEを鳴らします。
            if (e.PropertyName == "VoteLeaveTime")
            {
                var info = this.baseModel.VoteClient.VoteRoomInfo;
                var time = Ragnarok.Net.NtpClient.GetTime();
                var interval = TimeSpan.FromSeconds(1.5);

                if (info != null &&
                    time - info.BaseTimeNtp > interval &&
                    this.baseModel.VoteState == VoteState.Voting)
                {
                    var leaveTime = this.baseModel.VoteLeaveTime;
                    var leaveSeconds = (int)leaveTime.TotalSeconds;

                    Global.SoundManager.PlayCountdownSE(leaveSeconds);
                }
            }
        }

        /// <summary>
        /// プラグインのメニューを初期化します。
        /// </summary>
        private void InitPluginMenu(object sender, EventArgs e)
        {
            // プラグインのサブメニューを作ります。
            var list =
                from plugin in Global.PluginList
                select new MenuItem()
                {
                    Header = plugin.Name,
                    Command = Command.Commands.RunPlugin,
                    CommandParameter = plugin,
                };

            PluginMenuList = list.ToList();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainViewModel(MainModel model)
            : base(model)
        {
            Global.PluginLoaded += InitPluginMenu;

            this.PropertyChanged += this_PropertyChanged;
            this.baseModel = model;

            this.AddDependModel(model.NicoClient);
            this.AddDependModel(model.VoteClient);
        }
    }
}
