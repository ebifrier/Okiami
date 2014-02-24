using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
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
using Ragnarok.Net;
using Ragnarok.Update;
using Ragnarok.Utility;
using Ragnarok.Presentation;
using Ragnarok.Presentation.Control;

namespace VoteSystem.Protocol.View
{
    /// <summary>
    /// エンディングコントロールの状態を示します。
    /// </summary>
    public enum EndingState
    {
        /// <summary>
        /// 初期状態です。
        /// </summary>
        Idle,
        /// <summary>
        /// 動画ファイルのダウンロード中です。
        /// </summary>
        Downloading,
        /// <summary>
        /// 動画ファイルの読み込み中です。
        /// </summary>
        Loading,
        /// <summary>
        /// すべての順が完了しました。
        /// </summary>
        Completed,
        /// <summary>
        /// 動画のダウンロードに失敗しました。
        /// </summary>
        DownloadError,
        /// <summary>
        /// 動画の読み込みに失敗しました。
        /// </summary>
        OpenError,
    }

    /// <summary>
    /// エンドロールを流すためのコントロールです。
    /// </summary>
    /// <remarks>
    /// 動画のダウンロードや動画再生までの残り時間表示機能
    /// などがついています。
    /// </remarks>
    [TemplatePart(Type = typeof(Viewbox), Name = "PART_Viewbox")]
    [TemplatePart(Type = typeof(AnalogmaControl), Name = "PART_Analogma")]
    public partial class EndingControl : Control
    {
        private const string ElementViewboxName = "PART_Viewbox";
        private const string ElementAnalogmaName = "PART_Analogma";

        private readonly ReentrancyLock progressLock = new ReentrancyLock();
        private Viewbox viewbox;
        private AnalogmaControl analogma;
        private DispatcherTimer timer;
        private double prevProgressRate = -1.0;

        #region event
        /// <summary>
        /// 準備開始時に呼ばれるイベントです。
        /// </summary>
        public static readonly RoutedEvent PrepareStartedEvent =
            EventManager.RegisterRoutedEvent(
                "PrepareStarted", RoutingStrategy.Bubble,
                typeof(RoutedEventArgs), typeof(EndingControl));

        /// <summary>
        /// 準備開始時に呼ばれるイベントです。
        /// </summary>
        public event RoutedEventHandler PrepareStarted
        {
            add { AddHandler(PrepareStartedEvent, value); }
            remove { RemoveHandler(PrepareStartedEvent, value); }
        }

        /// <summary>
        /// 動画ファイルダウンロード後に呼ばれるイベントです。
        /// </summary>
        public static readonly RoutedEvent MovieDownloadedEvent =
            EventManager.RegisterRoutedEvent(
                "MovieDownloaded", RoutingStrategy.Bubble,
                typeof(RoutedEventArgs), typeof(EndingControl));

        /// <summary>
        /// 動画ファイルダウンロード後に呼ばれるイベントです。
        /// </summary>
        public event RoutedEventHandler MovieDownloaded
        {
            add { AddHandler(MovieDownloadedEvent, value); }
            remove { RemoveHandler(MovieDownloadedEvent, value); }
        }
        
        /// <summary>
        /// 動画ファイル読み込み完了後に呼ばれるイベントです。
        /// </summary>
        public static readonly RoutedEvent MovieLoadedEvent =
            EventManager.RegisterRoutedEvent(
                "MovieLoaded", RoutingStrategy.Bubble,
                typeof(RoutedEventArgs), typeof(EndingControl));

        /// <summary>
        /// 動画ファイル読み込み完了に呼ばれるイベントです。
        /// </summary>
        public event RoutedEventHandler MovieLoaded
        {
            add { AddHandler(MovieLoadedEvent, value); }
            remove { RemoveHandler(MovieLoadedEvent, value); }
        }
        #endregion

        #region property
        /// <summary>
        /// コントロールの状態を扱います。
        /// </summary>
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register(
                "State", typeof(EndingState), typeof(EndingControl),
                new FrameworkPropertyMetadata(EndingState.Idle,
                    OnStateChanged));

        /// <summary>
        /// コントロールの状態を取得します。
        /// </summary>
        public EndingState State
        {
            get { return (EndingState)GetValue(StateProperty); }
            private set { SetValue(StateProperty, value); }
        }

        private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (EndingControl)d;

            self.UpdateMessageText();
        }

