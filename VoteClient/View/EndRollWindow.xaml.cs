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
using System.Windows.Media.Effects;
using System.Windows.Shapes;

using Ragnarok.Presentation.Control;

namespace VoteSystem.Client.View
{
    using Protocol.View;
    using Protocol.Vote;

    /// <summary>
    /// エンドロールを流すウィンドウです。
    /// </summary>
    public partial class EndRollWindow : MovableWindow
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EndRollWindow()
        {
            InitializeComponent();

            this.endRoll.InitializeBindings(this);
            this.endRoll.DataGetter = GetVoterList;
        }

        /// <summary>
        /// 投票者リストを更新します。
        /// </summary>
        private static VoterList GetVoterList()
        {
            try
            {
                return Global.VoteClient.GetVoterList();
                //return Protocol.Model.TestVoterList.GetTestVoterList();
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "参加者リストの取得に失敗しました。(-A-;)");

                return null;
            }
        }
    }
}
