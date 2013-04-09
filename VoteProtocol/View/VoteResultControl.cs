using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;
using System.Globalization;

using Ragnarok;
using Ragnarok.Presentation;

namespace VoteSystem.Protocol.View
{
    using Vote;

    /// <summary>
    /// 投票結果を表示します。
    /// </summary>
    public partial class VoteResultControl : UserControl
    {
        /// <summary>
        /// 投票結果を扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty VoteResultProperty =
            DependencyProperty.Register(
                "VoteResult",
                typeof(VoteResult),
                typeof(VoteResultControl),
                new FrameworkPropertyMetadata(new VoteResult(),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnVoteResultChanged));

        /// <summary>
        /// 投票結果を取得または設定します。
        /// </summary>
        public VoteResult VoteResult
        {
            get { return (Vote.VoteResult)GetValue(VoteResultProperty); }
            set { SetValue(VoteResultProperty, value); }
        }

        /// <summary>
        /// 表示する投票候補の数を扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty DisplayCandidateCountProperty =
            DependencyProperty.Register(
                "DisplayCandidateCount",
                typeof(int),
                typeof(VoteResultControl),
                new FrameworkPropertyMetadata(5,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnVoteResultChanged));

        /// <summary>
        /// 表示する投票候補の数を取得または設定します。
        /// </summary>
        public int DisplayCandidateCount
        {
            get { return (int)GetValue(DisplayCandidateCountProperty); }
            set { SetValue(DisplayCandidateCountProperty, value); }
        }

        /// <summary>
        /// 表示する投票候補を扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty DisplayCandidateListProperty =
            DependencyProperty.Register(
                "DisplayCandidateList",
                typeof(VoteCandidatePair[]),
                typeof(VoteResultControl),
                new FrameworkPropertyMetadata(new VoteCandidatePair[0]));

        /// <summary>
        /// 表示する投票候補を取得します。
        /// </summary>
        public VoteCandidatePair[] DisplayCandidateList
        {
            get { return (VoteCandidatePair[])GetValue(DisplayCandidateListProperty); }
            private set { SetValue(DisplayCandidateListProperty, value); }
        }

        /// <summary>
        /// 表示用の投票候補リストを更新します。
        /// </summary>
        private static void OnVoteResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (VoteResultControl)d;

            self.UpdateDisplayCandidateList();
        }

        /// <summary>
        /// 表示用の投票候補リストを作成します。
        /// </summary>
        private void UpdateDisplayCandidateList()
        {
            if (VoteResult == null || VoteResult.CandidateList == null)
            {
                DisplayCandidateList = new VoteCandidatePair[0];
                return;
            }

            var candidateList = VoteResult.CandidateList;
            var count = DisplayCandidateCount;
            var shortCount = count - candidateList.Count();

            if (shortCount <= 0)
            {
                // 候補が指定の数より多い場合は、多い分を切り取ります。
                DisplayCandidateList = candidateList.Take(count).ToArray();
            }
            else
            {
                // 候補の数が不足している場合は、ダミーの結果を末尾に追加します。
                DisplayCandidateList = candidateList.Concat(
                    from i in Enumerable.Range(0, shortCount)
                    select new VoteCandidatePair()
                    {
                        Candidate = null,
                        Point = 0
                    }).ToArray();
            }
        }

        /// <summary>
        /// 投票状態を扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty VoteStateProperty =
            DependencyProperty.Register(
                "VoteState",
                typeof(VoteState),
                typeof(VoteResultControl),
                new FrameworkPropertyMetadata(VoteState.Stop));
        
        /// <summary>
        /// 投票状態を取得または設定します。
        /// </summary>
        public VoteState VoteState
        {
            get { return (VoteState)GetValue(VoteStateProperty); }
            set { SetValue(VoteStateProperty, value); }
        }

        /// <summary>
        /// 全投票時間を扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty TotalVoteLeaveTimeProperty =
            DependencyProperty.Register(
                "TotalVoteLeaveTime",
                typeof(TimeSpan),
                typeof(VoteResultControl),
                new FrameworkPropertyMetadata(TimeSpan.Zero));

