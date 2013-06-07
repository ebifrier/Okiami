using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

using Ragnarok;
using Ragnarok.Shogi;
using Ragnarok.Utility;
using Ragnarok.Presentation.Shogi;

namespace VoteSystem.PluginShogi.View
{
    using Protocol.View;
    using Effects;
    using Model;
    using ViewModel;

    /// <summary>
    /// エンドロールを流すためのコントロールです。
    /// </summary>
    /// <remarks>
    /// 動画のダウンロードや動画再生までの残り時間表示機能
    /// などがついています。
    /// </remarks>
    public partial class ShogiEndRollControl : UserControl
    {
        private EffectManager effectManager;
        private DispatcherTimer timer;
        private AutoPlayEx autoPlay;
        private TimeSpan prevPosition = TimeSpan.Zero;

        private static TimeSpan TimeSpanFrom(double seconds)
        {
            return TimeSpan.FromSeconds(seconds);
        }

        private static TimeSpan TimeSpanFrom(double minutes, double seconds)
        {
            return TimeSpan.FromSeconds(minutes * 60 + seconds);
        }

        /// <summary>
        /// 投票者リストを更新します。
        /// </summary>
        public static object GetVoterList()
        {
            try
            {
                if (ShogiGlobal.VoteClient == null)
                {
                    return null;
                }

                return new EndRollViewModel(
                    ShogiGlobal.VoteClient.GetVoterList());
                    //Protocol.Model.TestVoterList.GetTestVoterList());
            }
            catch (Exception ex)
            {
                ShogiGlobal.ErrorMessage(ex,
                    "参加者リストの取得に失敗しました。(-A-;)");

                return null;
            }
        }

        public MediaPlayer MoviePlayer
        {
            get { return Ending.MoviePlayer; }
        }

        public TimelineData MovieTimeline
        {
            get;
            set;
        }

        public TimelineData EndRollTime
        {
            get;
            set;
        }

        public TimelineData ShogiTimeline
        {
            get;
            set;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ShogiEndRollControl()
        {
            InitializeComponent();
            EndRoll.InitializeBindings(this);
            ShogiControl.InitializeBindings(this);

            /*MovieTimeline = new TimelineData
            {
                FadeInStartTime = TimeSpanFrom(0, 10),
                FadeInSpan = TimeSpanFrom(10),
                FadeOutStartTime = TimeSpanFrom(6, 26),
                FadeOutSpan = TimeSpanFrom(10),
            };

            EndRollTime = new TimelineData
            {
                FadeInStartTime = TimeSpanFrom(0, 26),
                FadeInSpan = TimeSpan.Zero,
                FadeOutStartTime = TimeSpanFrom(5, 42),
                FadeOutSpan = TimeSpan.Zero,
            };

            ShogiTimeline = new TimelineData
            {
                FadeInStartTime = MovieTimeline.FadeInEndTime,
                FadeInSpan = TimeSpanFrom(5),
                FadeOutStartTime = TimeSpanFrom(5, 0),
                FadeOutSpan = TimeSpanFrom(10),
            };*/

            MovieTimeline = new TimelineData
            {
                FadeInStartTime = TimeSpanFrom(0, 5),
                FadeInSpan = TimeSpanFrom(5),
                FadeOutStartTime = TimeSpanFrom(1, 28),
                FadeOutSpan = TimeSpanFrom(5),
            };

            EndRollTime = new TimelineData
            {
                FadeInStartTime = TimeSpanFrom(0, 10),
                FadeInSpan = TimeSpan.Zero,
                FadeOutStartTime = TimeSpanFrom(1, 32),
                FadeOutSpan = TimeSpan.Zero,
            };

            ShogiTimeline = new TimelineData
            {
                FadeInStartTime = TimeSpanFrom(0, 0),
                FadeInSpan = TimeSpanFrom(0),
                FadeOutStartTime = TimeSpanFrom(0, 0),
                FadeOutSpan = TimeSpanFrom(0),
            };

            MovieBrush.Drawing = new VideoDrawing
            {
                Player = MoviePlayer,
                Rect = new Rect(0, 0, 16, 9),
            };

            EndRoll.FormatFilePath = @"ShogiData/EndRoll/endroll_format.xml";
            EndRoll.DataGetter = GetVoterList;

            // エフェクト表示用のオブジェクト
            this.effectManager = new EffectManager
            {
                Background = ShogiBackground,
                EffectEnabled = false,
                EffectMoveCount = 0,
            };
            this.effectManager.ChangeMoveCount(1);
            ShogiControl.EffectManager = this.effectManager;

            Unloaded += (_, __) => OnUnloaded();
            DataContext = ShogiGlobal.ShogiModel;

            this.timer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(50),
                DispatcherPriority.Normal,
                (_, __) => Update(),
                Dispatcher);
            this.timer.Start();
        }

