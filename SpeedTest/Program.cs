using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Windows;

using Ragnarok;
using Ragnarok.Net.ProtoBuf;
using Ragnarok.Shogi;

namespace SpeedTest
{
    class Program
    {
        static void MeasureTime(string title, Action<int> func)
        {
            const int count = 10000;

            var start = DateTime.Now;
            func(count);
            var elapsed = DateTime.Now - start;

            Console.WriteLine("{0} elapsed: {1}ms ({2}回 {3}ms)",
                title,
                elapsed.TotalMilliseconds / count,
                count,
                elapsed.TotalMilliseconds);
        }

        static void MeasureCloneTime()
        {
            MeasureTime("board clone",
                count =>
                {
                    var board = new Board();

                    for (var i = 0; i < count; ++i)
                    {
                        board.Clone();
                    }
                });

            MeasureTime("list clone",
                count =>
                {
                    var x = new List<int>(Enumerable.Range(0, 1000));

                    for (var i = 0; i < count; ++i)
                    {
                        new List<int>(x);
                    }
                });
        }

        static double Perm(int n, int r)
        {
            double ret = 1.0;

            while (--r >= 0)
            {
                ret *= n--;
            }

            return ret;
        }

        public class ServerClass
        {
            public event Action<int, double> TestEvent;

            public void Fire(int x, double y)
            {
                var handler = TestEvent;

                if (handler != null)
                {
                    handler(x, y);
                }
            }
        }

        public class ClientClass : DependencyObject
        {
            // Using a DependencyProperty as the backing store for Test.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty TestProperty =
                DependencyProperty.Register(
                    "Test", typeof(int), typeof(ClientClass),
                    new UIPropertyMetadata(0, OnTestChanged));

            public int DependencyTest
            {
                get { return (int)GetValue(TestProperty); }
                set { SetValue(TestProperty, value); }
            }

            static void OnTestChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                var self = d as ClientClass;
                if (self != null)
                {
                }
            }

            public int NormalTest
            {
                get;
                set;
            }
            
            public static void s_TestMethod(int x, double y)
            {
                Console.WriteLine("s_TestMethod");
            }

            public void TestMethod(int x, double y)
            {
                Console.WriteLine("TestMethod");
            }
        }

        static ServerClass RegTest()
        {
            var s = new ServerClass();
            var c = new ClientClass();

            //s.TestEvent += Ragnarok.ObjectModel.WeakDelegateManager.MakeWeak(c.TestMethod);
            //s.TestEvent += Ragnarok.ObjectModel.WeakDelegateManager.MakeWeak(ClientClass.s_TestMethod);
            return s;
        }

        static void Atan2Test()
        {
        }

        static void Main(string[] args)
        {
            Ki2File file = new Ki2File();
            file.LoadFile(@"C:\Users\masahiro\Desktop\20110827_bonanza.kif");

            var board = file.CreateBoard();

            /*MeasureTime("test", count =>
            {
                for (var i = 0; i < count; ++i)
                {
                    Math.Atan2(i * 0.4, -i * 0.5);
                }
            });

            var c = new ClientClass();
            MeasureTime("dependency property", count =>
            {
                for (var i = 0; i < count; ++i)
                {
                    if (c.DependencyTest != i)
                    {
                        c.DependencyTest = i;
                    }
                }
            });

            c = new ClientClass();
            MeasureTime("CLR property", count =>
            {
                for (var i = 0; i < count; ++i)
                {
                    if (c.NormalTest != i)
                    {
                        c.NormalTest = i;
                    }
                }
            });*/

            //var r = RegTest();
            //GC.Collect();

            //r.Fire(0, 1.2);

            /*int N = 40;
            int R = 30;
            var all = Math.Pow(N, R);
            var diffs = Perm(N, R);
            var diffsExp1 = (N * Math.Pow(N - 1, R -1));

            Console.WriteLine("全組み合わせ {0}", all);
            Console.WriteLine("全部が違う組み合わせ {0}", diffs);
            Console.WriteLine("1の確立 {0}%", (1 - (diffs / all)) * 100);
            //Console.WriteLine("2の確立 {0}%", (1 - (diffsExp1 / all)) * 100);
            return;*/

            /*var piece = new BoardPiece(BWType.Black, PieceType.Hu, true);
            var data = PbUtil.Serialize(piece);
            Console.WriteLine("typical BoardPiece {0} {1}(x 81)",
                data.Length, data.Length * 81);

            var pieceArray = new BoardPiece[81];
            for (var i = 0; i < 81; ++i) pieceArray[i] = piece;
            data = PbUtil.Serialize(pieceArray);
            Console.WriteLine("typical BoardPiece {0}", data.Length);

            var capturedPieceBox = new CapturedPieceBox(BWType.White);
            data = PbUtil.Serialize(capturedPieceBox);
            Console.WriteLine("typical CapturedPieceBox {0}", data.Length);

            var position = new Position(4, 5);
            data = PbUtil.Serialize(position);
            Console.WriteLine("typical Position {0}", data.Length);

            var boardMove = new BoardMove()
            {
                ActionType = ActionType.None,
                BWType = BWType.Black,
                DropPieceType = PieceType.None,
                NewPosition = new Position(3, 4),
                OldPosition = new Position(3, 5),
            };
            data = PbUtil.Serialize(boardMove);
            Console.WriteLine("typical BoardMove {0} {1}(x 400)",
                data.Length, data.Length * 400);

            var boardMoveArray = new BoardMove[400];
            for (var i = 0; i < boardMoveArray.Length; ++i)
            {
                boardMoveArray[i] = boardMove;
            }
            data = PbUtil.Serialize(boardMoveArray);
            Console.WriteLine("typical BoardPieceList {0}", data.Length);

            var byteArray = new byte[4 * 400];
            for (var i = 0; i < boardMoveArray.Length; ++i)
            {
                byteArray[i] = 0xff;
            }
            data = PbUtil.Serialize(byteArray);
            Console.WriteLine("typical ByteList {0}", data.Length);


            var board = new Board();
            var moveList = BoardExtension.MakeMoveList(SampleMove.LongSample);
            var bmList = board.ConvertMove(moveList);

            foreach (var bm in bmList)
            {
                board.DoMove(bm);
            }

            MeasureTime("test", i =>
            {
                data = board.Serialize();
                Board.Deserialize(data);
            });

            data = board.Serialize();
            var board2 = Board.Deserialize(data);

            Console.WriteLine("{0} {1} {2} compare={3}",
                board, board2, data.Length,
                board.BoardEquals(board2));*/
        }
    }
}
