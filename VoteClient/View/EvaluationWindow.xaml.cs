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

using Ragnarok;
using Ragnarok.Presentation.Control;

namespace VoteSystem.Client.View
{
    /// <summary>
    /// 評価値ウィンドウです。
    /// </summary>
    public partial class EvaluationWindow : MovableWindow
    {
        /// <summary>
        /// 静的コンストラクタ
        /// </summary>
        static EvaluationWindow()
        {
            IsMovableProperty.OverrideMetadata(
                typeof(EvaluationWindow),
                new FrameworkPropertyMetadata(true));
            EdgeLengthProperty.OverrideMetadata(
                typeof(EvaluationWindow),
                new FrameworkPropertyMetadata(10.0));
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EvaluationWindow()
        {
            InitializeComponent();
            EvaluationControl.Bind(this);
        }
    }
}
