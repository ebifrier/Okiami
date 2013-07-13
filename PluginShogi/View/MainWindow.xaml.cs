﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
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

using Ragnarok;
using Ragnarok.Shogi;
using Ragnarok.Presentation.Shogi;
using Ragnarok.Presentation.Shogi.Effects;
using Ragnarok.Presentation.Utility;
using Ragnarok.Presentation.VisualObject;
using Ragnarok.Presentation.VisualObject.Control;

namespace VoteSystem.PluginShogi.View
{
    using Effects;
    using ViewModel;

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ShogiWindowViewModel model;

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
                using (var reader = new StreamReader(files[0], KifuObject.DefaultEncoding))
                {
                    Commands.LoadKif(reader);
                }
            }
        }

        /// <summary>
        /// 次のエフェクトを設定します。
        /// </summary>
        public void AddEffectKey(string effectKey)
        {
            // 背景エフェクトの作成。
            var effectInfo = new EffectInfo(effectKey, null);
            var effect = effectInfo.LoadBackground();

            this.background.AddEntity(effect);
        }

        /// <summary>
        /// 背景の初期化を行います。
        /// </summary>
        private void InitBackground()
        {
            if (ShogiGlobal.Settings.HasEffectFlag(EffectFlag.Background))
            {
                AddEffectKey("SpringEffect");
            }
        }

        /// <summary>
        /// 描画処理などを行います。
        /// </summary>
        public void Render(TimeSpan elapsedTime)
        {
            ShogiControl.Render(elapsedTime);

            this.background.Render(elapsedTime);
        }

        /// <summary>
        /// エンドロールを開始します。
        /// </summary>
        public void PlayEndRoll(DateTime startTimeNtp)
        {
            ShogiEndRoll.StartPrepare(startTimeNtp);
        }

        /// <summary>
        /// エンドロールを停止します。
        /// </summary>
        public void StopEndRoll()
        {
            ShogiEndRoll.Stop();
        }

        private void FrameTimer_EnterFrame(object sender, FrameEventArgs e)
        {
            Render(e.ElapsedTime);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow(ShogiWindowViewModel model)
        {
            InitializeComponent();
            Commands.Binding(CommandBindings);
            Commands.Binding(InputBindings);
            ShogiControl.InitializeBindings(this);

            //RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);

            this.voteResultControl.InitializeBindings(this);
            this.voteResultControl.SettingUpdated +=
                (_, __) => ShogiGlobal.Settings.Save();

            this.evaluationControl.InitializeBindings(this);
            this.evaluationControl.SettingUpdated +=
                (_, __) => ShogiGlobal.Settings.Save();

            // FPSを表示
            if (Client.Global.IsNonPublished)
            {
                var fpsCounter = new FpsCounter();
                fpsCounter.FpsChanged += (_, __) =>
                    Title = string.Format("FPS: {0:0.00}", fpsCounter.Fps, 30);
                ShogiControl.AddEffect(fpsCounter);
            }

            var manager = ShogiGlobal.EffectManager;
            if (manager != null)
            {
                manager.Background = this.background;
            }

            var timer = ShogiGlobal.FrameTimer;
            if (timer != null)
            {
                timer.EnterFrame += FrameTimer_EnterFrame;
            }

            this.model = model;
            DataContext = model;

            // 背景のトランジションが始まります。
            InitBackground();
        }
        
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            ShogiGlobal.ShogiModel.StopAutoPlay();

            var timer = ShogiGlobal.FrameTimer;
            if (timer != null)
            {
                timer.EnterFrame -= FrameTimer_EnterFrame;
            }

            var manager = ShogiGlobal.EffectManager;
            if (manager != null)
            {
                manager.Background = null;
            }

            if (Client.Global.IsNonPublished)
            {
                GC.Collect();

                // エフェクトがメモリリークしてないかチェックしています。
                var list = EntityObject.GetInstanceList();

                // 盤上の駒20枚×２ ＋ 駒台の駒９種(玉と無も込み)×２
                // ＋ ルート２個 ＋ 前回のマスの位置 ＋ 手番表示
                // ＋ 背景用オブジェクト×２
                var count = (20 * 2 + 9 * 2 + 2 + 2 + 2) * 2 + 1;
                if (list.Count() > count)
                {
                    Ragnarok.Presentation.DialogUtil.ShowError(
                        string.Format(
                            "エフェクトがリークしている可能性があります。{0}個",
                            list.Count() - count));
                }
            }
        }
    }
}
