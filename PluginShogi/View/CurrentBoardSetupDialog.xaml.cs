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
    /// 手番ごとの特殊操作を選択するときに使われます。
    /// </summary>
    public enum CBS_SelectedRadioButton
    {
        /// <summary>
        /// 特殊操作はありません。
        /// </summary>
        Default,
        /// <summary>
        /// 自分の手番のときに現局面を更新します。
        /// </summary>
        MyTurn,
        /// <summary>
        /// 相手の手番の時に現局面を更新します。
        /// </summary>
        YourTurn,
    }

    /// <summary>
    /// CurrentBoardSetupDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class CurrentBoardSetupDialog : Window
    {
        private Settings settings;

        /// <summary>
        /// 投票結果を一緒にクリアするかどうかを示す依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty SelectedRadioButtonProperty =
            DependencyProperty.Register(
                "SelectedRadioButton", typeof(CBS_SelectedRadioButton),
                typeof(CurrentBoardSetupDialog),
                new PropertyMetadata(default(CBS_SelectedRadioButton)));

        /// <summary>
        /// 投票結果を一緒にクリアするかどうかを取得または設定します。
        /// </summary>
        public CBS_SelectedRadioButton SelectedRadioButton
        {
            get { return (CBS_SelectedRadioButton)GetValue(SelectedRadioButtonProperty); }
            set { SetValue(SelectedRadioButtonProperty, value); }
        }

        /// <summary>
        /// 投票結果を一緒にクリアするかどうかを取得します。
        /// </summary>
        public bool IsClearVoteResult
        {
            get { return this.settings.CBS_IsClearVoteResult; }
        }

        /// <summary>
        /// 投票を開始するかどうかを取得します。
        /// </summary>
        public bool IsStartVote
        {
            get
            {
                // 相手の手番なら、投票を開始する可能性があります。
                if (SelectedRadioButton != CBS_SelectedRadioButton.YourTurn)
                {
                    return false;
                }

                return this.settings.CBS_IsStartVote;
            }
        }

        /// <summary>
        /// 投票を停止するかどうかを取得します。
        /// </summary>
        public bool IsStopVote
        {
            get
            {
                if (SelectedRadioButton != CBS_SelectedRadioButton.MyTurn)
                {
                    return false;
                }

                return this.settings.CBS_IsStopVote;
            }
        }

        /// <summary>
        /// 持ち時間を追加するかどうかを取得します。
        /// </summary>
        public bool IsAddLimitTime
        {
            get
            {
                if (SelectedRadioButton != CBS_SelectedRadioButton.MyTurn)
                {
                    return false;
                }

                return this.settings.CBS_IsAddLimitTime;
            }
        }

        /// <summary>
        /// 追加する持ち時間を取得します。
        /// </summary>
        public TimeSpan AddLimitTime
        {
            get
            {
                if (SelectedRadioButton != CBS_SelectedRadioButton.MyTurn)
                {
                    return TimeSpan.Zero;
                }

                return TimeSpan.FromSeconds(
                    this.settings.CBS_AddLimitTimeMinutes * 60 +
                    this.settings.CBS_AddLimitTimeSeconds);
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CurrentBoardSetupDialog(Settings settings)
        {
            InitializeComponent();

            this.settings = settings;
            DataContext = settings;

            CommandBindings.Add(
                new CommandBinding(
                    DialogCommands.OK,
                    (sender, args) =>
                    {
                        this.settings.Save();
                        DialogResult = true;
                    }));
            CommandBindings.Add(
                new CommandBinding(
                    DialogCommands.Cancel,
                    (sender, args) =>
                    {
                        this.settings.Reload();
                        DialogResult = false;
                    }));
        }
    }
}
