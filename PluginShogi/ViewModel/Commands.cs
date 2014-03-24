using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

using Ragnarok;
using Ragnarok.Net;
using Ragnarok.Shogi;
using Ragnarok.NicoNico;
using Ragnarok.NicoNico.Live;
using Ragnarok.Presentation;

namespace VoteSystem.PluginShogi.ViewModel
{
    using Model;
    using View;
    using Protocol;
    using Protocol.Vote;

    /// <summary>
    /// コマンドを保持します。
    /// </summary>
    public static class Commands
    {
        /// <summary>
        /// 将棋盤を表示します。
        /// </summary>
        public static readonly ICommand ShowBoard =
            new RoutedUICommand(
                "将棋盤を表示します。",
                "ShowBoard",
                typeof(Window));
        /// <summary>
        /// 設定ダイアログを表示します。
        /// </summary>
        public static readonly ICommand ShowSettingDialog =
            new RoutedUICommand(
                "設定ダイアログを表示します。",
                "ShowSettingDialog",
                typeof(Window));
        /// <summary>
        /// 変化コントロールを表示します。
        /// </summary>
        public static readonly ICommand ShowMoveManageView =
            new RoutedUICommand(
                "将棋コントロールを表示します。",
                "ShowMoveManageView",
                typeof(Window));
        /// <summary>
        /// エンディングの設定用コントロールを表示します。
        /// </summary>
        public static readonly ICommand ShowEndingSettingWindow =
            new RoutedUICommand(
                "エンディングの設定用コントロールを表示します。",
                "ShowEndingSettingWindow",
                typeof(Window));

        /// <summary>
        /// NCVのログファイルを読み込みます。
        /// </summary>
        public static readonly ICommand ReadNcvLog =
            new RoutedUICommand(
                "NCVのログファイルを読み込みます。",
                "ReadNcvLog",
                typeof(Window));
        /// <summary>
        /// 棋譜ファイルを読み込みます。
        /// </summary>
        public static readonly ICommand LoadKifFile =
            new RoutedUICommand(
                "棋譜ファイルを読み込みます。",
                "LoadKifFile",
                typeof(Window));
        /// <summary>
        /// 棋譜ファイルを保存します。
        /// </summary>
        public static readonly ICommand SaveKifFile =
            new RoutedUICommand(
                "棋譜ファイルを保存します。",
                "SaveKifFile",
                typeof(Window));
        /// <summary>
        /// 棋譜ファイルの貼り付けを行います。
        /// </summary>
        public static readonly ICommand PasteKifFile =
            new RoutedUICommand(
                "棋譜ファイルの貼り付けを行います。",
                "PasteKifFile",
                typeof(Window));
        /// <summary>
        /// 棋譜ファイルのコピーを行います。
        /// </summary>
        public static readonly ICommand CopyKifFile =
            new RoutedUICommand(
                "棋譜ファイルのコピーを行います。",
                "CopyKifFile",
                typeof(Window));

        /// <summary>
        /// エンドロールを開始します。
        /// </summary>
        public static readonly ICommand PlayEndRoll =
            new RoutedUICommand(
                "エンドロールを開始します。",
                "PlayEndRoll",
                typeof(Window));
        /// <summary>
        /// エンドロールを停止します。
        /// </summary>
        public static readonly ICommand StopEndRoll =
            new RoutedUICommand(
                "エンドロールを停止します。",
                "StopEndRoll",
                typeof(Window));

        /// <summary>
        /// 現局面を設定します。
        /// </summary>
        public static readonly ICommand SetCurrentBoard =
            new RoutedUICommand(
                "表示されている局面を現局面に設定します。",
                "SetCurrentBoard",
                typeof(Window));
        /// <summary>
        /// 現局面に戻します。
        /// </summary>
        public static readonly ICommand SetBackToCurrentBoard =
            new RoutedUICommand(
                "表示されている局面を現局面に戻します。",
                "SetBackToCurrentBoard",
                typeof(Window));
        /// <summary>
        /// 投票ルームから現局面を取得します。
        /// </summary>
        public static readonly ICommand GetCurrentBoardFromServer =
            new RoutedUICommand(
                "投票ルームから現局面を取得します。",
                "GetCurrentBoardFromServer",
                typeof(Window));

