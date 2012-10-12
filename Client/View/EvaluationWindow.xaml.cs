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
using System.Windows.Shapes;

using Ragnarok;
using Ragnarok.Presentation.Control;

namespace VoteSystem.Client.View
{
    using VoteSystem.Client.ViewModel;

    /// <summary>
    /// 評価値ウィンドウです。
    /// </summary>
    public partial class EvaluationWindow : MovableWindow
    {
        /// <summary>
        /// 設定ダイアログを開くコマンドです。
        /// </summary>
        public static readonly ICommand OpenSettingDialog =
            new RoutedUICommand(
                "設定ダイアログを開きます。",
                "OpenSettingDialog",
                typeof(EndRollWindow));

        /// <summary>
        /// 静的コンストラクタ
        /// </summary>
        static EvaluationWindow()
        {
            MovableWindow.IsMovableProperty.OverrideMetadata(
                typeof(EvaluationWindow),
                new FrameworkPropertyMetadata(true));

            MovableWindow.EdgeLengthProperty.OverrideMetadata(
                typeof(EvaluationWindow),
                new FrameworkPropertyMetadata(10.0));
        }

        /// <summary>
        /// 設定ダイアログを開きます。
        /// </summary>
        private void ExecuteOpenSettingDialog(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new Control.EvaluationWindowSettingDialog()
            {
                DataContext = this.layoutBase.DataContext,
                Owner = this,
            };

            dialog.ShowDialog();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EvaluationWindow(EvaluationWindowViewModel model)
        {
            InitializeComponent();

            CommandBindings.Add(
                new CommandBinding(
                    OpenSettingDialog,
                    ExecuteOpenSettingDialog));

            this.layoutBase.DataContext = model;
        }
    }
}
