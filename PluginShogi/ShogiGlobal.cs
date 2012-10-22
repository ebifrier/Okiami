using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using Ragnarok;
using Ragnarok.Shogi;
using Ragnarok.ObjectModel;
using Ragnarok.Presentation;

namespace VoteSystem.PluginShogi
{
    using Protocol;
    using ViewModel;

    /// <summary>
    /// 将棋プラグインのグローバルオブジェクトです。
    /// </summary>
    public static class ShogiGlobal
    {
        /// <summary>
        /// 静的コンストラクタ
        /// </summary>
        public static void Initialize(ShogiPlugin plugin)
        {
            Ragnarok.Initializer.Initialize();

            Ragnarok.Util.SetPropertyChangedCaller(
                WpfUtil.CallPropertyChanged);
            Ragnarok.Util.SetColletionChangedCaller(
                WpfUtil.CallCollectionChanged);
            Ragnarok.Util.SetEventCaller(
                WpfUtil.UIProcess);

            ShogiPlugin = plugin;
            Settings = Settings.CreateSettings<Settings>();
            SoundManager = new SoundManager();
            ShogiModel = new ViewModel.ShogiWindowViewModel(new Board()); 

            EffectManager = new ViewModel.EffectManager()
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
                typeof(Effects).GetFields()
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
            set;
        }

        /// <summary>
        /// 通信オブジェクトを取得または設定します。
        /// </summary>
        public static Client.Model.VoteClient VoteClient
        {
            get;
            set;
        }

        /// <summary>
        /// ニコニコへのログインオブジェクトを取得または設定します。
        /// </summary>
        public static Ragnarok.NicoNico.NicoClient NicoClient
        {
            get;
            set;
        }

        /// <summary>
        /// サウンド管理オブジェクトを取得または設定します。
        /// </summary>
        public static SoundManager SoundManager
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
        public static Ragnarok.Presentation.Control.MessageStatusBar MainStatusBar
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
        /// 将棋コントロールビューを取得または設定します。
        /// </summary>
        public static View.MoveManageView MoveManageView
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
        /// 将棋用のビューモデルを取得または設定します。
        /// </summary>
        public static ViewModel.ShogiWindowViewModel ShogiModel
        {
            get;
            private set;
        }

        /// <summary>
        /// 将棋盤のエフェクト管理オブジェクトを取得します。
        /// </summary>
        public static ViewModel.EffectManager EffectManager
        {
            get;
            private set;
        }

        /// <summary>
        /// エラーメッセージを出力します。
        /// </summary>
        public static void ErrorMessage(string format, params object[] args)
        {
            WpfUtil.UIProcess(() =>
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
