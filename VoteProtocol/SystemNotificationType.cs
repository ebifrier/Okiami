using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using Ragnarok.Utility;

namespace VoteSystem.Protocol
{
    /// <summary>
    /// システムメッセージの種類です。
    /// </summary>
    [DataContract()]
    [Serializable()]
    public enum SystemNotificationType
    {
        /// <summary>
        /// 不明。指定しない場合に使います。
        /// </summary>
        [LabelDescription(Label = "不明")]
        Unknown,
        /// <summary>
        /// 投票開始メッセージ
        /// </summary>
        [LabelDescription(Label = "投票開始")]
        VoteStart,
        /// <summary>
        /// 投票終了メッセージ
        /// </summary>
        [LabelDescription(Label = "投票終了")]
        VoteEnd,
        /// <summary>
        /// 投票一時停止メッセージ
        /// </summary>
        [LabelDescription(Label = "投票一時停止")]
        VotePause,
        /// <summary>
        /// 投票停止メッセージ
        /// </summary>
        [LabelDescription(Label = "投票停止")]
        VoteStop,
        /// <summary>
        /// 投票時間の変更メッセージ
        /// </summary>
        [LabelDescription(Label = "投票時間変更")]
        ChangeVoteSpan,
    }
}
