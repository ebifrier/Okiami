﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

using Ragnarok;

// コメント不足への警告を無くします。
#pragma warning disable 1591

namespace VoteSystem.Client
{
    [Serializable()]
    public sealed partial class Settings : Ragnarok.Presentation.WpfSettingsBase
    {
        #region TimeSpan
        [DefaultSettingValueAttribute("00:05:00")]
        public TimeSpan SetVoteTimeSpan
        {
            get { return (TimeSpan)this["SetVoteTimeSpan"]; }
            set { this["SetVoteTimeSpan"] = value; }
        }

        [DefaultSettingValueAttribute("00:01:00")]
        public TimeSpan AddVoteTimeSpan
        {
            get { return (TimeSpan)this["AddVoteTimeSpan"]; }
            set { this["AddVoteTimeSpan"] = value; }
        }

        [DefaultSettingValueAttribute("00:05:00")]
        public TimeSpan DefaultVoteTimeSpan
        {
            get { return (TimeSpan)this["DefaultVoteTimeSpan"]; }
            set { this["DefaultVoteTimeSpan"] = value; }
        }

        [DefaultSettingValueAttribute("00:03:00")]
        public TimeSpan AddTotalVoteTimeSpan
        {
            get { return (TimeSpan)this["AddTotalVoteTimeSpan"]; }
            set { this["AddTotalVoteTimeSpan"] = value; }
        }

        [DefaultSettingValueAttribute("100")]
        public double TimeSpanWindow_Left
        {
            get { return (double)this["TimeSpanWindow_Left"]; }
            set { this["TimeSpanWindow_Left"] = value; }
        }

        [DefaultSettingValueAttribute("100")]
        public double TimeSpanWindow_Top
        {
            get { return (double)this["TimeSpanWindow_Top"]; }
            set { this["TimeSpanWindow_Top"] = value; }
        }
        #endregion

        #region Setting Dialog
        [DefaultSettingValueAttribute("True")]
        public bool IsUseSE
        {
            get { return (bool)this["IsUseSE"]; }
            set { this["IsUseSE"] = value; }
        }

        [DefaultSettingValueAttribute("50")]
        public int SEVolume
        {
            get { return ((int)(this["SEVolume"])); }
            set { this["SEVolume"] = value; }
        }

        [DefaultSettingValueAttribute("")]
        public string SoundSetDir
        {
            get { return (string)this["SoundSetDir"]; }
            set { this["SoundSetDir"] = value; }
        }

        public Model.SystemMessage VoteStartSystemMessage
        {
            get { return (Model.SystemMessage)this["VoteStartSystemMessage"]; }
            set { this["VoteStartSystemMessage"] = value; }
        }

        public Model.SystemMessage VoteEndSystemMessage
        {
            get { return (Model.SystemMessage)this["VoteEndSystemMessage"]; }
            set { this["VoteEndSystemMessage"] = value; }
        }

        public Model.SystemMessage VotePauseSystemMessage
        {
            get { return (Model.SystemMessage)this["VotePauseSystemMessage"]; }
            set { this["VotePauseSystemMessage"] = value; }
        }

        public Model.SystemMessage VoteStopSystemMessage
        {
            get { return (Model.SystemMessage)this["VoteStopSystemMessage"]; }
            set { this["VoteStopSystemMessage"] = value; }
        }

        public Model.SystemMessage ChangeVoteSpanSystemMessage
        {
            get { return (Model.SystemMessage)this["ChangeVoteSpanSystemMessage"]; }
            set { this["ChangeVoteSpanSystemMessage"] = value; }
        }
        #endregion

        #region Auto Save
        [DefaultSettingValueAttribute("00000000-0000-0000-0000-000000000000")]
        public Guid AS_UserId
        {
            get { return (Guid)this["AS_UserId"]; }
            set { this["AS_UserId"] = value; }
        }

        [DefaultSettingValueAttribute("将棋投票所")]
        public string AS_VoteRoomName
        {
            get { return (string)this["AS_VoteRoomName"]; }
            set { this["AS_VoteRoomName"] = value; }
        }

