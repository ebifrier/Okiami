using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ragnarok;
using Ragnarok.ObjectModel;

namespace VoteSystem.PluginShogi.ViewModel
{
    using Protocol;
    using Protocol.Vote;

    public sealed class EndRollViewModel : DynamicViewModel
    {
        public VoterList VoterList
        {
            get;
            private set;
        }

        public List<VoterInfo> JoinedVoterViewList
        {
            get;
            private set;
        }

        public int JoinedVoterViewCount
        {
            get { return JoinedVoterViewList.Count; }
        }

        public int VoterOtherCount
        {
            get
            {
                return (
                    VoterList.JoinedVoterList.Count() -
                    JoinedVoterViewCount +
                    VoterList.UnjoinedVoterCount);
            }
        }

        public List<VoterInfo> LiveOwnerViewList
        {
            get;
            private set;
        }

        public int LiveOwnerViewCount
        {
            get { return LiveOwnerViewList.Count(); }
        }

        public int LiveOwnerOtherCount
        {
            get { return (VoterList.LiveOwnerCount - LiveOwnerViewCount); }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EndRollViewModel(VoterList voterList)
            : base(voterList)
        {
            VoterList = voterList;

            var list = VoterList.JoinedVoterList
                .OrderBy(_ => Guid.NewGuid())
                .ToList();
            JoinedVoterViewList = (list.Count() > 200 ?
                list.Take(200) :
                list.Concat(list).Take(200))
                .ToList();

            list = VoterList.LiveOwnerList
                .OrderBy(_ => Guid.NewGuid())
                .ToList();
            LiveOwnerViewList = (list.Count() > 20 ?
                list.Take(20) : list).ToList();
        }
    }
}
