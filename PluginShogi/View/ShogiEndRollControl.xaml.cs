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

namespace VoteSystem.PluginShogi.View
{
    using Protocol.View;
    using Effects;
    using ViewModel;

    /// <summary>
    /// エンドロールを流すウィンドウです。
    /// </summary>
    public partial class ShogiEndRollControl : Window
    {
        public sealed class TimelineData
        {
            public TimeSpan FadeInStartTime
            {
                get;
                set;
            }

            public TimeSpan FadeInSpan
            {
                get;
                set;
            }

            public TimeSpan FadeInEndTime
            {
                get
                {
                    return (FadeInStartTime + FadeInSpan);
                }
            }

            public TimeSpan FadeOutStartTime
            {
                get;
                set;
            }

            public TimeSpan FadeOutSpan
            {
                get;
                set;
            }

            public TimeSpan FadeOutEndTime
            {
                get
                {
                    return (FadeOutStartTime + FadeOutSpan);
                }
            }

            public double GetRatio(TimeSpan position)
            {
                if (position < FadeInStartTime)
                {
                    return 0.0;
                }
                else if (position < FadeInEndTime)
                {
                    var current = FadeInEndTime - position;
                    var r = current.TotalSeconds / FadeInSpan.TotalSeconds;

                    return MathEx.InterpLiner(1.0, 0.0, r);
                }
                else if (position < FadeOutStartTime)
                {
                    return 1.0;
                }
                else if (position < FadeOutEndTime)
                {
                    var current = FadeOutEndTime - position;
                    var r = current.TotalSeconds / FadeOutSpan.TotalSeconds;

                    return MathEx.InterpLiner(0.0, 1.0, r);
                }

                return 0.0;
            }
        }

        private EffectManager effectManager;
        private DispatcherTimer timer;
        private MediaPlayer player;
        private AutoPlayEx autoPlay;
        private TimeSpan prevPosition = TimeSpan.Zero;

        public TimelineData MovieTimeline
        {
            get;
            set;
        }

        public TimeSpan EndRollStartTime
        {
            get;
            set;
        }

        public TimeSpan EndRollEndTime
        {
            get;
            set;
        }

        public TimeSpan EndRollSpan
        {
            get
            {
                return (EndRollEndTime - EndRollStartTime);
            }
        }

        public TimelineData ShogiTimeline
        {
            get;
            set;
        }

        private static TimeSpan TimeSpanFrom(double seconds)
        {
            return TimeSpan.FromSeconds(seconds);
        }

        private static TimeSpan TimeSpanFrom(double minutes, double seconds)
        {
            return TimeSpan.FromSeconds(minutes * 60 + seconds);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ShogiEndRollControl()
        {
            InitializeComponent();
            EndRoll.InitializeBindings(this);
            ShogiControl.InitializeBindings(this);

            MovieTimeline = new TimelineData
            {
                FadeInStartTime = TimeSpanFrom(0, 13),
                FadeInSpan = TimeSpanFrom(10),
                FadeOutStartTime = TimeSpanFrom(5, 30),
                FadeOutSpan = TimeSpanFrom(10),
            };

            EndRollStartTime = TimeSpanFrom(0, 27);
            EndRollEndTime = TimeSpanFrom(5, 30);

            ShogiTimeline = new TimelineData
            {
                FadeInStartTime = TimeSpanFrom(0, 20),
                FadeInSpan = TimeSpanFrom(5),
                FadeOutStartTime = TimeSpanFrom(5, 0),
                FadeOutSpan = TimeSpanFrom(10),
            };

            this.player = new MediaPlayer
            {
                Volume = 0.1,
            };
            this.player.MediaOpened += MediaOpened;
            //this.player.Open(new Uri(@"E:\movies\ending\alice2.avi"));
            this.player.Open(new Uri(@"E:\movies\ending\ending2\alice_demo.mp3"));

            /*MovieBrush.Drawing = new VideoDrawing
            {
                Player = this.player,
                Rect = new Rect(0, 0, 100, 100),
            };*/

            EndRoll.FormatFilePath = @"ShogiData/EndRoll/endroll_format.xml";
            EndRoll.DataGetter = Protocol.Model.TestVoterList.GetTestVoterList;

            this.effectManager = new EffectManager
            {
                Background = ShogiBackground,
                EffectEnabled = false,
                EffectMoveCount = 0,
            };
            ShogiControl.EffectManager = this.effectManager;
            //ShogiControl.Board = new Board();
            //ShogiControl.Effect = new Ragnarok.Presentation.Effect.GrayscaleEffect();

            this.effectManager.ChangeMoveCount(1);

            DataContext = ShogiGlobal.ShogiModel;

            this.timer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(50),
                DispatcherPriority.Normal,
                (_, __) => UpdatePosition(this.player.Position),
                Dispatcher);
            this.timer.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (this.timer != null)
            {
                this.timer.Stop();
                this.timer = null;
            }

            if (this.player != null)
            {
                this.player.Stop();
                this.player = null;
            }

            EndRoll.Stop();

            base.OnClosed(e);
        }

