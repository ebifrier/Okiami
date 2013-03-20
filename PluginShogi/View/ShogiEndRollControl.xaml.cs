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

        private DispatcherTimer timer;
        private MediaPlayer player;
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
            EndRoll.InitializeCommands(CommandBindings);

            MovieTimeline = new TimelineData
            {
                FadeInStartTime = TimeSpanFrom(0, 10),
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
                FadeOutSpan = TimeSpanFrom(5),
            };

            this.player = new MediaPlayer
            {
                Volume = 0.1,
            };
            this.player.MediaOpened += MediaOpened;
            this.player.Open(new Uri(@"E:\movies\ending\alice2.avi"));

            MovieBrush.Drawing = new VideoDrawing
            {
                Player = this.player,
                Rect = new Rect(0, 0, 100, 100),
            };

            EndRoll.FormatFilePath = @"ShogiData/EndRoll/endroll_format.xml";
            EndRoll.DataGetter = Protocol.Model.TestVoterList.GetTestVoterList;

            //ShogiControl.Board = new Board();
            //ShogiControl.Effect = new Ragnarok.Presentation.Effect.GrayscaleEffect();

            // 背景エフェクトの作成。
            var effectInfo = new Effects.EffectInfo("SpringEffect", null);
            var effect = effectInfo.LoadBackground();
            ShogiBackground.AddEntity(effect);

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

        private void MediaOpened(object sender, EventArgs e)
        {
            Play();
        }

        public void Play()
        {
            this.player.Play();
            //this.player.Position = TimeSpan.FromSeconds(200);
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

            ShogiControl.Render(elapsed);
            ShogiBackground.Render(elapsed);
            
            ShogiGrid.Opacity = ShogiTimeline.GetRatio(position) * 0.37;
            MovieBrush.Opacity = MovieTimeline.GetRatio(position);

            this.prevPosition = position;
        }
    }
}
