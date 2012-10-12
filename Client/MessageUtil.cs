using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using Ragnarok;
using Ragnarok.Presentation;

namespace VoteSystem.Client
{
    using Protocol;

    /// <summary>
    /// エラーメッセージなどを出力します。
    /// </summary>
    public static class MessageUtil
    {
        /// <summary>
        /// エラーメッセージを表示します。
        /// </summary>
        public static void ErrorMessage(string message)
        {
            Global.UIProcess(() =>
            {
                //Log.Error(message);

                //Global.StatusMessageViewModel.SetErrorMessage(message);

                var dialog = DialogUtil.CreateDialog(
                    Global.MainWindow,
                    message,
                    "エラー！！",
                    MessageBoxButton.OK);

                // 最前面に表示するウィンドウがあるため、
                // そのウィンドウの下にきた場合、
                // こうしないとウィンドウが操作できなくなることがあります。
                dialog.Topmost = true;

                dialog.ShowDialog();
            });
        }

        /// <summary>
        /// エラーメッセージを表示します。
        /// </summary>
        public static void ErrorMessage(Exception ex, string message)
        {
            var text = string.Format(
                "{1}{0}{0}理由: {2}",
                Environment.NewLine,
                message, ex.Message);

            ErrorMessage(text);
        }

        /// <summary>
        /// エラーメッセージを表示します。
        /// </summary>
        public static void ErrorMessage(int error, string message)
        {
            var desc = ErrorCodeUtil.GetDescription(error);
            var text = string.Format(
                "{1}{0}{0} 詳細: {2}",
                Environment.NewLine,
                message, desc);

            ErrorMessage(text);
        }

        /// <summary>
        /// メッセージダイアログを表示します。
        /// </summary>
        public static void Message(string message)
        {
            Global.UIProcess(() =>
            {
                //Log.Trace(message);

                /*MessageBox.Show(
                    message,
                    "情報ボックス",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);*/
                Global.StatusBar.SetMessage(message);
            });
        }

        /// <summary>
        /// Yes/Noの確認メッセージを出します。
        /// </summary>
        public static bool Confirm(string message, string title)
        {
            var dialog = DialogUtil.CreateDialog(
                Global.MainWindow,
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxResult.No);

            // 最前面に表示するウィンドウがあるため、
            // そのウィンドウの下にきた場合、
            // こうしないとウィンドウが操作できなくなることがあります。
            dialog.Topmost = true;

            dialog.ShowDialog();
            return (dialog.ResultButton == MessageBoxResult.Yes);
        }
    }
}
