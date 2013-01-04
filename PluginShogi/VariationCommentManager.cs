using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ragnarok;
using Ragnarok.Shogi;

namespace VoteSystem.PluginShogi
{
    using Model;

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

        public bool IsHead
        {
            get { return (Id < 0 && NextId >= 0); }
        }

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
        /// 部分変化を処理します。
        /// </summary>
        private Variation AddPartialVariation(PartialVariation pv)
        {
            lock (this.pvList)
            {
                this.pvList.Add(pv);

                foreach(var head in this.pvList.Where(_ => _.IsHead))
                {
                    var result = new List<PartialVariation>();
                    result.Add(head);

                    var node = head;
                    while (true)
                    {
                        var next = FindPartialVariation(node.NextId);
                        if (next == null)
                        {
                            result = null;
                            break;
                        }

                        if (next.IsTail)
                        {
                            result.Add(next);
                            break;
                        }

                        result.Add(next);
                        node = next;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// 受信した変化を処理します。
        /// </summary>
        public Variation ProcessVariation(List<Move> moveList, string note,
                                          int id, int nextId)
        {
            if (moveList == null || !moveList.All(_ => _.Validate()))
            {
                return null;
            }

            if (id < 0 && nextId < 0)
            {
                // この変化のIDも、次変化のIDもない場合は、部分変化ではありません。
                var model = ShogiGlobal.ShogiModel;
                var variation = Variation.Create(
                    model.CurrentBoard, moveList,
                    note, true);
                if (variation == null ||
                    variation.MoveList.Count() <= Variation.ShortestMove)
                {
                    return null;
                }

                return variation;
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
