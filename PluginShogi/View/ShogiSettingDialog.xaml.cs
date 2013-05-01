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

using Ragnarok;
using Ragnarok.Presentation;

namespace VoteSystem.PluginShogi.View
{
    using VoteSystem.PluginShogi.ViewModel;

    /// <summary>
    /// ShogiSettingDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class ShogiSettingDialog : Window
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ShogiSettingDialog()
        {
            InitializeComponent();
            
            CommandBindings.Add(
                new CommandBinding(
                    ApplicationCommands.Open,
                    ExecuteOpenBackground));
            CommandBindings.Add(
                new CommandBinding(
                    RagnarokCommands.OK,
                    ExecuteOK));
            CommandBindings.Add(
                new CommandBinding(
                    RagnarokCommands.Cancel,
                    ExecuteCancel));
        }

        private void ExecuteOpenBackground(object sender, ExecutedRoutedEventArgs e)
        {
            var model = DataContext as ShogiSettingDialogViewModel;
            if (model == null)
            {
                return;
            }

            // 転送するログファイル名を選択。
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = ".jpg",
                ReadOnlyChecked = true,
                Title = "背景画像を選択します。",
                Multiselect = false,
                Filter = "画像ファイル(*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.gif|すべてのファイル(*.*)|*.*",
            };
            var result = dialog.ShowDialog();
            if (result == null || !result.Value)
            {
                return;
            }

            model.BackgroundPath = dialog.FileName;
        }

        private void ExecuteOK(object sender, ExecutedRoutedEventArgs e)
        {
            var model = ShogiGlobal.ShogiModel;

            ShogiGlobal.Settings.Save();

            // ここでエフェクト設定を更新しないと、
            // 一部のエフェクトに設定が反映されません。
            ShogiGlobal.EffectManager.InitEffect(model.Board.Turn);
            ShogiGlobal.EffectManager.UpdateBackground();

            DialogResult = true;
        }

        private void ExecuteCancel(object sender, ExecutedRoutedEventArgs e)
        {
            ShogiGlobal.Settings.Reload();

            DialogResult = false;
        }
    }
}