        /// <summary>
        /// 全投票時間を取得または設定します。
        /// </summary>
        public TimeSpan TotalVoteLeaveTime
        {
            get { return (TimeSpan)GetValue(TotalVoteLeaveTimeProperty); }
            set { SetValue(TotalVoteLeaveTimeProperty, value); }
        }

        /// <summary>
        /// 投票時間を扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty VoteLeaveTimeProperty =
            DependencyProperty.Register(
                "VoteLeaveTime",
                typeof(TimeSpan),
                typeof(VoteResultControl),
                new FrameworkPropertyMetadata(TimeSpan.Zero));

        /// <summary>
        /// 投票時間を取得または設定します。
        /// </summary>
        public TimeSpan VoteLeaveTime
        {
            get { return (TimeSpan)GetValue(VoteLeaveTimeProperty); }
            set { SetValue(VoteLeaveTimeProperty, value); }
        }

        /// <summary>
        /// 全投票時間を表示するかどうかを扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty IsShowTotalVoteTimeProperty =
            DependencyProperty.Register(
                "IsShowTotalVoteTime",
                typeof(bool),
                typeof(VoteResultControl),
                new FrameworkPropertyMetadata(true,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// 全投票時間を表示するかどうかを取得または設定します。
        /// </summary>
        public bool IsShowTotalVoteTime
        {
            get { return (bool)GetValue(IsShowTotalVoteTimeProperty); }
            set { SetValue(IsShowTotalVoteTimeProperty, value); }
        }

        /// <summary>
        /// ポイントを全角で表示するかを扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty IsDisplayPointFullWidthProperty =
            DependencyProperty.Register(
                "IsDisplayPointFullWidth",
                typeof(bool),
                typeof(VoteResultControl),
                new FrameworkPropertyMetadata(false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// ポイントを全角で表示するかどうかを取得または設定します。
        /// </summary>
        public bool IsDisplayPointFullWidth
        {
            get { return (bool)GetValue(IsDisplayPointFullWidthProperty); }
            set { SetValue(IsDisplayPointFullWidthProperty, value); }
        }

        /// <summary>
        /// 文字の縁取りを行うかを示す依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty IsShowStrokeProperty =
            DependencyProperty.Register(
                "IsShowStroke",
                typeof(bool),
                typeof(VoteResultControl),
                new FrameworkPropertyMetadata(true,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnIsShowStrokeChanged));

        private static void OnIsShowStrokeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (VoteResultControl)d;

            // StrokeThicknessの値は使っていないため、
            // とにかく変更通知が行けばよい。
            self.StrokeThickness = self.StrokeThickness + 1;
        }

        /// <summary>
        /// 文字の縁取りを行うかをどうかを取得または設定します。
        /// </summary>
        public bool IsShowStroke
        {
            get { return (bool)GetValue(IsShowStrokeProperty); }
            set { SetValue(IsShowStrokeProperty, value); }
        }

        /// <summary>
        /// 文字の縁取りを示す依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register(
                "Stroke",
                typeof(Brush),
                typeof(VoteResultControl),
                new FrameworkPropertyMetadata(Brushes.Transparent,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// 文字の縁を塗るブラシを取得または設定します。
        /// </summary>
        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        /// <summary>
        /// 文字の縁取り幅を示す依存プロパティです。(内部用)
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessInternalProperty =
            DependencyProperty.Register(
                "StrokeThicknessInternal",
                typeof(decimal),
                typeof(VoteResultControl),
                new FrameworkPropertyMetadata(0.5m,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnStrokeThicknessInternalChanged));

        private static void OnStrokeThicknessInternalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (VoteResultControl)d;

            // 値の変更通知を出す。
            self.StrokeThickness = self.StrokeThickness + 1;
        }

        /// <summary>
        /// 文字の縁の太さを取得または設定します。(内部用)
        /// </summary>
        public decimal StrokeThicknessInternal
        {
            get { return (decimal)GetValue(StrokeThicknessInternalProperty); }
            set { SetValue(StrokeThicknessInternalProperty, value); }
        }

        /// <summary>
        /// 文字の縁取り幅を示す依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(
                "StrokeThickness",
                typeof(decimal),
                typeof(VoteResultControl),
                new FrameworkPropertyMetadata(0.5m,
                    OnStrokeThicknessChanged, CoerceStrokeThickness));

        private static void OnStrokeThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // ダミー
        }

        private static object CoerceStrokeThickness(DependencyObject d, object baseValue)
        {
            var self = (VoteResultControl)d;
            if (!self.IsShowStroke)
            {
                return 0.0m;
            }
            
            // 内部的にはStrokeThicknessInternalの値を使う。
            return self.StrokeThicknessInternal;
        }

        /// <summary>
        /// 文字の縁の太さを取得または設定します。
        /// </summary>
        public decimal StrokeThickness
        {
            get { return (decimal)GetValue(StrokeThicknessProperty); }
            private set { SetValue(StrokeThicknessProperty, value); }
        }

        /// <summary>
        /// 設定が更新されたときに呼ばれるイベントです。
        /// </summary>
        public static readonly RoutedEvent SettingUpdatedEvent =
            EventManager.RegisterRoutedEvent(
                "SettingUpdated",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(VoteResultControl));

        /// <summary>
        /// 設定が更新されたときに呼ばれるイベントです。
        /// </summary>
        public event RoutedEventHandler SettingUpdated
        {
            add { AddHandler(SettingUpdatedEvent, value); }
            remove { RemoveHandler(SettingUpdatedEvent, value); }
        }

        /// <summary>
        /// 設定ダイアログを開きます。
        /// </summary>
        public static readonly ICommand OpenSettingDialog =
            new RoutedUICommand(
                "設定ダイアログを新たに開きます。",
                "OpenSettingDialog",
                typeof(Window));

        /// <summary>
        /// 設定ダイアログを新たに開きます。
        /// </summary>
        private void ExecuteOpenSettingDialog(object sender,
                                              ExecutedRoutedEventArgs e)
        {
            try
            {
                var dialog = new VoteResultSettingDialog(this)
                {
                    Topmost = true,
                };

                if (dialog.ShowDialog() == true)
                {
                    RaiseEvent(new RoutedEventArgs(SettingUpdatedEvent));
                }
            }
            catch (Exception ex)
            {
                Util.ThrowIfFatal(ex);

                Log.ErrorException(ex,
                    "設定ダイアログで例外が発生しました。");
            }
        }

        /// <summary>
        /// 静的コンストラクタ
        /// </summary>
        static VoteResultControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(VoteResultControl),
                new FrameworkPropertyMetadata(typeof(VoteResultControl)));
            BackgroundProperty.OverrideMetadata(
                typeof(VoteResultControl),
                new FrameworkPropertyMetadata(Brushes.Transparent,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
            ForegroundProperty.OverrideMetadata(
                typeof(VoteResultControl),
                new FrameworkPropertyMetadata(Brushes.Black,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
            FontFamilyProperty.OverrideMetadata(
                typeof(VoteResultControl),
                new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
            FontStyleProperty.OverrideMetadata(
                typeof(VoteResultControl),
                new FrameworkPropertyMetadata(FontStyles.Normal,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
            FontWeightProperty.OverrideMetadata(
                typeof(VoteResultControl),
                new FrameworkPropertyMetadata(FontWeights.Normal,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        }

        /// <summary>
        /// このコントロール専用のコマンドを登録します。
        /// </summary>
        public void InitializeBindings(UIElement element)
        {
            element.CommandBindings.Add(
                new CommandBinding(
                    OpenSettingDialog,
                    ExecuteOpenSettingDialog));
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VoteResultControl()
        {
            InitializeBindings(this);

            UpdateDisplayCandidateList();
        }
    }
}
