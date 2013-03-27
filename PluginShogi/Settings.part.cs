using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

using Ragnarok;
using Ragnarok.ObjectModel;
using Ragnarok.Shogi;
using Ragnarok.Utility;

// コメント不足への警告を無くします。
#pragma warning disable 1591

namespace VoteSystem.PluginShogi
{
    using Effects;
    using Protocol;

    [Serializable()]
    public sealed partial class Settings : Ragnarok.Presentation.WPFSettingsBase
    {
        #region Auto Save
        [DefaultValue("")]
        public string AS_Comment
        {
            get { return GetValue<string>("AS_Comment"); }
            set { SetValue("AS_Comment", value); }
        }

        [DefaultValue(true)]
        public bool AS_IsShowVoteResult
        {
            get { return GetValue<bool>("AS_IsShowVoteResult"); }
            set { SetValue("AS_IsShowVoteResult", value); }
        }
        #endregion

        #region 現局面設定ダイアログ
        [DefaultValue(true)]
        public bool CBS_IsClearVoteResult
        {
            get { return GetValue<bool>("CBS_IsClearVoteResult"); }
            set { SetValue("CBS_IsClearVoteResult", value); }
        }

        [DefaultValue(true)]
        public bool CBS_IsStartVote
        {
            get { return GetValue<bool>("CBS_IsStartVote"); }
            set { SetValue("CBS_IsStartVote", value); }
        }

        [DefaultValue(true)]
        public bool CBS_IsStopVote
        {
            get { return GetValue<bool>("CBS_IsStopVote"); }
            set { SetValue("CBS_IsStopVote", value); }
        }

        public SimpleTimeSpan CBS_AddLimitTime
        {
            get { return GetValue<SimpleTimeSpan>("CBS_AddLimitTime"); }
            set { SetValue("CBS_AddLimitTime", value); }
        }

        public SimpleTimeSpan CBS_VoteSpan
        {
            get { return GetValue<SimpleTimeSpan>("CBS_VoteSpan"); }
            set { SetValue("CBS_VoteSpan", value); }
        }
        #endregion

        #region 設定ダイアログ
        #region 一般
        [DefaultValue("てんて～")]
        public string SD_BlackPlayerName
        {
            get { return GetValue<string>("SD_BlackPlayerName"); }
            set { SetValue("SD_BlackPlayerName", value); }
        }

        [DefaultValue("ひふみん")]
        public string SD_WhitePlayerName
        {
            get { return GetValue<string>("SD_WhitePlayerName"); }
            set { SetValue("SD_WhitePlayerName", value); }
        }

        [DefaultValue(RenderingQuality.Normal)]
        public RenderingQuality SD_RenderingQuality
        {
            get { return GetValue<RenderingQuality>("SD_RenderingQuality"); }
            set { SetValue("SD_RenderingQuality", value); }
        }

        [DefaultValue(BWType.None)]
        public BWType SD_Teban
        {
            get { return GetValue<BWType>("SD_Teban"); }
            set { SetValue("SD_Teban", value); }
        }

        [DefaultValue(false)]
        public bool SD_IsAutoUpdateCurrentBoard
        {
            get { return GetValue<bool>("SD_IsAutoUpdateCurrentBoard"); }
            set { SetValue("SD_IsAutoUpdateCurrentBoard", value); }
        }
        #endregion

        #region 盤駒画像
        public KomaImageType SD_KomaImage
        {
            get { return GetValue<KomaImageType>("SD_KomaImage"); }
            set { SetValue("SD_KomaImage", value); }
        }

        public BanImageType SD_BanImage
        {
            get { return GetValue<BanImageType>("SD_BanImage"); }
            set { SetValue("SD_BanImage", value); }
        }

        public KomadaiImageType SD_KomadaiImage
        {
            get { return GetValue<KomadaiImageType>("SD_KomadaiImage"); }
            set { SetValue("SD_KomadaiImage", value); }
        }

        public string SD_BackgroundPath
        {
            get { return GetValue<string>("SD_BackgroundPath"); }
            set { SetValue("SD_BackgroundPath", value); }
        }

        [DefaultValue(0.6)]
        public double SD_BanOpacity
        {
            get { return GetValue<double>("SD_BanOpacity"); }
            set { SetValue("SD_BanOpacity", value); }
        }
        #endregion

        #region エフェクト
        [DefaultValue(true)]
        public bool SD_IsUseEffect
        {
            get { return GetValue<bool>("SD_IsUseEffect"); }
            set { SetValue("SD_IsUseEffect", value); }
        }

