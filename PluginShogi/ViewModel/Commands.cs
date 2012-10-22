using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

using Ragnarok;
using Ragnarok.Shogi;
using Ragnarok.Shogi.View;
using Ragnarok.NicoNico;
using Ragnarok.NicoNico.Live;
using Ragnarok.Presentation;

namespace VoteSystem.PluginShogi.ViewModel
{
    using VoteSystem.PluginShogi.Model;
    using VoteSystem.PluginShogi.View;
    using VoteSystem.Protocol;
    using VoteSystem.Protocol.Vote;

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
        /// 将棋コントロールを表示します。
        /// </summary>
        public static readonly ICommand ShowMoveManageView =
            new RoutedUICommand(
                "将棋コントロールを表示します。",
                "ShowMoveManageView",
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
        /// 盤面を反転します。
        /// </summary>
        public static readonly ICommand SetReverseBoard =
            new RoutedUICommand(
                "盤面を反転します。",
                "SetReverseBoard",
                typeof(Window));

        /// <summary>
        /// 開始局面へ。
        /// </summary>
        public static readonly ICommand GotoFirstState =
            new RoutedUICommand(
                "開始局面へ。",
                "GotoFirstState",
                typeof(Window));
        /// <summary>
        /// 最終局面へ。
        /// </summary>
        public static readonly ICommand GotoLastState =
            new RoutedUICommand(
                "最終局面へ。",
                "GotoLastState",
                typeof(Window));
        /// <summary>
        /// 手を一つ戻します。
        /// </summary>
        public static readonly ICommand MoveUndo =
            new RoutedUICommand(
                "手を一つ戻します。",
                "MoveUndo",
                typeof(Window));
        /// <summary>
        /// 手を一つ進めます。
        /// </summary>
        public static readonly ICommand MoveRedo =
            new RoutedUICommand(
                "手を一つ進めます。",
                "MoveRedo",
                typeof(Window));
        /// <summary>
        /// 連続して手を戻します。
        /// </summary>
        public static readonly ICommand MoveUndoContinue =
            new RoutedUICommand(
                "連続して手を戻します。",
                "MoveUndoContinue",
                typeof(Window));
        /// <summary>
        /// 連続して手を進めます。
        /// </summary>
        public static readonly ICommand MoveRedoContinue =
            new RoutedUICommand(
                "連続して手を進めます。",
                "MoveRedoContinue",
                typeof(Window));
        /// <summary>
        /// 再生中の手を停止します。
        /// </summary>
        public static readonly ICommand MoveStop =
            new RoutedUICommand(
                "再生中の手を停止します。",
                "MoveStop",
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
        /// コマンドをバインディングします。
        /// </summary>
        public static void Binding(CommandBindingCollection bindings)
        {
            bindings.Add(
                new CommandBinding(
                    Commands.ShowMoveManageView,
                    ExecuteShowMoveManageView, CanExecute));
            bindings.Add(
                new CommandBinding(
                    ShowSettingDialog,
                    ExecuteShowSettingDialog, CanExecute));

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
                    SetReverseBoard,
                    ExecuteSetReverseBoard, CanExecute));

            bindings.Add(
                new CommandBinding(
                    GotoFirstState,
                    ExecuteGotoFirstState, CanExecute));
            bindings.Add(
                new CommandBinding(
                    GotoLastState,
                    ExecuteGotoLastState, CanExecute));
            bindings.Add(
                new CommandBinding(
                    MoveUndo,
                    ExecuteMoveUndo, CanExecute));
            bindings.Add(
                new CommandBinding(
                    MoveRedo,
                    ExecuteMoveRedo, CanExecute));
            bindings.Add(
                new CommandBinding(
                    MoveUndoContinue,
                    ExecuteMoveUndoContinue, CanExecute));
            bindings.Add(
                new CommandBinding(
                    MoveRedoContinue,
                    ExecuteMoveRedoContinue, CanExecute));
            bindings.Add(
                new CommandBinding(
                    MoveStop,
                    ExecuteMoveStop, CanExecute));

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
        }

