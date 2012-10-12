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

using Ragnarok.Presentation.Control;

namespace VoteSystem.Client.View
{
    /// <summary>
    /// 投票結果を表示します。
    /// </summary>
    public partial class VoteResultWindow : MovableWindow
    {
        /// <summary>
        /// 設定ダイアログを開きます。
        /// </summary>
        public readonly static ICommand OpenSettingDialog =
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
            var dialog = new View.VoteResultSettingDialog()
            {
                DataContext = this.DataContext,
                Owner = this,
            };

            dialog.ShowDialog();
        }

        /// <summary>
        /// 静的コンストラクタ
        /// </summary>
        static VoteResultWindow()
        {
            TopmostProperty.OverrideMetadata(
                typeof(VoteResultWindow),
                new FrameworkPropertyMetadata(true));
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VoteResultWindow()
        {
            InitializeComponent();

            CommandBindings.Add(
                new CommandBinding(
                    OpenSettingDialog,
                    ExecuteOpenSettingDialog));

            EdgeLength = 20;
        }
    }
}
