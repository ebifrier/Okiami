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
        private CloneObject model;
        private EvaluationControl control;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EvaluationSettingDialog(EvaluationControl control)
        {
            InitializeComponent();
            InitCommands();
            RagnarokCommands.Bind(this);            

            this.model = new CloneObject(control);
            this.control = control;

            DataContext = model;
        }

        /// <summary>
        /// コマンドバインディングを行います。
        /// </summary>
        private void InitCommands()
        {
            CommandBindings.Add(
                new CommandBinding(
                    RagnarokCommands.OK,
                    ExecuteYes));
        }

        /// <summary>
        /// OK/YES
        /// </summary>
        private void ExecuteYes(object sender, ExecutedRoutedEventArgs e)
        {
            // OKの場合は、プロパティ値をコントロールに設定します。
            this.model.SetValuesToTarget(this.control);

            DialogResult = true;
        }
    }
}
