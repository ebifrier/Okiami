using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoteSystem.Server
{
    using VoteSystem.Protocol.Vote;

    /// <summary>
    /// 投票への参加者情報を管理します。
    /// </summary>
    public sealed class VoterListManager
    {
        private readonly VoteRoom voteRoom;
        private readonly Dictionary<string, VoterInfo> joinedVoterDic =
            new Dictionary<string, VoterInfo>();
        private readonly Dictionary<string, VoterInfo> unjoinedVoterDic =
            new Dictionary<string, VoterInfo>();
        private readonly Dictionary<string, VoterInfo> liveOwnerDic =
            new Dictionary<string, VoterInfo>();
        private readonly HashSet<string> modeCustomjoinerSet =
            new HashSet<string>();

        /// <summary>
        /// 投票者リストを取得します。
        /// </summary>
        public VoterList VoterList
        {
            get
            {
                lock (this.joinedVoterDic)
                lock (this.unjoinedVoterDic)
                lock (this.liveOwnerDic)
                lock (this.modeCustomjoinerSet)
                {
                    return new VoterList()
                    {
                        JoinedVoterList = this.joinedVoterDic.Values.ToList(),
                        UnjoinedVoterCount = this.unjoinedVoterDic.Count,
                        LiveOwnerList = this.liveOwnerDic.Values.ToList(),
                        ModeCustomJoinerList = this.modeCustomjoinerSet.ToList(),
                         
                        TotalLiveCount = this.voteRoom.TotalLiveCount,
                        TotalLiveCommentCount = this.voteRoom.TotalLiveCommentCount,
                        TotalLiveVisitorCount = this.voteRoom.TotalLiveVisitorCount,

                        /*JoinedVoterList = test.JoinedVoterList,
                        UnjoinedVoterCount = test.UnjoinedVoterCount,
                        LiveOwnerList = test.LiveOwnerList,
                        ModeCustomJoinerList = test.ModeCustomJoinerList,

                        TotalLiveCount = 256,
                        TotalLiveCommentCount = 562347162,
                        TotalLiveVisitorCount = 4328171,*/

                        DonorCount = 15,
                        DonorAmount = 278919,
                        DonorViewList = DonorViewList.ToList(),
                    };
                }
            }
        }

        private List<string> DonorViewList
        {
            get
            {
                return new List<string>
                {
                    "izuminoshita",
                    "石渡良二",
                    "リスナーが負けたら船江君はあきらめます",
                    "エスペラード",
                    "ebifrier",
                    "kokujira",
                    "むずでょ",
                    "I.M.",
                    "ゆうまくん",
                    "つーてんかく",
                };
            }
        }

        /// <summary>
        /// 参加していない投票者をリストに追加します。
        /// </summary>
        public void AddUnjoinedVoter(VoterInfo voter)
        {
            if (voter == null || !voter.Validate())
            {
                return;
            }

            lock (this.unjoinedVoterDic)
            {
                this.unjoinedVoterDic[voter.Id] = voter;
            }
        }

        /// <summary>
        /// IDから未登録の投票者を取得します。
        /// </summary>
        public VoterInfo GetUnjoinedVoter(string voterId)
        {
            if (string.IsNullOrEmpty(voterId))
            {
                return null;
            }

            lock (this.unjoinedVoterDic)
            {
                VoterInfo voter = null;

                if (!this.unjoinedVoterDic.TryGetValue(voterId, out voter))
                {
                    return null;
                }

                return voter;
            }
        }

        /// <summary>
        /// 参加済みの投票者をリストに追加します。
        /// </summary>
        public void AddJoinedVoter(VoterInfo voter)
        {
            if (voter == null || !voter.Validate())
            {
                return;
            }

            // 放送主と参加者には同時登録できるようにします。
            /*lock (this.liveOwnerDic)
            {
                if (this.liveOwnerDic.Contains(voter))
                {
                    return;
                }
            }*/

            lock (this.joinedVoterDic)
            {
                this.joinedVoterDic[voter.Id] = voter;
            }
        }

        /// <summary>
        /// 登録された投票者をIDから検索します。
        /// </summary>
        public VoterInfo GetJoinedVoter(string voterId)
        {
            if (string.IsNullOrEmpty(voterId))
            {
                return null;
            }

            lock (this.joinedVoterDic)
            {
                VoterInfo voter = null;

                if (!this.joinedVoterDic.TryGetValue(voterId, out voter))
                {
                    return null;
                }

                return voter;
            }
        }

        /// <summary>
        /// 放送主をリストに追加します。
        /// </summary>
        public void AddLiveOwnerVoter(VoterInfo voter)
        {
            if (voter == null || !voter.Validate())
            {
                return;
            }

            // 放送主と参加者には同時登録できるようにします。
            /*lock (this.joinedVoterDic)
            {
                this.joinedVoterDic.Remove(voter);
            }*/

            lock (this.liveOwnerDic)
            {
                this.liveOwnerDic[voter.Id] = voter;
            }
        }

        /// <summary>
        /// 各モード固有の参加者をリストに追加します。
        /// </summary>
        public void AddModeCustomJoiner(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            lock (this.modeCustomjoinerSet)
            {
                this.modeCustomjoinerSet.Add(name);
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VoterListManager(VoteRoom voteRoom)
        {
            this.voteRoom = voteRoom;
        }
    }
}
