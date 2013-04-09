using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ragnarok;
using Ragnarok.Shogi;
using Ragnarok.Presentation.Shogi;
using Ragnarok.Presentation.Shogi.View;

namespace VoteSystem.PluginShogi.ViewModel
{
    using Effects;
    using Model;

    /// <summary>
    /// 指し手の自動再生時に使われます。再生用の変化を保存します。
    /// </summary>
    public class AutoPlayEx : AutoPlay
    {
        private IEnumerator<bool> enumerator;

        /// <summary>
        /// エフェクト管理用のオブジェクトを取得または設定します。
        /// </summary>
        public EffectManager EffectManager
        {
            get;
            set;
        }

        /// <summary>
        /// カットインを使用するかどうかを取得または設定します。
        /// </summary>
        public bool IsUseCutIn
        {
            get;
            set;
        }

        /// <summary>
        /// カットインの表示間隔を取得または設定します。
        /// </summary>
        public TimeSpan CutInInternal
        {
            get;
            set;
        }

        /// <summary>
        /// 自動再生前に再生の確認を行うかどうかを取得または設定します。
        /// </summary>
        public bool IsConfirmPlay
        {
            get;
            set;
        }

        /// <summary>
        /// 自動再生前の確認メッセージを取得または設定します。
        /// </summary>
        public string ConfirmMessage
        {
            get;
            set;
        }

        /// <summary>
        /// コルーチン用のオブジェクトを返します。
        /// </summary>
        private IEnumerable<bool> MakeUpdateEnumerator()
        {
            if (EffectManager != null)
            {
                EffectManager.EffectEnabled = false;
                EffectManager.IsAutoPlayEffect = true;
                EffectManager.EffectMoveCount = 0;
            }

            // カットインが表示できたら、指定の時間だけ待ちます。
            if (IsUseCutIn &&
                EffectManager != null &&
                EffectManager.VariationCutIn())
            {
                while (PositionFromBase < CutInInternal)
                {
                    yield return true;
                }

                BasePosition += CutInInternal;
            }

            // コルーチンを進めます。
            while (this.enumerator.MoveNext())
            {
                yield return this.enumerator.Current;
            }

            if (EffectManager != null)
            {
                EffectManager.EffectEnabled = true;
                EffectManager.IsAutoPlayEffect = false;
                EffectManager.EffectMoveCount = 0;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AutoPlayEx(Variation variation)
            : this(variation.Board, variation.BoardMoveList)
        {
            ConfirmMessage = string.Format(
                "{1}{0}{0}コメント: {2}{0}{0}を再生しますか？",
                Environment.NewLine,
                variation.Label,
                variation.Comment);

            CutInInternal = TimeSpan.FromSeconds(2.0);

            UpdateEnumerator = MakeUpdateEnumerator().GetEnumerator();
            this.enumerator = base.GetUpdateEnumerator().GetEnumerator();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AutoPlayEx(Board board, IEnumerable<BoardMove> moveList)
            : base(board, moveList)
        {
            CutInInternal = TimeSpan.FromSeconds(2.0);

            UpdateEnumerator = MakeUpdateEnumerator().GetEnumerator();
            this.enumerator = base.GetUpdateEnumerator().GetEnumerator();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AutoPlayEx(Board board, AutoPlayType autoPlayType)
            : base(board, autoPlayType)
        {
            CutInInternal = TimeSpan.FromSeconds(2.0);

            UpdateEnumerator = MakeUpdateEnumerator().GetEnumerator();
            this.enumerator = base.GetUpdateEnumerator().GetEnumerator();
        }
    }
}
