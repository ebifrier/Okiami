using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Ragnarok.Shogi.ViewModel;
using Ragnarok.Shogi.ViewModel.Behaviors;
using Ragnarok.Extra.Sound;

namespace VoteSystem.PluginShogi.ViewModel
{
    public class PlaySoundBehavior : Behavior
    {
        /// <summary>
        /// 音声ファイルのパスを扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register(
                "Path", typeof(string),
                typeof(ScenarioBehavior),
                new UIPropertyMetadata(null));

        /// <summary>
        /// 音声ファイルのパスを取得または設定します。
        /// </summary>
        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        /// <summary>
        /// 音量を扱う依存プロパティです。
        /// </summary>
        public static readonly DependencyProperty VolumeProperty =
            DependencyProperty.Register(
                "Volume", typeof(double),
                typeof(ScenarioBehavior),
                new UIPropertyMetadata(1.0));

        /// <summary>
        /// 音量を取得または設定します。
        /// </summary>
        public double Volume
        {
            get { return (double)GetValue(VolumeProperty); }
            set { SetValue(VolumeProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
        }
    }
}