        /// <summary>
        /// ニコニコの放送に接続します。
        /// </summary>
        public static readonly ICommand ConnectToNicoLive =
            new RoutedUICommand(
                "ニコニコの放送に接続します。",
                "ConnectToNicoLive",
                typeof(Window));
        /// <summary>
        /// ニコニコの放送に切断します。
        /// </summary>
        public static readonly ICommand DisconnectToNicoLive =
            new RoutedUICommand(
                "ニコニコの放送に切断します。",
                "DisconnectToNicoLive",
                typeof(Window));
        /// <summary>
        /// 変化コメントを投稿します。
        /// </summary>
        public static readonly ICommand PostVariationComment =
            new RoutedUICommand(
                "変化を投稿します。",
                "PostVariationComment",
                typeof(Window));

        /// <summary>
        /// 変化後の局面に移動します。
        /// </summary>
        public static readonly ICommand MoveToVariationState =
            new RoutedUICommand(
                "この局面に移動します。",
                "MoveToVariationState",
                typeof(Window));
        /// <summary>
        /// 現在の局面を変化として登録します。
        /// </summary>
        public static readonly ICommand AddVariation =
            new RoutedUICommand(
                "局面を変化として登録します。",
                "AddVariation",
                typeof(Window));
        /// <summary>
        /// 変化を自動的に再生します。
        /// </summary>
        public static readonly ICommand PlayVariation =
            new RoutedUICommand(
                "変化を自動的に再生します。",
                "PlayVariation",
                typeof(Window));

        /// <summary>
        /// 対局開始します。
        /// </summary>
        public static readonly ICommand Start =
            new RoutedUICommand(
                "対局開始します。",
                "Start",
                typeof(Window));
        /// <summary>
        /// 投了します。
        /// </summary>
        public static readonly ICommand Resign =
            new RoutedUICommand(
                "投了します。",
                "Resign",
                typeof(Window));

        /// <summary>
        /// 公式放送用の背景画像を更新します。
        /// </summary>
        public static readonly ICommand NextOfficialBackground =
            new RoutedUICommand(
                "公式放送用の背景画像を次の画像にします。",
                "NextOfficialBackground",
                typeof(Window));

        /// <summary>
        /// コマンドをバインディングします。
        /// </summary>
        public static void Binding(CommandBindingCollection bindings)
        {
            bindings.Add(
                new CommandBinding(
                    ShowSettingDialog,
                    ExecuteShowSettingDialog, CanExecute));
            bindings.Add(
                new CommandBinding(
                    Commands.ShowMoveManageView,
                    ExecuteShowMoveManageView, CanExecute));
            bindings.Add(
                new CommandBinding(
                    ShowEndingSettingWindow,
                    ExecuteShowEndingSettingWindow, CanExecute));

            /*bindings.Add(
                new CommandBinding(
                    ReadNcvLog,
                    ExecuteReadNcvLog, CanExecute));*/
            bindings.Add(
                new CommandBinding(
                    ReadNcvLog,
                    ExecuteReadNcvLog, CanExecute));
            bindings.Add(
                new CommandBinding(
                    LoadKifFile,
                    ExecuteLoadKifFile, CanExecute));
            bindings.Add(
                new CommandBinding(
                    SaveKifFile,
                    ExecuteSaveKifFile, CanExecute));
            bindings.Add(
                new CommandBinding(
                    PasteKifFile,
                    ExecutePasteKifFile, CanExecute));
            bindings.Add(
                new CommandBinding(
                    CopyKifFile,
                    ExecuteCopyKifFile, CanExecute));

            bindings.Add(
                new CommandBinding(
                    PlayEndRoll,
                    ExecutePlayEndRoll, CanExecute));
            bindings.Add(
                new CommandBinding(
                    StopEndRoll,
                    ExecuteStopEndRoll, CanExecute));

            bindings.Add(
                new CommandBinding(
                    SetCurrentBoard,
                    ExecuteSetCurrentBoard, CanExecute));
            bindings.Add(
                new CommandBinding(
                    SetBackToCurrentBoard,
                    ExecuteSetBackToCurrentBoard, CanExecute));
            bindings.Add(
                new CommandBinding(
                    GetCurrentBoardFromServer,
                    ExecuteGetCurrentBoardFromServer, CanExecute));

            bindings.Add(
                 new CommandBinding(
                    ConnectToNicoLive,
                    (_, __) => DoConnectToNicoLive(
                        ShogiGlobal.ShogiModel.LiveUrl, true),
                    CanExecute));
            bindings.Add(
                 new CommandBinding(
                    DisconnectToNicoLive,
                    ExecuteDisconnectToNicoLive, CanExecute));
            bindings.Add(
                new CommandBinding(
                    PostVariationComment,
                    (_, __) => DoPostVariationComment(), CanExecute));

            bindings.Add(
                new CommandBinding(
                    MoveToVariationState,
                    ExecuteMoveToVariationState, CanExecute));
            bindings.Add(
                new CommandBinding(
                    PlayVariation,
                    ExecutePlayVariation, CanExecute));

            bindings.Add(
                new CommandBinding(
                    Start,
                    ExecuteStart, CanExecute));
            bindings.Add(
                new CommandBinding(
                    Resign,
                    ExecuteResign, CanExecute));

            bindings.Add(
                new CommandBinding(
                    NextOfficialBackground,
                    ExecuteNextOfficialBackground, CanExecute));            
        }

