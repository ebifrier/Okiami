using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using Ragnarok;
using Ragnarok.Extra.Sound;
using Ragnarok.Presentation;

namespace VoteSystem.Client.Command
{
    public static partial class Commands
    {
        /// <summary>
        /// ネットワーク関連の速度テストを行います。
        /// </summary>
        public readonly static ICommand NetworkProfile =
            new RoutedUICommand(
                "ネットワークの速度テストを行います。",
                "NetworkProfile",
                typeof(Window));
        /// <summary>
        /// NCVのログを投票サーバーに送ります。
        /// </summary>
        public readonly static ICommand PostComments =
            new RoutedUICommand(
                "NCVのログを投票サーバーに送ります。",
                "PostComments",
                typeof(Window));
        /// <summary>
        /// エラーログを送信します。
        /// </summary>
        public readonly static ICommand SendErrorLog =
            new RoutedUICommand(
                "エラーログを送信します。",
                "SendErrorLog",
                typeof(Window));
        /// <summary>
        /// バージョンを表示します。
        /// </summary>
        public readonly static ICommand ShowVersion =
            new RoutedUICommand(
                "バージョンを表示します。",
                "ShowVersion",
                typeof(Window));

        /// <summary>
        /// 指定のURLを開きます。
        /// </summary>
        public readonly static ICommand NavigateUrl =
            new RoutedUICommand(
                "指定のURLを開きます。",
                "NavigateUrl",
                typeof(Window));
        /// <summary>
        /// 指定の音声を再生します。
        /// </summary>
        public readonly static ICommand PlaySound =
            new RoutedUICommand(
                "指定の音声を再生します。",
                "PlaySound",
                typeof(Window));

        /// <summary>
        /// プラグインを実行します。
        /// </summary>
        public readonly static ICommand RunPlugin =
            new RoutedUICommand(
                "プラグインを実行します。",
                "RunPlugin",
                typeof(Window));
        /// <summary>
        /// 設定ダイアログを開きます。
        /// </summary>
        public readonly static ICommand OpenSettingDialog =
            new RoutedUICommand(
                "設定ダイアログを開きます。",
                "OpenSettingDialog",
                typeof(Window));
        /// <summary>
        /// 投票結果ウィンドウを新たに開きます。
        /// </summary>
        public readonly static ICommand OpenVoteResultWindow =
            new RoutedUICommand(
                "投票結果ウィンドウを新たに開きます。",
                "OpenVoteResultWindow",
                typeof(Window));
        /// <summary>
        /// 評価値ウィンドウを新たに開きます。
        /// </summary>
        public readonly static ICommand OpenEvaluationWindow =
            new RoutedUICommand(
                "評価値ウィンドウを新たに開きます。",
                "OpenEvaluationWindow",
                typeof(Window));
        /// <summary>
        /// エンドロールウィンドウを新たに開きます。
        /// </summary>
        public readonly static ICommand OpenEndRollWindow =
            new RoutedUICommand(
                "エンドロールウィンドウを新たに開きます。",
                "OpenEndRollWindow",
                typeof(Window));
    }

    /// <summary>
    /// ユーティリティ的なコマンドを実行します。
    /// </summary>
    public static class UtilCommand
    {
        /// <summary>
        /// コマンドをバインドします。
        /// </summary>
        public static void Bind(CommandBindingCollection bindings)
        {
            bindings.Add(
                new CommandBinding(
                    Commands.NetworkProfile,
                    ExecuteNetworkProfile));
            bindings.Add(
                new CommandBinding(
                    Commands.PostComments,
                    ExecutePostComments));
            bindings.Add(
                new CommandBinding(
                    Commands.SendErrorLog,
                    ExecuteSendErrorLog));
            bindings.Add(
                new CommandBinding(
                    Commands.ShowVersion,
                    ExecuteShowVersion));

            bindings.Add(
                new CommandBinding(
                    Commands.NavigateUrl,
                    ExecuteNavigateUrl));
            bindings.Add(
                new CommandBinding(
                    Commands.PlaySound,
                    ExecutePlaySound));

            bindings.Add(
                new CommandBinding(
                    Commands.RunPlugin,
                    ExecuteRunPlugin));
            bindings.Add(
                new CommandBinding(
                    Commands.OpenSettingDialog,
                    ExecuteOpenSettingDialog));
            bindings.Add(
                new CommandBinding(
                    Commands.OpenVoteResultWindow,
                    ExecuteOpenVoteResultWindow));
            bindings.Add(
                new CommandBinding(
                    Commands.OpenEvaluationWindow,
                    ExecuteOpenEvaluationWindow));
            bindings.Add(
                new CommandBinding(
                    Commands.OpenEndRollWindow,
                    ExecuteOpenEndRollWindow));
        }

