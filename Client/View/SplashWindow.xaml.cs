using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

using Ragnarok.Presentation.Control;

namespace VoteSystem.Client.View
{
    /// <summary>
    /// SplashWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SplashWindow : MovableWindow, IInitLogger
    {
        private DispatcherTimer timer;
        private string internalMessage = "";
        private int count;

        /// <summary>
        /// 表示メッセージを扱う依存プロパティです。
        /// </summary>
        private static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                "Message", typeof(string), typeof(SplashWindow),
                new UIPropertyMetadata(string.Empty));

        /// <summary>
        /// バージョン情報を取得します。
        /// </summary>
        public string Version
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
        /// 表示メッセージを取得または設定します。
        /// </summary>
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        /// <summary>
        /// 初期化ログを出力します。
        /// </summary>
        public void Log(string message)
        {
            this.internalMessage = (message != null ? message : "");
            this.count = 0;

            Message = internalMessage;
        }

        /// <summary>
        /// 一定間隔で呼ばれます。
        /// </summary>
        private void TimerCallback()
        {
            this.count = ++this.count % 5;

            Message = this.internalMessage + new string('。', this.count);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SplashWindow()
        {
            InitializeComponent();

            Loaded += SplashWindow_Loaded;
            Closed += (sender, e) =>
            {
                if (this.timer != null)
                {
                    this.timer.Stop();
                    this.timer = null;
                }
            };

            DataContext = this;
            Log("更新確認中");

            // 環境によっては更新確認に１０秒以上かかることがあります。
            // そういった場合に対応するため、スプラッシュウィンドウのメッセージを
            // アニメーションさせます。
            this.timer = new DispatcherTimer(
                TimeSpan.FromSeconds(0.5),
                DispatcherPriority.Render,
                (sender, e) => TimerCallback(),
                Dispatcher);
            this.timer.Start();

            UpdateChecker.UpdateFinished += UpdateChecker_UpdateFinished;
            UpdateChecker.CheckUpdate();
        }

        /// <summary>
        /// Load後でないと、ウィンドウの幅と高さが確定しません。
        /// </summary>
        void SplashWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var sw = SystemParameters.PrimaryScreenWidth;
            var sh = SystemParameters.PrimaryScreenHeight;

            Left = (sw - ActualWidth) / 2.0;
            Top = (sh - ActualHeight) / 2.0;
        }

        void UpdateChecker_UpdateFinished(object sender, EventArgs e)
        {
            Global.UIProcess(() =>
            {
                ((App)App.Current).Start(this);

                Close();
            });
        }
    }
}
