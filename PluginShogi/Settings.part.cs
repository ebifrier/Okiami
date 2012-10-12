﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

using Ragnarok;
using Ragnarok.ObjectModel;

// コメント不足への警告を無くします。
#pragma warning disable 1591

namespace VoteSystem.PluginShogi
{
    using ViewModel;

    [Serializable()]
    public sealed partial class Settings : Ragnarok.Presentation.WpfSettingsBase
    {
        #region Auto Save
        [DefaultSettingValueAttribute("")]
        public string AS_Comment
        {
            get { return GetValue<string>("AS_Comment"); }
            set { SetValue("AS_Comment", value); }
        }
        #endregion

        public bool IsClearVoteResult
        {
            get { return GetValue<bool>("IsClearVoteResult"); }
            set { SetValue("IsClearVoteResult", value); }
        }

        #region 設定ダイアログ
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

        [DefaultSettingValueAttribute("0.6")]
        public double SD_BanOpacity
        {
            get { return GetValue<double>("SD_BanOpacity"); }
            set { SetValue("SD_BanOpacity", value); }
        }

        [DefaultSettingValueAttribute("true")]
        public bool SD_IsUseEffect
        {
            get { return GetValue<bool>("SD_IsUseEffect"); }
            set { SetValue("SD_IsUseEffect", value); }
        }

        [DefaultSettingValueAttribute("All")]
        public EffectFlag SD_EffectFlag
        {
            get { return GetValue<EffectFlag>("SD_EffectFlag"); }
            set { SetValue("SD_EffectFlag", value); }
        }

        [DefaultSettingValueAttribute("true")]
        public bool SD_IsUseEffectSound
        {
            get { return GetValue<bool>("SD_IsUseEffectSound"); }
            set { SetValue("SD_IsUseEffectSound", value); }
        }

        [DefaultSettingValueAttribute("100")]
        public int SD_EffectVolume
        {
            get { return GetValue<int>("SD_EffectVolume"); }
            set { SetValue("SD_EffectVolume", value); }
        }
        #endregion
    }
}