        /// <summary>
        /// コマンドを操作にバインディングします。
        /// </summary>
        public static void Binding(InputBindingCollection inputs)
        {
            inputs.Add(
                new KeyBinding(LoadKifFile,
                    new KeyGesture(Key.O, ModifierKeys.Control)));
            inputs.Add(
                new KeyBinding(SaveKifFile,
                    new KeyGesture(Key.A, ModifierKeys.Control)));

            inputs.Add(
                new KeyBinding(PasteKifFile,
                    new KeyGesture(Key.V, ModifierKeys.Control)));
            inputs.Add(
                new KeyBinding(CopyKifFile,
                    new KeyGesture(Key.C, ModifierKeys.Control)));

            inputs.Add(
                new KeyBinding(ShowSettingDialog,
                    new KeyGesture(Key.D, ModifierKeys.Control | ModifierKeys.Shift)));
            inputs.Add(
                new KeyBinding(ShowEndingSettingWindow,
                    new KeyGesture(Key.E, ModifierKeys.Control | ModifierKeys.Shift)));
            inputs.Add(
                new KeyBinding(Protocol.View.EvaluationControl.OpenSettingDialog,
                    new KeyGesture(Key.L, ModifierKeys.Control | ModifierKeys.Shift)));
            inputs.Add(
                new KeyBinding(Protocol.View.VoteResultControl.OpenSettingDialog,
                    new KeyGesture(Key.V, ModifierKeys.Control | ModifierKeys.Shift)));

            inputs.Add(
                new KeyBinding(NextOfficialBackground,
                    new KeyGesture(Key.N, ModifierKeys.Control | ModifierKeys.Shift)));
        }

        /// <summary>
        /// コマンドが実行できるか調べます。
        /// </summary>
        private static void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var model = ShogiGlobal.ShogiModel;

            if (e.Command == PlayEndRoll || e.Command == StopEndRoll)
            {
                e.CanExecute = ShogiGlobal.VoteClient.IsVoteRoomOwner;
            }

            else if (e.Command == SetCurrentBoard)
            {
                e.CanExecute = ShogiGlobal.VoteClient.IsVoteRoomOwner;
            }
            else if (e.Command == GetCurrentBoardFromServer)
            {
                e.CanExecute = ShogiGlobal.VoteClient.IsLogined;
            }
            else if (e.Command == PostVariationComment)
            {
                e.CanExecute = true; // model.IsConnectedToLive;
            }

            else if (e.Command == Resign)
            {
                e.CanExecute = true; // ShogiGlobal.VoteClient.IsVoteRoomOwner;
            }
            else
            {
                e.CanExecute = true;
            }

            e.Handled = true;
        }

