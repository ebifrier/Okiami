using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ragnarok.Utility;

namespace VoteSystem.Protocol.Vote
{
    /// <summary>
    /// 投票時間の延長/短縮時に使われます。
    /// </summary>
    public enum TimeExtendKind
    {
        /// <summary>
        /// 投票時間の延長を要求します。
        /// </summary>
        [LabelDescription(Label = "延長")]
        Extend,

        /// <summary>
        /// 投票時間の現状維持を要求します。
        /// </summary>
        [LabelDescription(Label = "そのまま")]
        Stable,
    }
}
