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
using System.Globalization;
using System.ComponentModel;
using System.Threading;

using Ragnarok;
using Ragnarok.ObjectModel;
using Ragnarok.Presentation;
using Ragnarok.Presentation.Control;

namespace VoteSystem.Protocol.View
{
    using Model;

    /// <summary>
    /// エンドロールの流れている状態です。
    /// </summary>
    public enum EndRollState
    {
        /// <summary>
        /// 止まっています。
        /// </summary>
        Stop,
        /// <summary>
        /// 流れている最中です。
        /// </summary>
        Play,
        /// <summary>
        /// 一時停止中です。
        /// </summary>
        Pause,
    }

    /// <summary>
    /// エンドロールを流すコントロールです。
    /// </summary>
    /// <remarks>
    /// スタッフロールなどを表示するためのコントロールで、
    /// 与えられた文字列が順次上に流れていきます。
    /// 
    /// このクラスでは、画面の一番上を原点としているため、
    /// 扱っているY座標値はウィンドウ上の位置とそのまま対応しています。
    /// 
    /// エンドロールの各行は画面の一番下から、最後は一番上に切れるまで表示
    /// されるため、全体が流れる距離としては"画面高さ + 文字列全体の長さ"
    /// となります。
    /// これだけの距離を<see cref="RollTimeSeconds"/>で指定された時間で
    /// 流しています。
    /// 
    /// また、行の間隔は文字列のサイズに関わらず<see cref="LineHeight"/>で
    /// 指定するようになっています。
    /// </remarks>
    [TemplatePart(Type = typeof(Border), Name = "BackgroundPart")]
    [TemplatePart(Type = typeof(Canvas), Name = "TextPanelPart")]
    public class EndRollControl : UserControl
    {
        /// <summary>
        /// 背景色表示用のコントロール名
        /// </summary>
        private const string ElementBackgroundName = "BackgroundPart";
        /// <summary>
        /// 文字表示用のコントロール名
        /// </summary>
        private const string ElementTextPanelName = "TextPanelPart";

        /// <summary>
        /// 各行に含まれる各テキストの情報を保持します。
        /// </summary>
        private sealed class TextInfo
        {
            /// <summary>
            /// 表示位置などの情報を取得します。
            /// </summary>
            public Element Element
            {
                get;
                set;
            }

            /// <summary>
            /// 表示文字列を取得します。
            /// </summary>
            public DecoratedText DecoratedText
            {
                get;
                set;
            }
        }

        /// <summary>
        /// 各行文字列の情報を保持します。
        /// </summary>
        private sealed class LineInfo
        {
            /// <summary>
            /// 1行にある文字列のリストを取得します。
            /// </summary>
            public List<TextInfo> Texts
            {
                get;
                set;
            }

            /// <summary>
            /// １行目を０としたＹ方向の文字表示位置を取得または設定します。
            /// </summary>
            public double TotalHeight
            {
                get;
                set;
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public LineInfo()
            {
                Texts = new List<TextInfo>();
            }
        }

        /// <summary>
        /// エンドロールを開始します。
        /// </summary>
        public static readonly ICommand PlayCommand =
            new RoutedUICommand(
                "エンドロールを開始します。",
                "Play",
                typeof(EndRollControl));
        /// <summary>
        /// エンドロールを一時停止します。
        /// </summary>
        public static readonly ICommand PauseCommand =
            new RoutedUICommand(
                "エンドロールを一時停止します。",
                "Pause",
                typeof(EndRollControl));
        /// <summary>
        /// エンドロールを停止します。
        /// </summary>
        public static readonly ICommand StopCommand =
            new RoutedUICommand(
                "エンドロールを停止します。",
                "Stop",
                typeof(EndRollControl));

        /// <summary>
        /// エンドロールの再生開始時に呼ばれるイベントです。
        /// </summary>
        public static readonly RoutedEvent StartedEvent =
            EventManager.RegisterRoutedEvent(
                "Started", RoutingStrategy.Bubble,
                typeof(RoutedEventArgs), typeof(EndRollControl));
        /// <summary>
        /// エンドロールの一時停止時に呼ばれるイベントです。
        /// </summary>
        public static readonly RoutedEvent PausedEvent =
            EventManager.RegisterRoutedEvent(
                "Paused", RoutingStrategy.Bubble,
                typeof(RoutedEventArgs), typeof(EndRollControl));
        /// <summary>
        /// エンドロールの停止時に呼ばれるイベントです。
        /// </summary>
        public static readonly RoutedEvent StoppedEvent =
            EventManager.RegisterRoutedEvent(
                "Stopped", RoutingStrategy.Bubble,
                typeof(RoutedEventArgs), typeof(EndRollControl));