        /// <summary>
        /// エンディング動画のダウンロード先のパスを扱います。
        /// </summary>
        public static readonly DependencyProperty MovieUriProperty =
            DependencyProperty.Register(
                "MovieUri", typeof(Uri), typeof(EndingControl),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// エンディング動画のダウンロード先のパスを取得します。
        /// </summary>
        public Uri MovieUri
        {
            get { return (Uri)GetValue(MovieUriProperty); }
            private set { SetValue(MovieUriProperty, value); }
        }

        /// <summary>
        /// エンディング動画の拡張子(.付き)を扱います。
        /// </summary>
        public static readonly DependencyProperty MovieExtProperty =
            DependencyProperty.Register(
                "MovieExt", typeof(string), typeof(EndingControl),
                new FrameworkPropertyMetadata(string.Empty));

        /// <summary>
        /// エンディング動画の拡張子(.付き)を取得します。
        /// </summary>
        public string MovieExt
        {
            get { return (string)GetValue(MovieExtProperty); }
            private set { SetValue(MovieExtProperty, value); }
        }

        /// <summary>
        /// エンディング再生開始時間を取得または設定します。
        /// </summary>
        public static readonly DependencyProperty StartTimeNtpProperty =
            DependencyProperty.Register(
                "StartTimeNtp", typeof(DateTime), typeof(AnalogmaControl),
                new FrameworkPropertyMetadata(DateTime.MinValue));

        /// <summary>
        /// エンディング再生開始時間を取得または設定します。
        /// </summary>
        [Bindable(true)]
        public DateTime StartTimeNtp
        {
            get { return (DateTime)GetValue(StartTimeNtpProperty); }
            set { SetValue(StartTimeNtpProperty, value); }
        }
        #endregion

        #region object property
        /// <summary>
        /// 動画再生用オブジェクトを扱います。
        /// </summary>
        public static readonly DependencyProperty MoviePlayerProperty =
            DependencyProperty.Register(
                "MoviePlayer", typeof(MediaPlayer), typeof(EndingControl),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// 動画再生用オブジェクトを取得します。
        /// </summary>
        public MediaPlayer MoviePlayer
        {
            get { return (MediaPlayer)GetValue(MoviePlayerProperty); }
            private set { SetValue(MoviePlayerProperty, value); }
        }

        /// <summary>
        /// 動画再生用オブジェクトを扱います。
        /// </summary>
        public static readonly DependencyProperty DownloaderProperty =
            DependencyProperty.Register(
                "Downloader", typeof(Downloader), typeof(EndingControl),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// 動画再生用オブジェクトを取得します。
        /// </summary>
        public Downloader Downloader
        {
            get { return (Downloader)GetValue(DownloaderProperty); }
            private set { SetValue(DownloaderProperty, value); }
        }
        #endregion

        /// <summary>
        /// 静的コンストラクタ
        /// </summary>
        static EndingControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(EndingControl),
                new FrameworkPropertyMetadata(typeof(EndingControl)));
            VisibilityProperty.OverrideMetadata(
                typeof(EndingControl),
                new FrameworkPropertyMetadata(Visibility.Hidden));
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EndingControl()
        {
            MoviePlayer = new MediaPlayer();
            MoviePlayer.MediaOpened += MediaOpened;
            MoviePlayer.MediaFailed += MediaFailed;

            Downloader = new Downloader();
            Downloader.AddPropertyChangedHandler(
                "ProgressRate",
                (_, __) => WPFUtil.UIProcess(
                    () => OnProgressRateChanged(_, __)));

            Unloaded += (_, __) =>
            {
                if (this.timer != null)
                {
                    this.timer.Stop();
                    this.timer = null;
                }
            };

            this.timer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(100),
                DispatcherPriority.Normal,
                (_, __) => UpdateLeaveTime(),
                Dispatcher);
            this.timer.Start();
        }

        /// <summary>
        /// テンプレートが適用されたときに呼ばれます。
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.viewbox = GetTemplateChild(ElementViewboxName) as Viewbox;
            this.analogma = GetTemplateChild(ElementAnalogmaName) as AnalogmaControl;

            UpdateMessageText();
        }

        private static readonly TimeSpan AnalogmaHideInterval =
            TimeSpan.FromSeconds(5.0);
        private static readonly TimeSpan AnalogmaSizingInterval =
            TimeSpan.FromMinutes(5.0);
        private static readonly double AnalogmaMinHeight = 150.0;
        private static readonly double AnalogmaMaxHeight = 300.0;

