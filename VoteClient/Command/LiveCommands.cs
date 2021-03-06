﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Input;

using Ragnarok;
using Ragnarok.NicoNico;
using Ragnarok.NicoNico.Login;
using Ragnarok.NicoNico.Live;
using Ragnarok.Presentation.NicoNico;

namespace VoteSystem.Client.Command
{
    using Model.Live;

    /// <summary>
    /// コマンドを定義しています。
    /// </summary>
    public static partial class Commands
    {
        /// <summary>
        /// ニコニコへのログインコマンドです。
        /// </summary>
        public readonly static ICommand LoginNico =
            new RoutedUICommand(
                "ニコニコにログインします。",
                "LoginNico",
                typeof(Window));

        /// <summary>
        /// 放送への接続コマンドです。
        /// </summary>
        public readonly static ICommand ConnectToLive =
            new RoutedUICommand(
                "各サイトの放送に接続します。",
                "ConnectToLive",
                typeof(Window));
        /// <summary>
        /// 放送からの切断コマンドです。
        /// </summary>
        public readonly static ICommand DisconnectToLive =
            new RoutedUICommand(
                "各放送サイトから切断します。",
                "DisconnectToLive",
                typeof(Window));
    }

    /// <summary>
    /// ニコニコ関連コマンドの実行メソッドなどを持ちます。
    /// </summary>
    public static class LiveCommands
    {
        /// <summary>
        /// コマンドを指定のオブジェクトにバインディングします。
        /// </summary>
        public static void Bind(UIElement element)
        {
            element.CommandBindings.Add(
                new CommandBinding(
                    Commands.LoginNico,
                    ExecuteLoginNico,
                    CanExecuteCommand));

            element.CommandBindings.Add(
                new CommandBinding(
                    Commands.ConnectToLive,
                    ExecuteConnectToLive,
                    CanExecuteCommand));
            element.CommandBindings.Add(
                new CommandBinding(
                    Commands.DisconnectToLive,
                    ExecuteDisconnectToLive,
                    CanExecuteCommand));
        }

        /// <summary>
        /// 投票ルームに渡される放送主の画像が選択可能かどうかです。
        /// </summary>
        public static void CanExecuteCommand(object sender,
                                             CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        /// <summary>
        /// ニコニコにログインします。
        /// </summary>
        public static void ExecuteLoginNico(object sender,
                                            ExecutedRoutedEventArgs e)
        {
            var nicoClient = (NicoClient)e.Parameter;

            // ログインウィンドウを作成します。
            var loginWindow = new LoginWindow(nicoClient)
            {
                IsShowMessageDialog = true,
                Owner = Global.MainWindow,
            };

            // ログインウィンドウを開き、ニコニコへのログインを行います。            
            loginWindow.ShowDialog();
        }

        /// <summary>
        /// 各サイトの放送に接続します。
        /// </summary>
        public static void ExecuteConnectToLive(object sender,
                                                ExecutedRoutedEventArgs e)
        {
            try
            {
                var liveClient = (LiveClient)e.Parameter;

                if (string.IsNullOrEmpty(liveClient.LiveUrlText))
                {
                    MessageUtil.ErrorMessage(
                        "放送URLが設定されていません (*_ _)人");
                    return;
                }

                liveClient.ConnectCommand();
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "放送への接続に失敗しました (T∇T)");
                return;
            }

            MessageUtil.Message(
                "放送への接続に成功しました ♪(*^-^)/⌒☆ﾞ");
        }

        /// <summary>
        /// 各サイトの放送から切断します。
        /// </summary>
        public static void ExecuteDisconnectToLive(object sender,
                                                   ExecutedRoutedEventArgs e)
        {
            try
            {
                var liveClient = (LiveClient)e.Parameter;

                liveClient.DisconnectCommand();
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "放送からの切断に失敗しました (T∇T)");
                return;
            }

            MessageUtil.Message(
                "放送からの切断に成功しました ♪(*^-^)/⌒☆ﾞ");
        }
    }
}
