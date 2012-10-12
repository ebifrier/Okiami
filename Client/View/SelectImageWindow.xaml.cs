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

namespace VoteSystem.Client.View
{
    /// <summary>
    /// 画像選択のためのウィンドウです。
    /// </summary>
    public partial class SelectImageWindow : Window
    {
        /// <summary>
        /// ウィンドウのためのモデルデータです。
        /// </summary>
        public class SelectImageViewModel : NotifyObject
        {
            private const string ImagePrefix =
                "pack://application:,,,/Resources/Image/koma/";

            private static readonly List<List<string>> imageUrlList =
                new List<List<string>>()
            {
                new List<string>
                {
                    ImagePrefix + "koma_cat_r.png",
                    ImagePrefix + "koma_cat_pi.png",
                    ImagePrefix + "koma_cat_p.png",
                    ImagePrefix + "koma_cat_b.png",
                    ImagePrefix + "koma_cat_g.png",
                    ImagePrefix + "koma_cat_y.png",
                    ImagePrefix + "koma_cat_o.png",
                    ImagePrefix + "koma_cat_k.png",
                    ImagePrefix + "koma_cat_w.png",
                },
                new List<string>
                {
                    ImagePrefix + "koma_panda_r.png",
                    ImagePrefix + "koma_panda_pi.png",
                    ImagePrefix + "koma_panda_p.png",
                    ImagePrefix + "koma_panda_b.png",
                    ImagePrefix + "koma_panda_g.png",
                    ImagePrefix + "koma_panda_y.png",
                    ImagePrefix + "koma_panda_o.png",
                    ImagePrefix + "koma_panda_k.png",
                    ImagePrefix + "koma_panda_w.png",
                },
                new List<string>
                {
                    ImagePrefix + "koma_pig_r.png",
                    ImagePrefix + "koma_pig_pi.png",
                    ImagePrefix + "koma_pig_p.png",
                    ImagePrefix + "koma_pig_b.png",
                    ImagePrefix + "koma_pig_g.png",
                    ImagePrefix + "koma_pig_y.png",
                    ImagePrefix + "koma_pig_o.png",
                    ImagePrefix + "koma_pig_k.png",
                    ImagePrefix + "koma_pig_w.png",
                },
                new List<string>
                {
                    ImagePrefix + "koma_rabbit_r.png",
                    ImagePrefix + "koma_rabbit_pi.png",
                    ImagePrefix + "koma_rabbit_p.png",
                    ImagePrefix + "koma_rabbit_b.png",
                    ImagePrefix + "koma_rabbit_g.png",
                    ImagePrefix + "koma_rabbit_y.png",
                    ImagePrefix + "koma_rabbit_o.png",
                    ImagePrefix + "koma_rabbit_k.png",
                    ImagePrefix + "koma_rabbit_w.png",
                },
                new List<string>
                {
                    ImagePrefix + "koma_moe_gyoku.png",
                    ImagePrefix + "koma_noimage.png",
                },
            };
            private string selectedImageUrl = "";

            /// <summary>
            /// 画像URLのリストを取得します。
            /// このリストの内容は不変です。
            /// </summary>
            public static List<List<string>> ImageUrlList
            {
                get
                {
                    return imageUrlList;
                }
            }

            /// <summary>
            /// 選択された画像のURLを取得または設定します。
            /// </summary>
            public string SelectedImageUrl
            {
                get
                {
                    return this.selectedImageUrl;
                }
                set
                {
                    SetValue("SelectedImageUrl", value, ref this.selectedImageUrl);
                }
            }
        }

        private readonly SelectImageViewModel viewModel = new SelectImageViewModel();

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public SelectImageWindow()
        {
            InitializeComponent();

            this.DataContext = this.viewModel;
        }

        /// <summary>
        /// 選択された画像のURLを取得します。
        /// </summary>
        public string SelectedImageUrl
        {
            get
            {
                return this.viewModel.SelectedImageUrl;
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (ListBox)sender;

            this.viewModel.SelectedImageUrl = (string)listBox.SelectedItem;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