        /// <summary>
        /// エンドロールの再生開始時に呼ばれるイベントです。
        /// </summary>
        public event RoutedEventHandler Started
        {
            add { base.AddHandler(EndRollControl.StartedEvent, value); }
            remove { base.RemoveHandler(EndRollControl.StartedEvent, value); }
        }
        /// <summary>
        /// エンドロールの一時停止時に呼ばれるイベントです。
        /// </summary>
        public event RoutedEventHandler Paused
        {
            add { base.AddHandler(EndRollControl.PausedEvent, value); }
            remove { base.RemoveHandler(EndRollControl.PausedEvent, value); }
        }
        /// <summary>
        /// エンドロールの停止時に呼ばれるイベントです。
        /// </summary>
        public event RoutedEventHandler Stopped
        {
            add { base.AddHandler(EndRollControl.StoppedEvent, value); }
            remove { base.RemoveHandler(EndRollControl.StoppedEvent, value); }
        }

        /// <summary>
        /// 状態に応じたイベントを発行します。
        /// </summary>
        private void RaiseStateEvent(EndRollState state)
        {
            RoutedEvent ev = null;
            switch (state)
            {
                case EndRollState.Play:
                    ev = StartedEvent;
                    break;
                case EndRollState.Pause:
                    ev = PausedEvent;
                    break;
                case EndRollState.Stop:
                    ev = StoppedEvent;
                    break;
            }

            if (ev != null)
            {
                RaiseEvent(new RoutedEventArgs(ev, this));
            }
        }

        #region プロパティ
        /// <summary>
        /// フォーマットファイルのパスを示す依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty FormatFilePathProperty =
            DependencyProperty.Register(
                "FormatFilePath", typeof(string), typeof(EndRollControl),
                new FrameworkPropertyMetadata(@"Data/EndRoll/endroll_format.xml"));

        /// <summary>
        /// フォーマットファイルのパスを取得または設定します。
        /// </summary>
        [Bindable(true)]
        public string FormatFilePath
        {
            get { return (string)GetValue(FormatFilePathProperty); }
            set { SetValue(FormatFilePathProperty, value); }
        }

        /// <summary>
        /// 参加者一覧のデータを取得するためのメソッドを示す依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty DataGetterProperty =
            DependencyProperty.Register(
                "DataGetter", typeof(Func<object>), typeof(EndRollControl),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// 参加者一覧のデータを取得するためのメソッドを取得または設定します。
        /// </summary>
        [Bindable(true)]
        public Func<object> DataGetter
        {
            get { return (Func<object>)GetValue(DataGetterProperty); }
            set { SetValue(DataGetterProperty, value); }
        }

        /// <summary>
        /// １行ごとの高さを示す依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty LineHeightProperty =
            DependencyProperty.Register(
                "LineHeight", typeof(double), typeof(EndRollControl),
                new FrameworkPropertyMetadata(10.0));

        /// <summary>
        /// １行の高さを取得または設定します。
        /// </summary>
        [Bindable(true)]
        public double LineHeight
        {
            get { return (double)GetValue(LineHeightProperty); }
            set { SetValue(LineHeightProperty, value); }
        }

        /// <summary>
        /// エンドロールの流れる時間（秒）を示す依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty RollTimeSecondsProperty =
            DependencyProperty.Register(
                "RollTimeSeconds", typeof(int), typeof(EndRollControl),
                new FrameworkPropertyMetadata(300));

        /// <summary>
        /// エンドロールが流れる時間間隔を取得または設定します。
        /// </summary>
        [Bindable(true)]
        public int RollTimeSeconds
        {
            get { return (int)GetValue(RollTimeSecondsProperty); }
            set { SetValue(RollTimeSecondsProperty, value); }
        }

        /// <summary>
        /// エンドロールの開始と終了の何行後/何行前から文字の不透明度を
        /// １００％にするかを示す依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty OpacityLineCountProperty =
            DependencyProperty.Register(
                "OpacityLineCount", typeof(int), typeof(EndRollControl),
                new FrameworkPropertyMetadata(3));

        /// <summary>
        /// エンドロールの開始と終了の何行後/何行前から文字の不透明度を
        /// １００％にするかを示す依存プロパティです。
        /// </summary>
        [Bindable(true)]
        public int OpacityLineCount
        {
            get { return (int)GetValue(OpacityLineCountProperty); }
            set { SetValue(OpacityLineCountProperty, value); }
        }

        /// <summary>
        /// 背景を更新するかどうかを示す依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty IsUpdateBackgroundProperty =
            DependencyProperty.Register(
                "IsUpdateBackground", typeof(bool), typeof(EndRollControl),
                new FrameworkPropertyMetadata(false));

        /// <summary>
        /// 背景を更新するかどうかを取得または設定します。
        /// </summary>
        public bool IsUpdateBackground
        {
            get { return (bool)GetValue(IsUpdateBackgroundProperty); }
            set { SetValue(IsUpdateBackgroundProperty, value); }
        }

        /// <summary>
        /// 自動で進み具合を更新するかを示す依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty AutoUpdateProperty =
            DependencyProperty.Register(
                "AutoUpdate", typeof(bool), typeof(EndRollControl),
                new FrameworkPropertyMetadata(false,
                    OnAutoUpdateChanged));

        /// <summary>
        /// 自動で進み具合を更新するかを取得または設定します。
        /// </summary>
        public bool AutoUpdate
        {
            get { return (bool)GetValue(AutoUpdateProperty); }
            set { SetValue(AutoUpdateProperty, value); }
        }

        private static void OnAutoUpdateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (EndRollControl)d;

            self.UpdateAutoUpdate((bool)e.NewValue);
        }

