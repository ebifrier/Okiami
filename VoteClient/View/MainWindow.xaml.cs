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
            Command.UtilCommand.Bind(this);
            Command.LiveCommands.Bind(this);
            Command.VoteCommands.Bind(this);
            RagnarokCommands.Bind(this);

            Closing += (sender, e) =>
            {
                var voteClient = Global.VoteClient;
                if (voteClient != null && voteClient.IsLogined)
                {
                    e.Cancel = !MessageUtil.Confirm(string.Format(
                        "投票ルームに入室中です。{0}{0}" +
                        "それでもアプリを終了しますか？ (ﾟｰﾟ*?)",
                        Environment.NewLine),
                        "確認");
                }
            };

            Closed += (sender, e) =>
            {
                if (Global.VoteClient != null)
                {
                    Global.VoteClient.Disconnect();
                }

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