        /// <summary>
        /// 設定ダイアログを開きます。
        /// </summary>
        private static void ExecuteShowSettingDialog(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var model = new ShogiSettingDialogViewModel();
                var dialog = new ShogiSettingDialog()
                {
                    Owner = ShogiGlobal.MainWindow,
                    DataContext = model,
                };

                dialog.Loaded += (_, __) =>
                    dialog.AdjustInDisplay();

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                ShogiGlobal.ErrorMessage(ex,
                    "設定ダイアログの表示に失敗しました(￣ω￣;)");
                return;
            }
        }

        /// <summary>
        /// 将棋コントロールを開きます。
        /// </summary>
        private static void ExecuteShowMoveManageView(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var view = ShogiGlobal.MoveManageView;
                if (view != null)
                {
                    view.Activate();
                    return;
                }

                // 将棋コントロールを表示します。
                var moveManageView = new MoveManageView()
                {
                    Owner = ShogiGlobal.MainWindow,
                    DataContext = ShogiGlobal.ShogiModel,
                };
                moveManageView.Closed += (sender_, e_) =>
                {
                    ShogiGlobal.MoveManageView = null;
                };

                ShogiGlobal.MoveManageView = moveManageView;
                moveManageView.Show();
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "Failed Window_Loaded");
            }
        }

        /// <summary>
        /// エンディングの設定用コントロールを開きます。
        /// </summary>
        private static void ExecuteShowEndingSettingWindow(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var window = ShogiGlobal.MainWindow;
                var dialog = new EndingSettingWindow(window.ShogiEndRoll)
                {
                    Owner = window,
                };

                dialog.Loaded += (_, __) =>
                    dialog.AdjustInDisplay();

                dialog.Show();
            }
            catch (Exception ex)
            {
                ShogiGlobal.ErrorMessage(ex,
                    "エンディングの設定用コントロールの表示に失敗しました(￣ω￣;)");
            }
        }

        /// <summary>
        /// NCVのログから変化を読み込みます。
        /// </summary>
        private static void ExecuteReadNcvLog(object sender,
                                              ExecutedRoutedEventArgs e)
        {
            if (!ShogiGlobal.VoteClient.IsLogined)
            {
                DialogUtil.ShowError(
                    "投票ルームにログインしていません。");
                return;
            }

            // ログファイル名を選択。
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
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
                ReadNcvLogInternal(filepath);
            }

            DialogUtil.Show(
                "ログファイルの読み込みを完了しました。",
                "確認",
                MessageBoxButton.OK);
        }

        /// <summary>
        /// NCVのログファイルの変化を読み込みます。
        /// </summary>
        private static void ReadNcvLogInternal(string filepath)
        {
            var model = ShogiGlobal.ShogiModel;
            var board = model.Board.Clone();

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

                    var premium = reader.GetAttribute("premium");
                    var text = reader.ReadString();
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    if (premium == "3")
                    {
                        var m = System.Text.RegularExpressions.Regex.Match(
                            text, @"(\d+)手目　((▲|△)(.)(.)(.+))$");
                        if (!m.Success)
                        {
                            continue;
                        }

                        // 現局面設定コメントか？
                        var n = int.Parse(m.Groups[1].Value);

                        while (board.MoveCount > n)
                        {
                            board.Undo();
                        }

                        while (board.MoveCount < n)
                        {
                            board.Redo();
                        }

                        // 現局面を設定します。
                        model.SetCurrentBoard(board.Clone());
                    }
                    else
                    {
                        // 変化コメントか？
                        var variation = Variation.Parse(text);
                        if (variation != null)
                        {
                            model.AddVariation(variation, true, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DialogUtil.ShowError(ex,
                    "ログ送信に失敗しました。");
                return;
            }
        }

        /// <summary>
        /// 棋譜ファイルを読み込みます。
        /// </summary>
        private static void ExecuteLoadKifFile(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog()
                {
                    DefaultExt = ".kif",
                    Title = "棋譜ファイルの選択",
                    Multiselect = false,
                    RestoreDirectory = false,
                    Filter = "Kif Files(*.kif,*.ki2)|*.kif;*.ki2|All files (*.*)|*.*",
                    FilterIndex = 0,
                };
                var result = dialog.ShowDialog();
                if (result == null || !result.Value)
                {
                    return;
                }

                using (var reader = new StreamReader(dialog.FileName,
                                                     KifuObject.DefaultEncoding))
                {
                    LoadKif(reader);
                }
            }
            catch (Exception ex)
            {
                ShogiGlobal.ErrorMessage(ex,
                    "棋譜ファイルの読み込みに失敗しました。(￣ω￣;)");
            }
        }

        /// <summary>
        /// 棋譜ファイルの貼り付けを行います。
        /// </summary>
        private static void ExecutePasteKifFile(object sender, ExecutedRoutedEventArgs e)
        {
            var text = Clipboard.GetText(TextDataFormat.Text);

            using (var reader = new StringReader(text))
            {
                LoadKif(reader);
            }
        }

        /// <summary>
        /// 棋譜ファイルの読み込みを行います。
        /// </summary>
        public static void LoadKif(TextReader reader)
        {
            try
            {
                if (reader == null)
                {
                    return;
                }

                // ファイルを読み込み局面を作成します。
                var file = KifuReader.Load(reader);
                var board = file.CreateBoard();

                // 現局面は更新しません。
                ShogiGlobal.ShogiModel.SetBoard(board);
            }
            catch (Exception ex)
            {
                ShogiGlobal.ErrorMessage(ex,
                    "棋譜ファイルの読み込みに失敗しました(￣ω￣;)");
                return;
            }
        }

        /// <summary>
        /// 棋譜ファイルを保存します。
        /// </summary>
        private static void ExecuteSaveKifFile(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var dialog = new SaveFileDialog()
                {
                    AddExtension = true,
                    CheckFileExists = false,
                    OverwritePrompt = true,
                    CreatePrompt = false,
                    Title = "棋譜ファイルの選択",
                    RestoreDirectory = false,
                    Filter = "Kif Files(*.kif)|*.kif|All files (*.*)|*.*",
                    FilterIndex = 0,
                };
                var result = dialog.ShowDialog();
                if (result == null || !result.Value)
                {
                    return;
                }

                using (var writer = new StreamWriter(dialog.FileName, false,
                                                     KifuObject.DefaultEncoding))
                {
                    SaveKif(writer);
                }
            }
            catch (Exception ex)
            {
                ShogiGlobal.ErrorMessage(ex,
                    "棋譜ファイルの保存に失敗しました(￣ω￣;)");
            }
        }

        /// <summary>
        /// 棋譜ファイルのコピーを行います。
        /// </summary>
        private static void ExecuteCopyKifFile(object sender, ExecutedRoutedEventArgs e)
        {
            using (var writer = new StringWriter())
            {
                SaveKif(writer);

                Clipboard.SetText(writer.ToString());
            }
        }

        /// <summary>
        /// 棋譜ファイルの書き込みを行います。
        /// </summary>
        public static void SaveKif(TextWriter writer)
        {
            try
            {
                if (writer == null)
                {
                    return;
                }

                var model = ShogiGlobal.ShogiModel;
                var manager = model.MoveManager;
                var root = manager.CreateVariationNode(model.Board);

                var headers = new Dictionary<string, string>();
                headers["先手"] = model.Settings.SD_BlackPlayerName;
                headers["後手"] = model.Settings.SD_WhitePlayerName;

                var kifu = new KifuObject(headers, root);
                KifuWriter.Save(writer, kifu);
            }
            catch (Exception ex)
            {
                ShogiGlobal.ErrorMessage(ex,
                    "棋譜ファイルの出力に失敗しました(￣ω￣;)");
            }
        }

        /// <summary>
        /// ウィンドウから時間間隔を取得します。
        /// </summary>
        private static TimeSpan? GetTimeSpan(TimeSpan defaultValue)
        {
            return DialogUtil.ShowTimeSpanDialog(defaultValue);
        }

        /// <summary>
        /// エンドロールを開始します。
        /// </summary>
        private static void ExecutePlayEndRoll(object sender, ExecutedRoutedEventArgs e)
        {
            var voteClient = ShogiGlobal.VoteClient;
            if (voteClient == null)
            {
                return;
            }

            try
            {
                TimeSpan? timeSpan = TimeSpan.Zero;

                if (!Client.Global.IsOfficial)
                {
                    timeSpan = GetTimeSpan(TimeSpan.FromMinutes(0));
                    if (timeSpan == null)
                    {
                        return;
                    }
                }

                // 動画の開始時間を設定します。
                var startTimeNtp = NtpClient.GetTime() + timeSpan.Value;

                // 念のため、確認ダイアログを出します。
                var dialog = DialogUtil.CreateDialog(
                    ShogiGlobal.MainWindow,
                    string.Format(
                        @"{1:HH時mm分}{0}{0}この時刻に開始しますがよろしいですか？",
                        Environment.NewLine,
                        startTimeNtp),
                    "時刻確認",
                    MessageBoxButton.YesNo,
                    MessageBoxResult.No);
                if (dialog.ShowDialogCenterMouse() != true)
                {
                    return;
                }

                voteClient.SendStartEndRoll(startTimeNtp);
            }
            catch (Exception ex)
            {
                Util.ThrowIfFatal(ex);

                ShogiGlobal.ErrorMessage(ex,
                    "エンディングの開始に失敗しました。");
            }
        }

        /// <summary>
        /// エンドロールを停止します。
        /// </summary>
        private static void ExecuteStopEndRoll(object sender, ExecutedRoutedEventArgs e)
        {
            var voteClient = ShogiGlobal.VoteClient;
            if (voteClient == null)
            {
                return;
            }

            try
            {
                // 念のため、確認ダイアログを出します。
                var dialog = DialogUtil.CreateDialog(
                    ShogiGlobal.MainWindow,
                    "エンディングを停止しますがよろしいですか？",
                    "停止確認",
                    MessageBoxButton.YesNo,
                    MessageBoxResult.No);
                if (dialog.ShowDialogCenterMouse() != true)
                {
                    return;
                }

                voteClient.SendStopEndRoll();
            }
            catch (Exception ex)
            {
                Util.ThrowIfFatal(ex);

                ShogiGlobal.ErrorMessage(ex,
                    "エンディングの停止に失敗しました。");
            }
        }

        /// <summary>
        /// 現局面の設定を行います。
        /// </summary>
        private static void SendSetCurrentBoard(Board board)
        {
            if (board == null || !board.Validate())
            {
                return;
            }

            ShogiGlobal.VoteClient.SendCommand(new ShogiSetCurrentBoardCommand()
            {
                Board = board,
            });
        }

        /// <summary>
        /// <paramref name="value"/>がMinValue or MaxValueであれば
        /// <paramref name="dValue"/>を返します。
        /// そうでない場合は<paramref name="value"/>をそのまま返します。
        /// </summary>
        private static TimeSpan Normalize(TimeSpan value, TimeSpan dValue)
        {
            var flag = (value != TimeSpan.MinValue && value != TimeSpan.MaxValue);

            return (flag ? value : dValue);
        }

        /// <summary>
        /// 局面を現局面に設定します。
        /// </summary>
        private static void ExecuteSetCurrentBoard(object sender, ExecutedRoutedEventArgs e)
        {
            var voteClient = ShogiGlobal.VoteClient;
            var model = ShogiGlobal.ShogiModel;

            if (!voteClient.IsVoteRoomOwner)
            {
                return;
            }

            try
            {
                // サーバー側に現局面の変更を通知します。
                var dialog = new CurrentBoardSetupDialog(ShogiGlobal.Settings)
                {
                    Topmost = true,
                };

                var result = dialog.ShowDialogCenterMouse();
                if (result == true)
                {
                    if (dialog.IsClearVoteResult)
                    {
                        voteClient.ClearVote();
                    }

                    var addTime = Normalize(
                        dialog.AddLimitTime, TimeSpan.Zero);
                    if (dialog.IsVoteStop)
                    {
                        // 現局面更新前に投票を停止する場合
                        voteClient.StopVote(addTime);
                    }
                    else if (addTime != TimeSpan.Zero)
                    {
                        // 時間変更のみの場合
                        voteClient.AddTotalVoteSpan(addTime);
                    }

                    // 現局面更新
                    SendSetCurrentBoard(model.Board);

                    // 現局面更新後に投票を開始します。
                    if (dialog.IsStartVote)
                    {
                        var span = Normalize(
                            dialog.VoteSpan, TimeSpan.FromSeconds(-1));

                        voteClient.StartVote(span);
                    }
                }
            }
            catch (Exception ex)
            {
                DialogUtil.ShowError(ex,
                    "現局面の設定に失敗しました o(＞ω＜)o");
            }
        }

        /// <summary>
        /// 現局面に戻します。
        /// </summary>
        private static void ExecuteSetBackToCurrentBoard(object sender, ExecutedRoutedEventArgs e)
        {
            var model = ShogiGlobal.ShogiModel;

            model.SetBoard(model.CurrentBoard);
        }

        /// <summary>
        /// 投票サーバーから現局面を取得します。
        /// </summary>
        private static void ExecuteGetCurrentBoardFromServer(object sender, ExecutedRoutedEventArgs e)
        {
            ShogiGlobal.ShogiPlugin.UpdateCurrentBoard(true);
        }

        /// <summary>
        /// ニコ生への放送に接続します。
        /// </summary>
        public static bool DoConnectToNicoLive(string liveUrl, bool showDialog)
        {
            var model = ShogiGlobal.ShogiModel;
            var nicoClient = ShogiGlobal.ClientModel.NicoClient;
            var commentClient = model.CommentClient;

            try
            {
                commentClient.Connect(
                    liveUrl,
                    nicoClient.CookieContainer);
                commentClient.StartReceiveMessage(0);

                if (showDialog)
                {
                    DialogUtil.Show(
                        ShogiGlobal.MainWindow,
                        "接続できました (^^)",
                        "OK",
                        MessageBoxButton.OK);
                }

                return true;
            }
            catch (Exception ex)
            {
                if (showDialog)
                {
                    ShogiGlobal.ErrorMessage(ex,
                        "放送への接続に失敗しました (T∇T)");
                }

                return false;
            }
        }

        /// <summary>
        /// ニコ生から切断します。
        /// </summary>
        private static void ExecuteDisconnectToNicoLive(object sender, ExecutedRoutedEventArgs e)
        {
            var model = ShogiGlobal.ShogiModel;

            try
            {
                model.CommentClient.Disconnect();
            }
            catch (Exception ex)
            {
                ShogiGlobal.ErrorMessage(ex,
                    "放送の切断に失敗しました (T∇T)");
            }
        }

        /// <summary>
        /// 入室している投票ルームの参加放送URLをすべて取得します。
        /// </summary>
        public static List<LiveData> GetLiveList(LiveSite liveSite)
        {
            var voteClient = ShogiGlobal.VoteClient;
            if (voteClient == null || !voteClient.IsConnected)
            {
                return new List<LiveData>();
            }

            var voteRoomInfo = voteClient.VoteRoomInfo;
            if ((object)voteRoomInfo == null)
            {
                return new List<LiveData>();
            }

            // ログインユーザーの生放送で放送サイトが一致する
            // 放送情報を取得します。
            return
                (from participant in voteRoomInfo.ParticipantList
                 from liveData in participant.LiveDataList
                 where liveData.Site == liveSite
                 select liveData).ToList();
        }

        /// <summary>
        /// 放送URLに接続されていなければ接続します。
        /// </summary>
        public static bool CheckAndConnectToLive(CommentClient commentClient,
                                                 NicoClient nicoClient)
        {
            if (commentClient == null)
            {
                throw new ArgumentNullException("commentClient");
            }

            if (nicoClient == null)
            {
                throw new ArgumentNullException("nicoClient");
            }

            if (!commentClient.IsConnected)
            {
                if (!nicoClient.IsLogin)
                {
                    ShogiGlobal.ErrorMessage(
                        string.Format(
                            "ニコニコにログインしていません。 (/□≦､){0}{0}" +
                            "メインウィンドウからログインしてください。", 
                            Environment.NewLine));
                    return false;
                }

                var liveList = GetLiveList(LiveSite.NicoNama);
                if (!liveList.Any())
                {
                    ShogiGlobal.ErrorMessage(
                        "コメントを投稿可能な放送がありません。 (/□≦､)");
                    return false;
                }

                if (liveList.Count() > 1)
                {
                    ShogiGlobal.ErrorMessage(
                        string.Format(
                            "投票ルームに複数の放送があります。 (/□≦､){0}{0}" +
                            "「接続」ボタンから手動で放送に接続してください。",
                            Environment.NewLine));
                    return false;
                }

                // 実際に放送に接続しに行きます。
                if (!DoConnectToNicoLive(liveList[0].Url, false))
                {
                    ShogiGlobal.ErrorMessage(
                        "放送への接続に失敗しました。 (/□≦､)");
                    return false;
                }

                ShogiGlobal.ShogiModel.LiveUrl = liveList[0].Url;
            }

            return true;
        }

        /// <summary>
        /// 指し手をコメント投稿用の文字列に変換します。
        /// </summary>
        /// <remarks>
        /// 指し手が長すぎる場合は分割してコメント投稿します。
        /// </remarks>
        private static IEnumerable<string> MakePostComments(IEnumerable<Move> moveList,
                                                            string comment,
                                                            int lineMaxLength)
        {
            // 一時変数を使っているので、先に配列化しておきます。
            var index = 0;
            var nextList = moveList
                .Select(_ => Stringizer.ToString(_, MoveTextStyle.Simple))
                .Select(_ => new { Text = _, Index = (index += _.Length) })
                .ToList();

            var nextId = MathEx.RandInt(1, 9500);
            var nextMaxLength = lineMaxLength - 6; // 6は最後のID分
            var isFirst = true;
            var localComment = (comment ?? string.Empty);

            while (nextList.Any())
            {
                var list = nextList.TakeWhile(_ => _.Index < nextMaxLength);
                nextList = nextList.SkipWhile(_ => _.Index < nextMaxLength).ToList();

                var idText = (isFirst ?
                    "" : string.Format("${0:D4} ", nextId));
                var nextIdText = (
                    !nextList.Any() && string.IsNullOrEmpty(localComment) ?
                    "" : string.Format(" ${0:D4}", nextId + 1));

                var mainText =
                    string.Join("", list.Select(_ => _.Text).ToArray());
                if (!nextList.Any() &&
                    mainText.Length + localComment.Length < lineMaxLength - 6)
                {
                    mainText += "　" + localComment;
                    localComment = null;
                }

                yield return string.Format(
                    "{0}{1}{2}",
                    idText, mainText, nextIdText);

                nextMaxLength += lineMaxLength - 12; // 12は前後のID分
                nextId += 1;
                isFirst = false;
            }

            // もしコメントが残っていたら、それも送ります。
            if (!string.IsNullOrEmpty(localComment))
            {
                yield return string.Format(
                    "${0:D4} {1}",
                    nextId, localComment);
            }
        }

        /// <summary>
        /// 変化コメントを投稿します。
        /// </summary>
        public static void DoPostVariationComment()
        {
            var model = ShogiGlobal.ShogiModel;
            var nicoClient = ShogiGlobal.ClientModel.NicoClient;
            var commentClient = model.CommentClient;

            try
            {
                if (!CheckAndConnectToLive(commentClient, nicoClient))
                {
                    // エラーダイアログはメソッドの中で表示されます。
                    return;
                }

                if (!model.MoveFromCurrentBoard.Any())
                {
                    ShogiGlobal.ErrorMessage(
                        "指し手がありません (/□≦､)");
                    return;
                }

                const int NiconamaMaxCommentLength = 60 - 4;
                var lines = MakePostComments(
                    model.MoveFromCurrentBoard,
                    model.Comment,
                    NiconamaMaxCommentLength)
                    .ToArray();
                if (!lines.Any())
                {
                    ShogiGlobal.ErrorMessage(
                        "投稿用コメントの作成に失敗しました (/□≦､)");
                    return;
                }

                var result = DialogUtil.Show(
                    ShogiGlobal.MainWindow,
                    string.Format(
                        "コメント）{0}{1}{0}{0}を投稿します。(*´ω｀*)",
                        Environment.NewLine,
                        string.Join(Environment.NewLine, lines)),
                    "確認",
                    MessageBoxButton.OKCancel,
                    MessageBoxResult.OK);
                if (result != MessageBoxResult.OK)
                {
                    return;
                }

                lines.ForEach(_ => commentClient.SendComment(_, "184 "));
            }
            catch (Exception ex)
            {
                ShogiGlobal.ErrorMessage(ex,
                    "コメントの投稿に失敗しました (T∇T)");
            }
        }

        /// <summary>
        /// 指定の変化の局面に移動します。
        /// </summary>
        internal static void ExecuteMoveToVariationState(Variation variation)
        {
            var model = ShogiGlobal.ShogiModel;

            if (variation != null)
            {
                // 変化の基点となる局面から手を進めます。
                var cloned = variation.Board.Clone();
                foreach (var boardMove in variation.BoardMoveList)
                {
                    cloned.DoMove(boardMove);
                }

                model.SetBoard(cloned);
            }
        }

        /// <summary>
        /// 指定の変化の局面に移動します。
        /// </summary>
        private static void ExecuteMoveToVariationState(object sender, ExecutedRoutedEventArgs e)
        {
            var variation = e.Parameter as Variation;

            ExecuteMoveToVariationState(variation);
        }

        /// <summary>
        /// 変化を自動的に再生します。
        /// </summary>
        private static void ExecutePlayVariation(object sender, ExecutedRoutedEventArgs e)
        {
            var variation = e.Parameter as Variation;
            var model = ShogiGlobal.ShogiModel;

            if (variation != null)
            {
                var autoPlay = new AutoPlayEx(variation)
                {
                    IsChangeBackground = true,
                    IsUseCutIn = true,
                    IsConfirmPlay = false,
                };

                model.StartAutoPlay(autoPlay);
            }
        }

        /// <summary>
        /// 対局を開始します。
        /// </summary>
        private static void ExecuteStart(object sender, ExecutedRoutedEventArgs e)
        {
        }

        /// <summary>
        /// 投了します。
        /// </summary>
        private static void ExecuteResign(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var manager = ShogiGlobal.EffectManager;
                if (manager == null)
                {
                    return;
                }

                manager.Resign();
            }
            catch (Exception ex)
            {
                ShogiGlobal.ErrorMessage(ex,
                    "投了に失敗しました (T∇T)");
            }
        }

         /// <summary>
        /// 公式用の背景画像を次の画像に変更します。
        /// </summary>
        private static void ExecuteNextOfficialBackground(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var manager = ShogiGlobal.EffectManager;
                if (manager == null)
                {
                    return;
                }

                Effects.EffectManager.OfficialBackgroundImageIndex += 1;
                manager.UpdateBackground();
            }
            catch (Exception ex)
            {
                ShogiGlobal.ErrorMessage(ex,
                    "公式放送の背景画像の更新に失敗しました (T∇T)");
            }
        }
    }
}
