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
                /*var s = DateTime.Now.ToString("HH:mm:ss.ffff");
                Console.Write(s);

                Test();*/
                Start();
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
        internal void Start()
        {
            // 静的オブジェクトの初期化を行います。
            Global.Initialize();

            // 更新作業の開始
            UpdateChecker.CheckUpdate();

            // ウィンドウを作成します。
            var controlWindow = new View.MainWindow();
            controlWindow.Show();
            MainWindow = controlWindow;

            // ウィンドウ作成後にプラグインを初期化します。
            Global.InitPlugin();
        }

#if true
        private void Test()
        {
            var data = Ragnarok.Util.ReadFile("protobuf.dump");

            Ragnarok.Net.ProtoBuf.PbUtil.Deserialize(
                data, typeof(Protocol.SendVoteRoomInfoCommand));
        }
#endif

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.FatalException(e.Exception,
                "未処理の例外が発生しました。");
        }
    }
}
