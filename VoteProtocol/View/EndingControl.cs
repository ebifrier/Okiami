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
        /// エラーにより動画の準備に失敗しました。
        /// </summary>
        Error,
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

            self.UpdateState();
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
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EndingControl()
        {
            MoviePlayer = new MediaPlayer
            {
                Volume = 0.1,
            };
            MoviePlayer.MediaOpened += MediaOpened;

            Downloader = new Downloader();
            Downloader.AddPropertyChangedHandler(
                "ProgressRate",
                (_, __) => WPFUtil.UIProcess(
                    () => OnProgressRateChanged()));

            Unloaded += (_, __) =>
            {
                if (this.timer != null)
                {
                    this.timer.Stop();
                    this.timer = null;
                }
            };

            this.timer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(200),
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

            UpdateState();
        }

        /// <summary>
        /// 残り時間に追うおじて、アナロ熊のサイズなどを変更します。
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
                this.viewbox.Visibility = Visibility.Hidden;
            }
            else
            {
                this.viewbox.Visibility = Visibility.Visible;

                // ５分前になったらだんだん大きくします。
                var ViewSpan = TimeSpan.FromMinutes(5.0);
                var leaveTime = StartTimeNtp - NtpClient.GetTime();
                this.analogma.LeaveTime = leaveTime;
                if (leaveTime > ViewSpan)
                {
                    this.viewbox.Height = 150;
                }
                else if (leaveTime < TimeSpan.Zero)
                {
                    this.viewbox.Height = 150 + ViewSpan.TotalSeconds / 2;
                }
                else
                {
                    var time = ViewSpan - leaveTime;
                    this.viewbox.Height = 150 + time.TotalSeconds / 2;
                }
            }
        }

        private void OnProgressRateChanged()
        {
            using (var result = this.progressLock.Lock())
            {
                if (result == null) return;

                UpdateState();
            }
        }

        /// <summary>
        /// 更新状態を表示します。
        /// </summary>
        private void UpdateState()
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
                case EndingState.Error:
                    text = "エラーが発生しました (≧≦)";
                    break;
            }

            this.analogma.Text = text;
            UpdateLeaveTime();
        }

        /// <summary>
        /// 動画の再生準備を開始します。
        /// </summary>
        public void StartPrepare(Uri movieUri, DateTime startTimeNtp)
        {
            StartPrepare(movieUri, startTimeNtp, false);
        }

        /// <summary>
        /// 動画の再生準備を開始します。
        /// </summary>
        private void StartPrepare(Uri movieUri, DateTime startTimeNtp,
                                  bool retry)
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

                Downloader.BeginDownload(
                    movieUri,
                    (_, __) => WPFUtil.UIProcess(
                        () => OnMovieDownloaded(_, __)));
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
            StartTimeNtp = startTimeNtp;

            // 二度目の再生の可能性があるため。
            MoviePlayer.Stop();
            MoviePlayer.Position = TimeSpan.Zero;
        }

        /// <summary>
        /// 動画のURLから、ローカルの動画ファイルパスを取得します。
        /// </summary>
        private Uri GetLocalMoviePath(Uri movieUri)
        {
            var ext = System.IO.Path.GetExtension(movieUri.ToString());
            var path = "ShogiData/EndRoll/movie" + ext;

            return new Uri(path, UriKind.Relative);
        }

        private void OnMovieDownloaded(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    throw e.Error;
                }

                var localMoviePath = GetLocalMoviePath(MovieUri);
                using (var tmpfile = new PassingTmpFile(localMoviePath.ToString()))
                {
                    using (var stream = new FileStream(tmpfile.TmpFileName,
                                                       FileMode.Create))
                    {
                        stream.Write(e.Result, 0, e.Result.Length);
                    }

                    tmpfile.Success();
                }

                OpenMedia(localMoviePath);
            }
            catch (WebException ex)
            {
                Log.ErrorException(ex,
                    "動画ファイルのダウンロードに失敗しました。");

                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    State = EndingState.Error;
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
            StartPrepare(MovieUri, StartTimeNtp, true);
        }

        /// <summary>
        /// 動画の読み込みを開始します。
        /// </summary>
        private void OpenMedia(Uri localMoviePath)
        {
            try
            {
                MoviePlayer.Open(localMoviePath);
                MoviePlayer.Position = TimeSpan.Zero;

                State = EndingState.Loading;
            }
            catch (Exception ex)
            {
                Util.ThrowIfFatal(ex);
                Log.ErrorException(ex,
                    "動画ファイルの読み込みに失敗しました。");

                State = EndingState.Error;
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
