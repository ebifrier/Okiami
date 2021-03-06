﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using Ragnarok;
using Ragnarok.Utility;

namespace VoteSystem.Protocol
{
    /// <summary>
    /// 放送サイトの種類を決定します。
    /// </summary>
    [DataContract()]
    public enum LiveSite
    {
        /// <summary>
        /// 未設定などに使います。
        /// </summary>
        [LabelDescription(Label = "不明")]
        Unknown,
        /// <summary>
        /// ニコ生
        /// </summary>
        [LabelDescription(Label = "ニコ生")]
        NicoNama,
        /// <summary>
        /// Ustream
        /// </summary>
        [LabelDescription(Label = "Ustream")]
        Ustream,
        /// <summary>
        /// Justin
        /// </summary>
        [LabelDescription(Label = "Justin")]
        Justin,
        /// <summary>
        /// PeerCast
        /// </summary>
        [LabelDescription(Label = "PeerCast")]
        PeerCast,
    }

    /// <summary>
    /// 各放送の情報を保持します。
    /// </summary>
    [DataContract()]
    public sealed class LiveData : IEquatable<LiveData>
    {
        /// <summary>
        /// 放送サイトの種類を取得します。
        /// </summary>
        [DataMember(Order = 1, IsRequired = true)]
        public LiveSite Site
        {
            get;
            private set;
        }

        /// <summary>
        /// 放送IDを取得します。
        /// </summary>
        [DataMember(Order = 2, IsRequired = true)]
        public string LiveIdString
        {
            get;
            private set;
        }

        /// <summary>
        /// 放送タイトルを取得します。
        /// </summary>
        [DataMember(Order = 3, IsRequired = true)]
        public string LiveTitle
        {
            get;
            private set;
        }

        /// <summary>
        /// 放送オーナーのIDを取得します。
        /// </summary>
        [DataMember(Order = 4, IsRequired = true)]
        public string LiveOwnerId
        {
            get;
            private set;
        }

        /// <summary>
        /// 放送サイト名を取得します。
        /// </summary>
        public string SiteName
        {
            get { return EnumEx.GetLabel(Site); }
        }

        /// <summary>
        /// オブジェクトの各プロパティが正しく設定されているか調べます。
        /// </summary>
        public bool Validate()
        {
            if (!Enum.IsDefined(typeof(LiveSite), this.Site))
            {
                return false;
            }

            if (string.IsNullOrEmpty(this.LiveIdString))
            {
                return false;
            }

            /*if (string.IsNullOrEmpty(this.LiveTitle))
            {
                return false;
            }*/

            return true;
        }

        /// <summary>
        /// 放送を視聴できるURLを取得します。
        /// </summary>
        public string Url
        {
            get
            {
                // 放送IDがなければURLは何も返しません。
                if (string.IsNullOrEmpty(this.LiveIdString))
                {
                    return null;
                }

                switch (this.Site)
                {
                    case LiveSite.NicoNama:
                        return string.Format(
                            "http://live.nicovideo.jp/watch/{0}",
                            LiveIdString);
                    case LiveSite.Ustream:
                        return string.Format(
                            "http://www.ustream.tv/{0}",
                            LiveIdString);
                    case LiveSite.Justin:
                        return string.Format(
                            "http://justin.tv/{0}",
                            LiveIdString);
                    case LiveSite.PeerCast:
                        return "";
                }

                return null;
            }
        }

        /// <summary>
        /// 放送の説明を取得します。
        /// </summary>
        public string Explain
        {
            get
            {
                var result = SiteName;

                if (!string.IsNullOrEmpty(LiveIdString))
                {
                    result += ": ";
                    result += LiveIdString;
                }

                if (!string.IsNullOrEmpty(LiveTitle))
                {
                    result += " ";
                    result += LiveTitle.Quote("\"");
                }

                return result;
            }
        }

        /// <summary>
        /// 文字列化します。
        /// </summary>
        public override string ToString()
        {
            var result = SiteName;

            if (!string.IsNullOrEmpty(LiveIdString))
            {
                result += ": ";
                result += LiveIdString;
            }

            return result;
        }

        /// <summary>
        /// 等値性を判断します。
        /// </summary>
        public override bool Equals(object obj)
        {
            var result = this.PreEquals(obj);
            if (result.HasValue)
            {
                return result.Value;
            }

            return Equals(obj as LiveData);
        }

        /// <summary>
        /// 等値性を判断します。
        /// </summary>
        public bool Equals(LiveData other)
        {
            if ((object)other == null)
            {
                return false;
            }

            if (this.Site != other.Site)
            {
                return false;
            }

            if (this.LiveIdString != other.LiveIdString)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// ハッシュコードを取得します。
        /// </summary>
        public override int GetHashCode()
        {
            return (
                this.Site.GetHashCode() ^
                this.LiveIdString.GetHashCode());
        }

        /// <summary>
        /// == 演算子を実装します。
        /// </summary>
        public static bool operator ==(LiveData lhs, LiveData rhs)
        {
            return Util.GenericEquals(lhs, rhs);
        }

        /// <summary>
        /// != 演算子を実装します。
        /// </summary>
        public static bool operator !=(LiveData lhs, LiveData rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LiveData(LiveSite kind, string liveId)
        {
            Site = kind;
            LiveIdString = liveId;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LiveData(LiveSite kind, string liveId, string liveTitle,
                        string ownerId)
        {
            Site = kind;
            LiveIdString = liveId;
            LiveTitle = liveTitle;
            LiveOwnerId = ownerId;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        [Obsolete("protobuf用のコンストラクタなので、使わないでください。")]
        private LiveData()
        {
        }
    }
}
