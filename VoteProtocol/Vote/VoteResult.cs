using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace VoteSystem.Protocol.Vote
{
    /// <summary>
    /// 投票結果の候補とそれに対するポイントを保持します。
    /// </summary>
    [Serializable()]
    [DataContract()]
    public class VoteCandidatePair
    {
        /// <summary>
        /// 候補文字列を取得または設定します。
        /// </summary>
        [DataMember(Order = 1, IsRequired = true)]
        public string Candidate
        {
            get;
            set;
        }

        /// <summary>
        /// 候補持つポイントを取得または設定します。
        /// </summary>
        [DataMember(Order = 2, IsRequired = true)]
        public int Point
        {
            get;
            set;
        }

        /// <summary>
        /// オブジェクトの妥当性を確認します。
        /// </summary>
        public bool Validate()
        {
            if (string.IsNullOrEmpty(Candidate))
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// 投票結果を保持します。
    /// </summary>
    [Serializable()]
    [DataContract()]
    public class VoteResult
    {
        /// <summary>
        /// 候補文字列とポイントのペアを取得または設定します。
        /// </summary>
        [DataMember(Order = 1, IsRequired = true)]
        public VoteCandidatePair[] CandidateList
        {
            get;
            set;
        }

        /// <summary>
        /// 投票時間の延長に対して投票されたポイントを取得または設定します。
        /// </summary>
        [DataMember(Order = 2, IsRequired = true)]
        public int TimeExtendPoint
        {
            get;
            set;
        }

        /// <summary>
        /// 投票時間の延長拒否に対して投票されたポイントを取得または設定します。
        /// </summary>
        [DataMember(Order = 3, IsRequired = true)]
        public int TimeStablePoint
        {
            get;
            set;
        }

        /// <summary>
        /// ユーザーの評価値を取得または設定します。
        /// </summary>
        [DataMember(Order = 4, IsRequired = true)]
        public double EvaluationPoint
        {
            get;
            set;
        }

        /// <summary>
        /// オブジェクトの妥当性を確認します。
        /// </summary>
        public bool Validate()
        {
            if (CandidateList == null)
            {
                return false;
            }

            // null要素などは不正とみなします。
            if (CandidateList.Any(candidate =>
                candidate == null || !candidate.Validate()))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VoteResult()
        {
            this.CandidateList = new VoteCandidatePair[0];
        }
    }
}
