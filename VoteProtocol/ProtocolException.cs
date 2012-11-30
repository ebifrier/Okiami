using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoteSystem.Protocol
{
    /// <summary>
    /// 通信プロトコル関連のエラーを扱います。
    /// </summary>
    public class ProtocolException : Exception
    {
        /// <summary>
        /// エラーコードを取得します。
        /// </summary>
        public int ErrorCode
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ProtocolException(int error)
            : base(ErrorCodeUtil.GetDescription(error))
        {
            ErrorCode = error;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ProtocolException(int error, Exception innerException)
            : base(ErrorCodeUtil.GetDescription(error), innerException)
        {
            ErrorCode = error;
        }
    }
}
