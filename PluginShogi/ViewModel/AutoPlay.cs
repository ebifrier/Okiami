using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ragnarok;
using Ragnarok.Shogi;

namespace VoteSystem.PluginShogi.ViewModel
{
    using Model;
    using View;

    /// <summary>
    /// 指し手の自動再生時に使われます。再生用の変化を保存します。
    /// </summary>
    public sealed class AutoPlay_
    {
        private List<BoardMove> moveList;
        private IEnumerator<bool> enumerator;
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

        public ShogiControl Control
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
        /// 自動再生の種類を取得します。
        /// </summary>
        public AutoPlayType AutoPlayType
        {
            get;
            private set;
        }
        
        /// <summary>
        /// まだ指し手が残っているか取得します。
        /// </summary>
        private bool HasMove
        {
            get { return (this.moveIndex < this.maxMoveCount); }
        }

        private TimeSpan position;
        private TimeSpan elapsed;

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

        private void DoMove()
        {
            if (Board == null)
            {
                return;
            }

            switch (AutoPlayType)
            {
                case AutoPlayType.Normal:
                    var move = NextMove();
                    if (move != null)
                    {
                        Board.DoMove(move);
                    }
                    break;
                case AutoPlayType.Undo:
                    Board.Undo();
                    break;
                case AutoPlayType.Redo:
                    Board.Redo();
                    break;
            }
        }

        /// <summary>
        /// コルーチン用のオブジェクトを返します。
        /// </summary>
        private IEnumerable<bool> GetUpdateEnumerator()
        {
            Effects.EffectManager manager = null;
            if (Control != null && Control.EffectManager != null)
            {
                manager = Control.EffectManager;

                manager.EffectEnabled = false;
                //manager.IsAutoPlayEffect = true;
                manager.EffectMoveCount = 0;
            }

            Control.Board = Board;

            // 最後の指し手を動かした後に一手分だけ待ちます。
            // エフェクトを表示するためです。
            var didLastInterval = false;
            while (HasMove || !didLastInterval)
            {
                this.position += this.elapsed;
                if (this.position > Interval)
                {
                    this.position -= Interval;
                    didLastInterval = !HasMove; // NextMoveの前に呼ぶ
                    
                    if (manager != null)
                    {
                        manager.ChangeMoveCount(Board.MoveCount);
                    }
                    DoMove();
                }

                yield return true;
            }

            if (manager != null)
            {
                manager.EffectEnabled = true;
                //manager.IsAutoPlayEffect = false;
                manager.EffectMoveCount = 0;
            }
        }

        /// <summary>
        /// 更新します。
        /// </summary>
        public bool Update(TimeSpan elapsed)
        {
            if (this.enumerator == null)
            {
                return false;
            }

            // コルーチンを進めます。
            if (!this.enumerator.MoveNext())
            {
                this.enumerator = null;
                return false;
            }
            
            this.elapsed = elapsed;
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
        /// 共通コンストラクタ
        /// </summary>
        private AutoPlay_(ShogiControl control, Board board)
        {
            Board = board;
            Control = control;
            Interval = TimeSpan.FromSeconds(1.0);

            this.position = TimeSpan.Zero;
            this.elapsed = TimeSpan.Zero;
            this.enumerator = GetUpdateEnumerator().GetEnumerator();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AutoPlay_(ShogiControl control, Board board, IEnumerable<BoardMove> moveList)
            : this(control, board)
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
        public AutoPlay_(ShogiControl control, Board board, AutoPlayType autoPlayType)
            : this(control, board)
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
