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
        /// <summary>
        /// エフェクト管理用のオブジェクトを取得または設定します。
        /// </summary>
        public EffectManager EffectManager
        {
            get;
            set;
        }

        /// <summary>
        /// 指し手に合わせて背景画像を変えるかどうかを取得または設定します。
        /// </summary>
        public bool IsChangeMoveCount
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
        public TimeSpan CutInInterval
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
                EffectManager.IsAutoPlayEffect = true;
                EffectManager.EffectMoveCount = 0;
            }

            foreach (var result in WaitExecutor(BeginningInterval))
            {
                yield return result;
            }

            // 最初に背景色のみを更新します。
            foreach (var result in BackgroundFadeInExecutor())
            {
                yield return result;
            }

            // カットインが表示できたら、指定の時間だけ待ちます。
            if (IsUseCutIn &&
                EffectManager != null &&
                EffectManager.VariationCutIn())
            {
                while (PositionFromBase < CutInInterval)
                {
                    yield return true;
                }

                BasePosition += CutInInterval;
            }

            // 指し手を進めます。
            foreach (var result in DoMoveExecutor())
            {
                if (IsChangeMoveCount && EffectManager != null)
                {
                    EffectManager.ChangeMoveCount(Board.MoveCount);
                }

                yield return result;
            }

            // 背景色をもとに戻します。
            foreach (var result in BackgroundFadeOutExecutor())
            {
                yield return result;
            }

            foreach (var result in WaitExecutor(EndingInterval))
            {
                yield return result;
            }

            if (EffectManager != null)
            {
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
            var sb = new StringBuilder();
            sb.AppendFormat("{1}{0}{0}",
                Environment.NewLine,
                variation.Label);
            if (!string.IsNullOrEmpty(variation.Comment))
            {
                sb.AppendFormat("コメント: {1}{0}{0}",
                    Environment.NewLine,
                    variation.Comment);
            }
            sb.AppendFormat("を再生しますか？");

            ConfirmMessage = sb.ToString();
            CutInInterval = TimeSpan.FromSeconds(2.0);
            UpdateEnumerator = MakeUpdateEnumerator().GetEnumerator();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AutoPlayEx(Board board, IEnumerable<BoardMove> moveList)
            : base(board, moveList)
        {
            CutInInterval = TimeSpan.FromSeconds(2.0);
            UpdateEnumerator = MakeUpdateEnumerator().GetEnumerator();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AutoPlayEx(Board board, AutoPlayType autoPlayType)
            : base(board, autoPlayType)
        {
            CutInInterval = TimeSpan.FromSeconds(2.0);

            UpdateEnumerator = MakeUpdateEnumerator().GetEnumerator();
        }
    }
}
