﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows.Threading;
using System.ComponentModel;

using Ragnarok;
using Ragnarok.Shogi;
using Ragnarok.NicoNico.Live;
using Ragnarok.Net.ProtoBuf;
using Ragnarok.Presentation;

namespace VoteSystem.PluginShogi
{
    using VoteSystem.Client;

    using VoteSystem.Protocol;
    using VoteSystem.Protocol.Vote;

    using Model;

    /// <summary>
    /// クライアント側の将棋用プラグインです。
    /// </summary>
    public class ShogiPlugin : IPlugin
    {
        private List<string> whaleNameList;

        /// <summary>
        /// プラグイン名を取得します。
        /// </summary>
        public string Name
        {
            get
            {
                return "将棋";
            }
        }

        private void SpeedTest()
        {
            return;
            var count = 1;

            //ViewModel.Effects.PieceMove.LoadEffect();
            var milis = Measure(
                () => ViewModel.Effects.PieceMove.LoadEffect(),
                count);

            if (true)
            {
                Ragnarok.Presentation.DialogUtil.Show(
                    string.Format("{0}ms (１回:{1}ms)", milis, milis / count),
                    "計測結果",
                    MessageBoxButton.OK);
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private double Measure(Action f, int count)
        {
            var sw = new System.Diagnostics.Stopwatch();

            sw.Start();
            for (var i = 0; i < count; ++i)
            {
                f();
            }
            sw.Stop();

            return sw.Elapsed.TotalMilliseconds;
        }

        /// <summary>
        /// プラグインの初期化を行います。
        /// </summary>
        public void Initialize(PluginHost host)
        {
            try
            {
                ShogiGlobal.Initialize(this);

                ShogiGlobal.ClientWindow = host.Window;
                ShogiGlobal.VoteClient = host.VoteClient;
                ShogiGlobal.NicoClient = host.NicoClient;

                // ログイン時に現局面を取得するようにします。
                host.VoteClient.PropertyChanged += VoteClient_PropertyChanged;

                SpeedTest();
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "将棋プラグインの初期化に失敗しました。");
                throw;
            }
        }

        /// <summary>
        /// ログイン時に現局面を取得します。
        /// </summary>
        void VoteClient_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsLogined")
            {
                UpdateCurrentBoard(false);
                UpdateWhaleList();
            }
        }

        /// <summary>
        /// 初回起動時に現局面を取りに行きます。
        /// </summary>
        public void UpdateCurrentBoard(bool modifyBoard)
        {
            var voteClient = ShogiGlobal.VoteClient;
            if (voteClient == null || !voteClient.IsLogined)
            {
                ShogiGlobal.ShogiModel.CurrentBoard = new Board();
                return;
            }

            voteClient.SendRequest<ShogiGetCurrentBoardRequest,
                                   ShogiGetCurrentBoardResponse>(
                new ShogiGetCurrentBoardRequest(),
                (sender, e) =>
                {
                    if (e.Response == null)
                    {
                        ShogiGlobal.ErrorMessage(
                            "現局面の取得に失敗しました。(￣▽￣)ゞ");
                        return;
                    }

                    var board = e.Response.Board;
                    if (board == null || !board.Validate())
                    {
                        ShogiGlobal.ErrorMessage(
                            "取得した現局面が正しくありません。(￣▽￣)ゞ");
                        return;
                    }

                    ShogiGlobal.ShogiModel.CurrentBoard = board;
                    if (modifyBoard)
                    {
                        ShogiGlobal.ShogiModel.Board = board;
                    }
                });
        }

        /// <summary>
        /// 初回起動時に大合神クジラちゃんのクライアント一覧を取りに行きます。
        /// </summary>
        public void UpdateWhaleList()
        {
            var voteClient = ShogiGlobal.VoteClient;
            if (voteClient == null || !voteClient.IsLogined)
            {
                return;
            }

            var command = new ShogiGetWhaleClientListCommand();
            voteClient.SendCommand(command);
        }

        /// <summary>
        /// ウィンドウを表示します。
        /// </summary>
        public void Run()
        {
            var window = ShogiGlobal.MainWindow;
            if (window != null)
            {
                window.Show();
                window.Activate();
                return;
            }

            // 将棋ウィンドウを表示します。
            window = new View.MainWindow(ShogiGlobal.ShogiModel);
            window.Closed += (sender, e) =>
            {
                ShogiGlobal.MainWindow = null;
            };

            ShogiGlobal.MainWindow = window;
            window.Show();
        }

        /// <summary>
        /// 処理ハンドラを接続します。
        /// </summary>
        public void ConnectHandlers(PbConnection connection)
        {
            connection.AddCommandHandler<ShogiSetCurrentBoardCommand>(
                HandleSetCurrentBoardCommand);
            connection.AddCommandHandler<ShogiSetWhaleClientListCommand>(
                HandleSetWhaleClientListCommand);
        }

