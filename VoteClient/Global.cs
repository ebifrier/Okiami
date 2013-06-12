using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;

using Ragnarok.Net;
using Ragnarok.Net.ProtoBuf;
using Ragnarok.NicoNico.Live;
using Ragnarok.Presentation.Update;

namespace VoteSystem.Client
{
    using Protocol;
    using Protocol.Vote;

    /// <summary>
    /// グローバルなオブジェクトを保持します。
    /// </summary>
    public static class Global
    {
        /// <summary>
        /// アプリの更新用URLです。
        /// </summary>
        public static readonly string UpdateUrl =
            @"http://garnet-alice.net/programs/votesystem/update/versioninfo.xml";

        /// <summary>
        /// プラグインが読み込まれたときに呼ばれます。
        /// </summary>
        public static event EventHandler PluginLoaded;

        /// <summary>
        /// 公開用プログラムかどうかを取得します。
        /// </summary>
        public static bool IsPublished
        {
            get
            {
#if PUBLISHED
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// 公開しないプログラムかどうかを取得します。
        /// </summary>
        public static bool IsNonPublished
        {
            get
            {
                return !IsPublished;
            }
        }

        /// <summary>
        /// プログラム未公開時に表示するかどうかを取得します。
        /// </summary>
        public static Visibility IsNonPublishedVisibility
        {
            get
            {
                return (IsPublished ? Visibility.Collapsed : Visibility.Visible);
            }
        }

        /// <summary>
        /// バージョン情報を取得します。
        /// </summary>
        public static string Version
        {
            get
            {
                var asm = Assembly.GetExecutingAssembly();
                var version = FileVersionInfo.GetVersionInfo(asm.Location);

                return string.Format("{0}.{1}.{2}",
                    version.ProductMajorPart,
                    version.ProductMinorPart,
                    version.ProductBuildPart);
            }
        }

        /// <summary>
        /// 各モードのプラグインの一覧を取得します。
        /// </summary>
        public static List<IPlugin> PluginList
        {
            get;
            private set;
        }

        /// <summary>
        /// 設定ファイルオブジェクトを取得します。
        /// </summary>
        public static Settings Settings
        {
            get;
            private set;
        }

        /// <summary>
        /// ＳＥ管理オブジェクトを取得します。
        /// </summary>
        public static Model.SoundManager SoundManager
        {
            get;
            private set;
        }

        /// <summary>
        /// 各アドレス＆ポートの接続数管理オブジェクトを取得または設定します。
        /// </summary>
        public static Model.ConnectionCounter ConnectionCounter
        {
            get;
            private set;
        }

        /// <summary>
        /// 投票用のクライアントを取得します。
        /// </summary>
        public static Model.VoteClient VoteClient
        {
            get
            {
                if (MainModel == null)
                {
                    return null;
                }

                return MainModel.VoteClient;
            }
        }

        /// <summary>
        /// モデルを取得します。
        /// </summary>
        public static Model.MainModel MainModel
        {
            get;
            private set;
        }

        /// <summary>
        /// ビューモデルを取得します。
        /// </summary>
        public static ViewModel.MainViewModel MainViewModel
        {
            get;
            private set;
        }

        /// <summary>
        /// 投票ルームリストを取得するためのモデルを取得します。
        /// </summary>
        public static ViewModel.VoteRoomInfoViewModel VoteRoomInfoModel
        {
            get;
            private set;
        }

        /// <summary>
        /// 更新用オブジェクトを取得します。
        /// </summary>
        public static PresentationUpdater Updater
        {
            get;
            private set;
        }

        /// <summary>
        /// メインウィンドウを取得または設定します。
        /// </summary>
        public static View.MainWindow MainWindow
        {
            get;
            set;
        }

        /// <summary>
        /// ステータスメッセージ用のオブジェクトを取得します。
        /// </summary>
        public static Ragnarok.Presentation.Control.MessageStatusBar StatusBar
        {
            get;
            set;
        }

        /// <summary>
        /// UIスレッドに関連づけられたディスパッチャーを取得します。
        /// </summary>
        public static Dispatcher UIDispatcher
        {
            get
            {
                if (Application.Current == null)
                {
                    return null;
                }

                return Application.Current.Dispatcher;
            }
        }

        /// <summary>
        /// 与えられた手続きをUIスレッド上で実行します。
        /// </summary>
        public static void UIProcess(Action func)
        {
            var dispatcher = Global.UIDispatcher;
            if (dispatcher == null)
            {
                return;
            }

            if (dispatcher.CheckAccess())
            {
                func();
            }
            else
            {
                dispatcher.BeginInvoke(func);
            }
        }

        /// <summary>
        /// すべてのコマンドの実行可能性を調べます。
        /// </summary>
        public static void InvalidateCommand()
        {
            if (Global.UIDispatcher == null)
            {
                return;
            }

            UIProcess(CommandManager.InvalidateRequerySuggested);
        }

        /// <summary>
        /// ルームからの接続・切断時に特別な処理をする
        /// コメントクライアントを作成します。
        /// </summary>
        public static CommentClient CreateCommentClient()
        {
            var commentClient = new CommentClient();

            commentClient.ConnectedRoom += commentClient_ConnectedRoom;
            commentClient.DisconnectedRoom += commentClient_DisconnectedRoom;

            return commentClient;
        }

        /// <summary>
        /// コメントルームに接続したときに呼ばれます。
        /// </summary>
        static void commentClient_ConnectedRoom(object sender, CommentRoomEventArgs e)
        {
            var commentClient = (CommentClient)sender;
            if (commentClient == null)
            {
                return;
            }

            var roomInfo = commentClient.GetRoomInfo(e.RoomIndex);
            if (roomInfo == null)
            {
                return;
            }

            // あるコメントルームに接続したことを通知します。
            // これにより、このアドレス＆ポートの組がアプリ内からの接続であると
            // 判断されるようになります。
            ConnectionCounter.Connected(roomInfo.Address, roomInfo.Port);
        }

        /// <summary>
        /// コメントルームから切断されたときに呼ばれます。
        /// </summary>
        static void commentClient_DisconnectedRoom(object sender, CommentRoomDisconnectedEventArgs e)
        {
            var commentClient = (CommentClient)sender;
            if (commentClient == null)
            {
                return;
            }

            var roomInfo = commentClient.GetRoomInfo(e.RoomIndex);
            if (roomInfo == null)
            {
                return;
            }

            // あるコメントルームから切断したことを通知します。
            ConnectionCounter.Disconnected(roomInfo.Address, roomInfo.Port);

            if (e.Reason == DisconnectReason.DisconnectedByOpposite)
            {
                Global.UIProcess(() =>
                    MessageUtil.Confirm(
                        "コメントサーバーから切断されました。",
                        "確認"));
            }
        }

        /// <summary>
        /// 画像ファイル名をリソースのパス指定に使える形式に直します。
        /// </summary>
        public static Uri MakeImageUri(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }

            return new Uri("pack://application:,,,/Resources/Image/" + filename);
        }

        /// <summary>
        /// ウィンドウの作成が終わった後にプラグインの読み込みを行います。
        /// </summary>
        internal static void InitPlugin()
        {
            PluginList = PluginUtil.LoadPlugins();

            var handler = PluginLoaded;
            if (handler != null)
            {
                Ragnarok.Util.SafeCall(() =>
                    handler(null, EventArgs.Empty));
            }

            PluginList.ForEach(_ => _.Run());
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public static void Initialize()
        {
            Ragnarok.Presentation.WPFUtil.Init();

            Settings = Settings.CreateSettings<Settings>();
            SoundManager = new Model.SoundManager();
            ConnectionCounter = new Model.ConnectionCounter();
            MainModel = new Model.MainModel();
            MainViewModel = new ViewModel.MainViewModel(MainModel);
            VoteRoomInfoModel = new ViewModel.VoteRoomInfoViewModel();

            Updater = new PresentationUpdater(UpdateUrl);
            Updater.Start();
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        public static void Uninitialize()
        {
            // ソケットの接続を切らないと、接続が残ることがあります。
            var voteClient = VoteClient;
            if (voteClient != null)
            {
                voteClient.Disconnect();
            }
        }
    }
}
