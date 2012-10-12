using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using Ragnarok.Shogi;
using Ragnarok.Shogi.ViewModel;
using Ragnarok.Presentation.Utility;

namespace VoteSystem.PluginShogi.View
{
    using ViewModel;
    using Detail;

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ShogiWindowViewModel model;
        private ShogiBackgroundCore currentBg;
        private FrameTimer timer;

        /// <summary>
        /// 静敵コンストラクタ
        /// </summary>
        static MainWindow()
        {
            FlintSharp.Utils.ScreenSize = new Size(640, 480);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);

            e.Effects = 
                (e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy
                : DragDropEffects.None);
        }

        /// <summary>
        /// ドラッグ＆ドロップを実行します。
        /// </summary>
        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);

            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null && files.Any())
            {
                var text = Ragnarok.Util.ReadFile(files[0], Encoding.GetEncoding("SJIS"));

                Commands.LoadKifText(text);
            }
        }

        /// <summary>
        /// 背景のトランジションを開始します。
        /// </summary>
        private void StartTransition(ShogiBackgroundCore bgFore,
                                     ShogiBackgroundCore bgBack,
                                     bool fadeBan)
        {
            var fadeTime0 = TimeSpan.FromSeconds(0.0);
            var fadeTime1 = TimeSpan.FromSeconds(1.0);
            var fadeTime2 = TimeSpan.FromSeconds(2.0);
            var fadeTime3 = TimeSpan.FromSeconds(3.0);

            var animFore = new DoubleAnimationUsingKeyFrames();
            animFore.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, fadeTime0));
            animFore.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, fadeTime3));

            var animBack = new DoubleAnimationUsingKeyFrames();
            animBack.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, fadeTime0));
            animBack.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, fadeTime3));

            animFore.Completed += (sender, e) =>
            {
                this.currentBg =
                    (this.currentBg == this.background1
                    ? this.background2
                    : this.background1);

                this.currentBg.EffectKey = null;
            };

            bgFore.BeginAnimation(UIElement.OpacityProperty, animFore);
            bgBack.BeginAnimation(UIElement.OpacityProperty, animBack);

            if (fadeBan)
            {
                var banBrush = ShogiControl.BanBrush;
                if (banBrush != null)
                {
                    var opacity = banBrush.Opacity;
                    var anim = new DoubleAnimationUsingKeyFrames()
                    {
                        FillBehavior = FillBehavior.Stop,
                    };
                    anim.KeyFrames.Add(new LinearDoubleKeyFrame(opacity, fadeTime0));
                    anim.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, fadeTime1));
                    anim.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, fadeTime2));
                    anim.KeyFrames.Add(new LinearDoubleKeyFrame(opacity, fadeTime3));

                    banBrush.BeginAnimation(Brush.OpacityProperty, anim);
                }

                var komadaiBrush = ShogiControl.PieceBoxBrush;
                if (komadaiBrush != null)
                {
                    var opacity = komadaiBrush.Opacity;
                    var anim = new DoubleAnimationUsingKeyFrames()
                    {
                        FillBehavior = FillBehavior.Stop,
                    };
                    anim.KeyFrames.Add(new LinearDoubleKeyFrame(opacity, fadeTime0));
                    anim.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, fadeTime1));
                    anim.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, fadeTime2));
                    anim.KeyFrames.Add(new LinearDoubleKeyFrame(opacity, fadeTime3));

                    komadaiBrush.BeginAnimation(Brush.OpacityProperty, anim);
                }
            }
        }

        /// <summary>
        /// 背景の初期化を行います。
        /// </summary>
        private void InitBackground()
        {
            this.currentBg = this.background1;

            AddEffectKey("SpringEffect", false);
        }

        /// <summary>
        /// 次のエフェクトを設定します。
        /// </summary>
        public void AddEffectKey(string effectKey, bool fadeBan = true)
        {
            if (string.IsNullOrEmpty(effectKey))
            {
                return;
            }

            var back =
                (this.currentBg == this.background1
                ? this.background2
                : this.background1);

            this.currentBg.EffectKey = effectKey;
            StartTransition(this.currentBg, back, fadeBan);
        }

        /// <summary>
        /// 描画処理などを行います。
        /// </summary>
        public void Render(TimeSpan elapsedTime)
        {
            ShogiControl.Render(elapsedTime);

            this.background1.Render(elapsedTime);
            this.background2.Render(elapsedTime);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow(ShogiWindowViewModel model)
        {
            InitializeComponent();

            Commands.Binding(CommandBindings);
            Commands.Binding(InputBindings);

            Closed += MainWindow_Closed;

            if (Client.Global.IsNonPublished)
            {
                var fpsCounter = new FpsCounter();
                fpsCounter.FpsChanged += (sender, e) =>
                    Title = string.Format("FPS: {0:0.00}", fpsCounter.Fps, 30);
                ShogiControl.AddEffect(fpsCounter);
            }

            this.model = model;
            DataContext = model;

            // 背景のトランジションが始まります。
            InitBackground();

            this.timer = new FrameTimer();
            this.timer.EnterFrame += (sender, e) => Render(e.ElapsedTime);
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            if (Client.Global.IsNonPublished)
            {
                GC.Collect();

                // エフェクトがメモリリークしてないかチェックしています。
                var list = Ragnarok.Shogi.ViewModel.EntityObject.GetInstanceList();

                // 盤上の駒20枚×２ ＋ 駒台の駒９種(玉と無も込み)×２
                // ＋ ルート２個 ＋ 前回のマスの位置 ＋ 手番表示
                // ＋ 背景用オブジェクト×２
                var count = 20 * 2 + 9 * 2 + 2 + 2 + 2 + 1;
                if (list.Count() > count)
                {
                    Ragnarok.Presentation.DialogUtil.ShowError(
                        string.Format(
                            "エフェクトがリークしている可能性があります。{0}個",
                            list.Count() - count));
                }
            }

            if (this.timer != null)
            {
                this.timer.Dispose();
                this.timer = null;
            }
        }
    }
}
