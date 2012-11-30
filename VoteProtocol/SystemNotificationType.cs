using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

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
        Unknown,
        /// <summary>
        /// 投票開始メッセージ
        /// </summary>
        VoteStart,
        /// <summary>
        /// 投票終了メッセージ
        /// </summary>
        VoteEnd,
        /// <summary>
        /// 投票一時停止メッセージ
        /// </summary>
        VotePause,
        /// <summary>
        /// 投票停止メッセージ
        /// </summary>
        VoteStop,
        /// <summary>
        /// 投票時間の変更メッセージ
        /// </summary>
        ChangeVoteSpan,
    }
}