        private void UpdateAutoUpdate(bool value)
        {
            // デザイン時は更新処理は行いません。
            if (WPFUtil.IsInDesignMode)
            {
                return;
            }

            if (value)
            {
                CompositionTarget.Rendering += CompositionTarget_Rendering;
            }
            else
            {
                CompositionTarget.Rendering -= CompositionTarget_Rendering;
            }
        }

        /// <summary>
        /// エンドロールの進み具合を示す依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty CurrentPosProperty =
            DependencyProperty.Register(
                "CurrentPos", typeof(double), typeof(EndRollControl),
                new FrameworkPropertyMetadata(0.0));

        /// <summary>
        /// エンドロールの進み具合を取得または設定します。
        /// </summary>
        public double CurrentPos
        {
            get { return (double)GetValue(CurrentPosProperty); }
            set { SetValue(CurrentPosProperty, value); }
        }

        /// <summary>
        /// エンドロールの状態を示す依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register(
                "State", typeof(EndRollState), typeof(EndRollControl),
                new FrameworkPropertyMetadata(EndRollState.Stop,
                    OnStateChanged));

        /// <summary>
        /// エンドロールの状態を取得します。
        /// </summary>
        [Bindable(true)]
        public EndRollState State
        {
            get { return (EndRollState)GetValue(StateProperty); }
            private set { SetValue(StateProperty, value); }
        }

        private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (EndRollControl)d;

            WPFUtil.InvalidateCommand();
            self.RaiseStateEvent((EndRollState)e.NewValue);
        }

        /// <summary>
        /// 文字列の行数を取得します。
        /// </summary>
        public int LineCount
        {
            get { return this.lineList.Count(); }
        }

        /// <summary>
        /// 全文字列の表示時の長さを取得します。
        /// </summary>
        public double AllTextLength
        {
            get { return (LineCount * LineHeight); }
        }

        /// <summary>
        /// 文字列が流れる総高さを取得します。
        /// </summary>
        /// <remarks>
        /// エンドロールは画面の高さ + 文字列の全高さ分だけ流れます。
        /// </remarks>
        public double RollLength
        {
            get { return (ActualHeight + AllTextLength); }
        }

        /// <summary>
        /// 文字列の流れる早さを[px/s]で取得します。
        /// </summary>
        public double TextSpeed
        {
            get
            {
                if (RollTimeSeconds < 0.0)
                {
                    throw new InvalidOperationException(
                        "エンドロールの時間間隔が０以下になっています。");
                }

                // 全体の長さを時間で割って速度を出します。
                return (RollLength / RollTimeSeconds);
            }
        }
        #endregion

