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
        /// 現在の手数を取得または設定します。
        /// </summary>
        public int MoveCount
        {
            get { return GetValue<int>("MoveCount"); }
            set { SetValue("MoveCount", value); }
        }

        /// <summary>
        /// 対局中かどうかを取得または設定します。
        /// </summary>
        public bool? IsPlaying
        {
            get { return GetValue<bool?>("IsPlaying"); }
            set { SetValue("IsPlaying", value); }
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
        /// 先手の残り時間を自動的に同期しているかを取得または設定します。
        /// </summary>
        public bool IsBlackAutoSync
        {
            get { return GetValue<bool>("IsBlackAutoSync"); }
            set { SetValue("IsBlackAutoSync", value); }
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

        private void OnMoveCountChanged()
        {
            // 後手番の加算時間は
            // 0手目開始時 0分
            // 1手目開始時 0分
            // 2手目開始時 1分
            // 3手目開始時 1分
            // 4手目開始時 2分
            // となります。
            var newAddTimeCount = (int)Math.Floor(MoveCount / 2.0);

            var newAddTime = new TimeSpan(0, newAddTimeCount, 0);
            WhiteLeaveTime += newAddTime - WhiteAddTime;
            WhiteUsedTime = TimeSpan.FromMilliseconds(1000 - WhiteLeaveTime.Milliseconds);
            WhiteAddTime = newAddTime;

            Turn = ((MoveCount & 1) == 0 ? BWType.Black : BWType.White);
        }

        private void OnTimer(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            var elapsed = now - this.prevFrameTime;
            this.prevFrameTime = now;

            // 対局中なら時間を変動させます。
            if (IsPlaying == true)
            {
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
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainViewModel()
        {
            Turn = BWType.Black;
            BlackLeaveTime = new TimeSpan(2, 0, 0);
            IsBlackAutoSync = true;
            WhiteLeaveTime = new TimeSpan(2, 0, 0);
            WhiteUsedTime = TimeSpan.Zero;
            WhiteAddTime = TimeSpan.Zero;

            AddPropertyChangedHandler(
                "MoveCount",
                (_, __) => OnMoveCountChanged());

            this.timer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(1000.0 / 60),
                DispatcherPriority.Normal,
                OnTimer,
                WPFUtil.UIDispatcher);
        }
    }
}
