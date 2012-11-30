using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Ragnarok;
using Ragnarok.Utility;
using Ragnarok.Net.ProtoBuf;

namespace VoteSystem.Server.VoteStrategy
{
    using VoteSystem.Protocol;
    using VoteSystem.Protocol.Vote;

    /// <summary>
    /// 実際に投票を行うときに使うクラスの基底インターフェースです。
    /// </summary>
    public interface IVoteStrategy : ILogObject
    {
        /// <summary>
        /// 投票結果を取得します。
        /// </summary>
        VoteCandidatePair[] GetVoteResult();

        /// <summary>
        /// 各通知のハンドラを接続します。
        /// </summary>
        void ConnectHandlers(PbConnection connection);

        /// <summary>
        /// 各通知のハンドラを切り離します。
        /// </summary>
        void DisconnectHandlers(PbConnection connection);

        /// <summary>
        /// 投票ルームにたいする各種操作を処理します。
        /// </summary>
        void ProcessNotification(Notification notification,
                                 bool isFromVoteRoomOwner);

        /// <summary>
        /// 投票ルームにたいする投票を処理します。
        /// </summary>
        /// <remarks>
        /// 投票中でないとこのメソッドは呼ばれません。
        /// </remarks>
        void ProcessVoteNotification(Notification notification,
                                     bool isFromVoteRoomOwner);

        /// <summary>
        /// 投票結果をクリアします。
        /// </summary>
        void ClearVote();

        /// <summary>
        /// シグナル受信時に呼ばれます。
        /// </summary>
        void SignalReceived(int signum);
    }
}
