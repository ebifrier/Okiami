using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Ragnarok;
using Ragnarok.Utility;
using Ragnarok.ObjectModel;
using Ragnarok.Presentation;
using Ragnarok.Presentation.Utility;

namespace VoteSystem.Protocol.View
{
    /// <summary>
    /// VoteResultSettingDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class VoteResultSettingDialog : Window
    {
        private sealed class InternalModel : CloneModel
        {
            [DependOnProperty("IsShowStroke")]
            [DependOnProperty("StrokeThicknessInternal")]
            public decimal StrokeThickness
            {
                get
                {
                    return ((bool)this["IsShowStroke"] ?
                        (decimal)this["StrokeThicknessInternal"] :
                        0.0m);
                }
            }

            public InternalModel(VoteResultControl control)
                : base(control)
            {
            }
        }

        /// <summary>
        /// 背景の色選択コマンドです。
        /// </summary>
        public readonly static ICommand SelectBackgroundColor =
            new RoutedUICommand(
                "背景色を選択します。",
                "SelectBackgroundColor",
                typeof(Window));
        /// <summary>
        /// 文字色の選択コマンドです。
        /// </summary>
        public readonly static ICommand SelectFontColor =
            new RoutedUICommand(
                "文字色を選択します。",
                "SelectFontColor",
                typeof(Window));
        /// <summary>
        /// 文字の縁色の選択コマンドです。
        /// </summary>
        public readonly static ICommand SelectFontEdgeColor =
            new RoutedUICommand(
                "文字の縁色を選択します。",
                "SelectFontEdgeColor",
                typeof(Window));

        private readonly VoteResultControl control;
        private readonly InternalModel model;

        /// <summary>
        /// ListBoxの選択アイテムを更新します。
        /// </summary>
        private void VoteResultSettingDialog_Loaded(object sender, RoutedEventArgs e)
        {
            this.fontFamilyListBox.ScrollIntoView(
                this.control.FontFamily);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VoteResultSettingDialog(VoteResultControl control)
        {
            InitializeComponent();
            InitCommands();

            this.model = new InternalModel(control);
            this.control = control;

            DataContext = this.model;
            Loaded += VoteResultSettingDialog_Loaded;
        }

        /// <summary>
        /// コマンドバインディングを行います。
        /// </summary>
        private void InitCommands()
        {
            CommandBindings.Add(
                new CommandBinding(
                    RagnarokCommands.OK,
                    ExecuteYes));
            CommandBindings.Add(
                new CommandBinding(
                    RagnarokCommands.Cancel,
                    ExecuteNo));

            CommandBindings.Add(
                new CommandBinding(
                    SelectBackgroundColor,
                    (_, __) => SelectColor("Background")));
            CommandBindings.Add(
                new CommandBinding(
                    SelectFontColor,
                    (_, __) => SelectColor("Foreground")));
            CommandBindings.Add(
                new CommandBinding(
                    SelectFontEdgeColor,
                    (_, __) => SelectColor("Stroke")));
        }

        /// <summary>
        /// OK/YES
        /// </summary>
        private void ExecuteYes(object sender, ExecutedRoutedEventArgs e)
        {
            // OKの場合は、プロパティ値をコントロールに設定します。
            this.model.SetValuesToTarget(this.control);

            DialogResult = true;
        }

        /// <summary>
        /// Cancel/NO
        /// </summary>
        private void ExecuteNo(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// 色を選択します。
        /// </summary>
        private void SelectColor(string name)
        {
            var brush = (Brush)this.model[name];
            var solidColorBrush = brush as SolidColorBrush;
            if (solidColorBrush == null)
            {
                return;
            }

            var result = DialogUtil.ShowColorDialog(
                solidColorBrush.Color, this);
            this.model[name] =
                (result != null ?
                    new SolidColorBrush(result.Value) :
                    brush);
        }
    }
}
