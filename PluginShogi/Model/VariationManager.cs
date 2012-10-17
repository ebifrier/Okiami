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
        /// 本譜のみの差し手をツリー形式(内容は一本道)に直します。
        /// </summary>
        private VariationNode CreateHonpuNode()
        {
            var root = new VariationNode()
            {
                MoveCount = 0,
            };

            var prevNode = root;
            foreach (var manager in CurrentStateList)
            {
                var move = manager.Board.LastMove;
                if (move == null)
                {
                    continue;
                }

                var board = manager.Board.Clone();
                board.Undo();

                var node = new VariationNode()
                {
                    Move = board.ConvertMove(move, true),
                    MoveCount = manager.Board.MoveCount,
                };

                prevNode.NextChild = node;
                prevNode = node;
            }

            return root;
        }

        /// <summary>
        /// 変化ツリーに変化を登録します。
        /// </summary>
        private void AddVariationNode(VariationNode root,
                                      IEnumerable<Move> variationMoveList)
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

            // 変化の初手が次の手と一致したら、次の手に移動します。
            var child = root.NextChild;
            if (child != null && child.Move == firstMove)
            {
                AddVariationNode(child, leaveMoveList);
                return;
            }

            if (root.NextChild == null)
            {
                // 次の手が空いていれば次から変化をまとめて登録します。
                root.NextChild = KifuObject.Convert2Node(
                    variationMoveList,
                    root.MoveCount + 1);
            }
            else
            {
                // 次の手の変化に所望の変化があるか調べます。
                var prevNode = child;
                for (var next = child.NextVariation; next != null; next = next.NextVariation)
                {
                    if (next.Move == firstMove)
                    {
                        AddVariationNode(next, leaveMoveList);
                        return;
                    }

                    prevNode = next;
                }

                // 次の手を設定します。
                prevNode.NextVariation = KifuObject.Convert2Node(
                    variationMoveList,
                    root.MoveCount + 1);
            }
        }

        /// <summary>
        /// 指し手の移動前位置を設定します。
        /// </summary>
        private List<Move> ModifyMove(Variation variation)
        {
            var bmList = variation.BoardMoveList;

            return variation.MoveList
                .Select(_ => _.Clone())
                .SelectWithIndex((_, i) =>
                    _.Apply(__ => __.OldPosition = bmList[i].OldPosition))
                .ToList();
        }

        /// <summary>
        /// 差し手と変化を木構造で表現します。（主にファイル保存用）
        /// </summary>
        public VariationNode CreateVariationNode()
        {
            var root = CreateHonpuNode();
            var currentRoot = root;

            foreach (var manager in CurrentStateList)
            {
                // 各変化を登録します。
                foreach (var variation in manager.VariationList)
                {
                    var modifiedMoveList = ModifyMove(variation);

                    AddVariationNode(currentRoot, modifiedMoveList);
                }

                currentRoot = currentRoot.NextChild;
            }

            return root.NextChild;
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
