using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ragnarok;
using Ragnarok.Shogi;

namespace VoteSystem.PluginShogi.Model
{
    /// <summary>
    /// 投稿された変化の一部分を保持します。
    /// </summary>
    internal sealed class PartialVariation
    {
        private readonly List<Move> moveList;
        private readonly string note;
        private readonly int id;
        private readonly int nextId;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PartialVariation(List<Move> moveList, string note,
                                int id, int nextId)
        {
            this.moveList = moveList;
            this.note = note;
            this.id = id;
            this.nextId = nextId;
        }

        /// <summary>
        /// 部分的な変化を取得します。
        /// </summary>
        public List<Move> MoveList
        {
            get { return this.moveList; }
        }

        /// <summary>
        /// コメントのようなものを取得します。
        /// </summary>
        public string Note
        {
            get { return this.note; }
        }

        /// <summary>
        /// この部分変化の識別用IDを取得します。
        /// </summary>
        public int Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// 次の部分変化の識別用IDを取得します。
        /// </summary>
        public int NextId
        {
            get { return this.nextId; }
        }

        /// <summary>
        /// 最初の部分変化かどうかを取得します。
        /// </summary>
        public bool IsHead
        {
            get { return (Id < 0 && NextId >= 0); }
        }

        /// <summary>
        /// 最後の部分変化かどうかを取得します。
        /// </summary>
        public bool IsTail
        {
            get { return (NextId < 0); }
        }
    }

    /// <summary>
    /// 部分変化を管理し、必要なら一つの変化にまとめ上げます。
    /// </summary>
    /// <remarks>
    /// 文字数の関係で、すべての変化が一度に投稿されるとは限りません。
    /// その変化は分割されて管理されます。
    /// </remarks>
    internal sealed class VariationCommentManager
    {
        private readonly List<PartialVariation> pvList =
            new List<PartialVariation>();

        /// <summary>
        /// バッファにある全部分変化をクリアします。
        /// </summary>
        public void Clear()
        {
            lock (this.pvList)
            {
                this.pvList.Clear();
            }
        }

        /// <summary>
        /// 指定のIDを持つ部分変化を検索します。
        /// </summary>
        private PartialVariation FindPartialVariation(int id)
        {
            lock (this.pvList)
            {
                return this.pvList.FirstOrDefault(_ => _.Id == id);
            }
        }

        /// <summary>
        /// 部分変化から変化を作成します。
        /// </summary>
        private List<PartialVariation> CreateVariationFromPartial(PartialVariation head)
        {
            lock (this.pvList)
            {
                var result = new List<PartialVariation>();
                result.Add(head);

                PartialVariation next = null;
                for (var node = head; !node.IsTail; node = next)
                {
                    next = FindPartialVariation(node.NextId);
                    if (next == null)
                    {
                        return null;
                    }

                    result.Add(next);
                }

                // 変化完成
                return result;
            }
        }

        /// <summary>
        /// 部分変化を処理します。
        /// </summary>
        private Variation AddPartialVariation(PartialVariation pv)
        {
            lock (this.pvList)
            {
                // 最初に要素を追加します。
                this.pvList.Add(pv);

                foreach(var head in this.pvList.Where(_ => _.IsHead))
                {
                    var partialList = CreateVariationFromPartial(head);
                    if (partialList != null)
                    {
                        var variation = CreateVariation(
                            partialList.SelectMany(_ => _.MoveList),
                            partialList.Last().Note);

                        // リストから取り出された部分変化を削除します。
                        partialList.ForEach(_ => this.pvList.Remove(_));
                        return variation;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// 指し手から変化オブジェクトを作成します。
        /// </summary>
        private Variation CreateVariation(IEnumerable<Move> moveList, string note)
        {
            var model = ShogiGlobal.ShogiModel;
            var variation = Variation.Create(
                model.CurrentBoard,
                moveList,
                note, true);

            if (variation == null ||
                variation.MoveList.Count() <= Variation.ShortestMove)
            {
                return null;
            }

            return variation;
        }

        /// <summary>
        /// 受信した変化を処理します。
        /// </summary>
        public Variation ProcessMoveList(List<Move> moveList, string note,
                                         int id, int nextId)
        {
            if (moveList == null || !moveList.All(_ => _.Validate()))
            {
                return null;
            }

            if (id >= 0 && id == nextId)
            {
                return null;
            }

            if (id < 0 && nextId < 0)
            {
                // この変化のIDも、次変化のIDもない場合は、部分変化ではありません。
                return CreateVariation(moveList, note);
            }
            else
            {
                // 部分変化の一部です。
                return AddPartialVariation(
                    new PartialVariation(moveList, note, id, nextId));
            }
        }
    }
}
