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

namespace VoteSystem.Client.View
{
    /// <summary>
    /// クライアントのメインウィンドウです。
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Command.UtilCommand.Bind(CommandBindings);
            Command.LiveCommands.Bind(CommandBindings);
            Command.VoteCommands.Bind(CommandBindings);

            Closing += (sender, e) =>
            {
                var voteClient = Global.VoteClient;
                if (voteClient != null && voteClient.IsLogined)
                {
                    MessageUtil.ErrorMessage(string.Format(
                        "投票ルームに入室中です。{0}{0}" +
                        "一度退室してから、アプリを終了してください。(T▽T)",
                        Environment.NewLine));

                    e.Cancel = true;
                }
            };

            Closed += (sender, e) =>
            {
                Global.StatusBar = null;
                Global.MainWindow = null;

                // メインウィンドウが閉じられたら、
                // プラグインなどもすべて終了します。
                Application.Current.Shutdown();
            };

            this.DataContext = Global.MainViewModel;
            Global.MainWindow = this;
            Global.StatusBar = this.mainStatusBar;
        }
    }
}
