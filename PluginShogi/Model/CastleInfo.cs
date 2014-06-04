using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Ragnarok.Shogi;

namespace VoteSystem.PluginShogi.Model
{
    /// <summary>
    /// 囲いを規定する各駒の情報を保持します。
    /// </summary>
    public class CastlePiece
    {
        /// <summary>
        /// 駒の種類を取得します。
        /// </summary>
        public PieceType PieceType
        {
            get;
            private set;
        }

        /// <summary>
        /// 駒の位置を取得します。
        /// </summary>
        public Square Square
        {
            get;
            private set;
        }

        /// <summary>
        /// 必要なら先後の反転を行った駒の位置を取得します。
        /// </summary>
        public Square GetViewSquare(BWType side)
        {
            return (side == BWType.Black ? Square : Square.Flip());
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CastlePiece(PieceType type, Square square)
        {
            PieceType = type;
            Square = square;
        }
    }

    /// <summary>
    /// 囲いに関する情報を保持します。
    /// </summary>
    public partial class CastleInfo
    {
        /// <summary>
        /// 名前を取得します。
        /// </summary>
        public string Name
        {
            get;
            private set;
        }
        
        /// <summary>
        /// IDを取得します。
        /// </summary>
        public string Id
        {
            get;
            private set;
        }

        /// <summary>
        /// 表示させる囲い画像のファイル名を取得します。
        /// </summary>
        public string ImageName
        {
            get;
            private set;
        }
        
        /// <summary>
        /// 囲いの優先順位を取得します。
        /// </summary>
        public int Priority
        {
            get;
            private set;
        }

        /// <summary>
        /// 囲いに必要な駒とその位置のリストを取得します。
        /// </summary>
        public CastlePiece[] PieceList
        {
            get;
            private set;
        }

        /// <summary>
        /// 派生元となる囲いのリストを取得します。
        /// </summary>
        /// <remarks>
        /// たとえば、高美濃なら本美濃や片美濃がそれに当たります。
        /// </remarks>
        public string[] BaseCastleList
        {
            get;
            private set;
        }

        /*/// <summary>
        /// 画像名に拡張子などを付加します。
        /// </summary>
        private static string AdjustImageName(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
            {
                return null;
            }

            var ext = Path.GetExtension(imageName);
            if (string.IsNullOrEmpty(ext))
            {
                return imageName;
            }

            // ファイルはpng画像限定です。
            return (imageName + ".png");
        }*/

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CastleInfo(string name, string id, string imageName,
                          int priority, CastlePiece[] pieceList,
                          string[] baseCastleList)
        {
            Name = name;
            Id = id;
            ImageName = imageName;
            Priority = priority;
            PieceList = pieceList;
            BaseCastleList = baseCastleList;
        }

        /// <summary>
        /// 囲いのある駒が適切な位置にあるか調べます。
        /// </summary>
        private static bool IsMatchPiece(Board board, BWType side,
                                         Square square,
                                         CastlePiece pieceInfo)
        {
            var sq = pieceInfo.GetViewSquare(side);
            var boardPiece = board[sq];

            return (
                boardPiece != null &&
                !boardPiece.IsPromoted &&
                boardPiece.BWType == side &&
                boardPiece.PieceType == pieceInfo.PieceType);
        }

        /// <summary>
        /// 駒の位置が一致するか調べます。
        /// </summary>
        private static bool IsMatchSquare(BWType side, Square square,
                                          CastlePiece pieceInfo)
        {
            return (pieceInfo.GetViewSquare(side) == square);
        }

        /// <summary>
        /// <paramref name="side"/>側の囲いを判定します。
        /// </summary>
        public static IEnumerable<CastleInfo> Detect(Board board, BWType side,
                                                     Square square)
        {
            if (board == null)
            {
                throw new ArgumentNullException("board");
            }

            return CastleInfo.CastleTable
                .Where(_ => _.PieceList.All(
                    __ => IsMatchPiece(board, side, square, __)))
                .Where(_ => _.PieceList.Any(
                    __ => IsMatchSquare(side, square, __)));
        }
    }
}
