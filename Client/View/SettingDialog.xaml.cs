﻿using System;
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

using Ragnarok;
using Ragnarok.Presentation;

namespace VoteSystem.Client.View
{
    /// <summary>
    /// SettingDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingDialog : Window
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SettingDialog()
        {
            InitializeComponent();
            InitializeCommands();
        }

        /// <summary>
        /// コマンドを初期化します。
        /// </summary>
        private void InitializeCommands()
        {
            Command.UtilCommand.Bind(CommandBindings);

            CommandBindings.Add(
                new CommandBinding(
                    DialogCommands.OK,
                    ExecuteOk));
            CommandBindings.Add(
                new CommandBinding(
                    DialogCommands.Cancel,
                    ExecuteCancel));
        }

        /// <summary>
        /// OKボタン押下。
        /// </summary>
        private void ExecuteOk(object sender,
                               ExecutedRoutedEventArgs e)
        {
            Global.Settings.Save();

            DialogResult = true;
        }

        /// <summary>
        /// キャンセルボタン押下。
        /// </summary>
        private void ExecuteCancel(object sender,
                                   ExecutedRoutedEventArgs e)
        {
            Global.Settings.Reload();

            DialogResult = false;
        }
    }
}
