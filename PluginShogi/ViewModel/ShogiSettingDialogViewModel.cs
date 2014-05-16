using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Ragnarok;
using Ragnarok.ObjectModel;
using Ragnarok.Utility;
using Ragnarok.Shogi;
using Ragnarok.Presentation.Shogi;

namespace VoteSystem.PluginShogi.ViewModel
{
    using Effects;
    using Model;

    /// <summary>
    /// 設定ダイアログ用のビューモデルです。
    /// </summary>
    public sealed class ShogiSettingDialogViewModel : NotifyObject
    {
        /// <summary>
        /// 設定用オブジェクトを取得します。
        /// </summary>
        public Settings Settings
        {
            get { return ShogiGlobal.Settings; }
        }

        /// <summary>
        /// ダイアログのタブインデックスを取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_SelectedTabIndex")]
        public int SelectedTabIndex
        {
            get { return Settings.SD_SelectedTabIndex; }
            set { Settings.SD_SelectedTabIndex = value; }
        }

        #region エフェクト
        /// <summary>
        /// エフェクトの個別の使用フラグを取得します。
        /// </summary>
        private bool HasEffectFlag(EffectFlag flag)
        {
            return ((Settings.SD_EffectFlag & flag) != 0);
        }

        /// <summary>
        /// エフェクトの個別の使用フラグを設定します。
        /// </summary>
        private void SetEffectFlag(EffectFlag flag, bool isSet)
        {
            Settings.SD_EffectFlag =
                (isSet ?
                 (Settings.SD_EffectFlag | flag) :
                 (Settings.SD_EffectFlag & ~flag));
        }

        /// <summary>
        /// 一手前に動かした駒を強調表示するかどうかを取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_EffectFlag")]
        public bool IsUsePrevCellEffect
        {
            get { return HasEffectFlag(EffectFlag.PrevCell); }
            set { SetEffectFlag(EffectFlag.PrevCell, value); }
        }

        /// <summary>
        /// 動かせるマスを強調表示するかどうかを取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_EffectFlag")]
        public bool IsUseMovableCellEffect
        {
            get { return HasEffectFlag(EffectFlag.MovableCell); }
            set { SetEffectFlag(EffectFlag.MovableCell, value); }
        }

        /// <summary>
        /// 手番側を強調表示するかどうかを取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_EffectFlag")]
        public bool IsUseTebanEffect
        {
            get { return HasEffectFlag(EffectFlag.Teban); }
            set { SetEffectFlag(EffectFlag.Teban, value); }
        }

        /// <summary>
        /// 背景エフェクトの使用フラグを取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_EffectFlag")]
        public bool IsUseBackgroundEffect
        {
            get { return HasEffectFlag(EffectFlag.Background); }
            set { SetEffectFlag(EffectFlag.Background, value); }
        }

        /// <summary>
        /// シンプルな背景エフェクトの使用フラグを取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_EffectFlag")]
        public bool IsUseSimpleBackgroundEffect
        {
            get { return HasEffectFlag(EffectFlag.SimpleBackground); }
            set { SetEffectFlag(EffectFlag.SimpleBackground, value); }
        }

        /// <summary>
        /// 駒に関するエフェクトの使用フラグを取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_EffectFlag")]
        public bool IsUsePieceEffect
        {
            get { return HasEffectFlag(EffectFlag.Piece); }
            set { SetEffectFlag(EffectFlag.Piece, value); }
        }

        /// <summary>
        /// 囲いエフェクトの使用フラグを取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_EffectFlag")]
        public bool IsUseCastleEffect
        {
            get { return HasEffectFlag(EffectFlag.Castle); }
            set { SetEffectFlag(EffectFlag.Castle, value); }
        }

        /// <summary>
        /// 投票エフェクトの使用フラグを取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_EffectFlag")]
        public bool IsUseVoteEffect
        {
            get { return HasEffectFlag(EffectFlag.Vote); }
            set { SetEffectFlag(EffectFlag.Vote, value); }
        }

        /// <summary>
        /// 自動再生のエフェクトの使用フラグを取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_EffectFlag")]
        public bool IsUseAutoPlayEffect
        {
            get { return HasEffectFlag(EffectFlag.AutoPlay); }
            set { SetEffectFlag(EffectFlag.AutoPlay, value); }
        }

        /// <summary>
        /// 自動再生時のカットイン画像の使用フラグを取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_EffectFlag")]
        public bool IsUseAutoPlayCutIn
        {
            get { return HasEffectFlag(EffectFlag.AutoPlayCutIn); }
            set { SetEffectFlag(EffectFlag.AutoPlayCutIn, value); }
        }
        #endregion

        #region サウンド
        /// <summary>
        /// エフェクトの音量を取得または設定します。(0～100)
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_EffectVolume")]
        public int EffectVolume
        {
            get { return ShogiGlobal.Settings.SD_EffectVolume; }
            set { ShogiGlobal.Settings.SD_EffectVolume = MathEx.Between(0, 100, value); }
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ShogiSettingDialogViewModel()
        {
            this.AddDependModel(ShogiGlobal.Settings);
        }
    }
}
