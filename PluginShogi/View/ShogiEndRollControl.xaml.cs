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
    /// 時間管理をするためのクラスです。
    /// </summary>
    public sealed class TimelineData
    {
        /// <summary>
        /// フェードインが始まる時間を取得または設定します。
        /// </summary>
        public TimeSpan FadeInStartTime
        {
            get;
            set;
        }

        /// <summary>
        /// フェードインを行う時間を取得または設定します。
        /// </summary>
        public TimeSpan FadeInSpan
        {
            get;
            set;
        }

        /// <summary>
        /// フェードインが終わる時間を取得します。
        /// </summary>
        public TimeSpan FadeInEndTime
        {
            get { return (FadeInStartTime + FadeInSpan); }
        }

        /// <summary>
        /// フェードアウトが始まる時間を取得または設定します。
        /// </summary>
        public TimeSpan FadeOutStartTime
        {
            get;
            set;
        }

        /// <summary>
        /// フェードアウトが終わる時間を取得または設定します。
        /// </summary>
        public TimeSpan FadeOutSpan
        {
            get;
            set;
        }

        /// <summary>
        /// フェードアウトが終わる時間を取得または設定します。
        /// </summary>
        public TimeSpan FadeOutEndTime
        {
            get { return (FadeOutStartTime + FadeOutSpan); }
        }

        /// <summary>
        /// フェードイン終了後からフェードアウト開始までの時間を取得します。
        /// </summary>
        public TimeSpan VisibleSpan
        {
            get { return (FadeOutStartTime - FadeInEndTime); }
        }

        /// <summary>
        /// フェードイン開始からフェードアウト終了後までの時間を取得します。
        /// </summary>
        public TimeSpan FullVisibleSpan
        {
            get { return (FadeOutEndTime - FadeInStartTime); }
        }

        /// <summary>
        /// フェードイン・フェードアウトなどの、進行度を取得します。
        /// </summary>
        public double GetRatio(TimeSpan position)
        {
            if (position < FadeInStartTime)
            {
                // フェードイン前なら進行度は０
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
                // 表示中は進行度は１
                return 1.0;
            }
            else if (position < FadeOutEndTime)
            {
                var current = FadeOutEndTime - position;
                var r = current.TotalSeconds / FadeOutSpan.TotalSeconds;

                return MathEx.InterpLiner(0.0, 1.0, r);
            }

            // フェードアウト後なら進行度は０
            return 0.0;
        }
    }

    /// <summary>
    /// エンドロールを流すウィンドウです。
    /// </summary>
    public partial class ShogiEndRollControl : Window
    {
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

            EndRollTime = new TimelineData
            {
                FadeInStartTime = TimeSpanFrom(0, 27),
                FadeInSpan = TimeSpan.Zero,
                FadeOutStartTime = TimeSpanFrom(5, 30),
                FadeOutSpan = TimeSpan.Zero,
            };

            ShogiTimeline = new TimelineData
            {
                FadeInStartTime = MovieTimeline.FadeInEndTime,
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
            EndRoll.Stop();

            if (this.timer != null)
            {
                this.timer.Stop();
                this.timer = null;
            }

            if (this.oneTimer != null)
            {
                this.oneTimer.Dispose();
                this.oneTimer = null;
            }

            if (this.player != null)
            {
                this.player.Stop();
                this.player = null;
            }

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
            if (this.player == null)
            {
                return;
            }

            this.player.Play();
            //this.player.Position = TimeSpan.FromSeconds(280);

            // エンディングの前に現局面を設定します。
            var board = new Board();
            var moveList = BoardExtension.MakeMoveList(SampleMove.Tsume);
            var bmList = board.ConvertMove(moveList);

            var interval = ShogiTimeline.VisibleSpan - TimeSpan.FromSeconds(3);
            var count = bmList.Count() + 2;
            this.autoPlay = new ViewModel.AutoPlayEx(board, bmList)
            {
                EffectManager = this.effectManager,
                IsChangeMoveCount = true,
                Interval = TimeSpan.FromSeconds(interval.TotalSeconds / count),
            };
        }

        private void UpdatePosition(TimeSpan position)
        {
            var elapsed = position - this.prevPosition;

            if (position > EndRollTime.FadeInStartTime)
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

            ShogiControl.Render(elapsed);
            ShogiBackground.Render(elapsed);
            
            ShogiGrid.Opacity = ShogiTimeline.GetRatio(position) * 0.40;
            MovieBrush.Opacity = MovieTimeline.GetRatio(position);

            /*if (position > MovieTimeline.FadeInEndTime)
            {
                var total = MovieTimeline.FadeOutStartTime - MovieTimeline.FadeInEndTime - TimeSpan.FromSeconds(20);
                var basePos = position - MovieTimeline.FadeInEndTime;
                var ratio = basePos.TotalSeconds / total.TotalSeconds;

                // 800x4800 = 1:8
                //ImageBrush.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
                //ImageBrush.Viewport = new Rect(0, 1.0 - ratio, 1.0, 1.0);
                ImageBrush.ViewboxUnits = BrushMappingMode.RelativeToBoundingBox;
                ImageBrush.Viewbox = new Rect(0, 1.0 - ratio*(1.0 + 1.0/6), 1.0, 1.0/6);
                ImageBrush.Opacity = 0.4;
            }*/

            this.prevPosition = position;
        }
    }
}
