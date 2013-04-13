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
using System.ComponentModel;

using Ragnarok;
using Ragnarok.Presentation;
using Ragnarok.ObjectModel;

namespace VoteSystem.Client.View
{
    using Protocol;

    /// <summary>
    /// TimeSpanWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class TimeSpanWindow : Window
    {
        private readonly SimpleTimeSpan stimeSpan;

        /// <summary>
        /// 時間間隔を取得または設定します。
        /// </summary>
        public TimeSpan TimeSpan
        {
            get
            {
                return this.stimeSpan.TimeSpan;
            }
            set
            {
                this.stimeSpan.Minutes = (int)value.TotalMinutes;
                this.stimeSpan.Seconds = value.Seconds;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TimeSpanWindow(TimeSpan timeSpan)
        {
            InitializeComponent();
            InitializeCommands();

            this.stimeSpan = new SimpleTimeSpan();
            this.DataContext = this.stimeSpan;

            Activated += delegate { this.tabTopControl.Focus(); };
            Closed += delegate { Global.Settings.Reload(); };

            TimeSpan = timeSpan;
        }

        /// <summary>
        /// コマンドを初期化します。
        /// </summary>
        private void InitializeCommands()
        {
            CommandBindings.Add(
                new CommandBinding(
                    RagnarokCommands.OK,
                    ExecuteOk));
            CommandBindings.Add(
                new CommandBinding(
                    RagnarokCommands.Cancel,
                    ExecuteCancel));
        }

        /// <summary>
        /// OKボタン押下。
        /// </summary>
        private void ExecuteOk(object sender,
                               ExecutedRoutedEventArgs e)
        {
            Global.Settings.Save();

            DialogResult = true;
        }

        /// <summary>
        /// キャンセルボタン押下。
        /// </summary>
        private void ExecuteCancel(object sender,
                                   ExecutedRoutedEventArgs e)
        {
            Global.Settings.Reload();

            DialogResult = false;
        }
    }
}
