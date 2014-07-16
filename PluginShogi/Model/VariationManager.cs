using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Ragnarok;
using Ragnarok.Shogi;
using Ragnarok.ObjectModel;
using Ragnarok.Presentation;

namespace VoteSystem.PluginShogi.Model
{
    /// <summary>
    /// 現局面ごとの変化をすべて管理します。
    /// </summary>
    /// <remarks>
    /// 変化は各局面ごとに辞書管理し、現局面が更新されたら
    /// 差し手を巻き戻して局面管理オブジェクトを初手から検索します。
    /// 
    /// こうすることで、現局面を間違って設定してしまった場合でも
    /// その局面に登録された変化を保存することができます。
    /// </remarks>
    public sealed class VariationManager : NotifyObject
    {
        private readonly Dictionary<Board, EachStateManager> stateDic =
            new Dictionary<Board, EachStateManager>(new BoardComparer());

        /// <summary>
        /// 現局面を取得または設定します。
        /// </summary>
        public Board CurrentBoard
        {
            get { return GetValue<Board>("CurrentBoard"); }
            private set { SetValue("CurrentBoard", value); }
        }

        /// <summary>
        /// 現在の局面に至るまでの各局面の管理オブジェクトを取得します。
        /// </summary>
        public NotifyCollection<EachStateManager> CurrentStateList
        {
            get { return GetValue<NotifyCollection<EachStateManager>>("CurrentStateList"); }
            private set { SetValue("CurrentStateList", value); }
        }

        /// <summary>
        /// １つ前の局面から現局面の変化を調べます。
        /// </summary>
        private void AddVariationFromPreviousState(EachStateManager manager)
        {
            var board = manager.Board;
            var prevBoard = board.Clone();

            // 1つ前の局面がなければ何もしません。
            var lastMove = prevBoard.Undo();
            if (lastMove == null)
            {
                return;
            }

            // prevBoardの管理オブジェクトが無ければ、再度このメソッドが呼ばれます。
            var prevManager = GetOrCreateStateManager(prevBoard);

            // １つ前の局面の変化から、現在の局面に適用可能な変化を調べます。
            foreach (var variation in prevManager.VariationList)
            {
                // 現局面の最終手と変化の最初の手を比較
                if (lastMove.Equals(variation.BoardMoveList.First()))
                {
                    var newVariation = new Variation(
                        board,
                        variation.MoveList.Skip(1),
                        variation.BoardMoveList.Skip(1))
                    {
                        Comment = variation.Comment,
                        IsOriginal = false,
                    };

                    manager.TryAddVariation(newVariation);
                }
            }
        }

        /// <summary>
        /// 各局面の管理オブジェクトを取得 or 無ければ作成します。
        /// </summary>
        private EachStateManager GetOrCreateStateManager(Board board)
        {
            EachStateManager value;

            if (this.stateDic.TryGetValue(board, out value))
            {
                return value;
            }

            // 管理オブジェクトが無い場合は、新たに追加します。
            value = new EachStateManager(board);
            this.stateDic[value.Board] = value;

            // 一つ前の局面から適用可能な変化を探します。
            AddVariationFromPreviousState(value);
            return value;
        }

        /// <summary>
        /// 変化を管理する局面を設定します。
        /// </summary>
        public void SetCurrentBoard(Board board)
        {
            if (board == null)
            {
                return;
            }

            using (LazyLock())
            {
                if (CurrentBoard != null &&
                    board.BoardEquals(CurrentBoard))
                {
                    return;
                }

                var newStateList = new NotifyCollection<EachStateManager>();
                var tmpBoard = board.Clone();

                // 既に登録されている局面から、変化を検索します。
                do
                {
                    var stateManager = GetOrCreateStateManager(tmpBoard);

                    newStateList.Insert(0, stateManager);
                } while (tmpBoard.Undo() != null);

                CurrentStateList = newStateList;

                // クローン必要？
                CurrentBoard = board.Clone();
            }
        }

        /// <summary>
        /// 現局面に変化を追加します。
        /// </summary>
        public void AddVariation(Variation variation)
        {
            if (variation == null)
            {
                return;
            }

            var manager = CurrentStateList.LastOrDefault();
            if (manager == null)
            {
                return;
            }

            manager.TryAddVariation(variation);
        }

        /// <summary>
        /// 変化ツリーに変化を登録します。
        /// </summary>
        private void AddVariationNode(MoveNode root,
                                      IEnumerable<BoardMove> variationMoveList)
        {
            if (root == null)
            {
                return;
            }

            if (variationMoveList == null || !variationMoveList.Any())
            {
                return;
            }

            var firstMove = variationMoveList.FirstOrDefault();
            var leaveMoveList = variationMoveList.Skip(1);

            // 本譜の手を増やされても困るので、本譜がない場合は変化も追加しません。
            if (!root.NextNodes.Any())
            {
                return;
            }

            // 変化の初手が次の手と一致したら、次の手に移動します。
            foreach (var next in root.NextNodes)
            {
                if (next.Move == firstMove)
                {
                    AddVariationNode(next, leaveMoveList);
                    return;
                }
            }

            // 次の手を設定します。
            var newNode = KifuObject.Convert2Node(
                variationMoveList,
                root.MoveCount + 1);
            root.NextNodes.Add(newNode);
        }

        /// <summary>
        /// 本譜のみの差し手をツリー形式(内容は一本道)に直します。
        /// </summary>
        private MoveNode CreateHonpuNode(Board board)
        {
            using (LazyLock())
            {
                var root = new MoveNode();
                var moveCount = 1;
                var last = root;

                var tmpBoard = board.Clone();
                tmpBoard.ClearRedoList();
                tmpBoard.UndoAll();

                BoardMove move;
                while ((move = tmpBoard.Redo()) != null)
                {
                    var node = new MoveNode()
                    {
                        Move = move,
                        MoveCount = moveCount++,
                    };

                    last.NextNodes.Add(node);
                    last = node;
                }

                return root;
            }
        }

        /// <summary>
        /// 指し手と変化を木構造で表現します。（主にファイル保存用）
        /// </summary>
        public MoveNode CreateVariationNode(Board board)
        {
            if (board == null || !board.Validate())
            {
                return null;
            }

            using (LazyLock())
            {
                var root = CreateHonpuNode(board);
                var tmpBoard = board.Clone();
                var currentRoot = root;

                // できる限りUndoします。
                tmpBoard.ClearRedoList();
                tmpBoard.UndoAll();

                do
                {
                    var manager = GetOrCreateStateManager(tmpBoard);
                    if (manager == null)
                    {
                        break;
                    }

                    // 各変化を登録します。
                    foreach (var variation in manager.VariationList)
                    {
                        AddVariationNode(currentRoot, variation.BoardMoveList);
                    }

                    currentRoot = currentRoot.NextNode;
                } while (tmpBoard.Redo() != null);

                return root;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VariationManager()
        {
            CurrentStateList = new NotifyCollection<EachStateManager>();
        }
    }
}