        [DefaultValue(EffectFlag.All)]
        public EffectFlag SD_EffectFlag
        {
            get { return GetValue<EffectFlag>("SD_EffectFlag"); }
            set { SetValue("SD_EffectFlag", value); }
        }
        #endregion

        #region サウンド
        [DefaultValue(true)]
        public bool SD_IsUseEffectSound
        {
            get { return GetValue<bool>("SD_IsUseEffectSound"); }
            set { SetValue("SD_IsUseEffectSound", value); }
        }

        [DefaultValue(100)]
        public int SD_EffectVolume
        {
            get { return GetValue<int>("SD_EffectVolume"); }
            set { SetValue("SD_EffectVolume", value); }
        }
        #endregion
        #endregion

        #region VoteResultWindow
        /// <summary>
        /// ウィンドウ色を取得または設定します。
        /// </summary>
        [DefaultValue("#B4FFFFFF")]
        public Color VR_BackgroundColor
        {
            get { return (Color)this["VR_BackgroundColor"]; }
            set { this["VR_BackgroundColor"] = value; }
        }

        /// <summary>
        /// 表示する投票結果の数を取得または設定します。
        /// </summary>
        [DefaultValue(5)]
        public int VR_DisplayResultCount
        {
            get { return (int)this["VR_DisplayResultCount"]; }
            set { this["VR_DisplayResultCount"] = value; }
        }

        /// <summary>
        /// 表示する数字が半角か全角かを取得または設定します。
        /// </summary>
        [DefaultValue(true)]
        public bool VR_IsDisplayPointFullWidth
        {
            get { return (bool)this["VR_IsDisplayPointFullWidth"]; }
            set { this["VR_IsDisplayPointFullWidth"] = value; }
        }

        /// <summary>
        /// フォントファミリ名を取得または設定します。
        /// </summary>
        [DefaultValue("ＭＳ ゴシック")]
        public string VR_FontFamilyName
        {
            get { return (string)this["VR_FontFamilyName"]; }
            set
            {
                // WPFで初期化時にnullが設定されるため必要。
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

                this["VR_FontFamilyName"] = value;
            }
        }

        /// <summary>
        /// フォントの太さを取得または設定します。
        /// </summary>
        public FontWeight VR_FontWeight
        {
            get { return (FontWeight)this["VR_FontWeight"]; }
            set { this["VR_FontWeight"] = value; }
        }

        /// <summary>
        /// フォントスタイルを取得または設定します。
        /// </summary>
        public FontStyle VR_FontStyle
        {
            get { return (FontStyle)this["VR_FontStyle"]; }
            set { this["VR_FontStyle"] = value; }
        }

        /// <summary>
        /// フォントの色を取得または設定します。
        /// </summary>
        [DefaultValue("#FF000000")]
        public Color VR_FontColor
        {
            get { return (Color)this["VR_FontColor"]; }
            set { this["VR_FontColor"] = value; }
        }

        /// <summary>
        /// 縁取りを行うかどうかを取得または設定します。
        /// </summary>
        [DefaultValue(true)]
        public bool VR_IsShowStroke
        {
            get { return (bool)this["VR_IsShowStroke"]; }
            set { this["VR_IsShowStroke"] = value; }
        }

        /// <summary>
        /// フォントの縁色を取得または設定します。
        /// </summary>
        [DefaultValue("#FF00FFFF")]
        public Color VR_StrokeColor
        {
            get { return (Color)this["VR_StrokeColor"]; }
            set { this["VR_StrokeColor"] = value; }
        }

        /// <summary>
        /// フォントの縁の太さを取得または設定します。
        /// </summary>
        [DefaultValue("0.5")]
        public decimal VR_StrokeThicknessInternal
        {
            get { return (decimal)this["VR_StrokeThicknessInternal"]; }
            set { this["VR_StrokeThicknessInternal"] = value; }
        }
        #endregion

        protected override void OnSettingsLoaded(object sender, SettingsLoadedEventArgs e)
        {
            base.OnSettingsLoaded(sender, e);

            if (CBS_AddLimitTime == null)
            {
                CBS_AddLimitTime = new SimpleTimeSpan
                {
                    Minutes = 3,
                };
            }

            if (CBS_VoteSpan == null)
            {
                CBS_VoteSpan = new SimpleTimeSpan
                {
                    Minutes = 3,
                };
            }
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
        }
    }
}
