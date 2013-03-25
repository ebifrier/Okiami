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

using Ragnarok.ObjectModel;
using Ragnarok.Presentation;

namespace VoteSystem.Protocol.View
{
    /// <summary>
    /// EvaluationSettingDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class EvaluationSettingDialog : Window
    {
        private sealed class InternalModel : CloneObject
        {
            public InternalModel(VoteResultControl control)
                : base(control)
            {
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EvaluationSettingDialog()
        {
            InitializeComponent();

            RagnarokCommands.Bind(this);
        }
    }
}
