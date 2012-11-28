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

// コメント不足への警告を無くします。
#pragma warning disable 1591

namespace VoteSystem.PluginShogi
{
    using ViewModel;

    [Serializable()]
    public sealed partial class Settings : Ragnarok.Presentation.WpfSettingsBase
    {
        #region Auto Save
        [DefaultValue("")]
        public string AS_Comment
        {
            get { return GetValue<string>("AS_Comment"); }
            set { SetValue("AS_Comment", value); }
        }
        #endregion

        #region 現局面設定ダイアログ
        public bool CBS_IsClearVoteResult
        {
            get { return GetValue<bool>("CBS_IsClearVoteResult"); }
            set { SetValue("CBS_IsClearVoteResult", value); }
        }

        public bool CBS_IsStartVote
        {
            get { return GetValue<bool>("CBS_IsStartVote"); }
            set { SetValue("CBS_IsStartVote", value); }
        }

        public bool CBS_IsStopVote
        {
            get { return GetValue<bool>("CBS_IsStopVote"); }
            set { SetValue("CBS_IsStopVote", value); }
        }

        public ShogiTimeSpan CBS_AddLimitTime
        {
            get { return GetValue<ShogiTimeSpan>("CBS_AddLimitTime"); }
            set { SetValue("CBS_AddLimitTime", value); }
        }

        public ShogiTimeSpan CBS_VoteSpan
        {
            get { return GetValue<ShogiTimeSpan>("CBS_VoteSpan"); }
            set { SetValue("CBS_VoteSpan", value); }
        }
        #endregion

        #region 設定ダイアログ
        [DefaultValue(false)]
        public bool SD_IsAutoUpdateCurrentBoard
        {
            get { return GetValue<bool>("SD_IsAutoUpdateCurrentBoard"); }
            set { SetValue("SD_IsAutoUpdateCurrentBoard", value); }
        }
        
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

        protected override void OnSettingsLoaded(object sender, SettingsLoadedEventArgs e)
        {
            base.OnSettingsLoaded(sender, e);

            if (CBS_AddLimitTime == null)
            {
                CBS_AddLimitTime = new ShogiTimeSpan
                {
                    Minutes = 3,
                };
            }

            if (CBS_VoteSpan == null)
            {
                CBS_VoteSpan = new ShogiTimeSpan()
                {
                    Minutes = 3,
                };
            }
        }
    }
}