        /// <summary>
        /// コマンドを操作にバインディングします。
        /// </summary>
        public static void Binding(InputBindingCollection inputs)
        {
            inputs.Add(
                new KeyBinding(MoveUndo,
                    new KeyGesture(Key.Left)));
            inputs.Add(
                new KeyBinding(MoveRedo,
                    new KeyGesture(Key.Right)));

            inputs.Add(
                new KeyBinding(LoadKifFile,
                    new KeyGesture(Key.O, ModifierKeys.Control)));
            inputs.Add(
                new KeyBinding(SaveKifFile,
                    new KeyGesture(Key.S, ModifierKeys.Control)));

            inputs.Add(
                new KeyBinding(PasteKifFile,
                    new KeyGesture(Key.V, ModifierKeys.Control)));
        }

        /// <summary>
        /// コマンドが実行できるか調べます。
        /// </summary>
        private static void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var model = ShogiGlobal.ShogiModel;

            if (e.Command == GotoFirstState)
            {
                e.CanExecute = model.CanUndo;
            }
            else if (e.Command == GotoLastState)
            {
                e.CanExecute = model.CanRedo;
            }

            else if (e.Command == MoveUndo)
            {
                e.CanExecute = model.CanUndo;
            }
            else if (e.Command == MoveRedo)
            {
                e.CanExecute = model.CanRedo;
            }
            else if (e.Command == MoveUndoContinue)
            {
                e.CanExecute = model.CanUndo;
            }
            else if (e.Command == MoveRedoContinue)
            {
                e.CanExecute = model.CanRedo;
            }
            else if (e.Command == MoveStop)
            {
                e.CanExecute = (model.VariationState == VariationState.Playing);
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
                e.CanExecute = ShogiGlobal.VoteClient.IsVoteRoomOwner;
            }
            else
            {
                e.CanExecute = true;
            }

