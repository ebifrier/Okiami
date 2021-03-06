﻿using System;
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
using Ragnarok.Presentation;
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
        private AutoPlayEx autoPlay;
        private EndRollViewModel endRollData;
        private TimeSpan prevPosition = TimeSpan.Zero;
        private TimeSpan interval = TimeSpan.Zero;
        private ReentrancyLock positionLock = new ReentrancyLock();

        #region 基本プロパティ
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
        /// テスト用の参加者を表示するかどうかを扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty IsTestProperty =
            DependencyProperty.Register(
                "IsTest", typeof(bool), typeof(ShogiEndRollControl),
                new FrameworkPropertyMetadata(false));

        /// <summary>
        /// テスト用の参加者を表示するかどうかを取得または設定します。
        /// </summary>
        public bool IsTest
        {
            get { return (bool)GetValue(IsTestProperty); }
            set { SetValue(IsTestProperty, value); }
        }

        /// <summary>
        /// 映像品質を扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty EndRollQualityProperty =
            DependencyProperty.Register(
                "EndRollQuality", typeof(EndRollQuality), typeof(ShogiEndRollControl),
                new FrameworkPropertyMetadata(EndRollQuality.Poor, OnEndRollQualityChanged));

        /// <summary>
        /// 映像品質を取得または設定します。
        /// </summary>
        public EndRollQuality EndRollQuality
        {
            get { return (EndRollQuality)GetValue(EndRollQualityProperty); }
            set { SetValue(EndRollQualityProperty, value); }
        }

        private static void OnEndRollQualityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (ShogiEndRollControl)d;
            var quality = (EndRollQuality)e.NewValue;

            // 映像の更新間隔を設定します。
            self.interval = TimeSpan.FromSeconds(1.0 / EndRollQualityUtil.GetFPS(quality));
        }

        /// <summary>
        /// 最大表示視聴者数を扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty MaximumDisplayListenersProperty =
            DependencyProperty.Register(
                "MaximumDisplayListeners", typeof(int), typeof(ShogiEndRollControl),
                new FrameworkPropertyMetadata(300));

        /// <summary>
        /// 最大表示視聴者数を取得または設定します。
        /// </summary>
        public int MaximumDisplayListeners
        {
            get { return (int)GetValue(MaximumDisplayListenersProperty); }
            set { SetValue(MaximumDisplayListenersProperty, value); }
        }

        /// <summary>
        /// 最大表示生主数を扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty MaximumDisplayLiveOwnersProperty =
            DependencyProperty.Register(
                "MaximumDisplayLiveOwners", typeof(int), typeof(ShogiEndRollControl),
                new FrameworkPropertyMetadata(30));

        /// <summary>
        /// 最大表示生主数を取得または設定します。
        /// </summary>
        public int MaximumDisplayLiveOwners
        {
            get { return (int)GetValue(MaximumDisplayLiveOwnersProperty); }
            set { SetValue(MaximumDisplayLiveOwnersProperty, value); }
        }

        /// <summary>
        /// 将棋盤の不透明度を扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty ShogiOpacityProperty =
            DependencyProperty.Register(
                "ShogiOpacity", typeof(double), typeof(ShogiEndRollControl),
                new FrameworkPropertyMetadata(0.3));

        /// <summary>
        /// 将棋盤の不透明度を取得または設定します。
        /// </summary>
        public double ShogiOpacity
        {
            get { return (double)GetValue(ShogiOpacityProperty); }
            set { SetValue(ShogiOpacityProperty, value); }
        }
        #endregion

        #region 動画
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
        /// 動画の拡張子(.付き)を扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty MovieExtProperty =
            DependencyProperty.Register(
                "MovieExt", typeof(string), typeof(ShogiEndRollControl),
                new FrameworkPropertyMetadata(string.Empty));

        /// <summary>
        /// 動画の拡張子(.付き)を取得または設定します。
        /// </summary>
        public string MovieExt
        {
            get { return (string)GetValue(MovieExtProperty); }
            set { SetValue(MovieExtProperty, value); }
        }

        /// <summary>
        /// 動画の長さを扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty MovieDurationProperty =
            DependencyProperty.Register(
                "MovieDuration", typeof(TimeSpan), typeof(ShogiEndRollControl),
                new FrameworkPropertyMetadata(TimeSpan.Zero));

        /// <summary>
        /// 動画の長さを取得または設定します。
        /// </summary>
        public TimeSpan MovieDuration
        {
            get { return (TimeSpan)GetValue(MovieDurationProperty); }
            private set { SetValue(MovieDurationProperty, value); }
        }

        /// <summary>
        /// 動画の再生位置を扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(
                "Square", typeof(TimeSpan), typeof(ShogiEndRollControl),
                new FrameworkPropertyMetadata(TimeSpan.Zero, OnPositionChanged));

        /// <summary>
        /// 動画の再生位置を取得または設定します。
        /// </summary>
        public TimeSpan Position
        {
            get { return (TimeSpan)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (ShogiEndRollControl)d;
            var position = (TimeSpan)e.NewValue;

            self.PositionSeconds = position.TotalSeconds;
            self.ResetPosition(position);
        }

        /// <summary>
        /// 動画の再生位置を扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty PositionSecondsProperty =
            DependencyProperty.Register(
                "PositionSeconds", typeof(double), typeof(ShogiEndRollControl),
                new FrameworkPropertyMetadata(0.0, OnPositionDoubleChanged));

        /// <summary>
        /// 動画の再生位置を取得または設定します。
        /// </summary>
        public double PositionSeconds
        {
            get { return (double)GetValue(PositionSecondsProperty); }
            set { SetValue(PositionSecondsProperty, value); }
        }

        private static void OnPositionDoubleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (ShogiEndRollControl)d;
            var seconds = (double)e.NewValue;

            self.Position = TimeSpan.FromSeconds(seconds);
        }
        #endregion

        #region タイムラインプロパティ
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
        #endregion

        #region サウンドプロパティ
        /// <summary>
        /// 動画がミュートかどうかを扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty IsMovieMuteProperty =
            DependencyProperty.Register(
                "IsMovieMute", typeof(bool), typeof(ShogiEndRollControl),
                new FrameworkPropertyMetadata(true,
                    OnIsMovieMuteChanged));

        /// <summary>
        /// 動画がミュートかどうかを取得または設定します。
        /// </summary>
        public bool IsMovieMute
        {
            get { return (bool)GetValue(IsMovieMuteProperty); }
            set { SetValue(IsMovieMuteProperty, value); }
        }

        private static void OnIsMovieMuteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (ShogiEndRollControl)d;

            self.Ending.MoviePlayer.IsMuted = (bool)e.NewValue;
        }

        /// <summary>
        /// 動画の音量を扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty MovieVolumeProperty =
            DependencyProperty.Register(
                "MovieVolume", typeof(int), typeof(ShogiEndRollControl),
                new FrameworkPropertyMetadata(0,
                    OnMovieVolumeChanged));

        /// <summary>
        /// 動画の音量を取得または設定します。
        /// </summary>
        public int MovieVolume
        {
            get { return (int)GetValue(MovieVolumeProperty); }
            set { SetValue(MovieVolumeProperty, value); }
        }

        private static void OnMovieVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (ShogiEndRollControl)d;

            self.Ending.MoviePlayer.Volume =
                MathEx.Between(0, 100, (int)e.NewValue) / 100.0;
        }
        #endregion

        /// <summary>
        /// 投票者リストを更新します。
        /// </summary>
        public EndRollViewModel GetVoterList()
        {
            try
            {
                if (ShogiGlobal.VoteClient == null)
                {
                    return null;
                }

                return new EndRollViewModel(
                    IsTest ?
                    Protocol.Model.TestVoterList.GetTestVoterList() :
                    ShogiGlobal.VoteClient.GetVoterList());
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

            MovieBrush.Drawing = new VideoDrawing
            {
                Player = Ending.MoviePlayer,
                Rect = new Rect(0, 0, 16, 9),
            };

            // エフェクト表示用のオブジェクト
            this.effectManager = new EffectManager
            {
                Background = ShogiBackground,
                EffectEnabled = false,
                EffectMoveCount = 0,
            };
            this.effectManager.ChangeMoveCount(1);

            // EndRoll.DataGetter = GetVoterList;
            ShogiControl.EffectManager = this.effectManager;

            Ending.MoviePlayer.MediaOpened += MoviePlayer_MediaOpened;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            DataContext = ShogiGlobal.ShogiModel;

            // プロパティ設定
            FormatFilePath = @"Plugin/ShogiData/EndRoll/endroll_format.xml";
            IsMovieMute = false;
            MovieVolume = 50;
            EndRollQuality = EndRollQuality.Best;
        }

        void MoviePlayer_MediaOpened(object sender, EventArgs e)
        {
            try
            {
                var player = (MediaPlayer)sender;
                var duration = player.NaturalDuration;
                var span =
                    duration.HasTimeSpan ?
                    duration.TimeSpan : TimeSpan.Zero;

                WPFUtil.UIProcess(() =>
                    MovieDuration = span);
            }
            catch (Exception ex)
            {
                Util.ThrowIfFatal(ex);
                Log.ErrorException(ex,
                    "動画の長さの取得に失敗しました。");
            }
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            var nowNtp = Ragnarok.Net.NtpClient.GetTime();

            // 動画再生がまだ始まっていなければ、準備を開始します。
            if (nowNtp < ShogiGlobal.StartEndrollTimeNtp)
            {
                StartPrepare(ShogiGlobal.StartEndrollTimeNtp);
            }

            if (!WPFUtil.IsInDesignMode)
            {
                CompositionTarget.Rendering += (_, __) => Update();
            }
        }

        private void OnUnloaded(object sender, EventArgs e)
        {
            CompositionTarget.Rendering -= (_, __) => Update();

            Stop();
            ShogiControl.Unload();
            ShogiBackground.Unload();
        }

        /// <summary>
        /// xmlファイルを読み込みます。
        /// </summary>
        private void LoadFormat(string filepath)
        {
            try
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

                // 動画の拡張子はURLに指定されているものと違う可能性があるため、
                // 別に指定できるようにしています。
                attr = doc.Attribute("MovieExt");
                MovieExt = (attr != null ? attr.Value : string.Empty);

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
            catch (Exception ex)
            {
                Util.ThrowIfFatal(ex);
                Log.ErrorException(ex,
                    "エンディングのフォーマットファイルの解析に失敗しました。");
            }
        }

        /// <summary>
        /// 動画の再生準備を開始します。
        /// </summary>
        public void StartPrepare(DateTime startTimeNtp)
        {
            if (MovieUrl == null)
            {
                ShogiGlobal.ErrorMessage(
                    "動画ファイルのURLがありませんorz");
                return;
            }

            Ending.StartPrepare(MovieUrl, MovieExt, startTimeNtp);
        }

        private void SaveData(EndRollViewModel model)
        {
            using (var writer = new System.IO.StreamWriter(
                "data.txt", false, Encoding.UTF8))
            {
                writer.WriteLine("参加者");
                model.VoterList.JoinedVoterList
                    .ForEach(_ => writer.WriteLine("{0,24}　{1}", _.Name, _.Skill));

                writer.WriteLine("参加者数: {0}",
                    model.VoterList.JoinedVoterList.Count());
                writer.WriteLine("その他: {0}",
                    model.VoterList.UnjoinedVoterCount);
            }
        }

        /// <summary>
        /// エンディングの再生を開始します。
        /// </summary>
        private void Play()
        {
            // エンドロール用の参加者一覧は動画再生直前に取得します。
            var data = GetVoterList();
            data.JoinedVoterViewMaximumCount = MaximumDisplayListeners;
            data.LiveOwnerViewMaximumCount = MaximumDisplayLiveOwners;
            this.endRollData = data;

            this.prevPosition = TimeSpan.Zero;
            Position = TimeSpan.Zero;
            Ending.PlayMovie();

            // エンディングの前に現局面を設定します。
            var board = ShogiGlobal.ShogiModel.CurrentBoard.Clone();
            board.UndoAll();
            ShogiControl.Board = board;

            // 指し手の設定を行います。
            /*var board = new Board();
            var moveList = BoardExtension.MakeMoveList(SampleMove.Tsume);
            var bmList = board.ConvertMove(moveList);*/

            var interval = ShogiTimeline.VisibleSpan - TimeSpan.FromSeconds(3);
            var count = board.CanRedoCount + 2;
            this.autoPlay = new AutoPlayEx(board, AutoPlayType.Redo)
            {
                EffectManager = this.effectManager,
                IsChangeMoveCount = true,
                Interval = TimeSpan.FromSeconds(interval.TotalSeconds / count),
            };

            // 将棋盤の背景画像を更新しておきます。
            this.effectManager.ChangeMoveCount(0);
        }

        /// <summary>
        /// エンディングの再生を停止します。
        /// </summary>
        public void Stop()
        {
            EndRoll.Stop();
            Ending.Stop();

            //ShogiControl.StopAutoPlay();
            ShogiGrid.Opacity = 0.0;
            MovieBrush.Opacity = 0.0;
            Position = TimeSpan.Zero;

            this.prevPosition = TimeSpan.Zero;
            this.autoPlay = null;
            this.endRollData = null;
        }

        /// <summary>
        /// 定期的に呼ばれます。
        /// </summary>
        private void Update()
        {
            if (Ending.State == EndingState.Idle)
            {
                // 再生中または待機時
                UpdatePosition(Ending.MoviePlayer.Position);
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
        /// 動画の再生ポジションを更新します。
        /// </summary>
        private void UpdatePosition(TimeSpan position)
        {
            using (var result = this.positionLock.Lock())
            {
                if (result == null) return;

                var absElapsed = MathEx.Abs(position - this.prevPosition);
                if (absElapsed < this.interval)
                {
                    return;
                }
                this.prevPosition = position;

                // スタッフロールの更新
                if (position < EndRollTimeline.FadeInStartTime ||
                    position > EndRollTimeline.FadeOutEndTime)
                {
                    EndRoll.Stop();
                }
                else
                {
                    if (EndRoll.State == EndRollState.Stop)
                    {
                        var span = EndRollTimeline.VisibleSpan;

                        EndRoll.RollTimeSeconds = (int)Math.Ceiling(span.TotalSeconds);
                        EndRoll.Play(this.endRollData);
                    }

                    EndRoll.UpdateScreen(position - EndRollTimeline.FadeInStartTime);
                }

                // 指し手の自動再生を開始
                if (position < ShogiTimeline.FadeInEndTime + TimeSpan.FromSeconds(1))
                {
                    ShogiControl.StopAutoPlay();
                }
                else
                {
                    if (ShogiControl.AutoPlayState == AutoPlayState.None)
                    {
                        ShogiControl.StartAutoPlay(this.autoPlay);
                    }
                }

                // 長い時間でコントロールの更新を行うと
                // 処理に膨大な時間がかかります。
                // 動画の途中からの再生時（テスト用）にしか
                // こんな時間はかからないので、一部の処理をパスします。
                if (absElapsed < TimeSpan.FromSeconds(10))
                {
                    ShogiControl.Render(absElapsed);
                    ShogiBackground.Render(absElapsed);
                }

                ShogiGrid.Opacity = ShogiTimeline.GetRatio(position) * ShogiOpacity;
                MovieBrush.Opacity = MovieTimeline.GetRatio(position);
                Position = position;
            }
        }

        /// <summary>
        /// エンディングの再生位置を再設定します。
        /// </summary>
        public void ResetPosition(TimeSpan position)
        {
            if (Ending.MoviePlayer == null)
            {
                return;
            }

            using (var result = this.positionLock.Lock())
            {
                if (result == null) return;

                Ending.MoviePlayer.Position = position;
                UpdatePosition(position);
            }
        }
    }
}
