using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ragnarok;
using Ragnarok.Shogi;

namespace VoteSystem.PluginShogi.ViewModel
{
    using Model;

    /// <summary>
    /// 自動再生の種別です。
    /// </summary>
    public enum AutoPlayType
    {
        /// <summary>
        /// 指し手を動かしません。
        /// </summary>
        None,
        /// <summary>
        /// 与えられた指し手を自動再生します。
        /// </summary>
        Normal,
        /// <summary>
        /// 局面を元に戻しながら自動再生します。
        /// </summary>
        Undo,
        /// <summary>
        /// 局面を次に進めながら自動再生します。
        /// </summary>
        Redo,
    }

    /// <summary>
    /// 駒の自動再生時に使います。
    /// </summary>
    /// <remarks>
    /// 次タイミングの駒の再生情報や背景の不透明度を保持します。
    /// </remarks>
    internal sealed class NextPlayInfo
    {
        /// <summary>
        /// 自動再生の種類を取得または設定します。
        /// </summary>
        public AutoPlayType AutoPlayType
        {
            get;
            set;
        }

        /// <summary>
        /// 次に指す指し手を取得または設定します。
        /// </summary>
        public BoardMove Move
        {
            get;
            set;
        }
      
        /// <summary>
        /// 自動再生の最後の指し手であるかどうかを取得または設定します。
        /// </summary>
        public bool IsLastMove
        {
            get;
            set;
        }
      
        /// <summary>
        /// 背景の不透明度を取得または設定します。
        /// </summary>
        public double Opacity
        {
            get;
            set;
        }
      
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NextPlayInfo()
        {
            AutoPlayType = AutoPlayType.None;
            Opacity = 1.0;
        }
    }
  
    /// <summary>
    /// 指し手の自動再生時に使われます。再生用の変化を保存します。
    /// </summary>
    public sealed class AutoPlay
    {
        /// <summary>
        /// カットイン画像の表示時間です。
        /// </summary>
        public static readonly TimeSpan CutInInterval = TimeSpan.FromSeconds(2.0);

        private List<BoardMove> moveList;
        private IEnumerator<NextPlayInfo> enumerator;
        private int moveIndex;
        private int maxMoveCount;

        /// <summary>
        /// 開始局面を取得または設定します。
        /// </summary>
        public Board Board
        {
            get;
            private set;
        }

        /// <summary>
        /// 指し手の再生間隔を取得または設定します。
        /// </summary>
        public TimeSpan Interval
        {
            get;
            set;
        }

        /// <summary>
        /// 背景がフェードイン／アウトする時間を取得または設定します。
        /// </summary>
        public TimeSpan FadeInterval
        {
            get;
            set;
        }
        
        /// <summary>
        /// 自動再生の種類を取得します。
        /// </summary>
        public AutoPlayType AutoPlayType
        {
            get;
            private set;
        }
        
        /// <summary>
        /// 背景を変化させるかどうかを取得または設定します。
        /// </summary>
        public bool IsChangeBackground
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
        /// まだ指し手が残っているか取得します。
        /// </summary>
        private bool HasMove
        {
            get { return (this.moveIndex < this.maxMoveCount); }
        }

        /// <summary>
        /// 自動再生の次の手を取得します。
        /// </summary>
        private BoardMove NextMove()
        {
            if (!HasMove)
            {
                return null;
            }

            // アンドゥやリドゥの場合は、指し手がありません。
            if (AutoPlayType != AutoPlayType.Normal)
            {
                this.moveIndex++;
                return null;
            }

            return this.moveList[this.moveIndex++];
        }
        
        /// <summary>
        /// 背景色変更中の情報を取得します。
        /// </summary>
        private NextPlayInfo GetFadingNextPlay(DateTime baseTime, bool isReverse)
        {
            var progress = DateTime.Now - baseTime;
            if (progress >= FadeInterval)
            {
                return null;
            }

            // 背景の不透明度を更新します。
            var progressRate = (progress.TotalSeconds / FadeInterval.TotalSeconds);
            return new NextPlayInfo
            {
                Opacity = (isReverse ? 1.0 - progressRate : progressRate),
            };
        }

