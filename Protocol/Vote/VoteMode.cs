using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace VoteSystem.Protocol.Vote
{
    using Ragnarok.Utility;

    /// <summary>
    /// 投票モードです。
    /// </summary>
    [DataContract()]
    public enum VoteMode
    {
        /// <summary>
        /// 投稿コメントのミラーリングのみを行います。
        /// </summary>
        [LabelDescription(Label = "ミラー")]
        Mirror,

        /*/// <summary>
        /// 投稿コメントの内容による投票モードです。
        /// </summary>
        [LabelDescription(Label = "カオス")]
        Kaos,*/

        /// <summary>
        /// 将棋の投票モードです。
        /// </summary>
        [LabelDescription(Label = "将棋")]
        Shogi,
    }
}
