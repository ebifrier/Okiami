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
        /// <summary>
        /// エンディングモードかどうかを扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty IsEndingModeProperty =
            DependencyProperty.Register(
                "IsEndingMode", typeof(bool), typeof(EndingSettingWindow),
                new FrameworkPropertyMetadata(false, OnEndingModeChanged));

        /// <summary>
        /// エンディングモードかどうかを取得または設定します。
        /// </summary>
        public bool IsEndingMode
        {
            get { return (bool)GetValue(IsEndingModeProperty); }
            set { SetValue(IsEndingModeProperty, value); }
        }

        private static void OnEndingModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (ShogiGlobal.GlobalModel != null)
            {
                ShogiGlobal.GlobalModel.IsEndingMode = (bool)e.NewValue;
            }
        }

        private ShogiEndRollControl model;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EndingSettingWindow(ShogiEndRollControl control)
        {
            InitializeComponent();
            ViewModel.Commands.Binding(CommandBindings);
            ViewModel.Commands.Binding(InputBindings);

            this.model = control;
            DataContext = control;
        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            this.model.IsMovieMute = !this.model.IsMovieMute;
        }

        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var position = TimeSpan.FromSeconds(e.NewValue);

            this.model.ResetPosition(position);
        }
    }
}
