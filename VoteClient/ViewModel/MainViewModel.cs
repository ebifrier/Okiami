using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
        private VoteState oldVoteState = VoteState.Stop;

        /// <summary>
        /// ログ出力用の名前を取得します。
        /// </summary>
        public string LogName
        {
            get { return "MainViewModel"; }
        }

        /// <summary>
        /// 一言メッセージを取得または設定します。
        /// </summary>
        public string MessageString
        {
            get { return GetValue<string>("MessageString"); }
            set { SetValue("MessageString", value); }
        }

        /// <summary>
        /// 通知メッセージを取得または設定します。
        /// </summary>
        public string NotificationString
        {
            get { return GetValue<string>("NotificationString"); }
            set { SetValue("NotificationString", value); }
        }

        /// <summary>
        /// 各プラグインのメニュー一覧を取得します。
        /// </summary>
        public List<MenuItem> PluginMenuList
        {
            get { return GetValue<List<MenuItem>>("PluginMenuList"); }
            private set { SetValue("PluginMenuList", value); }
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
                        Text = NotificationString,
                        FromLiveRoom = null,
                        VoterId = "$self",
                        VoterName = this.baseModel.NickName,
                        Timestamp = DateTime.Now,
                    },
                    IsFromLiveOwner = true,
                };
            }
        }

        /// <summary>
        /// ニコニコへのログイン名を取得します。
        /// </summary>
        [DependOnProperty(typeof(NicoClient), "LoginName")]
        public string NicoLoginName
        {
            get { return this.baseModel.NicoClient.LoginName; }
        }

        /// <summary>
        /// ログインユーザーの紹介ページURLを取得します。
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

                return NicoString.GetUserInfoUrl(id);
            }
        }

        /*
        // TODO
        [DependOnProperty(typeof(VoteClient), "ParticipantList")]
        public CollectionView ParticipantList
        {
            get
            {
            }
        }*/

        /// <summary>
        /// 中継したコメント一覧を取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteClient), "PostCommentList")]
        public CollectionView PostCommentList
        {
            get
            {
                var voteClient = this.baseModel.VoteClient;
                if (voteClient == null)
                {
                    return null;
                }

                var view = (CollectionView)
                    CollectionViewSource.GetDefaultView(
                        voteClient.PostCommentList);

                view.SortDescriptions.Add(
                    new SortDescription(
                        "Timestamp",
                        ListSortDirection.Descending));

                return view;
            }
        }

        private void this_PropertyChanged(object sender,
                                          PropertyChangedEventArgs e)
        {
            if (this.baseModel == null)
            {
                return;
            }

            var voteClient = this.baseModel.VoteClient;
            if (voteClient == null)
            {
                return;
            }

            // 投票状態の変更に応じてSEを再生します。
            if (e.PropertyName == "VoteState")
            {
                var state = voteClient.VoteState;
                if (state != this.oldVoteState)
                {
                    Global.SoundManager.PlayVoteSE(state);
                    this.oldVoteState = state;
                }
            }

            // 秒読みSEを鳴らします。
            if (e.PropertyName == "VoteLeaveTime")
            {
                var info = voteClient.VoteRoomInfo;
                var time = Ragnarok.Net.NtpClient.GetTime();
                var interval = TimeSpan.FromSeconds(1.5);

                // 状態変更SEと秒読みSEが重ならないようにします。
                if (info != null &&
                    time - info.BaseTimeNtp > interval &&
                    voteClient.VoteState == VoteState.Voting)
                {
                    var leaveTime = voteClient.VoteLeaveTime;
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
            MessageString = string.Empty;
            NotificationString = string.Empty;
            PluginMenuList = new List<MenuItem>();

            Global.PluginLoaded += InitPluginMenu;

            this.PropertyChanged += this_PropertyChanged;
            this.baseModel = model;

            this.AddDependModel(model.NicoClient);
            this.AddDependModel(model.VoteClient);
        }
    }
}