        #region アクション
        /// <summary>
        /// エンドロールの各要素から行を構成する文字列情報を作成します。
        /// </summary>
        private TextInfo CreateTextInfo(Element elem)
        {
            var decoratedText = new DecoratedText
            {
                IsUpdateVisual = false, // 字形の自動作成はしない。
                Text = elem.Text,
                FontSize = elem.FontSize,
                Foreground = new SolidColorBrush(elem.Color),
                FontStyle = elem.FontStyle,
                FontWeight = elem.FontWeight,
                Stroke = Brushes.Black,
                StrokeThickness = 0.1,
                Effect = new DropShadowEffect() {Opacity = 0.6},
            };

            return new TextInfo
            {
                Element = elem,
                DecoratedText = decoratedText,
            };
        }

        /// <summary>
        /// 投票者リストを取得し、それをvisualオブジェクトとに直します。
        /// </summary>
        private List<LineInfo> GetLineList(object data)
        {
            try
            {
                if (data == null)
                {
                    data = DataGetter();
                    if (data == null)
                    {
                        return new List<LineInfo>();
                    }
                }

                var stuffList = new EndRollList();
                stuffList.Load(FormatFilePath, data);

                // TODO
                this.columnList = stuffList.ColumnList;

                // 前後には空行を何行か入れます。
                var lines = new List<LineInfo>();
                for (var i = 0; i < OpacityLineCount; ++i)
                {
                    lines.Add(new LineInfo());
                }

                lines.AddRange(
                    from line in stuffList.LineList
                    select new LineInfo
                    {
                        Texts = line.ElementList.Select(CreateTextInfo).ToList(),
                    });

                // 前後には空行を何行か入れます。
                for (var i = 0; i < OpacityLineCount; ++i)
                {
                    lines.Add(new LineInfo());
                }

                return lines;
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "エンドロールの参加者一覧の作成に失敗しました(^^;");
            }

