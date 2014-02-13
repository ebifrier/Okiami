using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Ragnarok;
using Ragnarok.ObjectModel;

namespace VoteSystem.PluginShogi.ViewModel
{
    using Protocol;
    using Protocol.Vote;

    /// <summary>
    /// エンディング用のモデルオブジェクトです。
    /// </summary>
    public sealed class EndRollViewModel : DynamicViewModel
    {
        /// <summary>
        /// 自分の名前を取得します。
        /// </summary>
        public string NickName
        {
            get
            {
                var model = ShogiGlobal.ClientModel;
                if (model == null)
                {
                    return "名無し";
                }

                return model.NickName;
            }
        }

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

        /// <summary>
        /// 寄付者数を取得します。
        /// </summary>
        public int DonorViewCount
        {
            get { return VoterList.DonorViewList.Count(); }
        }

        /// <summary>
        /// その他の寄付者人数を取得します。
        /// </summary>
        public int DonorOtherCount
        {
            get { return (VoterList.DonorCount - DonorViewCount); }
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
        /// 参加者が参加した放送があれば１を返します。
        /// </summary>
        int HasLiveRoom(LiveData liveData)
        {
            if (liveData == null || !liveData.Validate())
            {
                return 0;
            }

            return (ShogiGlobal.ClientModel.HasLiveRoom(liveData) ? 1 : 0);
        }

        /// <summary>
        /// ニコニコの184IDかどうか調べます。
        /// </summary>
        int IsAnonymous(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return 1;
            }

            return (id.All(_ => char.IsNumber(_)) ? 0 : 1);
        }

        IEnumerable<VoterInfo> SortJoinedVoterList()
        {
            if (VoterList.JoinedVoterList == null)
            {
                return new VoterInfo[0];
            }

            return
                from voter in VoterList.JoinedVoterList
                where voter != null
                orderby HasLiveRoom(voter.LiveData) descending/*,
                        IsAnonymous(voter.Id) ascending*/
                select voter;
        }

        IEnumerable<VoterInfo> GetLiveOwnerList()
        {
            if (VoterList.LiveOwnerList == null)
            {
                return new VoterInfo[0];
            }

            return VoterList.LiveOwnerList;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EndRollViewModel(VoterList voterList)
            : base(voterList)
        {
            VoterList = voterList;

            // 参加者は一度指定条件でソートし、カットした後、
            // ランダムに並び替えます。
            JoinedVoterViewList = SortJoinedVoterList()
                .Take(400)
                .OrderBy(_ => Guid.NewGuid())
                .ToList();

            // 放送主はランダムに並び替えます。
            LiveOwnerViewList = GetLiveOwnerList()
                .OrderBy(_ => Guid.NewGuid())
                //.Take(10)
                .ToList();
        }
    }
}