        /// <summary>
        /// 残り時間に応じて、アナロ熊のサイズなどを変更します。
        /// </summary>
        private void UpdateLeaveTime()
        {
            if (this.viewbox == null || this.analogma == null ||
                StartTimeNtp == DateTime.MinValue)
            {
                return;
            }

            if (State == EndingState.Idle)
            {
                // エンディング準備前、または再生後なら
                // アナロ熊を隠します。
                Visibility = Visibility.Hidden;
            }
            else
            {
                Visibility = Visibility.Visible;

                var leaveTime = StartTimeNtp - NtpClient.GetTime();
                this.analogma.LeaveTime = leaveTime;                

                // 数秒前になったら熊をだんだん消します。
                if (leaveTime > AnalogmaHideInterval ||
                    leaveTime < -AnalogmaHideInterval)
                {
                    Opacity = 1.0;
                }
                else
                {
                    var time = AnalogmaHideInterval - leaveTime;
                    var rate = time.TotalSeconds /
                        AnalogmaHideInterval.TotalSeconds;

                    Opacity = MathEx.InterpEaseOut(1.0, 0.0, rate);
                }

                // 数分前になったらだんだん大きくします。
                if (leaveTime > AnalogmaSizingInterval)
                {
                    this.viewbox.Height = AnalogmaMinHeight;
                }
                else if (leaveTime < TimeSpan.Zero)
                {
                    this.viewbox.Height = AnalogmaMaxHeight;
                }
                else
                {
                    var time = AnalogmaSizingInterval - leaveTime;
                    var rate = time.TotalSeconds /
                        AnalogmaSizingInterval.TotalSeconds;

                    this.viewbox.Height = MathEx.InterpLiner(
                        AnalogmaMinHeight, AnalogmaMaxHeight, rate);
                }
            }
        }

        /// <summary>
        /// ダウンロード進捗度の更新時に呼ばれます。
        /// </summary>
        private void OnProgressRateChanged(object sender, PropertyChangedEventArgs e)
        {
            using (var result = this.progressLock.Lock())
            {
                if (result == null) return;

                // あまりに小さい進捗は無視します。
                var rate = Downloader.ProgressRate;
                var diff = Math.Abs(this.prevProgressRate - rate);
                if (diff < 0.0001)
                {
                    return;
                }

                this.prevProgressRate = rate;
                UpdateMessageText();
            }
        }

        /// <summary>
        /// 状態表示を表示します。
        /// </summary>
        private void UpdateMessageText()
        {
            if (this.analogma == null)
            {
                return;
            }

            var text = string.Empty;
            switch (State)
            {
                case EndingState.Idle:
                    text = "準備中。。。";
                    break;
                case EndingState.Downloading:
                    text = string.Format(
                        @"動画ファイルダウンロード中　{0:p1}",
                        Downloader.ProgressRate);
                    break;
                case EndingState.Loading:
                    text = "動画ファイル読み込み中。。。";
                    break;
                case EndingState.Completed:
                    text = "準備完了！";
                    break;
                case EndingState.DownloadError:
                    text = "動画のダウンロードに失敗しました (≧≦)";
                    break;
                case EndingState.OpenError:
                    text = "動画の読み込みに失敗しました (≧≦)";
                    break;
            }

            this.analogma.Text = text;
            UpdateLeaveTime();
        }

        /// <summary>
        /// エンディングの再生を取りやめます。
        /// </summary>
        public void Stop()
        {
            State = EndingState.Idle;
            MovieUri = null;
            StartTimeNtp = DateTime.MinValue;
            MoviePlayer.Stop();

            Downloader.CancelAll();
        }

        /// <summary>
        /// 動画の再生準備を開始します。
        /// </summary>
        public void StartPrepare(Uri movieUri, string movieExt, DateTime startTimeNtp)
        {
            StartPrepare(movieUri, movieExt, startTimeNtp, false);
        }

        /// <summary>
        /// 動画の再生準備を開始します。
        /// </summary>
        private void StartPrepare(Uri movieUri, string movieExt,
                                  DateTime startTimeNtp, bool retry)
        {
            if (movieUri == null)
            {
                throw new ArgumentNullException("movieUri");
            }

            // 依然と違うURLであればダウンロードします。
            // というのも、開始時間が変わるとStartPrepareが
            // 何度も呼ばれる可能性があるためです。
            if (retry || MovieUri != movieUri)
            {
                Downloader.CancelAll();

                if (!movieUri.IsFile)
                {
                    Downloader.BeginDownload(
                        movieUri,
                        (_, __) => WPFUtil.UIProcess(
                            () => OnMovieDownloaded(_, __)));
                }
            }

            // ダウンロードが残っていたらダウンロード中止とし、
            // それ以外の場合は（＝再スタートなど）
            // 状態を変えずにそのままにします。
            if (Downloader.LeaveCount != 0)
            {
                State = EndingState.Downloading;
            }
            else if (State == EndingState.Idle && Downloader.LeaveCount == 0)
            {
                // 二度目の再生時にはここに来ます。
                State = EndingState.Completed;
            }

            MovieUri = movieUri;
            MovieExt = movieExt ?? string.Empty;
            StartTimeNtp = startTimeNtp;

            // ファイルの拡張子が無ければ、リンクURLから取得します。
            if (string.IsNullOrEmpty(movieExt))
            {
                MovieExt = System.IO.Path.GetExtension(movieUri.LocalPath);
            }

            // 二度目の再生の可能性があるため。
            MoviePlayer.Stop();
            MoviePlayer.Position = TimeSpan.Zero;

            if (movieUri.IsFile)
            {
                OpenMedia(movieUri);
            }
            //OpenMedia(new Uri("file://E:/movies/alice/alice.wav"));
        }

