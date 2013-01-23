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

namespace VoteSystem.Client.View
{
    /// <summary>
    /// 画像選択のためのウィンドウです。
    /// </summary>
    public partial class SelectImageWindow : Window
    {
        private const string ImagePrefix =
            "pack://application:,,,/Resources/Image/koma/";

        /// <summary>
        /// 画像URLのリストを取得します。
        /// このリストの内容は不変です。
        /// </summary>
        public static readonly List<List<string>> ImageUrlList =
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

        /// <summary>
        /// 選択された画像URLを扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty SelectedImageUrlProperty =
            DependencyProperty.Register(
                "SelectedImageUrl",
                typeof(string),
                typeof(SelectImageWindow),
                new UIPropertyMetadata(string.Empty));

        /// <summary>
        /// 選択された画像のURLを取得または設定します。
        /// </summary>
        public string SelectedImageUrl
        {
            get { return (string)GetValue(SelectedImageUrlProperty); }
            set { SetValue(SelectedImageUrlProperty, value); }
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public SelectImageWindow()
        {
            InitializeComponent();
            DialogCommands.BindCommands(CommandBindings);
        }
    }
}
