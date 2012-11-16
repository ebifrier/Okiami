using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Ragnarok;
using Ragnarok.Utility;
using Ragnarok.ObjectModel;

namespace VoteSystem.PluginShogi.ViewModel
{
    using VoteSystem.PluginShogi.Model;

    /// <summary>
    /// 設定ダイアログ用のビューモデルです。
    /// </summary>
    public class ShogiSettingDialogViewModel : NotifyObject
    {
        /// <summary>
        /// 現局面を自動的に更新するかを取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_IsAutoUpdateCurrentBoard")]
        public bool IsAutoUpdateCurrentBoard
        {
            get { return ShogiGlobal.Settings.SD_IsAutoUpdateCurrentBoard; }
            set { ShogiGlobal.Settings.SD_IsAutoUpdateCurrentBoard = value; }
        }

        /// <summary>
        /// 選択されている駒画像を取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_KomaImage")]
        public KomaImageType KomaImage
        {
            get { return ShogiGlobal.Settings.SD_KomaImage; }
            set { ShogiGlobal.Settings.SD_KomaImage = value; }
        }

        /// <summary>
        /// 選択されている盤画像を取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_BanImage")]
        public BanImageType BanImage
        {
            get { return ShogiGlobal.Settings.SD_BanImage; }
            set { ShogiGlobal.Settings.SD_BanImage = value; }
        }

        /// <summary>
        /// 選択されている駒台画像を取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_KomadaiImage")]
        public KomadaiImageType KomadaiImage
        {
            get { return ShogiGlobal.Settings.SD_KomadaiImage; }
            set { ShogiGlobal.Settings.SD_KomadaiImage = value; }
        }

        /// <summary>
        /// 背景画像のパスを取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_BackgroundPath")]
        public string BackgroundPath
        {
            get { return ShogiGlobal.Settings.SD_BackgroundPath; }
            set { ShogiGlobal.Settings.SD_BackgroundPath = value; }
        }

        /// <summary>
        /// 盤の不透明度を取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_BanOpacity")]
        public double BanOpacity
        {
            get { return ShogiGlobal.Settings.SD_BanOpacity; }
            set { ShogiGlobal.Settings.SD_BanOpacity = value; }
        }

        /// <summary>
        /// エフェクトを使用するかどうかを取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_IsUseEffect")]
        public bool IsUseEffect
        {
            get { return ShogiGlobal.Settings.SD_IsUseEffect; }
            set { ShogiGlobal.Settings.SD_IsUseEffect = value; }
        }

        /// <summary>
        /// エフェクト効果音を使用するかどうかを取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_IsUseEffectSound")]
        public bool IsUseEffectSound
        {
            get { return ShogiGlobal.Settings.SD_IsUseEffectSound; }
            set { ShogiGlobal.Settings.SD_IsUseEffectSound = value; }
        }

        /// <summary>
        /// エフェクトの音量を取得または設定します。(0～100)
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_EffectVolume")]
        public int EffectVolume
        {
            get { return ShogiGlobal.Settings.SD_EffectVolume; }
            set { ShogiGlobal.Settings.SD_EffectVolume = MathEx.Between(0, 100, value); }
        }

        /// <summary>
        /// エフェクトの個別の使用フラグを取得します。
        /// </summary>
        private bool HasEffectFlag(EffectFlag flag)
        {
            return ((ShogiGlobal.Settings.SD_EffectFlag & flag) != 0);
        }

        /// <summary>
        /// エフェクトの個別の使用フラグを設定します。
        /// </summary>
        private void SetEffectFlag(EffectFlag flag, bool isSet)
        {
            ShogiGlobal.Settings.SD_EffectFlag =
                (isSet ?
                 (ShogiGlobal.Settings.SD_EffectFlag | flag) :
                 (ShogiGlobal.Settings.SD_EffectFlag & ~flag));
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

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ShogiSettingDialogViewModel()
        {
            this.AddDependModel(ShogiGlobal.Settings);
        }
    }
}
