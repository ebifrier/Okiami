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

namespace VoteSystem.Protocol.View
{
    /// <summary>
    /// VoteResultSettingDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class VoteResultSettingDialog : Window
    {
        private sealed class InternalModel : DynamicDictionary
        {
            public int DisplayCandidateCount
            {
                get { return GetValue<int>("DisplayCandidateCount"); }
                set { SetValue("DisplayCandidateCount", value); }
            }

            [DependOnProperty("IsShowStroke")]
            [DependOnProperty("StrokeThicknessInternal")]
            public decimal StrokeThickness
            {
                get
                {
                    return (GetValue<bool>("IsShowStroke") ?
                        Convert.ToDecimal(GetValue<object>("StrokeThicknessInternal")) :
                        0.0m);
                }
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

        /// <summary>
        /// 一時的に保存するプロパティ値のリストです。
        /// </summary>
        private static readonly Tuple<string, Type>[] SavePropertyList =
        {
            Tuple.Create("Background", typeof(Brush)),
            Tuple.Create("Foreground", typeof(Brush)),            
            Tuple.Create("FontFamily", typeof(FontFamily)),
            Tuple.Create("FontWeight", typeof(FontWeight)),
            Tuple.Create("FontStyle", typeof(FontStyle)),
            Tuple.Create("IsShowStroke", typeof(bool)),
            Tuple.Create("Stroke", typeof(Brush)),
            Tuple.Create("StrokeThicknessInternal", typeof(decimal)),
            Tuple.Create("DisplayCandidateCount", typeof(int)),
            Tuple.Create("IsDisplayPointFullWidth", typeof(bool)),
        };

        private InternalModel SavePropertyValues(VoteResultControl control)
        {
            var model = new InternalModel();

            SavePropertyList.ForEach(_ =>
                model.SetValue(
                    _.Item1,
                    MethodUtil.GetPropertyValue(control, _.Item1)));

            return model;
        }

        /// <summary>
        /// doubleやintがdecimalとして扱われることがあるので、わざわざ変換します。
        /// </summary>
        private object CastValue(object value, Type type)
        {
            if (type == typeof(int))
            {
                return Convert.ToInt32(value);
            }
            else if (type == typeof(decimal))
            {
                return Convert.ToDecimal(value);
            }
            else if (type == typeof(bool))
            {
                return Convert.ToBoolean(value);
            }
            else
            {
                return value;
            }
        }

        private void RestorePropertyValues(VoteResultControl control, InternalModel model)
        {
            SavePropertyList.ForEach(_ =>
                MethodUtil.SetPropertyValue(
                    control, _.Item1,
                    CastValue(model.GetValue<object>(_.Item1), _.Item2)));
        }

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

            this.model = SavePropertyValues(control);
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
                    DialogCommands.OK,
                    ExecuteYes));
            CommandBindings.Add(
                new CommandBinding(
                    DialogCommands.Cancel,
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
            RestorePropertyValues(this.control, this.model);

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
            var brush = this.model.GetValue<Brush>(name);
            var solidColorBrush = brush as SolidColorBrush;
            if (solidColorBrush == null)
            {
                return;
            }

            var result = DialogUtil.ShowColorDialog(
                solidColorBrush.Color, this);
            this.model.SetValue(name,
                (result != null ?
                    new SolidColorBrush(result.Value) :
                    brush));
        }
    }
}
