using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Ragnarok;
using Ragnarok.Shogi;
using Ragnarok.ObjectModel;
using Ragnarok.Presentation;
using Ragnarok.Presentation.Control;
using Ragnarok.Presentation.Extra.Effect;
using Ragnarok.Presentation.Shogi;
using Ragnarok.Presentation.Shogi.View;
using Ragnarok.Presentation.Utility;

namespace VoteSystem.PluginShogi
{
    using Protocol;
    using Protocol.Vote;
    using Effects;
    using ViewModel;

    /// <summary>
    /// 将棋プラグインのグローバルオブジェクトです。
    /// </summary>
    public static class ShogiGlobal
    {
        /// <summary>
        /// 静的コンストラクタ
        /// </summary>
        public static void Initialize(ShogiPlugin plugin, Client.PluginHost host)
        {
            ShogiInitializer.Initialize(
                System.Reflection.Assembly.GetExecutingAssembly(),
                "ShogiData");

            ClientWindow = host.Window;
            VoteClient = host.VoteClient;
            ClientModel = host.MainModel;
            ShogiPlugin = plugin;

            // FrameTimerはSettingsの前に初期化します。
            FrameTimer = new FrameTimer()
            {
                TargetFPS = 30.0,
            };
            FrameTimer.Start();

            // FrameTimerはSettingsの前に初期化します。
            Settings = Settings.CreateSettings<Settings>();
            GlobalModel = new GlobalModel();
            ShogiModel = new ShogiWindowViewModel(Settings, GlobalModel, new Board());
            StartEndrollTimeNtp = DateTime.MinValue;

            EffectManager = new Effects.EffectManager()
            {
                EffectEnabled = true,
                IsAutoPlayEffect = false,
            };

            InitializeEffect();
        }

        /// <summary>
        /// XAMLの事前読み込みを行います。
        /// </summary>
        /// <remarks>
        /// XAML読み込みは初回の読み込みに多大な時間がかかります。
        /// 例）
        /// １回目:100ms、２回目:1ms
        /// 
        /// そのため、全エフェクトを一度事前に読み込みます。
        /// </remarks>
        private static void InitializeEffect()
        {
            try
            {
                typeof(EffectTable).GetFields()
                    .Where(_ => _.FieldType == typeof(EffectInfo))
                    .Select(_ => (EffectInfo)_.GetValue(null))
                    .ForEach(_ => _.PreLoad());
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "エフェクトの初期化に失敗しました。");
            }
        }

        /// <summary>
        /// ホスト側のウィンドウを取得または設定します。
        /// </summary>
        public static Client.View.MainWindow ClientWindow
        {
            get;
            private set;
        }

        /// <summary>
        /// 通信オブジェクトを取得または設定します。
        /// </summary>
        public static Client.Model.VoteClient VoteClient
        {
            get;
            private set;
        }

        /// <summary>
        /// クライアントのメインモデルを取得または設定します。
        /// </summary>
        public static Client.Model.MainModel ClientModel
        {
            get;
            private set;
        }

        /// <summary>
        /// プラグインオブジェクトを取得または設定します。
        /// </summary>
        public static ShogiPlugin ShogiPlugin
        {
            get;
            private set;
        }

        /// <summary>
        /// 将棋のウィンドウを取得または設定します。
        /// </summary>
        public static View.MainWindow MainWindow
        {
            get;
            set;
        }

        /// <summary>
        /// メッセージ表示用のステータスバーを取得または設定します。
        /// </summary>
        public static MessageStatusBar MainStatusBar
        {
            get
            {
                if (MainWindow == null)
                {
                    return null;
                }

                return MainWindow.MainStatusBar;
            }
        }

        /// <summary>
        /// 将棋コントロールを取得または設定します。
        /// </summary>
        public static ShogiUIElement3D ShogiControl
        {
            get
            {
                if (MainWindow == null)
                {
                    return null;
                }

                return MainWindow.ShogiControl;
            }
        }

        /// <summary>
        /// 将棋コントロールビューを取得または設定します。
        /// </summary>
        public static View.MoveManageView MoveManageView
        {
            get;
            set;
        }

        /// <summary>
        /// エンディングの設定ウィンドウを取得または設定します。
        /// </summary>
        public static View.EndingSettingWindow EndingSettingWindow
        {
            get;
            set;
        }

        /// <summary>
        /// 設定オブジェクトを取得します。
        /// </summary>
        public static Settings Settings
        {
            get;
            private set;
        }

        /// <summary>
        /// 全体的な設定を保持するモデルを取得します。
        /// </summary>
        public static GlobalModel GlobalModel
        {
            get;
            private set;
        }

        /// <summary>
        /// 将棋用のビューモデルを取得します。
        /// </summary>
        public static ShogiWindowViewModel ShogiModel
        {
            get;
            private set;
        }

        /// <summary>
        /// 将棋盤のエフェクト管理オブジェクトを取得します。
        /// </summary>
        public static Effects.EffectManager EffectManager
        {
            get;
            private set;
        }

        /// <summary>
        /// FPSタイマーを取得します。
        /// </summary>
        public static FrameTimer FrameTimer
        {
            get;
            private set;
        }

        /// <summary>
        /// エンドロールの再生開始時刻を取得または設定します。
        /// </summary>
        /// <remarks>
        /// エンドロールの再生時刻は投票サーバーから設定されます。
        /// これを保持しておかないと、ウィンドウを閉じたときなどに
        /// 再生時刻が分からなくなります。
        /// </remarks>
        public static DateTime StartEndrollTimeNtp
        {
            get;
            set;
        }

        /// <summary>
        /// エラーメッセージを出力します。
        /// </summary>
        public static void ErrorMessage(string format, params object[] args)
        {
            WPFUtil.UIProcess(() =>
                DialogUtil.Show(
                    MainWindow,
                    string.Format(format, args),
                    "エラー",
                    MessageBoxButton.OK));
        }

        /// <summary>
        /// エラーメッセージを出力します。
        /// </summary>
        public static void ErrorMessage(Exception ex, string format,
                                        params object[] args)
        {
            var message = string.Format(format, args);

            ErrorMessage(
                "{1}{0}{0}理由: {2}",
                Environment.NewLine,
                message, ex.Message);
        }
    }
}
