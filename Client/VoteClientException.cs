using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ragnarok.Net.ProtoBuf;

namespace VoteSystem.Client
{
    /// <summary>
    /// バージョン不一致時の例外クラスです。
    /// </summary>
    public class VersionUnmatchedException : Exception
    {
        /// <summary>
        /// 不一致の結果を取得または設定します。
        /// </summary>
        public PbVersionCheckResult Result
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public VersionUnmatchedException(PbVersionCheckResult result,
                                         string message)
            : base(message)
        {
            Result = result;
        }
    }

    /// <summary>
    /// 投票クライアント用の例外です。
    /// </summary>
    public class VoteClientException : Exception
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public VoteClientException(string message)
            : base(message)
        {
        }
    }
}
