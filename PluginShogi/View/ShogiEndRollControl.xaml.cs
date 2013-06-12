using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
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

        /// <summary>
        /// フォーマットファイルのパスを扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty FormatFilePathProperty =
            DependencyProperty.Register(
                "FormatFilePath", typeof(string), typeof(ShogiEndRollControl),
                new FrameworkPropertyMetadata(string.Empty,
                    OnFormatFilePathChanged));

        /// <summary>
        /// フォーマットファイルのパスを取得または設定します。
        /// </summary>
        public string FormatFilePath
        {
            get { return (string)GetValue(FormatFilePathProperty); }
            set { SetValue(FormatFilePathProperty, value); }
        }

        private static void OnFormatFilePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (ShogiEndRollControl)d;
            var path = (string)e.NewValue;

            self.LoadFormat(path);

            if (self.EndRoll != null)
            {
                self.EndRoll.FormatFilePath = path;
            }
        }

        /// <summary>
        /// 動画のダウンロード先URLを扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty MovieUrlProperty =
            DependencyProperty.Register(
                "MovieUrl", typeof(Uri), typeof(ShogiEndRollControl),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// 動画のダウンロード先URLを取得または設定します。
        /// </summary>
        public Uri MovieUrl
        {
            get { return (Uri)GetValue(MovieUrlProperty); }
            set { SetValue(MovieUrlProperty, value); }
        }

        /// <summary>
        /// 映像の表示タイミングを扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty MovieTimelineProperty =
            DependencyProperty.Register(
                "MovieTimeline", typeof(TimelineData), typeof(ShogiEndRollControl),
                new FrameworkPropertyMetadata(new TimelineData()));

        /// <summary>
        /// 映像の表示タイミングを取得または設定します。
        /// </summary>
        /// <remarks>
        /// 映像は音声の再生後に表示されます。
        /// </remarks>
        public TimelineData MovieTimeline
        {
            get { return (TimelineData)GetValue(MovieTimelineProperty); }
            set { SetValue(MovieTimelineProperty, value); }
        }

        /// <summary>
        /// スタッフ一覧などの表示タイミングを扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty EndRollTimelineProperty =
            DependencyProperty.Register(
                "EndRollTimeline", typeof(TimelineData), typeof(ShogiEndRollControl),
                new FrameworkPropertyMetadata(new TimelineData()));

        /// <summary>
        /// スタッフ一覧などの表示タイミングを取得または設定します。
        /// </summary>
        public TimelineData EndRollTimeline
        {
            get { return (TimelineData)GetValue(EndRollTimelineProperty); }
            set { SetValue(EndRollTimelineProperty, value); }
        }

        /// <summary>
        /// 将棋盤の表示タイミングを扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty ShogiTimelineProperty =
            DependencyProperty.Register(
                "ShogiTimeline", typeof(TimelineData), typeof(ShogiEndRollControl),
                new FrameworkPropertyMetadata(new TimelineData()));

        /// <summary>
        /// 将棋盤の表示タイミングを取得または設定します。
        /// </summary>
        public TimelineData ShogiTimeline
        {
            get { return (TimelineData)GetValue(ShogiTimelineProperty); }
            set { SetValue(ShogiTimelineProperty, value); }
        }

        /// <summary>
        /// 自分で使う用
        /// </summary>
        private MediaPlayer MoviePlayer
        {
            get { return Ending.MoviePlayer; }
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

            MovieBrush.Drawing = new VideoDrawing
            {
                Player = MoviePlayer,
                Rect = new Rect(0, 0, 16, 9),
            };

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

            // フォーマットファイル設定
            FormatFilePath = @"ShogiData/EndRoll/endroll_format.xml";

            if (!Ragnarok.Presentation.WPFUtil.IsInDesignMode)
            {
                this.timer = new DispatcherTimer(
                    TimeSpan.FromMilliseconds(50),
                    DispatcherPriority.Normal,
                    (_, __) => Update(),
                    Dispatcher);
                this.timer.Start();
            }
        }

        private void OnUnloaded()
        {
            EndRoll.Stop();

            if (MoviePlayer != null)
            {
                MoviePlayer.Stop();
            }

            if (this.timer != null)
            {
                this.timer.Stop();
                this.timer = null;
            }
        }

        /// <summary>
        /// xmlファイルを読み込みます。
        /// </summary>
        private void LoadFormat(string filepath)
        {
            if (string.IsNullOrEmpty(filepath) ||
                !System.IO.File.Exists(filepath))
            {
                Log.Error(
                    "{0}: ファイルが存在しません。",
                    filepath);
                return;
            }

            var doc = XElement.Load(filepath, LoadOptions.SetLineInfo);

            // 動画URLはフォーマットファイルに書かれています。
            var attr = doc.Attribute("MovieUrl");
            if (attr != null)
            {
                MovieUrl = new Uri(attr.Value);
            }

            foreach (var elem in doc.Elements())
            {
                var name = elem.Name.LocalName;

                if (name == "MovieTimeline")
                {
                    MovieTimeline = TimelineData.Create(elem);
                }
                else if (name == "EndRollTimeline")
                {
                    EndRollTimeline = TimelineData.Create(elem);
                }
                else if (name == "ShogiTimeline")
                {
                    ShogiTimeline = TimelineData.Create(elem);
                }
            }
        }

        /// <summary>
        /// 動画の再生準備を開始します。
        /// </summary>
        public void StartPrepare(DateTime startTimeNtp)
        {
            Ending.StartPrepare(MovieUrl, startTimeNtp);
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
            Ending.PlayMovie();
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
            var elapsed = MathEx.Max(TimeSpan.Zero, position - this.prevPosition);
            this.prevPosition = position;

            if (elapsed == TimeSpan.Zero)
            {
                return;
            }

            if (position > EndRollTimeline.FadeInStartTime &&
                position < EndRollTimeline.FadeOutEndTime)
            {
                if (EndRoll.State == EndRollState.Stop)
                {
                    var span = EndRollTimeline.VisibleSpan;

                    EndRoll.RollTimeSeconds = (int)Math.Ceiling(span.TotalSeconds);
                    EndRoll.Play();
                }

                EndRoll.UpdateScreen(position - EndRollTimeline.FadeInStartTime);
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