        private static List<Model.VoteClient> voteClients =
            new List<Model.VoteClient>();

        /// <summary>
        /// ネットワークの負荷テストを行います。
        /// </summary>
        private static void ExecuteNetworkProfile(object sender,
                                                  ExecutedRoutedEventArgs e)
        {
            voteClients.ForEach(_ => _.Disconnect());
            voteClients.Clear();

            for (var i = 0; i < 100; ++i)
            {
                var client = new Model.VoteClient(false)
                {
                    IsShowErrorMessage = false,
                };

                client.Connect(
                    Protocol.ServerSettings.VoteAddress,
                    Protocol.ServerSettings.VotePort);
                client.EnterVoteRoom(
                    0, null, Guid.NewGuid(),
                    "テストさん" + i,
                    null,
                    (_, e_) =>
                    {
                        if (e_.ErrorCode == 0)
                        {
                            lock (voteClients)
                            {
                                voteClients.Add(client);
                            }
                        }
                    });
                System.Threading.Thread.Sleep(2000);
            }
#if false
            const int MaxCount = 50;
            DateTime startTime;
            var count = 0;

            using (var voteClient = new Model.VoteClient(false))
            {
                try
                {
                    voteClient.Connect(
                        Protocol.ServerSettings.VoteAddress,
                        Protocol.ServerSettings.VotePort);

                    startTime = DateTime.Now;

                    System.Threading.Tasks.Parallel.For(
                        0, MaxCount,
                        (index) =>
                        {
                            voteClient.GetVoteRoomCount(
                                (sender_, e_) =>
                                {
                                    System.Threading.Interlocked.Increment(ref count);
                                });
                        });

                    while (true)
                    {
                        if (count != MaxCount)
                        {
                            System.Threading.Thread.Yield();
                            continue;
                        }

                        break;
                    }
                }
                catch (Exception ex)
                {
                    MessageUtil.ErrorMessage(ex,
                        "ネットワークテストに失敗しました。");
                    return;
                }
            }

            var ellapse = DateTime.Now - startTime;
            MessageBox.Show(
                string.Format(
                    "ネットワーク経過時間\n" +
                    "    {0}回 {1}ms\n" +
                    "       1回 {2}ms",
                    MaxCount,
                    ellapse.TotalMilliseconds,
                    ellapse.TotalMilliseconds / MaxCount));
#endif
        }

        /// <summary>
        /// NCVのログから投稿されたニコ生のコメントを投票サーバーにすべて送ります。
        /// </summary>
        private static void ExecutePostComments(object sender,
                                                ExecutedRoutedEventArgs e)
        {
            if (!Global.VoteClient.IsLogined)
            {
                MessageUtil.ErrorMessage(
                    "投票ルームにログインしていません。");
                return;
            }

            // 転送するログファイル名を選択。
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists=true,
                DefaultExt = ".xml",
                ReadOnlyChecked = true,
                Title = "NCVのログファイルを選択します。",
                Multiselect = true,
                Filter = "(*)|*",
            };
            var result = dialog.ShowDialog();
            if (result == null || !result.Value)
            {
                return;
            }

            foreach (var filepath in dialog.FileNames)
            {
                SendNcvLog(filepath);
            }

            DialogUtil.Show(
                "ログ転送を完了しました。",
                "確認",
                MessageBoxButton.OK);
        }