            return new List<LineInfo>();
        }

        /// <summary>
        /// エンドロールを開始します。
        /// </summary>
        public void Play(object data = null)
        {
            switch (State)
            {
                case EndRollState.Play:
                    break;
                case EndRollState.Stop:
                    this.lineList = GetLineList(data);

                    State = EndRollState.Play;
                    CurrentPos = 0.0;
                    this.prevUpdateTime = DateTime.Now;
                    break;
                case EndRollState.Pause:
                    State = EndRollState.Play;
                    this.prevUpdateTime = DateTime.Now;
                    break;
            }
        }

        /// <summary>
        /// エンドロールを停止します。
        /// </summary>
        public void Stop()
        {
            State = EndRollState.Stop;
            CurrentPos = 0.0;

            UpdateScreen(TimeSpan.Zero);
        }

        /// <summary>
        /// エンドロールを一時停止します。
        /// </summary>
        public void Pause()
        {
            switch (State)
            {
                case EndRollState.Play:
                    State = EndRollState.Pause;
                    break;
                case EndRollState.Stop:
                    break;
                case EndRollState.Pause:
                    break;
            }
        }
        #endregion

        #region エンドロール
        /// <summary>
        /// エンドロールの更新を行います。
        /// </summary>
        public bool UpdateScreen(TimeSpan position)
        {
            if (this.background == null || this.textPanel == null)
            {
                return false;
            }

            if (State == EndRollState.Play)
            {
                // 再生ポジションが進みすぎないように気をつけます。
                var newPos = position.TotalSeconds;
                if (newPos >= RollTimeSeconds)
                {
                    CurrentPos = RollTimeSeconds;
                    State = EndRollState.Stop;
                }
                else
                {
                    CurrentPos = newPos;
                }
            }

            // テキストを消す処理なども行うため、
            // 再生以外の状態でも更新処理を呼びます。
            for (var line = 0; line < LineCount; ++line)
            {
                UpdateLineText(CurrentPos, line);
            }

            if (IsUpdateBackground)
            {
                this.background.Opacity = CalcBackgroundOpacity(CurrentPos);
            }

            return true;
        }

        /// <summary>
        /// 1行の実際の高さを取得します。
        /// </summary>
        private double ActualLineHeight(int line)
        {
            var lineInfo = this.lineList[line];
            if (!lineInfo.Texts.Any())
            {
                return 0.0;
            }

            return lineInfo.Texts.Max(textInfo =>
                (textInfo.DecoratedText.FormattedText != null ?
                    textInfo.DecoratedText.FormattedText.Height :
                    0.0));
        }

        /// <summary>
        /// <paramref name="line"/>行目のテキストが表示されるか調べます。
        /// </summary>
        private bool IsShowLineText(double posY, int line)
        {
            var height = ActualLineHeight(line);

            // 文字オブジェクトの高さよりも上に行っていたら、
            // 完全に隠れています。
            if (posY < -height)
            {
                return false;
            }

            if (posY > ActualHeight)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 背景の不透明度を取得します。
        /// </summary>
        private double CalcBackgroundOpacity(double currentPos)
        {
            if (State == EndRollState.Stop)
            {
                return 0.0;
            }

            // 不透明である長さを速度で割って、透明度があるような時間を取得します。
            var opacityTime = OpacityLineCount * LineHeight / TextSpeed;

            if (currentPos >= opacityTime &&
                currentPos <= RollTimeSeconds - opacityTime)
            {
                return 1.0;
            }

            double rate = 0.0;
            if (currentPos < opacityTime)
            {
                rate = (currentPos / opacityTime);
            }
            else
            {
                rate = (RollTimeSeconds - currentPos) / opacityTime;
            }

            return Math.Min(1.0, Math.Max(rate, 0.0));
        }

        /// <summary>
        /// 文字の不透明度を計算します。
        /// </summary>
        private double GetOpacity(DecoratedText element, double posY)
        {
            if (element == null)
            {
                return 0.0;
            }

            var length = OpacityLineCount * LineHeight;

            // 画面外からはみ出していれば当然０
            if (posY < 0 || ActualHeight + element.ActualHeight < posY)
            {
                return 0.0;
            }

            // posYは減っていくので、tも同じように減っていきます。
            var t = posY + element.ActualHeight;
            if (t < length)
            {
                // length位置にあれば1.0, 画面端にあれば0.0
                return Math.Pow(t / length, 2);
            }

            // posYは減っていくので、tはだんだん増えていきます。
            t = ActualHeight + element.ActualHeight - posY;
            if (t < length)
            {
                // length位置にあれば1.0, 画面端にあれば0.0
                return Math.Pow(t / length, 2);
            }

            return 1.0;
        }

        /// <summary>
        /// 指定の横列から横列までの幅を取得します。
        /// </summary>
        private double GetColumnRate(int column, int columnSpan = -1)
        {
            var columns = this.columnList.Skip(column);
            if (columnSpan >= 0)
            {
                columns = columns.Take(columnSpan);
            }

            if (!columns.Any())
            {
                return 0.0;
            }

            return columns.Sum(col => col.Width);
        }

        /// <summary>
        /// 各テキストの表示Y位置を取得します。
        /// </summary>
        private double GetTextTop(TextInfo textInfo, double posY, FormattedText ft)
        {
            var decoratedText = textInfo.DecoratedText;

            // FormattedTextからパスに変換したとき、文字列に空白部分があると
            // その部分が消えてしまいます。
            // 文字列の位置はbaselineでそろえる必要があるため、
            // ここで文字列の上端からの消えた長さを計算しています。
            // この分をFormattedTextで計算されたY位置に加算することで、
            // 正しい位置が計算できます。
            var overhangBefore = (
                (ft.Height - decoratedText.ActualHeight) + ft.OverhangAfter);

            return (posY - ft.Baseline + overhangBefore);
        }

        /// <summary>
        /// 各テキストの表示X位置を取得します。
        /// </summary>
        private double GetTextLeft(TextInfo textInfo)
        {
            var elem = textInfo.Element;
            var decoratedText = textInfo.DecoratedText;

            // 列番号を取得します。
            var column = MathEx.Between(0, this.columnList.Count - 1, elem.Column);
            var columnSpan = MathEx.Between(1, this.columnList.Count, elem.ColumnSpan);

            // テキストが入る列の左端と右端の相対座標を取得します。
            var leftColumnRate = GetColumnRate(0, column);
            var rightColumnRate = leftColumnRate +
                GetColumnRate(column, columnSpan);

            // テキストが入る列の左端と右端の座標値を取得します。
            var allWidth = ActualWidth;
            var allWidthRate = this.columnList.Sum(col => col.Width);

            var columnLeft = allWidth * leftColumnRate / allWidthRate;
            var columnRight = allWidth * rightColumnRate / allWidthRate;

            switch (elem.HorizontalAlignment)
            {
                // 左に合わせる
                case HorizontalAlignment.Left:
                    return (columnLeft + 10);

                // 中心
                case HorizontalAlignment.Center:
                    return
                        ((columnLeft + columnRight) / 2) -
                        (decoratedText.ActualWidth / 2);

                // 右に合わせる
                case HorizontalAlignment.Right:
                    return (columnRight - 10 - decoratedText.ActualWidth);
            }

            // 不明なので適当に。
            return (columnLeft + 10);
        }

        /// <summary>
        /// <paramref name="line"/>行目にあるテキストを更新します。
        /// </summary>
        private void UpdateLineText(double currentPos, int line)
        {
            // 画面一番上を原点とした場合のline行目の位置を取得します。
            // エンドロールは上に流れるので、流れた分を減算しています。
            var posY = ActualHeight + line * LineHeight
                - TextSpeed * currentPos;

            if (State == EndRollState.Stop || !IsShowLineText(posY, line))
            {
                foreach (var textInfo in this.lineList[line].Texts)
                {
                    var decoratedText = textInfo.DecoratedText;

                    // 消しておきます。
                    decoratedText.IsUpdateVisual = false;
                    this.textPanel.Children.Remove(decoratedText);                    
                }

                return;
            }

            // 表示されるなら、指定の表示位置に文字オブジェクトを表示します。
            for (var i = 0; i < this.lineList[line].Texts.Count(); ++i)
            {
                var textInfo = this.lineList[line].Texts[i];
                var decoratedText = textInfo.DecoratedText;

                // FormattedTextの作成を行います。
                decoratedText.IsUpdateVisual = true;
                var ft = decoratedText.FormattedText;

                decoratedText.Opacity = GetOpacity(decoratedText, posY);
                Canvas.SetLeft(decoratedText, GetTextLeft(textInfo));
                Canvas.SetTop(decoratedText, GetTextTop(textInfo, posY, ft));

                // 追加しておきます。
                if (!this.textPanel.Children.Contains(decoratedText))
                {
                    this.textPanel.Children.Add(decoratedText);
                }
            }
        }
        #endregion

        private Border background;
        private Canvas textPanel;
        private List<Column> columnList = new List<Column>();
        private List<LineInfo> lineList = new List<LineInfo>();
        private DateTime prevUpdateTime = DateTime.Now;

        /// <summary>
        /// テンプレートが適用されたときに呼ばれます。
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.background = GetTemplateChild(ElementBackgroundName) as Border;
            this.textPanel = GetTemplateChild(ElementTextPanelName) as Canvas;

            // このコントロールの背景を実際の背景色に設定します。
            if (this.background != null)
            {
                this.background.Background = Background;
            }
        }

        /// <summary>
        /// 背景色の変更時に呼ばれます。
        /// </summary>
        private static void BackgroundChanged(DependencyObject d,
                                              DependencyPropertyChangedEventArgs e)
        {
            var self = d as EndRollControl;

            if (self != null && self.background != null)
            {
                self.background.Background = (Brush)e.NewValue;
            }
        }

        /// <summary>
        /// 静的コンストラクタ
        /// </summary>
        static EndRollControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(EndRollControl),
                new FrameworkPropertyMetadata(typeof(EndRollControl)));
            BackgroundProperty.OverrideMetadata(
                typeof(EndRollControl),
                new FrameworkPropertyMetadata(Brushes.Black, BackgroundChanged));
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EndRollControl()
        {
            InitializeBindings(this);

            // Unloadedはアプリ終了時には呼ばれませんが
            // この場合は問題ありません。
            Unloaded += (_, __) => UpdateAutoUpdate(false);

            UpdateAutoUpdate(AutoUpdate);
        }

        /// <summary>
        /// 定期的な画面更新を行います。
        /// </summary>
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            var diff = now - this.prevUpdateTime;
            if (diff < TimeSpan.FromMilliseconds(20))
            {
                return;
            }

            if (UpdateScreen(TimeSpan.FromSeconds(CurrentPos) + diff))
            {
                this.prevUpdateTime = now;
            }
        }

        /// <summary>
        /// コマンドをバインドします。
        /// </summary>
        public void InitializeBindings(UIElement element)
        {
            element.CommandBindings.Add(
                new CommandBinding(
                    PlayCommand,
                    (sender, e) => Play(),
                    (sender, e) => e.CanExecute = (State != EndRollState.Play)));
            element.CommandBindings.Add(
                new CommandBinding(
                    PauseCommand,
                    (sender, e) => Pause(),
                    (sender, e) => e.CanExecute = (State == EndRollState.Play)));
            element.CommandBindings.Add(
                new CommandBinding(
                    StopCommand,
                    (sender, e) => Stop(),
                    (sender, e) => e.CanExecute = (State != EndRollState.Stop)));
        }
    }
}
