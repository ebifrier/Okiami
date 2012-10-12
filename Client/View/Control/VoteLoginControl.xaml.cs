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

namespace VoteSystem.Client.View.Control
{
    /// <summary>
    /// VoteLoginControl.xaml の相互作用ロジック
    /// </summary>
    public partial class VoteLoginControl : UserControl
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VoteLoginControl()
        {
            InitializeComponent();

            this.password.Password = Global.MainModel.VoteRoomPassword;
        }

        private void voteRoomPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var password = (PasswordBox)sender;

            Global.MainModel.VoteRoomPassword = password.Password;
        }
    }
}
