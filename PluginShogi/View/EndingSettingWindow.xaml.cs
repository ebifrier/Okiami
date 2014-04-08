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

namespace VoteSystem.PluginShogi.View
{
    /// <summary>
    /// EndingSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EndingSettingWindow : Window
    {
        private ShogiEndRollControl model;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EndingSettingWindow(ShogiEndRollControl control)
        {
            InitializeComponent();
            ViewModel.Commands.BindCommands(this);
            ViewModel.Commands.BindInputs(this);

            // テスト用のフラグはクリアしておきます。
            control.IsTest = false;

            this.model = control;
            DataContext = control;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            ShogiGlobal.Settings.Save();
        }

        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var position = TimeSpan.FromSeconds(e.NewValue);

            this.model.ResetPosition(position);
        }
    }
}
