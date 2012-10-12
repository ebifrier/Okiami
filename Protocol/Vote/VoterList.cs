using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace VoteSystem.Protocol.Vote
{
    /// <summary>
    /// 投票者の一覧を取得します。
    /// </summary>
    [DataContract()]
    [Serializable()]
    public class VoterList
    {
        /// <summary>
        /// "参加"した投票者一覧を取得または設定します。
        /// </summary>
        [DataMember(Order = 1, IsRequired = true)]
        public List<VoterInfo> JoinedVoterList
        {
            get;
            set;
        }

        /// <summary>
        /// "参加"していない投票者の人数を取得または設定します。
        /// </summary>
        [DataMember(Order = 2, IsRequired = true)]
        public int UnjoinedVoterCount
        {
            get;
            set;
        }

        /// <summary>
        /// 放送主一覧を取得または設定します。
        /// </summary>
        [DataMember(Order = 3, IsRequired = true)]
        public List<VoterInfo> LiveOwnerList
        {
            get;
            set;
        }

        /// <summary>
        /// デシリアライズ後に呼ばれます。
        /// </summary>
        [OnDeserialized()]
        private void OnDeserialized(StreamingContext context)
        {
            if (JoinedVoterList == null)
            {
                JoinedVoterList = new List<VoterInfo>();
            }

            if (LiveOwnerList == null)
            {
                LiveOwnerList = new List<VoterInfo>();
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VoterList()
        {
            this.JoinedVoterList = new List<VoterInfo>();
            this.LiveOwnerList = new List<VoterInfo>();
        }
    }
}