        /// <summary>
        /// ローカルの動画ファイルパスを取得します。
        /// </summary>
        /// <remarks>
        /// 拡張子は元動画のURLと同じにします。
        /// そうしないと動画が正しく再生されません。
        /// </remarks>
        private Uri GetLocalMoviePath(Uri movieUri)
        {
            var path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                Guid.NewGuid().ToString() + MovieExt);

            return new Uri(path, UriKind.Absolute);
        }

        /// <summary>
        /// 動画データのダウンロード完了後に呼ばれます。
        /// </summary>
        private void OnMovieDownloaded(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                // キャンセルされた場合は素直に帰ります。
                if (e.Cancelled)
                {
                    return;
                }

                if (e.Error != null)
                {
                    throw e.Error;
                }

                // 動画データをファイルに保存します。
                // 一時ファイルを作った後でリネームするようにしています。
                var localMoviePath = GetLocalMoviePath(MovieUri);
                using (var stream = new FileStream(localMoviePath.LocalPath,
                                                   FileMode.Create))
                {
                    stream.Write(e.Result, 0, e.Result.Length);
                }

                Log.Debug("{0}: 動画ファイルを保存しました。", localMoviePath);

                OpenMedia(localMoviePath);
            }
            catch (WebException ex)
            {
                Log.ErrorException(ex,
                    "動画ファイルのダウンロードに失敗しました。");

                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    State = EndingState.DownloadError;
                }
                else
                {
                    RetryDownload();
                }
            }
            catch (Exception ex)
            {
                Util.ThrowIfFatal(ex);
                Log.ErrorException(ex,
                    "動画ファイルのダウンロードに失敗しました。");

                RetryDownload();
            }
        }

        /// <summary>
        /// 一度キャンセルして、ダウンロードを再トライします。
        /// </summary>
        private void RetryDownload()
        {
            Downloader.CancelAll();
            StartPrepare(MovieUri, MovieExt, StartTimeNtp, true);
        }

        /// <summary>
        /// 動画の読み込みを開始します。
        /// </summary>
        private void OpenMedia(Uri localMoviePath)
        {
            try
            {
                // メディアを一度閉じてから再度開かないと
                // 2度目の読み込み時にMediaOpenedが呼ばれません。
                var volume = MoviePlayer.Volume;
                var isMuted = MoviePlayer.IsMuted;
                MoviePlayer.Close();

                MoviePlayer.Open(localMoviePath);
                MoviePlayer.Position = TimeSpan.Zero;
                MoviePlayer.Volume = volume;
                MoviePlayer.IsMuted = isMuted;

                State = EndingState.Loading;
            }
            catch (Exception ex)
            {
                Util.ThrowIfFatal(ex);
                Log.ErrorException(ex,
                    "動画ファイルの読み込みに失敗しました。");

                State = EndingState.OpenError;
            }
        }

        /// <summary>
        /// メディアファイルの読み込み完了時に呼ばれます。
        /// </summary>
        private void MediaOpened(object sender, EventArgs e)
        {
            WPFUtil.UIProcess(() =>
                State = EndingState.Completed);
        }

        /// <summary>
        /// メディアファイルの読み込み失敗時に呼ばれます。
        /// </summary>
        private void MediaFailed(object sender, ExceptionEventArgs e)
        {
            Log.ErrorException(e.ErrorException,
                "動画の読み込みに失敗しました。");

            WPFUtil.UIProcess(() =>
            {
                State = EndingState.OpenError;

                DialogUtil.ShowError(
                    e.ErrorException,
                    "動画の読み込みに失敗しました。");
            });
        }

        /// <summary>
        /// エンドロールの再生を開始します。
        /// </summary>
        public void PlayMovie()
        {
            WPFUtil.UIProcess(() =>
            {
                if (State != EndingState.Completed)
                {
                    return;
                }

                MoviePlayer.Play();
                State = EndingState.Idle;
            });
        }
    }
}
