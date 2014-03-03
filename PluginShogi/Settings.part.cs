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
using Ragnarok.Utility;
using Ragnarok.Shogi;
using Ragnarok.Presentation.Shogi;

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

        [DefaultValue(true)]
        public bool AS_IsShowEvaluationValue
        {
            get { return GetValue<bool>("AS_IsShowEvaluationValue"); }
            set { SetValue("AS_IsShowEvaluationValue", value); }
        }

        [DefaultValue("伊吹萃香")]
        public string AS_EvaluationImageSetTitle
        {
            get { return GetValue<string>("AS_EvaluationImageSetTitle"); }
            set { SetValue("AS_EvaluationImageSetTitle", value); }
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
        public bool CBS_IsVoteStop
        {
            get { return GetValue<bool>("CBS_IsVoteStop"); }
            set { SetValue("CBS_IsVoteStop", value); }
        }

        public bool CBS_IsUseAddLimitTime
        {
            get { return GetValue<bool>("CBS_IsUseAddLimitTime"); }
            set { SetValue("CBS_IsUseAddLimitTime", value); }
        }

        [DefaultValue("00:02:00")]
        public TimeSpan CBS_AddLimitTime
        {
            get { return GetValue<TimeSpan>("CBS_AddLimitTime"); }
            set { SetValue("CBS_AddLimitTime", value); }
        }

        public bool CBS_IsUseVoteSpan
        {
            get { return GetValue<bool>("CBS_IsUseVoteSpan"); }
            set { SetValue("CBS_IsUseVoteSpan", value); }
        }

        [DefaultValue("00:05:00")]
        public TimeSpan CBS_VoteSpan
        {
            get { return GetValue<TimeSpan>("CBS_VoteSpan"); }
            set { SetValue("CBS_VoteSpan", value); }
        }
        #endregion

        #region 設定ダイアログ
        #region 一般
        [DefaultValue("リスナー")]
        public string SD_BlackPlayerName
        {
            get { return GetValue<string>("SD_BlackPlayerName"); }
            set { SetValue("SD_BlackPlayerName", value); }
        }

        [DefaultValue("ボナンザ")]
        public string SD_WhitePlayerName
        {
            get { return GetValue<string>("SD_WhitePlayerName"); }
            set { SetValue("SD_WhitePlayerName", value); }
        }

        [DefaultValue(BWType.None)]
        public BWType SD_Teban
        {
            get { return GetValue<BWType>("SD_Teban"); }
            set { SetValue("SD_Teban", value); }
        }

        [DefaultValue(RenderingQuality.Normal)]
        public RenderingQuality SD_RenderingQuality
        {
            get { return GetValue<RenderingQuality>("SD_RenderingQuality"); }
            set { SetValue("SD_RenderingQuality", value); }
        }

        [DefaultValue(false)]
        public bool SD_IsAutoUpdateCurrentBoard
        {
            get { return GetValue<bool>("SD_IsAutoUpdateCurrentBoard"); }
            set { SetValue("SD_IsAutoUpdateCurrentBoard", value); }
        }

        [DefaultValue(true)]
        public bool SD_IsUseLiveNotConnectWarning
        {
            get { return GetValue<bool>("SD_IsUseLiveNotConnectWarning"); }
            set { SetValue("SD_IsUseLiveNotConnectWarning", value); }
        }

        [DefaultValue(true)]
        public bool SD_IsUseVariation
        {
            get { return GetValue<bool>("SD_IsUseVariation"); }
            set { SetValue("SD_IsUseVariation", value); }
        }

        [DefaultValue(true)]
        public bool SD_IsPostCurrentBoardComment
        {
            get { return GetValue<bool>("SD_IsPostCurrentBoardComment"); }
            set { SetValue("SD_IsPostCurrentBoardComment", value); }
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
        [DefaultValue("#5D000000")]
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
        [DefaultValue("#FFFFFFFF")]
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
        [DefaultValue("#FF00FF7E")]
        public Color VR_StrokeColor
        {
            get { return (Color)this["VR_StrokeColor"]; }
            set { this["VR_StrokeColor"] = value; }
        }

        /// <summary>
        /// フォントの縁の太さを取得または設定します。
        /// </summary>
        [DefaultValue("0.30")]
        public decimal VR_StrokeThicknessInternal
        {
            get { return (decimal)this["VR_StrokeThicknessInternal"]; }
            set { this["VR_StrokeThicknessInternal"] = value; }
        }
        #endregion
    }
}
