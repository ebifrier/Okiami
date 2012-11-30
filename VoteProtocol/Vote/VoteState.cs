using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace VoteSystem.Protocol.Vote
{
    using Ragnarok.Utility;

    /// <summary>
    /// 投票状態を判別します。
    /// </summary>
    [DataContract()]
    public enum VoteState
    {
        /// <summary>
        /// 投票停止状態です。
        /// </summary>
        [LabelDescription(Label = "投票停止")]
        Stop,

        /// <summary>
        /// 投票中です。
        /// </summary>
        [LabelDescription(Label = "投票中")]
        Voting,

        /// <summary>
        /// 投票一時停止中です。
        /// </summary>
        [LabelDescription(Label = "一時停止")]
        Pause,

        /// <summary>
        /// 投票期間が終了しました。
        /// </summary>
        [LabelDescription(Label = "投票終了")]
        End,
    }
}
