using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

using Ragnarok;

namespace VoteSystem.Client.Command
{
    using VoteSystem.Protocol;
    using VoteSystem.Protocol.Vote;
    using VoteSystem.Client.View;

    public static partial class Commands
    {
        /// <summary>
        /// 対局相手の指し手を設定するコマンドです。
        /// </summary>
        public readonly static ICommand SetOpponentMove =
            new RoutedUICommand(
                "相手の指し手を設定します。",
                "SetOpponentMove",
                typeof(Window));
    }

    /// <summary>
    /// 将棋関連のコマンドの実行メソッドなどを持ちます。
    /// </summary>
    public static class ShogiCommands
    {
        private static Move prevOpponentMove = null;

        /// <summary>
        /// コマンドを指定のオブジェクトにバインディングします。
        /// </summary>
        public static void Bind(CommandBindingCollection bindings)
        {
            bindings.Add(
                new CommandBinding(
                    Commands.SetOpponentMove,
                    ExecuteSetOpponentMove,
                    CanExecuteCommand));
        }

        /// <summary>
        /// コマンドが実行できるか調べます。
        /// </summary>
        public static void CanExecuteCommand(object sender,
                                             CanExecuteRoutedEventArgs e)
        {
            if (e.Command == Commands.SetOpponentMove)
            {
                e.CanExecute = (
                    Global.VoteClient.IsLogined &&
                    (Global.VoteClient.VoteState == VoteState.Voting ||
                     Global.VoteClient.VoteState == VoteState.Pause));
            }
            else
            {
                e.CanExecute = true;
            }

            e.Handled = true;
        }

        /// <summary>
        /// 対局相手の指し手を設定します。
        /// </summary>
        public static void ExecuteSetOpponentMove(object sender,
                                                  ExecutedRoutedEventArgs e)
        {
            try
            {
                var cm = new OpponentMoveWindow(prevOpponentMove);
                cm.Owner = Global.MainWindow;

                // ダイアログから指し手を設定してもらいます。
                var result = cm.ShowDialog();
                if (result != null && result.Value)
                {
                    var move = cm.Move;

                    // 指し手をサーバーに送信します。
                    Global.VoteClient.SetOpponentMove(move);

                    // 次設定するときのために、指し手を記憶しておきます。
                    prevOpponentMove = move;
                }
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "指し手の設定に失敗しました (;´Д`)");
            }
        }
    }
}