        private void OnUnloaded()
        {
            EndRoll.Stop();

            if (this.timer != null)
            {
                this.timer.Stop();
                this.timer = null;
            }
        }

        /// <summary>
        /// 動画の再生準備を開始します。
        /// </summary>
        public void StartPrepare(Uri movieUri, DateTime startTimeNtp)
        {
            Ending.StartPrepare(movieUri, startTimeNtp);
        }

        /// <summary>
        /// 定期的に呼ばれます。
        /// </summary>
        private void Update()
        {
            if (Ending.State == EndingState.Idle)
            {
                // 再生中または待機時
                UpdatePosition(MoviePlayer.Position);
                return;
            }
            else if (Ending.State == EndingState.Completed)
            {
                var now = Ragnarok.Net.NtpClient.GetTime();

                // 時刻はNTPで比較します。
                if (now >= Ending.StartTimeNtp)
                {
                    Play();
                }
            }
        }

        /// <summary>
        /// エンディングの再生を開始します。
        /// </summary>
        public void Play()
        {
            Ending.MoviePlayed();
            MoviePlayer.Play();
            //MoviePlayer.Position = TimeSpan.FromSeconds(300);

            // エンディングの前に現局面を設定します。
            var board = ShogiGlobal.ShogiModel.CurrentBoard.Clone();
            board.UndoAll();

            /*var board = new Board();
            var moveList = BoardExtension.MakeMoveList(SampleMove.Tsume);
            var bmList = board.ConvertMove(moveList);*/

            var interval = ShogiTimeline.VisibleSpan - TimeSpan.FromSeconds(3);
            var count = board.CanRedoCount + 2;
            this.autoPlay = new ViewModel.AutoPlayEx(board, AutoPlayType.Redo)
            {
                EffectManager = this.effectManager,
                IsChangeMoveCount = true,
                Interval = TimeSpan.FromSeconds(interval.TotalSeconds / count),
            };
        }

        private void UpdatePosition(TimeSpan position)
        {
            var elapsed = position - this.prevPosition;
            this.prevPosition = position;

            if (elapsed == TimeSpan.Zero)
            {
                return;
            }

            if (position > EndRollTime.FadeInStartTime &&
                position < EndRollTime.FadeOutEndTime)
            {
                if (EndRoll.State == EndRollState.Stop)
                {
                    var span = EndRollTime.VisibleSpan;

                    EndRoll.RollTimeSeconds = (int)Math.Ceiling(span.TotalSeconds);
                    EndRoll.Play();
                }

                EndRoll.UpdateScreen(position - EndRollTime.FadeInStartTime);
            }

            if (this.autoPlay != null &&
                position > ShogiTimeline.FadeInEndTime + TimeSpan.FromSeconds(3))
            {
                ShogiControl.StartAutoPlay(this.autoPlay);
                this.autoPlay = null;
            }

            if (elapsed < TimeSpan.FromSeconds(10))
            {
                ShogiControl.Render(elapsed);
                ShogiBackground.Render(elapsed);
            }
            
            ShogiGrid.Opacity = ShogiTimeline.GetRatio(position) * 0.40;
            MovieBrush.Opacity = MovieTimeline.GetRatio(position);
        }
    }
}
