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
using System.ComponentModel;

namespace VoteSystem.PluginShogi.View
{
    using VoteSystem.PluginShogi.Model;
    using VoteSystem.PluginShogi.ViewModel;

    /// <summary>
    /// MoveManageView.xaml の相互作用ロジック
    /// </summary>
    public partial class MoveManageView : Window
    {
        void State_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var control = (FrameworkElement)sender;
            var stateManager = control.DataContext as EachStateManager;
            var model = ShogiGlobal.ShogiModel;

            if (stateManager != null)
            {
                model.Board = stateManager.Board.Clone();
            }
        }

        void Variation_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var control = (FrameworkElement)sender;
            var variation = control.DataContext as Variation;

            Commands.ExecuteMoveToVariationState(variation);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MoveManageView()
        {
            InitializeComponent();

            Commands.Binding(CommandBindings);
            Commands.Binding(InputBindings);
        }
    }
}
