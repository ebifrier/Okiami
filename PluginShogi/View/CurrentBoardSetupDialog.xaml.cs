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

using Ragnarok;
using Ragnarok.Shogi;

namespace VoteSystem.PluginShogi.View
{
    using Protocol;

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
                if (SelectedRadioButton != CBS_SelectedRadioButton.MyTurn)
                {
                    return false;
                }

                return this.settings.CBS_IsStartVote;
            }
        }

        /// <summary>
        /// 投票開始時の投票時間を取得します。
        /// </summary>
        public TimeSpan VoteSpan
        {
            get
            {
                if (SelectedRadioButton != CBS_SelectedRadioButton.MyTurn)
                {
                    return TimeSpan.MinValue;
                }

                if (!this.settings.CBS_IsUseVoteSpan)
                {
                    return TimeSpan.MinValue;
                }

                return this.settings.CBS_VoteSpan;
            }
        }

        /// <summary>
        /// 投票を停止するかどうかを取得します。
        /// </summary>
        public bool IsVoteStop
        {
            get
            {
                if (SelectedRadioButton != CBS_SelectedRadioButton.YourTurn)
                {
                    return false;
                }

                return this.settings.CBS_IsVoteStop;
            }
        }

        /// <summary>
        /// 追加する持ち時間を取得します。
        /// </summary>
        public TimeSpan AddLimitTime
        {
            get
            {
                if (SelectedRadioButton != CBS_SelectedRadioButton.YourTurn)
                {
                    return TimeSpan.MinValue;
                }

                if (!this.settings.CBS_IsUseAddLimitTime)
                {
                    return TimeSpan.MinValue;
                }

                return this.settings.CBS_AddLimitTime;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CurrentBoardSetupDialog(Settings settings)
        {
            InitializeComponent();

            // ダイアログ表示時に操作対象となる手番を設定します。
            var board = ShogiGlobal.ShogiModel.Board;
            if (board != null)
            {
                if (board.Turn == BWType.None ||
                    settings.SD_Teban == BWType.None)
                {
                    SelectedRadioButton = CBS_SelectedRadioButton.Default;
                }
                else
                {
                    // "次の手番"が自分の手番ならば、
                    // 前に指したのは相手の手です。
                    SelectedRadioButton =
                        (board.Turn == settings.SD_Teban ?
                        CBS_SelectedRadioButton.MyTurn :
                        CBS_SelectedRadioButton.YourTurn);
                }
            }

            CommandBindings.Add(
                new CommandBinding(
                    RagnarokCommands.OK,
                    (sender, args) =>
                    {
                        this.settings.Save();
                        DialogResult = true;
                    }));
            CommandBindings.Add(
                new CommandBinding(
                    RagnarokCommands.Cancel,
                    (sender, args) =>
                    {
                        this.settings.Reload();
                        DialogResult = false;
                    }));

            this.settings = settings;
            DataContext = settings;
        }
    }
}
