using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ragnarok.Shogi;

namespace VoteSystem.PluginShogi.Model
{
    /// <summary>
    /// 各変化を管理します。
    /// </summary>
    public sealed class Variation
    {
        /// <summary>
        /// 再生する変化の最短手数です。
        /// </summary>
        public const int ShortestMove = 3;

        /// <summary>
        /// 変化を文字列化したものを取得します。
        /// </summary>
        public string Label
        {
            get;
            private set;
        }

        /// <summary>
        /// コメントを取得または設定します。
        /// </summary>
        public string Comment
        {
            get;
            set;
        }

        /// <summary>
        /// これがこの局面を基点とした変化かどうかを取得または設定します。
        /// </summary>
        /// <remarks>
        /// 局面をさかのぼって同じ変化を適用するため、高速化のため
        /// このフラグが必要になります。
        /// </remarks>
        public bool IsOriginal
        {
            get;
            set;
        }

        /// <summary>
        /// 変化の基点となる局面を取得します。
        /// </summary>
        public Board Board
        {
            get;
            private set;
        }

        /// <summary>
        /// 指し手のリスト(XX -> YY形式)を取得します。
        /// </summary>
        public List<BoardMove> BoardMoveList
        {
            get;
            private set;
        }

        /// <summary>
        /// 指し手のリスト(位置 + 駒形式)を取得します。
        /// </summary>
        public List<Move> MoveList
        {
            get;
            private set;
        }

        /// <summary>
        /// ３手以上から表示可能な変化としています。
        /// </summary>
        public bool CanShow
        {
            get
            {
                return (MoveList.Count >= ShortestMove);
            }
        }

        /// <summary>
        /// 短い方に合わせた長さで良いので、指し手が一致するか調べます。
        /// </summary>
        public bool IsMatchShort(IEnumerable<BoardMove> otherBoardMoveList)
        {
            if (otherBoardMoveList == null)
            {
                return false;
            }

            var length = Math.Min(
                BoardMoveList.Count(),
                otherBoardMoveList.Count());

            return BoardMoveList.Take(length).SequenceEqual(
                otherBoardMoveList.Take(length));
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Variation(Board board, IEnumerable<Move> moveList,
                         IEnumerable<BoardMove> boardMoveList)
        {
            Board = board.Clone();
            BoardMoveList = boardMoveList.ToList();
            MoveList = moveList.ToList();
            IsOriginal = true;

            Label = string.Join("",
                        MoveList.Select(move => move.ToString()).ToArray());
        }

        /// <summary>
        /// 変化オブジェクトを指し手と局面から作成します。
        /// </summary>
        public static Variation Create(Board board, IEnumerable<Move> moveList,
                                       string comment, bool isOriginal)
        {
            // 何度も巡回するため、最初にリスト化しておきます。
            var moveListEx = moveList.ToList();

            // 先後をきっちりと設定します。
            var bwType = board.Turn;
            foreach (var move in moveListEx)
            {
                move.BWType = bwType;
                bwType = bwType.Toggle();
            }

            // 指し手は正しく動かせるところまでしか変換しないため、
            // 変換後の差し手は最後の方が切れることがあります。
            var result = board.ConvertMove(moveListEx);
            if (moveListEx.Count() > result.Count())
            {
                moveListEx = moveListEx.Take(result.Count()).ToList();
            }

            return new Variation(board, moveListEx, result)
            {
                Comment = comment,
                IsOriginal = isOriginal,
            };
        }

        /// <summary>
        /// 変化コメントを処理します。
        /// </summary>
        public static Variation Parse(string commentText)
        {
            // まず、文字列から指し手リストを作成します。
            var notes = string.Empty;
            var moveList = BoardExtension.MakeMoveList(
                commentText, out notes);

            // 最短手数以上の変化なら、さらに現局面から再生可能な
            // 変化かどうか調べます。
            if (moveList.Count() >= Variation.ShortestMove)
            {
                var variation = Variation.Create(
                    ShogiGlobal.ShogiModel.CurrentBoard,
                    moveList,
                    notes,
                    true);
                if (variation == null ||
                    variation.MoveList.Count() < Variation.ShortestMove)
                {
                    return null;
                }

                return variation;
            }

            return null;
        }
    }
}