            e.Handled = true;
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
        /// 設定ダイアログを開きます。
        /// </summary>
        private static void ExecuteShowSettingDialog(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var model = new ShogiSettingDialogViewModel();
                var dialog = new ShogiSettingDialog()
                {
                    DataContext = model,
                };

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

                // ファイルを読み込み局面を作成します。
                var file = KifuReader.LoadFile(dialog.FileName);
                var board = file.CreateBoard();

                // 現局面は更新しません。
                ShogiGlobal.ShogiModel.SetBoard(board);
            }
            catch (Exception ex)
            {
                ShogiGlobal.ErrorMessage(ex,
                    "棋譜ファイルの読み込みに失敗しました(￣ω￣;)");
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

                // ファイルに保存します。
                var manager = ShogiGlobal.ShogiModel.MoveManager;
                var root = manager.CreateVariationNode();

                var headers = new Dictionary<string, string>();
                headers["先手"] = "あなた";
                headers["後手"] = "あなた２";

                var kifu = new KifuObject(headers, root);
                KifuWriter.SaveFile(dialog.FileName, kifu);
            }
            catch (Exception ex)
            {
                ShogiGlobal.ErrorMessage(ex,
                    "棋譜ファイルの保存に失敗しました(￣ω￣;)");
            }
        }

        /// <summary>
        /// 棋譜ファイルの貼り付けを行います。
        /// </summary>
        private static void ExecutePasteKifFile(object sender, ExecutedRoutedEventArgs e)
        {
            var text = Clipboard.GetText(TextDataFormat.Text);

            LoadKifText(text);
        }

        /// <summary>
        /// 棋譜ファイルの読み込みを行います。
        /// </summary>
        public static void LoadKifText(string text)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                {
                    return;
                }

                // ファイルを読み込み局面を作成します。
                var file = KifuReader.LoadFrom(text);
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
        /// 盤面を反転します。
        /// </summary>
        private static void ExecuteSetReverseBoard(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var shogi = ShogiGlobal.MainWindow;
                var isWhite = (bool)e.Parameter;
                var side = (isWhite ? BWType.White : BWType.Black);

                shogi.ShogiControl.ViewSide = side;
            }
            catch (Exception ex)
            {
                ShogiGlobal.ErrorMessage(ex,
                    "盤面の反転に失敗しました(￣ω￣;)");
            }
        }

        /// <summary>
        /// 開始局面へ。
        /// </summary>
        private static void ExecuteGotoFirstState(object sender, ExecutedRoutedEventArgs e)
        {
            var model = ShogiGlobal.ShogiModel;

            // 局面をUndoします。
            var cloned = model.Board.Clone();
            while (cloned.Undo() != null)
            {
            }

            model.SetBoard(cloned);
        }

        /// <summary>
        /// 最終局面へ。
        /// </summary>
        private static void ExecuteGotoLastState(object sender, ExecutedRoutedEventArgs e)
        {
            var model = ShogiGlobal.ShogiModel;

            // 局面をRedoします。
            var cloned = model.Board.Clone();
            while (cloned.Redo() != null)
            {
            }

            model.SetBoard(cloned);
        }

        /// <summary>
        /// １手戻します。
        /// </summary>
        private static void ExecuteMoveUndo(object sender, ExecutedRoutedEventArgs e)
        {
            var model = ShogiGlobal.ShogiModel;

            model.Undo();
        }

        /// <summary>
        /// １手進めます。
        /// </summary>
        private static void ExecuteMoveRedo(object sender, ExecutedRoutedEventArgs e)
        {
            var model = ShogiGlobal.ShogiModel;

            model.Redo();
        }

        /// <summary>
        /// 連続して手を戻します。
        /// </summary>
        private static void ExecuteMoveUndoContinue(object sender, ExecutedRoutedEventArgs e)
        {
            var model = ShogiGlobal.ShogiModel;
            var autoPlay = new AutoPlay(model.Board, AutoPlayType.Undo)
            {
                IsChangeBackground = false,
                IsConfirmPlay = false,
            };

            model.StartAutoPlay(autoPlay);
        }

        /// <summary>
        /// 連続して手を進めます。
        /// </summary>
        private static void ExecuteMoveRedoContinue(object sender, ExecutedRoutedEventArgs e)
        {
            var model = ShogiGlobal.ShogiModel;
            var autoPlay = new AutoPlay(model.Board, AutoPlayType.Redo)
            {
                IsChangeBackground = false,
                IsConfirmPlay = false,
            };

            model.StartAutoPlay(autoPlay);
        }

        /// <summary>
        /// 再生中の手を停止します。
        /// </summary>
        private static void ExecuteMoveStop(object sender, ExecutedRoutedEventArgs e)
        {
            var model = ShogiGlobal.ShogiModel;

            model.StopAutoPlay();
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
                var dialog = new CurrentBoardSetupDialog()
                {
                    Topmost = true,
                };
                var result = dialog.ShowDialogCenterMouse();

                if (result == true)
                {
                    SendSetCurrentBoard(model.Board);

                    if (dialog.IsClearVoteResult)
                    {
                        voteClient.ClearVote();
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
            var nicoClient = ShogiGlobal.NicoClient;
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
        /// 変化コメントを投稿します。
        /// </summary>
        public static void DoPostVariationComment()
        {
            var model = ShogiGlobal.ShogiModel;
            var nicoClient = ShogiGlobal.NicoClient;
            var commentClient = model.CommentClient;

            try
            {
                if (!CheckAndConnectToLive(commentClient, nicoClient))
                {
                    // エラーダイアログはメソッドの中で表示されます。
                    return;
                }

                if (string.IsNullOrEmpty(model.MoveTextFromCurrentBoard))
                {
                    ShogiGlobal.ErrorMessage(
                        "指し手がありません (/□≦､)");
                    return;
                }

                var text = string.Format(
                    "{0}{2}{1}",
                    model.MoveTextFromCurrentBoard,
                    model.Comment,
                    (string.IsNullOrEmpty(model.Comment) ? "" : "　"));

                const int NiconamaMaxCommentLength = 60 - 4;
                if (text.Length > NiconamaMaxCommentLength)
                {
                    ShogiGlobal.ErrorMessage(
                        "投稿コメント）{0}{1}{0}{0}文字数制限{0}{2}文字オーバーしてます (/□≦､)",
                        Environment.NewLine,
                        text,
                        text.Length - NiconamaMaxCommentLength);
                    return;
                }

                var result = DialogUtil.Show(
                    ShogiGlobal.MainWindow,
                    string.Format(
                        "コメント）{0}{1}{0}{0}を投稿します。(*´ω｀*)",
                        Environment.NewLine,
                        text),
                    "確認",
                    MessageBoxButton.OKCancel,
                    MessageBoxResult.OK);
                if (result != MessageBoxResult.OK)
                {
                    return;
                }

                commentClient.SendComment(text, "184 ");
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
                var autoPlay = new AutoPlay(variation)
                {
                    IsChangeBackground = true,
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
            var manager = ShogiGlobal.EffectManager;
            if (manager == null)
            {
                return;
            }

            manager.Resign();
        }
    }
}
