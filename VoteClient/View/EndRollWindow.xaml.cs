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
            this.endRoll.InitializeCommands(CommandBindings);
        }
    }
}
