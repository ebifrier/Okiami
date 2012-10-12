using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

using Ragnarok;

namespace VoteSystem.Client
{
    /// <summary>
    /// アプリケーションクラスです。
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            try
            {
                Initializer.Initialize();

                var window = new View.SplashWindow();
                window.Show();
                MainWindow = window;
                //Start(null);
            }
            catch (Exception ex)
            {
                Log.FatalException(ex,
                    "未処理の例外が発生しました。");

                throw;
            }
        }

        /// <summary>
        /// アプリ本体を開始します。
        /// </summary>
        internal void Start(IInitLogger logger)
        {
            // 静的オブジェクトの初期化を行います。
            Global.Initialize();

            // ウィンドウを作成します。
            // ここでGlobal.MainWindowが設定されます。
            var controlWindow = new View.MainWindow();

            // ウィンドウ作成後にプラグインを初期化します。
            Global.InitPlugin(logger);

            // すべての初期化終了後にウィンドウを表示します。
            controlWindow.Show();
            MainWindow = controlWindow;
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.FatalException(e.Exception,
                "未処理の例外が発生しました。");
        }
    }
}