        [DefaultSettingValueAttribute("")]
        public string AS_VoteRoomPassword
        {
            get { return (string)this["AS_VoteRoomPassword"]; }
            set { this["AS_VoteRoomPassword"] = value; }
        }

        [DefaultSettingValueAttribute("名無し名人")]
        public string AS_LoginName
        {
            get { return (string)this["AS_LoginName"]; }
            set { this["AS_LoginName"] = value; }
        }

        [DefaultSettingValueAttribute("pack://application:,,,/Resources/Image/koma/koma_noimage.png")]
        public string AS_LoginImageUrl
        {
            get { return (string)this["AS_LoginImageUrl"]; }
            set { this["AS_LoginImageUrl"] = value; }
        }

        [DefaultSettingValueAttribute("True")]
        public bool AS_IsUseAsNicoCommenter
        {
            get { return (bool)this["AS_IsUseAsNicoCommenter"]; }
            set { this["AS_IsUseAsNicoCommenter"] = value; }
        }

        [DefaultSettingValueAttribute("False")]
        public bool AS_IsNicoCommenterAutoStart
        {
            get { return (bool)this["AS_IsNicoCommenterAutoStart"]; }
            set { this["AS_IsNicoCommenterAutoStart"] = value; }
        }

        public Ragnarok.NicoNico.Login.LoginData AS_OwnerNicoLoginData
        {
            get { return (Ragnarok.NicoNico.Login.LoginData)this["AS_OwnerNicoLoginData"]; }
            set { this["AS_OwnerNicoLoginData"] = value; }
        }

        [DefaultSettingValueAttribute("")]
        public string AS_ImageSetDirName
        {
            get { return (string)this["AS_ImageSetDirName"]; }
            set { this["AS_ImageSetDirName"] = value; }
        }
        #endregion

        #region VoteResultWindow
        /// <summary>
        /// 固定時のウィンドウ色を取得または設定します。
        /// </summary>
        [DefaultSettingValueAttribute("#B4FFFFFF")]
        public Color VR_FixingBackgroundColor
        {
            get { return (Color)this["VR_FixingBackgroundColor"]; }
            set { this["VR_FixingBackgroundColor"] = value; }
        }

        /// <summary>
        /// 表示する投票結果の数を取得または設定します。
        /// </summary>
        [DefaultSettingValueAttribute("10")]
        public int VR_DisplayResultCount
        {
            get { return (int)this["VR_DisplayResultCount"]; }
            set { this["VR_DisplayResultCount"] = value; }
        }

        /// <summary>
        /// 表示する数字が半角か全角かを取得または設定します。
        /// </summary>
        [DefaultSettingValueAttribute("true")]
        public bool VR_IsDisplayPointFullWidth
        {
            get { return (bool)this["VR_IsDisplayPointFullWidth"]; }
            set { this["VR_IsDisplayPointFullWidth"] = value; }
        }

        /// <summary>
        /// フォントファミリ名を取得または設定します。
        /// </summary>
        [DefaultSettingValueAttribute("ＭＳ ゴシック")]
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
        [DefaultSettingValueAttribute("#FF000000")]
        public Color VR_FontColor
        {
            get { return (Color)this["VR_FontColor"]; }
            set { this["VR_FontColor"] = value; }
        }

        /// <summary>
        /// 縁取りを行うかどうかを取得または設定します。
        /// </summary>
        [DefaultSettingValueAttribute("True")]
        public bool VR_IsEdged
        {
            get { return (bool)this["VR_IsEdged"]; }
            set { this["VR_IsEdged"] = value; }
        }

        /// <summary>
        /// フォントの縁の太さを取得または設定します。
        /// </summary>
        [DefaultSettingValueAttribute("0.5")]
        public decimal VR_FontEdgeLengthInternal
        {
            get { return (decimal)this["VR_FontEdgeLengthInternal"]; }
            set { this["VR_FontEdgeLengthInternal"] = value; }
        }

        /// <summary>
        /// フォントの縁色を取得または設定します。
        /// </summary>
        [DefaultSettingValueAttribute("#FF00FFFF")]
        public Color VR_FontEdgeColor
        {
            get { return (Color)this["VR_FontEdgeColor"]; }
            set { this["VR_FontEdgeColor"] = value; }
        }
        #endregion
    }
}