        /// <summary>
        /// NCVのログファイルを通知として転送します。
        /// </summary>
        private static void SendNcvLog(string filepath)
        {
            try
            {
                var reader = System.Xml.XmlReader.Create(filepath);

                // xmlの各chat要素からコメント内容を取得します。
                while (reader.ReadToFollowing("chat"))
                {
                    if (reader.NodeType != System.Xml.XmlNodeType.Element)
                    {
                        continue;
                    }

                    var id = reader.GetAttribute("user_id");
                    var text = reader.ReadString();
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    // 投票ルームに通知としてコメントを送信。
                    Global.VoteClient.SendNotification(
                        new Protocol.Notification()
                        {
                            Text = text,
                            VoterId = id,
                            Timestamp = Ragnarok.Net.NtpClient.GetTime(),
                        },
                        false);
                }
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "ログ送信に失敗しました。");
                return;
            }
        }

        /// <summary>
        /// エラーログを送信します。
        /// </summary>
        private static void ExecuteSendErrorLog(object sender,
                                                ExecutedRoutedEventArgs e)
        {
            try
            {
                var model = new Ragnarok.Presentation.Debug.ReportDialogModel();
                model.OpenErrorLog("logs/client.err");

                var dialog = new Ragnarok.Presentation.Debug.SendLogDialog(model)
                {
                    Owner = Global.MainWindow,
                };
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "ダイアログの表示に失敗しました。");
            }
        }

        /// <summary>
        /// バージョンを表示します。
        /// </summary>
        private static void ExecuteShowVersion(object sender,
                                               ExecutedRoutedEventArgs e)
        {
            try
            {
                var dialog = new View.VersionWindow()
                {
                    Owner = Global.MainWindow,
                };
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "バージョン情報の表示に失敗しました。");
            }
        }

        /// <summary>
        /// 指定のURLをブラウザで開きます。
        /// </summary>
        private static void ExecuteNavigateUrl(object sender,
                                               ExecutedRoutedEventArgs e)
        {
            if (e.Parameter == null)
            {
                return;
            }

            var uri = e.Parameter.ToString();
            if (string.IsNullOrEmpty(uri))
            {
                return;
            }

            try
            {
                // 与えられたURLを開きます。
                System.Diagnostics.Process.Start(uri);
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    string.Format(
                        "'{0}'の実行に失敗しました。", uri));
            }
        }

        /// <summary>
        /// 指定の音声ファイルを再生します。
        /// </summary>
        private static void ExecutePlaySound(object sender,
                                             ExecutedRoutedEventArgs e)
        {
            try
            {
                if (e.Parameter == null)
                {
                    return;
                }

                var info = (PlaySoundInfo)e.Parameter;
                var uri = info.FilePath;
                if (string.IsNullOrEmpty(uri))
                {
                    return;
                }

                // 与えられたファイルを再生します。
                Global.SoundManager.PlaySE(uri);
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "音声の再生に失敗しました。");
            }
        }

        /// <summary>
        /// プラグインを実行します。
        /// </summary>
        private static void ExecuteRunPlugin(object sender,
                                             ExecutedRoutedEventArgs e)
        {
            try
            {
                var plugin = e.Parameter as IPlugin;
                if (plugin == null)
                {
                    MessageUtil.ErrorMessage(
                        "プラグインがnullです。");
                    return;
                }

                plugin.Run();
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "プラグインの実行に失敗しました。");
            }
        }

        /// <summary>
        /// 設定ダイアログを開きます。
        /// </summary>
        private static void ExecuteOpenSettingDialog(object sender,
                                                     ExecutedRoutedEventArgs e)
        {
            try
            {
                var dialog = new View.SettingDialog()
                {
                    DataContext = Global.Settings,
                    Owner = Global.MainWindow,
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "設定ダイアログの表示に失敗しました。");
            }
        }

        /// <summary>
        /// 投票結果ウィンドウを新たに開きます。
        /// </summary>
        private static void ExecuteOpenVoteResultWindow(object sender,
                                                        ExecutedRoutedEventArgs e)
        {
            var model = new ViewModel.VoteResultWindowViewModel(Global.VoteClient);
            var window = new View.VoteResultWindow()
            {
                DataContext = model,
                Owner = Global.MainWindow,
            };

            window.Show();
        }

        /// <summary>
        /// 評価値ウィンドウを新たに開きます。
        /// </summary>
        private static void ExecuteOpenEvaluationWindow(object sender,
                                                        ExecutedRoutedEventArgs e)
        {
            var model = new ViewModel.EvaluationWindowViewModel();
            var window = new View.EvaluationWindow(model)
            {
                DataContext = model,
                Owner = Global.MainWindow,
            };

            window.Show();
        }

        /// <summary>
        /// エンドロールウィンドウを新たに開きます。
        /// </summary>
        private static void ExecuteOpenEndRollWindow(object sender,
                                                     ExecutedRoutedEventArgs e)
        {
            var model = new ViewModel.EndRollWindowViewModel
            {
                Background = new SolidColorBrush(Color.FromArgb(170, 0, 0, 0)),
                RollTimeSeconds = 30,
                LineHeight = 30.0,
                OpacityLineCount = 3,
                Topmost = true,
                IsShowBorder = true,
                EdgeLength = 10,
            };

            var window = new View.EndRollWindow
            {
                DataContext = model,
                Owner = Global.MainWindow,
            };

            window.Show();
        }
    }
}