        /// <summary>
        /// 古い局面から新しい局面に到達するための指し手のリストを取得します。
        /// </summary>
        private List<Move> GetMoveListFrom(Board oldBoard, Board newBoard)
        {
            var moveList = new List<Move>();

            // １手戻したとき、今設定されている局面と同じか調べます。
            // もし同じなら、最終手を通知します。
            var tmpBoard = newBoard.Clone();

            // 古い局面から到達可能か調べます。
            while (tmpBoard.MoveCount > oldBoard.MoveCount)
            {
                var boardMove = tmpBoard.Undo();
                if (boardMove == null || !boardMove.Validate())
                {
                    return new List<Move>();
                }

                var move = tmpBoard.ConvertMove(boardMove, false);
                if (move == null || !move.Validate())
                {
                    return new List<Move>();
                }

                // moveListの先頭により古い手を入れます。
                moveList.Insert(0, move);
            }

            // 戻した局面と古い局面を比較します。
            return (tmpBoard.BoardEquals(oldBoard) ? moveList : new List<Move>());
        }

        /// <summary>
        /// 局面が変わったとき、古い局面と新しい局面との差が一手なら
        /// その一手をメッセージとして出力します。
        /// </summary>
        private IEnumerable<string> MakeBoardChangedMessageList(IEnumerable<Move> moveList,
                                                                Board oldBoard)
        {
            if (moveList == null || !moveList.Any())
            {
                return new string[] { "現局面が更新されますた" };
            }

            // 一手戻した局面と古い局面を比較します。
            return moveList.SelectWithIndex((move, i) =>
                string.Format(
                    "{0}手目　{1}",
                    oldBoard.MoveCount + 1 + i,
                    move.ToString()));
        }

        /// <summary>
        /// 必要なら表示局面を更新します。
        /// </summary>
        private void UpdateBoard(Board board)
        {
            var statusBar = ShogiGlobal.MainStatusBar;
            var model = ShogiGlobal.ShogiModel;

            // 表示局面と違う場合は更新するか確認します。
            if (statusBar != null &&
                (model.Board == null || !model.Board.BoardEquals(board)))
            {
                statusBar.SetMessage(
                    "現局面が更新されました。左上のボタンから局面が更新できます");
            }
        }

        /// <summary>
        /// 現局面を設定します。
        /// </summary>
        private void HandleSetCurrentBoardCommand(
            object sender,
            PbCommandEventArgs<ShogiSetCurrentBoardCommand> e)
        {
            var board = e.Command.Board;
            if (board == null || !board.Validate())
            {
                return;
            }

            // もし同じ内容なら、局面の更新は行いません。
            var model = ShogiGlobal.ShogiModel;
            if (model.CurrentBoard != null &&
                model.CurrentBoard.BoardEquals(board))
            {
                return;
            }

            // 局面を更新します。
            var oldBoard = model.CurrentBoard;
            model.CurrentBoard = board;

            // 表示局面と違う場合は更新するか確認します。
            WpfUtil.UIProcess(() => UpdateBoard(board));

            // 重要メッセージとして差し手を表示します。
            var moveList = GetMoveListFrom(oldBoard, board);
            var messageList = MakeBoardChangedMessageList(moveList, oldBoard);
            foreach (var message in messageList)
            {
                ShogiGlobal.VoteClient.OnNotificationReceived(
                    new Notification()
                    {
                        Type = NotificationType.Important,
                        Text = message,
                        VoterId = "$system$",
                        Timestamp = Ragnarok.Net.NtpClient.GetTime(),
                    });
            }

            // エフェクトの表示を行います。
            var effectManager = ShogiGlobal.EffectManager;
            if (effectManager != null)
            {
                effectManager.ChangeMoveCount(board.MoveCount);
            }
        }

        /// <summary>
        /// 大合神クジラちゃんの参加者が変わったときに呼ばれます。
        /// </summary>
        private void HandleSetWhaleClientListCommand(
            object sender,
            PbCommandEventArgs<ShogiSetWhaleClientListCommand> e)
        {
            var nameList = e.Command.NameList;
            nameList = (nameList ?? new List<string>());

            // 投票モード固有の評価値を設定します。
            Global.ModeCustomPoint = e.Command.Value;

            // 初回取得時は、取得のみを行います。
            if (this.whaleNameList == null)
            {
                this.whaleNameList = nameList;
                return;
            }

            // 増えた参加者のみを表示します。
            foreach (var client in nameList.Except(this.whaleNameList))
            {
                var message = string.Format(
                    "大合神クジラちゃん: {0}さんが参加しました。",
                    client);

                ShogiGlobal.VoteClient.OnNotificationReceived(
                    new Notification()
                    {
                        Type = NotificationType.Important,
                        Text = message,
                        VoterId = "$system$",
                        Timestamp = Ragnarok.Net.NtpClient.GetTime(),
                    });
            }

            // 名前一覧は更新します。
            this.whaleNameList = nameList;
        }

        /// <summary>
        /// 受信した通知を処理します。
        /// </summary>
        public void HandleNotification(Notification notification)
        {
            var effectManager = ShogiGlobal.EffectManager;
            if (effectManager == null)
            {
                return;
            }

            // 何もしません。
            if (notification.Type == NotificationType.Vote)
            {
                var move = ShogiParser.ParseMove(notification.Text, true);
                if ((object)move == null || !move.Validate())
                {
                    return;
                }

                if (move.SameAsOld)
                {
                    var board = ShogiGlobal.ShogiModel.CurrentBoard;
                    if (board != null)
                    {
                        move.NewPosition = board.PrevMovedPosition;
                    }
                }

                // エフェクトを表示します。
                effectManager.Voted(move);
            }
        }
    }
}