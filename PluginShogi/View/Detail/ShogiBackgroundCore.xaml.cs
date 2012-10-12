using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Ragnarok.Shogi.ViewModel;

namespace VoteSystem.PluginShogi.View.Detail
{
    using ViewModel;

    /// <summary>
    /// ShogiBackground.xaml の相互作用ロジック
    /// </summary>
    public partial class ShogiBackgroundCore : UserControl
    {
        private EntityObject rootEffect = null;

        /// <summary>
        /// 背景エフェクト名を扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty EffectKeyroperty =
            DependencyProperty.Register(
                "EffectKey", typeof(string), typeof(ShogiBackgroundCore),
                new FrameworkPropertyMetadata(string.Empty, OnEffectKeyChanged));

        /// <summary>
        /// 背景画像を取得または設定します。
        /// </summary>
        public string EffectKey
        {
            get { return (string)GetValue(EffectKeyroperty); }
            set { SetValue(EffectKeyroperty, value); }
        }

        /// <summary>
        /// エフェクトのキー名が変わったときに呼ばれます。
        /// </summary>
        static void OnEffectKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as ShogiBackgroundCore;

            if (self != null)
            {
                self.OnEffectKeyChanged((string)e.NewValue);
            }
        }

        /// <summary>
        /// 背景用のエミッター付きエフェクトを初期化します。
        /// </summary>
        private void OnEffectKeyChanged(string effectKey)
        {
            if (Ragnarok.Presentation.WpfUtil.IsInDesignMode)
            {
                return;
            }

            if (string.IsNullOrEmpty(effectKey))
            {
                EffectGroup.Children.Clear();
                this.rootEffect = null;
                return;
            }

            // 背景エフェクトの作成。
            var effectInfo = new EffectInfo(effectKey, null);
            var effect = effectInfo.LoadBackground();

            if (effect != null)
            {
                this.rootEffect = effect;

                EffectGroup.Children.Clear();
                EffectGroup.Children.Add(this.rootEffect.ModelGroup);
            }
        }

        /// <summary>
        /// 描画処理などを行います。
        /// </summary>
        public void Render(TimeSpan elapsedTime)
        {
            if (this.rootEffect != null)
            {
                this.rootEffect.DoEnterFrame(elapsedTime);
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ShogiBackgroundCore()
        {
            InitializeComponent();

            Unloaded += Control_Unloaded;
        }

        /// <summary>
        /// エフェクトオブジェクトは後片付けしないと、リークします。
        /// </summary>
        void Control_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.rootEffect != null)
            {
                this.rootEffect.Terminate();
                this.rootEffect = null;
            }
        }
    }
}
