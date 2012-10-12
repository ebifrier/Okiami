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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ragnarok.Presentation;

namespace VoteSystem.PluginShogi.View
{
    /// <summary>
    /// CurrentBoardSetupDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class CurrentBoardSetupDialog : Window
    {
        /// <summary>
        /// 投票結果を一緒にクリアするかどうかを示す依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty IsClearVoteResultProperty =
            DependencyProperty.Register(
                "IsClearVoteResult", typeof(bool),
                typeof(CurrentBoardSetupDialog),
                new PropertyMetadata(default(bool)));

        /// <summary>
        /// 投票結果を一緒にクリアするかどうかを取得または設定します。
        /// </summary>
        public bool IsClearVoteResult
        {
            get { return (bool)GetValue(IsClearVoteResultProperty); }
            set { SetValue(IsClearVoteResultProperty, value); }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CurrentBoardSetupDialog()
        {
            InitializeComponent();

            IsClearVoteResult = ShogiGlobal.Settings.IsClearVoteResult;

            CommandBindings.Add(
                new CommandBinding(
                    DialogCommands.OK,
                    (sender, args) =>
                    {
                        ShogiGlobal.Settings.IsClearVoteResult = IsClearVoteResult;
                        ShogiGlobal.Settings.Save();
                        DialogResult = true;
                    }));
            CommandBindings.Add(
                new CommandBinding(
                    DialogCommands.Cancel,
                    (sender, args) => DialogResult = false));
        }
    }
}
