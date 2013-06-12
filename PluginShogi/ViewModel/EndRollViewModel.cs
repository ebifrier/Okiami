﻿using System;
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
        /// <summary>
        /// 参加者リストを取得します。
        /// </summary>
        public VoterList VoterList
        {
            get;
            private set;
        }

        /// <summary>
        /// 表示用の参加者リストを取得します。
        /// </summary>
        public List<VoterInfo> JoinedVoterViewList
        {
            get;
            private set;
        }

        /// <summary>
        /// 表示用の参加者人数を取得します。
        /// </summary>
        public int JoinedVoterViewCount
        {
            get { return JoinedVoterViewList.Count; }
        }

        /// <summary>
        /// その他の参加者人数を取得します。
        /// </summary>
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

        /// <summary>
        /// 表示用の生主一覧を取得します。
        /// </summary>
        public List<VoterInfo> LiveOwnerViewList
        {
            get;
            private set;
        }

        /// <summary>
        /// 表示用の生主人数を取得します。
        /// </summary>
        public int LiveOwnerViewCount
        {
            get { return LiveOwnerViewList.Count(); }
        }

        /// <summary>
        /// その他の生主人数を取得します。
        /// </summary>
        public int LiveOwnerOtherCount
        {
            get { return (VoterList.LiveOwnerCount - LiveOwnerViewCount); }
        }

        private static string[] UnitTable = new string[]
        {
            "万", "億", "兆", "京",
        };

        /// <summary>
        /// 億、万などの単位を追加した文字列を作成します。
        /// </summary>
        /// <example>
        /// 12345 → 1万2345
        /// 123456789 → 1億2345万6789
        /// </example>
        public static string AddUnit(int n)
        {
            var result = new StringBuilder();
            var i = 0;

            while (n > 10000)
            {
                var v = n % 10000;
                n /= 10000;

                result.Insert(0, string.Format("{0:0000}", v));
                result.Insert(0, UnitTable[i++]);
            }

            result.Insert(0, n);
            return result.ToString();
        }

        /// <summary>
        /// 単位を含めた放送数合計を取得します。
        /// </summary>
        public string TotalLiveCountText
        {
            get { return AddUnit(VoterList.TotalLiveCount); }
        }

        /// <summary>
        /// 単位を含めた来場者数合計を取得します。
        /// </summary>
        public string TotalLiveVisitorCountText
        {
            get { return AddUnit(VoterList.TotalLiveVisitorCount); }
        }

        /// <summary>
        /// 単位を含めたコメント数合計を取得します。
        /// </summary>
        public string TotalLiveCommentCountText
        {
            get { return AddUnit(VoterList.TotalLiveCommentCount); }
        }

        /// <summary>
        /// 単位を含めた寄付金額合計を取得します。
        /// </summary>
        public string DonorAmountText
        {
            get { return AddUnit(VoterList.DonorAmount); }
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
            JoinedVoterViewList = (list.Count() > 210 ?
                list.Take(210) :
                list.Concat(list).Take(210))
                .ToList();

            list = VoterList.LiveOwnerList
                .OrderBy(_ => Guid.NewGuid())
                .ToList();
            LiveOwnerViewList = (list.Count() > 20 ?
                list.Take(20) : list).ToList();
        }
    }
}