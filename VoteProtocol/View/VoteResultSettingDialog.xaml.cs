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
        private sealed class InternalModel : ViewModelProxy
        {
            private Color backgroundColor;
            private Color foregroundColor;
            private Color strokeColor;

            public Color BackgroundColor
            {
                get { return this.backgroundColor; }
                set
                {
                    if (this.backgroundColor != value)
                    {
                        this.backgroundColor = value;
                        this["Background"] = new SolidColorBrush(this.backgroundColor);

                        this.RaisePropertyChanged("BackgroundColor");
                    }
                }
            }

            public Color ForegroundColor
            {
                get { return this.foregroundColor; }
                set
                {
                    if (this.foregroundColor != value)
                    {
                        this.foregroundColor = value;
                        this["Foreground"] = new SolidColorBrush(this.foregroundColor);

                        this.RaisePropertyChanged("ForegroundColor");
                    }
                }
            }

            public Color StrokeColor
            {
                get { return this.strokeColor; }
                set
                {
                    if (this.strokeColor != value)
                    {
                        this.strokeColor = value;
                        this["Stroke"] = new SolidColorBrush(this.strokeColor);

                        this.RaisePropertyChanged("StrokeColor");
                    }
                }
            }

            [DependOnProperty(typeof(VoteResultControl), "IsShowStroke")]
            [DependOnProperty(typeof(VoteResultControl), "StrokeThicknessInternal")]
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
                var brush = this["Background"] as SolidColorBrush;
                if (brush != null)
                {
                    this.backgroundColor = brush.Color;
                }

                brush = this["Foreground"] as SolidColorBrush;
                if (brush != null)
                {
                    this.foregroundColor = brush.Color;
                }

                brush = this["Stroke"] as SolidColorBrush;
                if (brush != null)
                {
                    this.strokeColor = brush.Color;
                }
            }
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
        }

        /// <summary>
        /// OK/YES
        /// </summary>
        private void ExecuteYes(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>
        /// Cancel/NO
        /// </summary>
        private void ExecuteNo(object sender, ExecutedRoutedEventArgs e)
        {
            // Cancelの場合は、プロパティ値を元の状態に戻します。
            this.model.RollbackViewModel();

            DialogResult = false;
        }
    }
}
