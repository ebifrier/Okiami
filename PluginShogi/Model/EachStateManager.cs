using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ragnarok;
using Ragnarok.Shogi;
using Ragnarok.ObjectModel;

namespace VoteSystem.PluginShogi.Model
{
    /// <summary>
    /// 各局面における変化を管理します。
    /// </summary>
    public class EachStateManager
    {
        private readonly Board board;
        private readonly NotifyCollection<Variation> variationList =
            new NotifyCollection<Variation>();

        /// <summary>
        /// ラベルを取得します。
        /// </summary>
        public string Label
        {
            get
            {
                var lastMove = this.board.LastMove;

                if (lastMove == null)
                {
                    return "開始時";
                }
                else
                {
                    var tmpBoard = this.board.Clone();

                    // lastMoveを形式変換します。
                    tmpBoard.Undo();
                    var move = tmpBoard.ConvertMove(lastMove, false);

                    return string.Format(
                        "{0}手目 {1}",
                        this.board.MoveCount,
                        (move == null ? "???" : move.ToString()));
                }
            }
        }

        /// <summary>
        /// 対象となる局面を取得します。
        /// </summary>
        public Board Board
        {
            get
            {
                return this.board;
            }
        }

        /// <summary>
        /// 現局面からの変化リストを取得します。
        /// </summary>
        public NotifyCollection<Variation> VariationList
        {
            get
            {
                return this.variationList;
            }
        }

        /// <summary>
        /// 指し手が一致する変化を探します。
        /// </summary>
        public Variation SearchVariation(Variation variation)
        {
            var boardMoveList = variation.BoardMoveList;

            return this.variationList.FirstOrDefault(
                v => v.IsMatchShort(boardMoveList));
        }

        /// <summary>
        /// 現局面から可能な変化なら、その変化を追加します。
        /// </summary>
        public Variation TryAddVariation(Variation variation)
        {
            if (variation == null)
            {
                return null;
            }

            var already = SearchVariation(variation);
            if (already != null)
            {
                // 既にある変化の方が長ければ、そちらを採用します。
                if (already.BoardMoveList.Count() >=
                    variation.BoardMoveList.Count())
                {
                    return null;
                }

                this.variationList.Remove(already);
            }

            this.variationList.Add(variation);
            return variation;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EachStateManager(Board board)
        {
            this.board = board.Clone();

            this.board.ClearRedoList();
        }
    }
}
