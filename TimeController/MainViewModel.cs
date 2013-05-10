using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

using Ragnarok;
using Ragnarok.ObjectModel;
using Ragnarok.Presentation;
using Ragnarok.Shogi;

namespace TimeController
{
    public sealed class MainViewModel : NotifyObject
    {
        private DispatcherTimer timer;
        private DateTime prevFrameTime = DateTime.Now;

        /// <summary>
        /// 現在の手番を取得または設定します。
        /// </summary>
        public BWType Turn
        {
            get { return GetValue<BWType>("Turn"); }
            set { SetValue("Turn", value); }
        }

        /// <summary>
        /// 先手の残り時間を取得または設定します。
        /// </summary>
        public TimeSpan BlackLeaveTime
        {
            get { return GetValue<TimeSpan>("BlackLeaveTime"); }
            set { SetValue("BlackLeaveTime", value); }
        }

        /// <summary>
        /// 一手ごとに追加する時間（後手側）を取得または設定します。
        /// </summary>
        public TimeSpan WhiteAddTime
        {
            get { return GetValue<TimeSpan>("WhiteAddTime"); }
            set { SetValue("WhiteAddTime", value); }
        }

        /// <summary>
        /// 後手の残り時間を取得または設定します。
        /// </summary>
        public TimeSpan WhiteLeaveTime
        {
            get { return GetValue<TimeSpan>("WhiteLeaveTime"); }
            set { SetValue("WhiteLeaveTime", value); }
        }

        /// <summary>
        /// 後手がその手にかけている時間を取得または設定します。
        /// </summary>
        public TimeSpan WhiteUsedTime
        {
            get { return GetValue<TimeSpan>("WhiteUsedTime"); }
            set { SetValue("WhiteUsedTime", value); }
        }

        /// <summary>
        /// タイマーを開始します。
        /// </summary>
        public void StartTimer()
        {
            if (this.timer != null)
            {
                this.timer.Start();
            }
        }

        private void OnTimer(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            var elapsed = now - this.prevFrameTime;
            this.prevFrameTime = now;

            if (Turn == BWType.Black)
            {
                BlackLeaveTime = MathEx.Max(BlackLeaveTime - elapsed, TimeSpan.Zero);
            }
            else
            {
                WhiteLeaveTime = MathEx.Max(WhiteLeaveTime - elapsed, TimeSpan.Zero);
                WhiteUsedTime = MathEx.Max(WhiteUsedTime + elapsed, TimeSpan.Zero);
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainViewModel()
        {
            Turn = BWType.White;
            BlackLeaveTime = TimeSpan.FromMinutes(10);
            WhiteLeaveTime = TimeSpan.FromMinutes(10);
            WhiteUsedTime = TimeSpan.Zero;
            WhiteAddTime = TimeSpan.FromSeconds(60);

            this.timer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(1000.0 / 60),
                DispatcherPriority.Normal,
                OnTimer,
                WPFUtil.UIDispatcher);
        }
    }
}
