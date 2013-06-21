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
    public sealed class VoterList
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
        /// 全投票者の数を取得します。
        /// </summary>
        public int VoterCount
        {
            get { return (JoinedVoterList.Count() + UnjoinedVoterCount); }
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
        /// 放送主数を取得します。
        /// </summary>
        public int LiveOwnerCount
        {
            get { return LiveOwnerList.Count(); }
        }

        /// <summary>
        /// モード固有の参加者一覧を取得または設定します。
        /// </summary>
        [DataMember(Order = 4, IsRequired = true)]
        public List<string> ModeCustomJoinerList
        {
            get;
            set;
        }

        /// <summary>
        /// モード固有の参加者数を取得します。
        /// </summary>
        public int ModeCustomJoinerCount
        {
            get { return ModeCustomJoinerList.Count(); }
        }

        /// <summary>
        /// EDに載せる寄付者一覧を取得または設定します。
        /// </summary>
        [DataMember(Order = 5, IsRequired = true)]
        public List<string> DonorViewList
        {
            get;
            set;
        }

        /// <summary>
        /// 放送数合計を取得または設定します。
        /// </summary>
        [DataMember(Order = 10, IsRequired = true)]
        public int TotalLiveCount
        {
            get;
            set;
        }

        /// <summary>
        /// 来場者数合計を取得または設定します。
        /// </summary>
        [DataMember(Order = 11, IsRequired = true)]
        public int TotalLiveVisitorCount
        {
            get;
            set;
        }

        /// <summary>
        /// コメント数合計を取得または設定します。
        /// </summary>
        [DataMember(Order = 12, IsRequired = true)]
        public int TotalLiveCommentCount
        {
            get;
            set;
        }

        /// <summary>
        /// 寄付金額合計を取得または設定します。
        /// </summary>
        [DataMember(Order = 13, IsRequired = true)]
        public int DonorAmount
        {
            get;
            set;
        }

        /// <summary>
        /// 寄付者総数を取得または設定します。
        /// </summary>
        [DataMember(Order = 14, IsRequired = true)]
        public int DonorCount
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

            if (ModeCustomJoinerList == null)
            {
                ModeCustomJoinerList = new List<string>();
            }

            if (DonorViewList == null)
            {
                DonorViewList = new List<string>();
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VoterList()
        {
            JoinedVoterList = new List<VoterInfo>();
            LiveOwnerList = new List<VoterInfo>();
            ModeCustomJoinerList = new List<string>();
            DonorViewList = new List<string>();
        }
    }
}
