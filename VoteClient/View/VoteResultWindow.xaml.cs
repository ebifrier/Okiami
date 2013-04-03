using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;
using System.Globalization;

using Ragnarok.Presentation.Control;

namespace VoteSystem.Client.View
{
    /// <summary>
    /// 投票結果を表示します。
    /// </summary>
    public partial class VoteResultWindow : MovableWindow
    {
        /// <summary>
        /// モデルデータです。
        /// </summary>
        private class InternalModel
        {
            /// <summary>
            /// 投票用オブジェクトを取得します。
            /// </summary>
            public Model.VoteClient VoteClient
            {
                get { return Global.VoteClient; }
            }

            /// <summary>
            /// 設定オブジェクトを取得します。
            /// </summary>
            public Settings Settings
            {
                get { return Global.Settings; }
            }
        }

        /// <summary>
        /// 静的コンストラクタ
        /// </summary>
        static VoteResultWindow()
        {
            TopmostProperty.OverrideMetadata(
                typeof(VoteResultWindow),
                new FrameworkPropertyMetadata(true));
            EdgeLengthProperty.OverrideMetadata(
                typeof(VoteResultWindow),
                new FrameworkPropertyMetadata(10.0));
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VoteResultWindow()
        {
            InitializeComponent();

            DataContext = new InternalModel();

            this.voteResultControl.BindCommands(this);
            this.voteResultControl.SettingUpdated += VoteResultControl_SettingUpdated;
        }

        private void VoteResultControl_SettingUpdated(object sender, RoutedEventArgs e)
        {
            Global.Settings.Save();
        }
    }
}