        /// <summary>
        /// コルーチン用のオブジェクトを返します。
        /// </summary>
        private IEnumerable<NextPlayInfo> GetUpdateEnumerator()
        {
            var baseTime = DateTime.Now;
            
            // 最初に背景色のみを更新します。
            if (IsChangeBackground)
            {
                while (true)
                {
                    var nextPlay = GetFadingNextPlay(baseTime, false);
                    if (nextPlay == null)
                    {
                        baseTime += FadeInterval;
                        break;
                    }

                    yield return nextPlay;
                }
            }

            if (IsUseCutIn)
            {
                // カットインが表示できたら、指定の時間だけ待ちます。
                if (ShogiGlobal.EffectManager.VariationCutIn())
                {
                    while (DateTime.Now - baseTime < CutInInterval)
                    {
                        yield return new NextPlayInfo
                        {
                            Opacity = (IsChangeBackground ? 1.0 : 0.0),
                        };
                    }

                    baseTime += CutInInterval;
                }
            }

            // 最初の一手はすぐに表示します。
            baseTime -= Interval;

            // 最後の指し手を動かした後に一手分だけ待ちます。
            // エフェクトを表示するためです。
            var didLastInterval = false;
            while (HasMove || !didLastInterval)
            {
                var progress = DateTime.Now - baseTime;
                if (progress > Interval)
                {
                    baseTime += Interval;
                    didLastInterval = !HasMove; // NextMoveの前に呼ぶ
                    
                    yield return new NextPlayInfo
                    {
                        AutoPlayType = AutoPlayType,
                        Move = NextMove(),
                        IsLastMove = !HasMove,
                        Opacity = (IsChangeBackground ? 1.0 : 0.0),
                    };
                }
                else
                {
                    yield return new NextPlayInfo
                    {
                        Opacity = (IsChangeBackground ? 1.0 : 0.0),
                    };
                }
            }

            // 背景色をもとに戻します。
            if (IsChangeBackground)
            {
                while (true)
                {
                    var nextPlay = GetFadingNextPlay(baseTime, true);
                    if (nextPlay == null)
                    {
                        baseTime += FadeInterval;
                        break;
                    }

                    yield return nextPlay;
                }
            }
        }

        /// <summary>
        /// 更新します。
        /// </summary>
        internal NextPlayInfo Update()
        {
            if (this.enumerator == null)
            {
                return null;
            }

            // コルーチンを進めます。
            if (!this.enumerator.MoveNext())
            {
                this.enumerator = null;
                return null;
            }
            
            return this.enumerator.Current;
        }

        /// <summary>
        /// オブジェクトの妥当性を検証します。
        /// </summary>
        public bool Validate()
        {
            if (Board == null)
            {
                return false;
            }

            if (AutoPlayType == AutoPlayType.Normal)
            {
                if (this.moveList == null)
                {
                    return false;
                }

                return Board.CanMoveList(this.moveList);
            }

            return true;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AutoPlay(Variation variation)
            : this(variation.Board, variation.BoardMoveList)
        {
            ConfirmMessage = string.Format(
                "{1}{0}{0}コメント: {2}{0}{0}を再生しますか？",
                Environment.NewLine,
                variation.Label,
                variation.Comment);
        }

        /// <summary>
        /// 共通コンストラクタ
        /// </summary>
        private AutoPlay(Board board)
        {
            Board = board;
            Interval = TimeSpan.FromSeconds(1.0);
            FadeInterval = TimeSpan.FromSeconds(0.2); //Interval.TotalSeconds / 2);
            IsConfirmPlay = true;
            IsUseCutIn = true;

            this.enumerator = GetUpdateEnumerator().GetEnumerator();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AutoPlay(Board board, IEnumerable<BoardMove> moveList)
            : this(board)
        {
            if (moveList == null)
            {
                throw new ArgumentNullException("moveList");
            }

            AutoPlayType = AutoPlayType.Normal;

            this.moveList = new List<BoardMove>(moveList);
            this.maxMoveCount = moveList.Count();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AutoPlay(Board board, AutoPlayType autoPlayType)
            : this(board)
        {
            if (autoPlayType != AutoPlayType.Undo &&
                autoPlayType != AutoPlayType.Redo)
            {
                throw new ArgumentException(
                    "アンドゥかリドゥを選択してください。",
                    "autoPlayType");
            }

            AutoPlayType = autoPlayType;

            this.maxMoveCount =
                (autoPlayType == AutoPlayType.Undo ?
                 board.CanUndoCount :
                 board.CanRedoCount);
        }
    }
}
