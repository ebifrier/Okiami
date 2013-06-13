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
        /// 音量ミュートかどうかを扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty IsMuteProperty =
            DependencyProperty.Register(
                "IsMute", typeof(bool), typeof(EndingSettingWindow),
                new FrameworkPropertyMetadata(false, OnIsMuteChanged));

        /// <summary>
        /// 音量ミュートかどうかを取得または設定します。
        /// </summary>
        public bool IsMute
        {
            get { return (bool)GetValue(IsMuteProperty); }
            set { SetValue(IsMuteProperty, value); }
        }

        private static void OnIsMuteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (EndingSettingWindow)d;

            if (self.model != null)
            {
                self.model.IsMovieMute = (bool)e.NewValue;
            }
        }

        /// <summary>
        /// 音量を扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty VolumeProperty =
            DependencyProperty.Register(
                "Volume", typeof(int), typeof(EndingSettingWindow),
                new FrameworkPropertyMetadata(100, OnVolumeChanged));

        /// <summary>
        /// 音量を取得または設定します。
        /// </summary>
        public int Volume
        {
            get { return (int)GetValue(VolumeProperty); }
            set { SetValue(VolumeProperty, value); }
        }

        private static void OnVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (EndingSettingWindow)d;

            if (self.model != null)
            {
                self.model.MovieVolume = (int)e.NewValue;
            }
        }

        private ShogiEndRollControl model;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EndingSettingWindow(MainWindow window)
        {
            InitializeComponent();
            ViewModel.Commands.Binding(CommandBindings);
            ViewModel.Commands.Binding(InputBindings);

            if (window != null)
            {
                this.model = window.ShogiEndRoll;

                IsMute = this.model.IsMovieMute;
                Volume = this.model.MovieVolume;
            }
        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            IsMute = !IsMute;
        }
    }
}
