using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

using Ragnarok.ObjectModel;

namespace VoteSystem.Client.Command
{
    using Model;
    using View;

    /// <summary>
    /// 各コントロールの処理を記述しています。
    /// </summary>
    public static partial class Commands
    {
        /// <summary>
        /// 情報表示ウィンドウの表示/非表示を切り替えるコマンドです。
        /// </summary>
        public readonly static ICommand ToggleShowVoteResultWindow =
            new RoutedUICommand(
                "投票結果ウィンドウを表示します。",
                "ToggleShowVoteResultWindow",
                typeof(Window));
        /// <summary>
        /// 情報表示ウィンドウを可動状態にするためのコマンドです。
        /// </summary>
        public readonly static ICommand MakeMoveWindow =
            new RoutedUICommand(
                "ウィンドウの移動を可能にします。",
                "MakeMoveWindow",
                typeof(Window));
        /// <summary>
        /// 情報表示ウィンドウを固定状態にするためのコマンドです。
        /// </summary>
        public readonly static ICommand MakeFixWindow =
            new RoutedUICommand(
                "ウィンドウの移動を停止にします。",
                "MakeFixWindow",
                typeof(Window));
    }
}
