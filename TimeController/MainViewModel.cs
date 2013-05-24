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
    using VoteSystem.Protocol;

    public sealed class MainViewModel : NotifyObject
    {
        private DispatcherTimer leaveTimeTimer;
        private DispatcherTimer syncTimeTimer;
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
            set
            {
                if (value != TimeSpan.MinValue && value != TimeSpan.MaxValue)
                {
                    // 時間表示を同期させるため、
                    // 後手残り時間の(1000 - ミリ秒)を思考時間に加えます。
                    // 残り時間は減っていきますが、思考時間は増えるため、
                    // このような操作を行います。
                    value = TimeSpan.FromSeconds(
                        (int)value.TotalSeconds +
                        (1000 - WhiteLeaveTime.Milliseconds) / 1000.0);
                }

                SetValue("WhiteUsedTime", value);
            }
        }

        /// <summary>
        /// タイマーを開始します。
        /// </summary>
        public void StartTimer()
        {
            if (this.leaveTimeTimer != null)
            {
                this.leaveTimeTimer.Start();
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
            WhiteUsedTime = TimeSpan.Zero;
            WhiteAddTime = newAddTime;

            Turn = ((MoveCount & 1) == 0 ? BWType.Black : BWType.White);
        }

        /// <summary>
        /// 残り時間表示の更新用コールバックです。
        /// </summary>
        private void OnLeaveTimeTimer(object sender, EventArgs e)
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
        /// VoteClientとの時刻同期のためのコールバックです。
        /// </summary>
        private void OnSyncTimeTimer(object sender, EventArgs e)
        {
            var span = ProtocolUtil.ReadTotalVoteSpan();
            if (span == TimeSpan.MinValue)
            {
                IsBlackAutoSync = false;
                return;
            }

            // 表示と１秒以上違っていたら、時刻を更新します。
            var diff = BlackLeaveTime - span;
            if (diff < TimeSpan.Zero) diff = -diff;
            if (IsPlaying != null && diff > TimeSpan.FromSeconds(1))
            {
                BlackLeaveTime = span;
            }

            IsBlackAutoSync = true;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainViewModel()
        {
            Turn = BWType.Black;
            BlackLeaveTime = new TimeSpan(2, 0, 0);
            IsBlackAutoSync = false;
            WhiteLeaveTime = new TimeSpan(2, 0, 0);
            WhiteUsedTime = TimeSpan.Zero;
            WhiteAddTime = TimeSpan.Zero;

            AddPropertyChangedHandler(
                "MoveCount",
                (_, __) => OnMoveCountChanged());

            this.leaveTimeTimer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(1000.0 / 60),
                DispatcherPriority.Normal,
                OnLeaveTimeTimer,
                WPFUtil.UIDispatcher);
            this.syncTimeTimer = new DispatcherTimer(
                TimeSpan.FromSeconds(3.0),
                DispatcherPriority.Normal,
                OnSyncTimeTimer,
                WPFUtil.UIDispatcher);
        }
    }
}
