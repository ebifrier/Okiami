using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using Ragnarok;

namespace VoteSystem.Protocol
{
    /// <summary>
    /// 通知されるメッセージのタイプです。
    /// </summary>
    [DataContract()]
    public enum NotificationType
    {
        /// <summary>
        /// 不明。主にクライアントからの通知で使われます。
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// 重要メッセージ(投票ルームのオーナーしか送れません)
        /// </summary>
        Important = 1,
        /// <summary>
        /// 投票開始/終了などのシステムメッセージ
        /// </summary>
        System = 2,
        /// <summary>
        /// 通常メッセージ
        /// </summary>
        Message = 3,
        /// <summary>
        /// 投票メッセージ
        /// </summary>
        Vote = 4,
        /// <summary>
        /// 参加者登録メッセージ
        /// </summary>
        Join = 5,
        /// <summary>
        /// 時間延長要求メッセージ
        /// </summary>
        TimeExtend = 6,
        /// <summary>
        /// 評価値メッセージ
        /// </summary>
        Evaluation = 7,
    }

    /// <summary>
    /// 通知されるメッセージのタイプのマスクです。
    /// </summary>
    [Flags()]
    [DataContract()]
    public enum NotificationTypeMask
    {
        /// <summary>
        /// 無し
        /// </summary>
        None = 0,
        /// <summary>
        /// 不明。主にクライアントからの通知で使われます。
        /// </summary>
        Unknown = (1 << NotificationType.Unknown),
        /// <summary>
        /// 重要メッセージ(投票ルームのオーナーしか送れません)
        /// </summary>
        Important = (1 << NotificationType.Important),
        /// <summary>
        /// システムメッセージ
        /// </summary>
        System = (1 << NotificationType.System),
        /// <summary>
        /// 通常メッセージ
        /// </summary>
        Message = (1 << NotificationType.Message),
        /// <summary>
        /// 投票メッセージ
        /// </summary>
        Vote = (1 << NotificationType.Vote),
        /// <summary>
        /// 参加者登録メッセージ
        /// </summary>
        Join = (1 << NotificationType.Join),
        /// <summary>
        /// 延長メッセージ
        /// </summary>
        TimeExtend = (1 << NotificationType.TimeExtend),
        /// <summary>
        /// 評価値メッセージ
        /// </summary>
        Evaluation = (1 << NotificationType.Evaluation),

        /// <summary>
        /// 全メッセージ
        /// </summary>
        All = (SystemAll | CommentAll),
        /// <summary>
        /// 全システム関連メッセージ
        /// </summary>
        SystemAll = (Important | System),
        /// <summary>
        /// システム以外の全メッセージ
        /// </summary>
        CommentAll = (Message | Vote | Join | TimeExtend | Evaluation),
        /// <summary>
        /// 重要と思われる全メッセージ(システム除く)
        /// </summary>
        CommentCore = (Message | Vote | Join | TimeExtend),
    }
}
