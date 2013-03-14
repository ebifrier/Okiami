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
using System.Windows.Shapes;

namespace VoteSystem.Client.View.Control
{
    /// <summary>
    /// EvaluationWindowSettingDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class EvaluationWindowSettingDialog : Window
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EvaluationWindowSettingDialog()
        {
            InitializeComponent();

            Command.UtilCommand.Bind(this);
        }
    }
}
