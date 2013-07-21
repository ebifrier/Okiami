using System;
using System.Collections.Generic;
using System.IO;
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
    using VoteSystem.Protocol.Vote;

    public sealed class MainViewModel : NotifyObject
    {
        private FileSystemWatcher fileWatcher;
        private DispatcherTimer leaveTimeTimer;
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
        /// 先手がその手にかけている時間を取得または設定します。
        /// </summary>
        public TimeSpan BlackUsedTime
        {
            get { return GetValue<TimeSpan>("BlackUsedTime"); }
            set
            {
                if (value != TimeSpan.MinValue && value != TimeSpan.MaxValue)
                {
                    // 時間表示を同期させるため、
                    // 後手残り時間の(999 - ミリ秒)を思考時間に加えます。
                    // 残り時間は減っていきますが、思考時間は増えるため、
                    // このような操作を行います。
                    value = TimeSpan.FromSeconds(
                        (int)value.TotalSeconds +
                        (999 - BlackLeaveTime.Milliseconds) / 1000.0);
                }

                SetValue("BlackUsedTime", value);
            }
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
        /// 先手側の投票状態を取得します。
        /// </summary>
        public VoteState BlackVoteState
        {
            get { return GetValue<VoteState>("BlackVoteState"); }
            private set { SetValue("BlackVoteState", value); }
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
                    // 後手残り時間の(999 - ミリ秒)を思考時間に加えます。
                    // 残り時間は減っていきますが、思考時間は増えるため、
                    // このような操作を行います。
                    value = TimeSpan.FromSeconds(
                        (int)value.TotalSeconds +
                        (999 - WhiteLeaveTime.Milliseconds) / 1000.0);
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

            // 後手の時刻に自動加算の時間を加えます。
            var newAddTime = new TimeSpan(0, newAddTimeCount, 0);
            WhiteLeaveTime += newAddTime - WhiteAddTime;
            WhiteAddTime = newAddTime;

            // 思考時間は０にします。
            BlackUsedTime = TimeSpan.Zero;
            WhiteUsedTime = TimeSpan.Zero;

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
                    // 自動同期無し、もしくは投票中は時間を進めます。
                    if (!IsBlackAutoSync || BlackVoteState == VoteState.Voting)
                    {
                        BlackLeaveTime = MathEx.Max(BlackLeaveTime - elapsed, TimeSpan.Zero);
                        BlackUsedTime = BlackUsedTime + elapsed;
                    }
                }
                else
                {
                    WhiteLeaveTime = MathEx.Max(WhiteLeaveTime - elapsed, TimeSpan.Zero);
                    WhiteUsedTime = WhiteUsedTime + elapsed;
                }
            }
        }

        /// <summary>
        /// VoteClientとの時刻同期用ファイルの監視を開始します。
        /// </summary>
        public void StartToObserveSyncFile()
        {
            try
            {
                if (this.fileWatcher != null)
                {
                    return;
                }

                var filepath = ProtocolUtil.TotalVoteSpanFilePath;
                var watcher = new FileSystemWatcher()
                {
                    Path = Path.GetDirectoryName(filepath),
                    NotifyFilter = NotifyFilters.LastAccess |
                                   NotifyFilters.LastWrite |
                                   NotifyFilters.FileName,
                    Filter = Path.GetFileName(filepath),
                };

                watcher.Changed += watcher_FileChanged;
                watcher.Created += watcher_FileChanged;
                watcher.Deleted += watcher_FileChanged;

                watcher.EnableRaisingEvents = true;
                this.fileWatcher = watcher;

                // 時刻の同期を開始します。
                WPFUtil.UIProcess(() => SyncTime());
            }
            catch (Exception ex)
            {
                Util.ThrowIfFatal(ex);
            }
        }

        /// <summary>
        /// 時刻同期ファイルの更新時に呼ばれます。
        /// </summary>
        private void watcher_FileChanged(object sender, FileSystemEventArgs e)
        {
            WPFUtil.UIProcess(() => SyncTime());
        }

        /// <summary>
        /// 時刻同期ファイルから時刻を同期します。
        /// </summary>
        private void SyncTime()
        {
            VoteState state;
            DateTime startTime;
            TimeSpan totalSpan;
            TimeSpan progressSpan;

            if (!ProtocolUtil.ReadTotalVoteSpan(
                    out state, out startTime, out totalSpan, out progressSpan))
            {
                BlackVoteState = VoteState.Stop;
                IsBlackAutoSync = false;
                return;
            }

            BlackLeaveTime = ProtocolUtil.CalcTotalVoteLeaveTime(
                state, startTime, totalSpan);
            BlackUsedTime = ProtocolUtil.CalcThinkTime(
                state, startTime, progressSpan);
            BlackVoteState = state;
            IsBlackAutoSync = true;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainViewModel()
        {
            Turn = BWType.Black;

            BlackLeaveTime = new TimeSpan(2, 0, 0);
            BlackUsedTime = TimeSpan.Zero;
            BlackVoteState = VoteState.Stop;
            IsBlackAutoSync = false;

            WhiteLeaveTime = new TimeSpan(2, 0, 0);
            WhiteUsedTime = TimeSpan.Zero;
            WhiteAddTime = TimeSpan.Zero;

            AddPropertyChangedHandler(
                "MoveCount",
                (_, __) => OnMoveCountChanged());

            StartToObserveSyncFile();

            this.leaveTimeTimer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(1000.0 / 60),
                DispatcherPriority.Normal,
                OnLeaveTimeTimer,
                WPFUtil.UIDispatcher);
        }
    }
}