        private System.Threading.Timer oneTimer;

        private void MediaOpened(object sender, EventArgs e)
        {
            this.oneTimer = new System.Threading.Timer(
                _ => Ragnarok.Presentation.WPFUtil.UIProcess(Play),
                null,
                20 * 1000, -1);
            //Play();
        }

        public void Play()
        {
            this.player.Play();
            //this.player.Position = TimeSpan.FromSeconds(280);

            // エンディングの前に現局面を設定します。
            var board = new Board();
            var moveList = BoardExtension.MakeMoveList(SampleMove.Tsume);
            var bmList = board.ConvertMove(moveList);

            var interval = ShogiTimeline.FadeOutStartTime - ShogiTimeline.FadeInEndTime
                - TimeSpan.FromSeconds(3);
            this.autoPlay = new ViewModel.AutoPlayEx(board, bmList)
            {
                Interval = TimeSpan.FromSeconds(interval.TotalSeconds / (bmList.Count() + 2)),
            };
        }

        private void UpdatePosition(TimeSpan position)
        {
            var elapsed = position - this.prevPosition;

            if (position > EndRollStartTime)
            {
                if (EndRoll.State == EndRollState.Stop)
                {
                    EndRoll.RollTimeSeconds = (int)Math.Ceiling(EndRollSpan.TotalSeconds);
                    EndRoll.Play();
                }

                EndRoll.UpdateScreen(position - EndRollStartTime);
            }

            if (this.autoPlay != null &&
                position > ShogiTimeline.FadeInEndTime + TimeSpan.FromSeconds(3))
            {
                ShogiControl.StartAutoPlay(this.autoPlay);
                this.autoPlay = null;
            }

            ShogiControl.Render(elapsed);
            ShogiBackground.Render(elapsed);
            
            ShogiGrid.Opacity = ShogiTimeline.GetRatio(position) * 0.40;
            MovieBrush.Opacity = MovieTimeline.GetRatio(position);

            if (position > MovieTimeline.FadeInEndTime)
            {
                var total = MovieTimeline.FadeOutStartTime - MovieTimeline.FadeInEndTime - TimeSpan.FromSeconds(20);
                var basePos = position - MovieTimeline.FadeInEndTime;
                var ratio = basePos.TotalSeconds / total.TotalSeconds;

                // 800x4800 = 1:8
                //ImageBrush.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
                //ImageBrush.Viewport = new Rect(0, 1.0 - ratio, 1.0, 1.0);
                /*ImageBrush.ViewboxUnits = BrushMappingMode.RelativeToBoundingBox;
                ImageBrush.Viewbox = new Rect(0, 1.0 - ratio*(1.0 + 1.0/6), 1.0, 1.0/6);
                ImageBrush.Opacity = 0.4;*/
            }

            this.prevPosition = position;
        }
    }
}
