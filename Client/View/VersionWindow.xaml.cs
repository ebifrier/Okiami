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

using Ragnarok.Presentation;

namespace VoteSystem.Client.View
{
    /// <summary>
    /// VersionWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class VersionWindow : Window
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VersionWindow()
        {
            InitializeComponent();

            CommandBindings.Add(
                new CommandBinding(
                    DialogCommands.OK,
                    ExecuteOK));
        }

        void ExecuteOK(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
