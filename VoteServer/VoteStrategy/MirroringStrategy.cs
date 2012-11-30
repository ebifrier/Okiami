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
    /// コメントのミラーリングのみを行います。
    /// </summary>
    internal sealed class MirroringStrategy : IVoteStrategy
    {
        private readonly VoteRoom voteRoom;

        /// <summary>
        /// ログ表示名を取得します。
        /// </summary>
        public string LogName
        {
            get
            {
                return "ミラーモード";
            }
        }

        /// <summary>
        /// 投票結果を取得します。
        /// </summary>
        public VoteCandidatePair[] GetVoteResult()
        {
            return new VoteCandidatePair[0];
        }
        
        /// <summary>
        /// 投票結果をすべてクリアします。
        /// </summary>
        public void ClearVote()
        {
            // 何もしません。
        }

        /// <summary>
        /// シグナル受信時に呼ばれます。
        /// </summary>
        public void SignalReceived(int signum)
        {
            // 何もしません。
        }

        /// <summary>
        /// 各通知のハンドラを接続します。
        /// </summary>
        public void ConnectHandlers(PbConnection connection)
        {
            // 何もしません。
        }

        /// <summary>
        /// 各通知のハンドラを切り離します。
        /// </summary>
        public void DisconnectHandlers(PbConnection connection)
        {
            // 何もしません。
        }

        /// <summary>
        /// 投票ルームにたいする各種操作を処理します。
        /// </summary>
        public void ProcessNotification(Notification notification,
                                        bool isFromVoteRoomOwner)
        {
            // 何もしません。
        }

        /// <summary>
        /// 票通知を処理します。投票時にしか呼ばれません。
        /// </summary>
        public void ProcessVoteNotification(Notification notification,
                                            bool isFromVoteRoomOwner)
        {
            // ロックしていないので、フィールドを一度コピーしてから
            // ヌルポチェックをしています。
            var voteRoom = this.voteRoom;
            if (voteRoom != null)
            {
                voteRoom.BroadcastNotification(
                    NotificationType.Message, notification, false, true);
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MirroringStrategy(VoteRoom voteRoom)
        {
            this.voteRoom = voteRoom;
        }
    }
}
