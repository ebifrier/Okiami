using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoteSystem.Server.VoteStrategy
{
    using VoteSystem.Protocol;
    using VoteSystem.Protocol.Vote;

#if false
    /// <summary>
    /// 
    /// </summary>
    internal sealed class KaosVoteStrategy : VoteStrategy
    {
        private VoteModel model;
        private Dictionary<string, DateTime> playerPool =
            new Dictionary<string, DateTime>();
        private Dictionary<string, int> votePool =
            new Dictionary<string, int>();

        /// <summary>
        /// 投票結果を取得します。
        /// </summary>
        protected override VoteResult GetVoteResult()
        {
            lock (this.votePool)
            {
                return new VoteResult()
                {
                    EvaluationPoint = EvaluationPoint,
                    TimeExtendPoint = TimeExtendPoint,
                    TimeStablePoint = TimeStablePoint,
                    CandidateList = this.votePool.Select(pair =>
                        new VoteCandidatePair()
                        {
                            Candidate = pair.Key,
                            Point = pair.Value,
                        }).ToArray(),
                };
            }
        }
        
        /// <summary>
        /// 投票結果をすべてクリアします。
        /// </summary>
        protected override void ClearVote()
        {
            lock (this.votePool)
            {
                this.votePool.Clear();
            }
        }

        /// <summary>
        /// 投票ルームにたいする各種操作を行います。
        /// </summary>
        protected override void ProcessNotification_(VoteParticipant sender,
                                                     Notification notification,
                                                     bool isFromVoteRoomOwner)
        {
            // 投票中じゃなければ何もしません。
            if (VoteModel.VoteState != VoteState.Voting)
            {
                return;
            }

            var now = DateTime.Now;

            // 最大２秒に１票までしか、投票できません。
            lock (this.playerPool)
            {
                DateTime time;

                if (this.playerPool.TryGetValue(notification.VoterId, out time))
                {
                    if (now < time + TimeSpan.FromSeconds(2))
                    {
                        return;
                    }
                }

                this.playerPool[notification.VoterId] = now;
            }

            // 投票用の文字列を作成します。
            var text = ModifyText(notification.Text);
            lock (this.votePool)
            {
                // ポイントを10点加算します。                
                int point;
                if (!this.votePool.TryGetValue(text, out point))
                {
                    point = 0;
                }

                this.votePool[text] = point + 10;
            }

            OnVoteResultChanged();

            // 票を受信したことを通知するメッセージを送ります。
            this.model.SendNotification(
                new Notification()
                {
                    Text = text,
                    Timestamp = now,
                    Type = NotificationType.Vote,
                    VoterId = notification.VoterId,
                    Color = notification.Color,
                    FromLiveRoom = notification.FromLiveRoom,
                });
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public KaosVoteStrategy(VoteModel model)
            : base(model)
        {
            this.model = model;
        }
    }
#endif
}
