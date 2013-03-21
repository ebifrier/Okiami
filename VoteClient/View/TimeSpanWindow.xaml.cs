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
    /// <summary>
    /// TimeSpanWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class TimeSpanWindow : Window
    {
        private readonly TimeSpanWindowModel model;

        /// <summary>
        /// 時間間隔を取得または設定します。
        /// </summary>
        public TimeSpan TimeSpan
        {
            get
            {
                return this.model.TimeSpan;
            }
            set
            {
                this.model.Minutes = (int)value.TotalMinutes;
                this.model.Seconds = value.Seconds;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TimeSpanWindow(TimeSpan timeSpan)
        {
            InitializeComponent();
            InitializeCommands();

            this.model = new TimeSpanWindowModel();
            this.DataContext = this.model;

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

    /// <summary>
    /// <see cref="TimeSpanWindow"/>用のモデルオブジェクトです。
    /// </summary>
    public class TimeSpanWindowModel : IModel
    {
        private int minutes = 0;
        private int seconds = 0;

        /// <summary>
        /// プロパティ値の変更イベントです。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// プロパティ値の変更を通知します。
        /// </summary>
        public void NotifyPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = this.PropertyChanged;

            if (handler != null)
            {
                Util.SafeCall(() =>
                    handler(this, e));
            }
        }

        /// <summary>
        /// 分間隔を取得または設定します。
        /// </summary>
        public int Minutes
        {
            get
            {
                return this.minutes;
            }
            set
            {
                if (this.minutes != value)
                {
                    this.minutes = value;

                    this.RaisePropertyChanged("Minutes");
                }
            }
        }

        /// <summary>
        /// 秒間隔を取得または設定します。
        /// </summary>
        public int Seconds
        {
            get
            {
                return this.seconds;
            }
            set
            {
                if (this.seconds != value)
                {
                    this.seconds = value;

                    this.RaisePropertyChanged("Seconds");
                }
            }
        }

        /// <summary>
        /// 時間間隔を取得します。
        /// </summary>
        [DependOnProperty("Minutes")]
        [DependOnProperty("Seconds")]
        public TimeSpan TimeSpan
        {
            get
            {
                return TimeSpan.FromSeconds(
                    60 * this.minutes + this.seconds);
            }
        }
    }
}
