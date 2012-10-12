using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Ragnarok;
using Ragnarok.Presentation;

namespace VoteSystem.Client.View
{
    /// <summary>
    /// VoteResultSettingDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class VoteResultSettingDialog : Window
    {
        /// <summary>
        /// 背景の色選択コマンドです。
        /// </summary>
        public readonly static ICommand SelectFixingBackgroundColor =
            new RoutedUICommand(
                "背景色を選択します。",
                "SelectFixingBackgroundColor",
                typeof(Window));
        /// <summary>
        /// 文字色の選択コマンドです。
        /// </summary>
        public readonly static ICommand SelectFontColor =
            new RoutedUICommand(
                "文字色を選択します。",
                "SelectFontColor",
                typeof(Window));
        /// <summary>
        /// 文字の縁色の選択コマンドです。
        /// </summary>
        public readonly static ICommand SelectFontEdgeColor =
            new RoutedUICommand(
                "文字の縁色を選択します。",
                "SelectFontEdgeColor",
                typeof(Window));

        /// <summary>
        /// フォントファミリの一覧を取得します。
        /// </summary>
        public static List<string> FontFamilyNameList
        {
            get;
            private set;
        }

        /// <summary>
        /// キャストされたモデルを取得します。
        /// </summary>
        private ViewModel.VoteResultWindowViewModel Model
        {
            get
            {
                return (ViewModel.VoteResultWindowViewModel)DataContext;
            }
        }

        /// <summary>
        /// 静的コンストラクタ
        /// </summary>
        static VoteResultSettingDialog()
        {
            var language = XmlLanguage.GetLanguage("ja-jp");

            // 日本語フォントのみをリストアップします。
            FontFamilyNameList = Fonts.SystemFontFamilies
                .Select(ff => ff.FamilyNames.FirstOrDefault(fn => fn.Key == language))
                .Where(fn => fn.Key != null)
                .Select(fn => fn.Value)
                .ToList();
        }

        /// <summary>
        /// ListBoxの選択アイテムを更新します。
        /// </summary>
        private void VoteResultSettingDialog_Loaded(object sender, RoutedEventArgs e)
        {
            var model = DataContext as ViewModel.VoteResultWindowViewModel;
            if (model == null)
            {
                return;
            }

            this.fontFamilyListBox.ScrollIntoView(
                Global.Settings.VR_FontFamilyName);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VoteResultSettingDialog()
        {
            InitializeComponent();
            InitCommands();

            Loaded += new RoutedEventHandler(VoteResultSettingDialog_Loaded);
        }

        /// <summary>
        /// コマンドバインディングを行います。
        /// </summary>
        private void InitCommands()
        {
            CommandBindings.Add(
                new CommandBinding(
                    DialogCommands.OK,
                    ExecuteOK));
            CommandBindings.Add(
                new CommandBinding(
                    DialogCommands.Cancel,
                    ExecuteCancel));

            CommandBindings.Add(
                new CommandBinding(
                    SelectFixingBackgroundColor,
                    ExecuteSelectFixingBackgroundColor));
            CommandBindings.Add(
                new CommandBinding(
                    SelectFontColor,
                    ExecuteSelectFontColor));
            CommandBindings.Add(
                new CommandBinding(
                    SelectFontEdgeColor,
                    ExecuteSelectFontEdgeColor));
        }

        /// <summary>
        /// OKボタン
        /// </summary>
        private void ExecuteOK(object sender, ExecutedRoutedEventArgs e)
        {
            Global.Settings.Save();

            DialogResult = true;
        }

        /// <summary>
        /// キャンセルボタン
        /// </summary>
        private void ExecuteCancel(object sender, ExecutedRoutedEventArgs e)
        {
            Global.Settings.Reload();

            DialogResult = false;
        }

        /// <summary>
        /// 背景色を選択します。
        /// </summary>
        private void ExecuteSelectFixingBackgroundColor(object sender,
                                                        ExecutedRoutedEventArgs e)
        {
            var result = DialogUtil.ShowColorDialog(
                Global.Settings.VR_FixingBackgroundColor, this);

            if (result != null)
            {
                Global.Settings.VR_FixingBackgroundColor = result.Value;
            }
        }

        /// <summary>
        /// 文字色を選択します。
        /// </summary>
        private void ExecuteSelectFontColor(object sender,
                                            ExecutedRoutedEventArgs e)
        {
            var result = DialogUtil.ShowColorDialog(
                Global.Settings.VR_FontColor, this);

            if (result != null)
            {
                Global.Settings.VR_FontColor = result.Value;
            }
        }

        /// <summary>
        /// 文字の縁色を選択します。
        /// </summary>
        private void ExecuteSelectFontEdgeColor(object sender,
                                                ExecutedRoutedEventArgs e)
        {
            var result = DialogUtil.ShowColorDialog(
                Global.Settings.VR_FontEdgeColor, this);

            if (result != null)
            {
                Global.Settings.VR_FontEdgeColor = result.Value;
            }
        }
    }
}
