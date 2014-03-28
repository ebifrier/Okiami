using System;
using System.ComponentModel;
using System.Reflection;

using Ragnarok;
using Ragnarok.Utility;

namespace VoteSystem.PluginShogi
{
    /// <summary>
    /// 設定を保存するオブジェクトです。
    /// </summary>
    public sealed partial class Settings
    {
        /// <summary>
        /// エフェクトの個別の使用フラグを取得します。
        /// </summary>
        public bool HasEffectFlag(Effects.EffectFlag flag)
        {
            if (!SD_IsUseEffect)
            {
                return false;
            }

            return ((SD_EffectFlag & flag) != 0);
        }

        protected override void OnSettingsLoaded(object sender, SettingsLoadedEventArgs e)
        {
            base.OnSettingsLoaded(sender, e);
        }

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(sender, e);

            if (e.PropertyName == "SD_RenderingQuality")
            {
                // ここでFPSを変更します。
                var timer = ShogiGlobal.FrameTimer;
                if (timer != null)
                {
                    timer.TargetFPS = RenderingQualityUtil.GetFPS(
                        SD_RenderingQuality);
                }
            }

            if (e.PropertyName == "SD_AutoPlayIntervalMS")
            {
                this.NotifyPropertyChanged(
                    new PropertyChangedEventArgs("SD_AutoPlayInterval"));
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Settings()
        {
            var asm = Assembly.GetExecutingAssembly();
            SetLocationFromAssembly(asm);
        }
    }
}
