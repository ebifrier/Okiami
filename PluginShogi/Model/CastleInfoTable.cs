using System;
using System.Collections.Generic;
using System.Linq;

using Ragnarok.Shogi;

namespace VoteSystem.PluginShogi.Model
{
    /// <summary>
    /// 囲いに関する情報を保持します。
    /// </summary>
    public partial class CastleInfo
    {
        /// <summary>
        /// 囲いに関する情報がテーブル化されています。
        /// </summary>
        public readonly static List<CastleInfo> CastleTable =
            new List<CastleInfo>
            {
                new CastleInfo(
                    "ダイアモンド美濃", "DiamondMino", "Mino.png", 20,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(2, 8)),
                        new CastlePiece(PieceType.Gin, new Square(3, 8)),
                        new CastlePiece(PieceType.Kin, new Square(4, 9)),
                        new CastlePiece(PieceType.Kin, new Square(5, 8)),
                        new CastlePiece(PieceType.Gin, new Square(4, 7)),
                    },
                    new string[] { "片美濃囲い", "本美濃囲い", }),
                
                new CastleInfo(
                    "すごく固い穴熊", "Anaguma3", "Anaguma3.png", 20,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(1, 9)),
                        new CastlePiece(PieceType.Kyo, new Square(1, 8)),
                        new CastlePiece(PieceType.Kei, new Square(2, 9)),
                        new CastlePiece(PieceType.Gin, new Square(2, 8)),
                        new CastlePiece(PieceType.Kin, new Square(3, 9)),
                        new CastlePiece(PieceType.Kin, new Square(3, 8)),
                    },
                    new string[] { "穴熊", "固い穴熊", "固い穴熊", }),
                
                new CastleInfo(
                    "超固い居飛車穴熊", "IbisyaAnaguma3", "Anaguma3.png", 20,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(9, 9)),
                        new CastlePiece(PieceType.Kyo, new Square(9, 8)),
                        new CastlePiece(PieceType.Kei, new Square(8, 9)),
                        new CastlePiece(PieceType.Gin, new Square(8, 8)),
                        new CastlePiece(PieceType.Kin, new Square(7, 9)),
                        new CastlePiece(PieceType.Kin, new Square(7, 8)),
                    },
                    new string[] { "居飛車穴熊", "固い居飛車穴熊", "固い居飛車穴熊", }),
                
                new CastleInfo(
                    "松尾流穴熊", "MatsuoAnaguma", "Anaguma3.png", 20,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(9, 9)),
                        new CastlePiece(PieceType.Kyo, new Square(9, 8)),
                        new CastlePiece(PieceType.Kei, new Square(8, 9)),
                        new CastlePiece(PieceType.Gin, new Square(8, 8)),
                        new CastlePiece(PieceType.Gin, new Square(7, 9)),
                        new CastlePiece(PieceType.Kin, new Square(7, 8)),
                        new CastlePiece(PieceType.Kin, new Square(6, 7)),
                    },
                    new string[] { "居飛車穴熊", "固い居飛車穴熊", }),
                
                new CastleInfo(
                    "本美濃囲い", "HonMino", "Mino.png", 10,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(2, 8)),
                        new CastlePiece(PieceType.Gin, new Square(3, 8)),
                        new CastlePiece(PieceType.Kin, new Square(4, 9)),
                        new CastlePiece(PieceType.Kin, new Square(5, 8)),
                    },
                    new string[] { "片美濃囲い", }),
                
                new CastleInfo(
                    "高美濃囲い", "TakaMino", "Mino.png", 10,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(2, 8)),
                        new CastlePiece(PieceType.Gin, new Square(3, 8)),
                        new CastlePiece(PieceType.Kin, new Square(4, 9)),
                        new CastlePiece(PieceType.Kin, new Square(4, 7)),
                    },
                    new string[] { "片美濃囲い", }),
                
                new CastleInfo(
                    "銀美濃囲い", "GinMino", "Mino.png", 10,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(2, 8)),
                        new CastlePiece(PieceType.Gin, new Square(3, 8)),
                        new CastlePiece(PieceType.Kin, new Square(4, 9)),
                        new CastlePiece(PieceType.Gin, new Square(5, 8)),
                    },
                    new string[] { "片美濃囲い", }),
                
                new CastleInfo(
                    "ヒラメ", "Hirame", "", 10,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(2, 8)),
                        new CastlePiece(PieceType.Gin, new Square(3, 8)),
                        new CastlePiece(PieceType.Kin, new Square(4, 9)),
                        new CastlePiece(PieceType.Kin, new Square(5, 9)),
                    },
                    new string[] { "片美濃囲い", }),
                
                new CastleInfo(
                    "銀冠", "Ginkan", "Ginkan.png", 10,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(2, 8)),
                        new CastlePiece(PieceType.Gin, new Square(2, 7)),
                        new CastlePiece(PieceType.Kin, new Square(3, 8)),
                        new CastlePiece(PieceType.Kin, new Square(4, 7)),
                    },
                    new string[] { "片銀冠", }),
                
                new CastleInfo(
                    "固い穴熊", "Anaguma2", "Anaguma2.png", 10,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(1, 9)),
                        new CastlePiece(PieceType.Kyo, new Square(1, 8)),
                        new CastlePiece(PieceType.Kei, new Square(2, 9)),
                        new CastlePiece(PieceType.Gin, new Square(2, 8)),
                        new CastlePiece(PieceType.Kin, new Square(3, 9)),
                    },
                    new string[] { "穴熊", }),
                
                new CastleInfo(
                    "固い穴熊", "Anaguma2", "Anaguma2.png", 10,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(1, 9)),
                        new CastlePiece(PieceType.Kyo, new Square(1, 8)),
                        new CastlePiece(PieceType.Kei, new Square(2, 9)),
                        new CastlePiece(PieceType.Gin, new Square(2, 8)),
                        new CastlePiece(PieceType.Kin, new Square(3, 8)),
                    },
                    new string[] { "穴熊", }),
                
                new CastleInfo(
                    "総矢倉", "SouYagura", "Yagura.png", 10,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(8, 8)),
                        new CastlePiece(PieceType.Kin, new Square(7, 8)),
                        new CastlePiece(PieceType.Gin, new Square(7, 7)),
                        new CastlePiece(PieceType.Kin, new Square(6, 7)),
                        new CastlePiece(PieceType.Gin, new Square(5, 7)),
                    },
                    new string[] { "矢倉", }),
                
                new CastleInfo(
                    "菱矢倉", "HishiYagura", "Yagura.png", 10,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(8, 8)),
                        new CastlePiece(PieceType.Kin, new Square(7, 8)),
                        new CastlePiece(PieceType.Gin, new Square(7, 7)),
                        new CastlePiece(PieceType.Kin, new Square(6, 7)),
                        new CastlePiece(PieceType.Gin, new Square(6, 6)),
                    },
                    new string[] { "矢倉", }),
                
                new CastleInfo(
                    "居飛車銀冠", "IbisyaGinkan", "Ginkan.png", 10,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(8, 8)),
                        new CastlePiece(PieceType.Gin, new Square(8, 7)),
                        new CastlePiece(PieceType.Kin, new Square(7, 8)),
                        new CastlePiece(PieceType.Kin, new Square(6, 7)),
                    },
                    new string[] { "居飛車片銀冠", }),
                
                new CastleInfo(
                    "固い居飛車穴熊", "IbisyaAnaguma2", "Anaguma2.png", 10,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(9, 9)),
                        new CastlePiece(PieceType.Kyo, new Square(9, 8)),
                        new CastlePiece(PieceType.Kei, new Square(8, 9)),
                        new CastlePiece(PieceType.Gin, new Square(8, 8)),
                        new CastlePiece(PieceType.Kin, new Square(7, 9)),
                    },
                    new string[] { "居飛車穴熊", }),
                
                new CastleInfo(
                    "固い居飛車穴熊", "IbisyaAnaguma2", "Anaguma2.png", 10,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(9, 9)),
                        new CastlePiece(PieceType.Kyo, new Square(9, 8)),
                        new CastlePiece(PieceType.Kei, new Square(8, 9)),
                        new CastlePiece(PieceType.Gin, new Square(8, 8)),
                        new CastlePiece(PieceType.Kin, new Square(7, 8)),
                    },
                    new string[] { "居飛車穴熊", }),
                
                new CastleInfo(
                    "ビッグ４", "Big4", "", 10,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(9, 9)),
                        new CastlePiece(PieceType.Kyo, new Square(9, 8)),
                        new CastlePiece(PieceType.Kei, new Square(8, 9)),
                        new CastlePiece(PieceType.Kin, new Square(8, 8)),
                        new CastlePiece(PieceType.Kin, new Square(7, 8)),
                        new CastlePiece(PieceType.Gin, new Square(8, 7)),
                        new CastlePiece(PieceType.Gin, new Square(7, 7)),
                    },
                    new string[] { "居飛車銀冠穴熊", }),
                
                new CastleInfo(
                    "片美濃囲い", "KataMino", "Mino.png", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(2, 8)),
                        new CastlePiece(PieceType.Gin, new Square(3, 8)),
                        new CastlePiece(PieceType.Kin, new Square(4, 9)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "金美濃囲い", "KinMino", "Mino.png", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(2, 8)),
                        new CastlePiece(PieceType.Kin, new Square(3, 8)),
                        new CastlePiece(PieceType.Gin, new Square(4, 8)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "木村美濃", "KimuraMino", "Mino.png", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(2, 8)),
                        new CastlePiece(PieceType.Kin, new Square(3, 8)),
                        new CastlePiece(PieceType.Gin, new Square(4, 7)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "片銀冠", "KataGinkan", "Ginkan.png", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(2, 8)),
                        new CastlePiece(PieceType.Gin, new Square(2, 7)),
                        new CastlePiece(PieceType.Kin, new Square(3, 8)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "銀冠穴熊", "GinkanAnaguma", "Ginkan.png", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(1, 9)),
                        new CastlePiece(PieceType.Kyo, new Square(1, 8)),
                        new CastlePiece(PieceType.Kei, new Square(2, 9)),
                        new CastlePiece(PieceType.Gin, new Square(2, 7)),
                        new CastlePiece(PieceType.Kin, new Square(3, 8)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "穴熊", "Anaguma", "Anaguma1.png", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(1, 9)),
                        new CastlePiece(PieceType.Kyo, new Square(1, 8)),
                        new CastlePiece(PieceType.Kei, new Square(2, 9)),
                        new CastlePiece(PieceType.Gin, new Square(2, 8)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "早囲い", "Haya", "", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(3, 8)),
                        new CastlePiece(PieceType.Gin, new Square(4, 8)),
                        new CastlePiece(PieceType.Kin, new Square(4, 9)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "金無双", "Kinmusou", "", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(3, 8)),
                        new CastlePiece(PieceType.Kin, new Square(5, 8)),
                        new CastlePiece(PieceType.Kin, new Square(4, 8)),
                        new CastlePiece(PieceType.Gin, new Square(2, 8)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "金無双", "Kinmusou", "", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(3, 8)),
                        new CastlePiece(PieceType.Kin, new Square(5, 8)),
                        new CastlePiece(PieceType.Kin, new Square(4, 8)),
                        new CastlePiece(PieceType.Gin, new Square(3, 9)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "カニ囲い", "Kani", "", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(6, 9)),
                        new CastlePiece(PieceType.Kin, new Square(7, 8)),
                        new CastlePiece(PieceType.Gin, new Square(6, 8)),
                        new CastlePiece(PieceType.Kin, new Square(5, 8)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "矢倉", "Yagura", "Yagura.png", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(8, 8)),
                        new CastlePiece(PieceType.Kin, new Square(7, 8)),
                        new CastlePiece(PieceType.Gin, new Square(7, 7)),
                        new CastlePiece(PieceType.Kin, new Square(6, 7)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "銀立ち矢倉", "GintachiYagura", "Yagura.png", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(8, 8)),
                        new CastlePiece(PieceType.Kin, new Square(7, 8)),
                        new CastlePiece(PieceType.Kin, new Square(6, 7)),
                        new CastlePiece(PieceType.Gin, new Square(7, 6)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "銀矢倉", "GinYagura", "Yagura.png", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(8, 8)),
                        new CastlePiece(PieceType.Kin, new Square(7, 8)),
                        new CastlePiece(PieceType.Gin, new Square(7, 7)),
                        new CastlePiece(PieceType.Gin, new Square(6, 7)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "ボナンザ囲い", "Bonanza", "", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(7, 8)),
                        new CastlePiece(PieceType.Gin, new Square(7, 7)),
                        new CastlePiece(PieceType.Kin, new Square(6, 8)),
                        new CastlePiece(PieceType.Kin, new Square(5, 8)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "菊水矢倉", "KikusuiYagura", "Yagura.png", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(8, 9)),
                        new CastlePiece(PieceType.Kin, new Square(7, 8)),
                        new CastlePiece(PieceType.Gin, new Square(8, 8)),
                        new CastlePiece(PieceType.Kin, new Square(6, 7)),
                        new CastlePiece(PieceType.Kei, new Square(7, 7)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "凹み矢倉", "HekomiYagura", "Yagura.png", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(8, 8)),
                        new CastlePiece(PieceType.Kin, new Square(7, 8)),
                        new CastlePiece(PieceType.Gin, new Square(7, 7)),
                        new CastlePiece(PieceType.Kin, new Square(6, 8)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "片矢倉", "KataYagura", "Yagura.png", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(7, 8)),
                        new CastlePiece(PieceType.Gin, new Square(7, 7)),
                        new CastlePiece(PieceType.Kin, new Square(6, 7)),
                        new CastlePiece(PieceType.Kin, new Square(6, 8)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "居飛車片銀冠", "IbisyaKataGinkan", "Ginkan.png", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(8, 8)),
                        new CastlePiece(PieceType.Gin, new Square(8, 7)),
                        new CastlePiece(PieceType.Kin, new Square(7, 8)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "居飛車銀冠穴熊", "IbisyaGinkanAnaguma", "Ginkan.png", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(9, 9)),
                        new CastlePiece(PieceType.Kyo, new Square(9, 8)),
                        new CastlePiece(PieceType.Kei, new Square(8, 9)),
                        new CastlePiece(PieceType.Gin, new Square(8, 7)),
                        new CastlePiece(PieceType.Kin, new Square(7, 8)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "居飛車穴熊", "IbisyaAnaguma", "Anaguma1.png", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(9, 9)),
                        new CastlePiece(PieceType.Kyo, new Square(9, 8)),
                        new CastlePiece(PieceType.Kei, new Square(8, 9)),
                        new CastlePiece(PieceType.Gin, new Square(8, 8)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "ミレニアム", "Millennium", "", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(8, 9)),
                        new CastlePiece(PieceType.Kei, new Square(7, 7)),
                        new CastlePiece(PieceType.Gin, new Square(8, 8)),
                        new CastlePiece(PieceType.Kin, new Square(7, 9)),
                        new CastlePiece(PieceType.Kin, new Square(7, 8)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "船囲い", "Hune", "", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(7, 8)),
                        new CastlePiece(PieceType.Gin, new Square(7, 9)),
                        new CastlePiece(PieceType.Kin, new Square(6, 9)),
                        new CastlePiece(PieceType.Kin, new Square(5, 8)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "箱入り娘", "Hakoirimusume", "", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(7, 8)),
                        new CastlePiece(PieceType.Gin, new Square(7, 9)),
                        new CastlePiece(PieceType.Kin, new Square(6, 9)),
                        new CastlePiece(PieceType.Kin, new Square(6, 8)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "セメント囲い", "Cement", "", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(7, 8)),
                        new CastlePiece(PieceType.Gin, new Square(6, 7)),
                        new CastlePiece(PieceType.Gin, new Square(5, 7)),
                        new CastlePiece(PieceType.Kin, new Square(6, 8)),
                        new CastlePiece(PieceType.Kin, new Square(5, 8)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "中住まい", "Nakazumai", "", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(5, 8)),
                        new CastlePiece(PieceType.Kin, new Square(3, 8)),
                        new CastlePiece(PieceType.Kin, new Square(7, 8)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "中原囲い", "Nakahara", "", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(6, 9)),
                        new CastlePiece(PieceType.Kin, new Square(5, 9)),
                        new CastlePiece(PieceType.Gin, new Square(4, 8)),
                        new CastlePiece(PieceType.Kin, new Square(7, 8)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "左美濃", "HidariMino", "Mino.png", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(8, 8)),
                        new CastlePiece(PieceType.Gin, new Square(7, 8)),
                        new CastlePiece(PieceType.Kin, new Square(6, 9)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "天守閣美濃", "TenshukakuMino", "Mino.png", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(8, 7)),
                        new CastlePiece(PieceType.Kaku, new Square(8, 8)),
                        new CastlePiece(PieceType.Gin, new Square(7, 8)),
                        new CastlePiece(PieceType.Kin, new Square(6, 9)),
                        new CastlePiece(PieceType.Kin, new Square(5, 8)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "米長玉", "YonenagaGyoku", "", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(9, 8)),
                        new CastlePiece(PieceType.Kyo, new Square(9, 9)),
                        new CastlePiece(PieceType.Gin, new Square(8, 7)),
                        new CastlePiece(PieceType.Kin, new Square(7, 8)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "串カツ囲い", "Kushikatsu", "", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(9, 8)),
                        new CastlePiece(PieceType.Kyo, new Square(9, 9)),
                        new CastlePiece(PieceType.Gin, new Square(8, 8)),
                        new CastlePiece(PieceType.Kin, new Square(7, 9)),
                        new CastlePiece(PieceType.Kin, new Square(7, 8)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "雁木", "Gangi", "", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(6, 9)),
                        new CastlePiece(PieceType.Kin, new Square(7, 8)),
                        new CastlePiece(PieceType.Kin, new Square(5, 8)),
                        new CastlePiece(PieceType.Gin, new Square(6, 7)),
                        new CastlePiece(PieceType.Gin, new Square(5, 7)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "無敵囲い", "Muteki", "", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(5, 9)),
                        new CastlePiece(PieceType.Kin, new Square(6, 9)),
                        new CastlePiece(PieceType.Kin, new Square(4, 9)),
                        new CastlePiece(PieceType.Gin, new Square(6, 8)),
                        new CastlePiece(PieceType.Gin, new Square(4, 8)),
                        new CastlePiece(PieceType.Hisya, new Square(5, 8)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "風車", "Kazaguruma", "", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Kei, new Square(7, 7)),
                        new CastlePiece(PieceType.Gin, new Square(6, 7)),
                        new CastlePiece(PieceType.Kin, new Square(7, 8)),
                        new CastlePiece(PieceType.Kei, new Square(3, 7)),
                        new CastlePiece(PieceType.Gin, new Square(4, 7)),
                    },
                    new string[] { }),
                
                new CastleInfo(
                    "イチゴ囲い", "Ichigo", "", 0,
                    new []
                    {
                        new CastlePiece(PieceType.Gyoku, new Square(6, 8)),
                        new CastlePiece(PieceType.Kin, new Square(7, 8)),
                        new CastlePiece(PieceType.Kin, new Square(5, 8)),
                        new CastlePiece(PieceType.Gin, new Square(7, 9)),
                    },
                    new string[] { }),
            };
    }
}
