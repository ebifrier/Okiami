using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace VoteSystem.Protocol
{
    /// <summary>
    /// 放送情報とタイトルなどの情報を一緒に保持します。
    /// </summary>
    [DataContract()]
    public sealed class LiveInfoData2
    {
        /// <summary>
        /// 対象となる放送を取得または設定します。
        /// </summary>
        [DataMember(Order = 1, IsRequired = true)]
        public LiveData LiveData
        {
            get;
            private set;
        }

        /// <summary>
        /// 放送タイトルを取得します。
        /// </summary>
        [DataMember(Order = 2, IsRequired = true)]
        public string LiveTitle
        {
            get;
            private set;
        }

        /// <summary>
        /// 放送オーナーのIDを取得します。
        /// </summary>
        [DataMember(Order = 3, IsRequired = true)]
        public string LiveOwnerId
        {
            get;
            private set;
        }

        /// <summary>
        /// オブジェクトの各プロパティが正しく設定されているか調べます。
        /// </summary>
        public bool Validate()
        {
            if (!this.LiveData.Validate())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LiveInfoData2(LiveData liveData, string liveTitle, string ownerId)
        {
            LiveData = liveData;
            LiveTitle = liveTitle;
            LiveOwnerId = ownerId;
        }
    }
}
