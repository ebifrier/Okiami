using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using Ragnarok;

namespace VoteSystem.Protocol.Vote
{
    /// <summary>
    /// 各サイトから、およびそうでない場合の投票者のIDを保持します。
    /// </summary>
    [Serializable()]
    [DataContract()]
    public class VoterId : IEquatable<VoterId>
    {
        /// <summary>
        /// 投票者の所属する配信サイトを取得または設定します。
        /// </summary>
        /// <remarks>
        /// <see cref="VoteSystem.Protocol.LiveSite.Unknown"/>の場合は、
        /// 生放送サイト以外の場所から投票に参加したことを意味します。
        /// (例えば、ツールから直接投票したなど)
        /// </remarks>
        [DataMember(Order = 1, IsRequired = true)]
        public LiveSite LiveSite
        {
            get;
            set;
        }

        /// <summary>
        /// 放送サイトごとのIDを取得または設定します。
        /// </summary>
        [DataMember(Order = 2, IsRequired = true)]
        public string Id
        {
            get;
            set;
        }

        /// <summary>
        /// オブジェクトの比較を行います。
        /// </summary>
        public override bool Equals(object obj)
        {
            var result = this.PreEquals(obj);
            if (result.HasValue)
            {
                return result.Value;
            }

            return Equals(obj as VoterId);
        }

        /// <summary>
        /// オブジェクトの比較を行います。
        /// </summary>
        public bool Equals(VoterId other)
        {
            if ((object)other == null)
            {
                return false;
            }

            return (
                LiveSite == other.LiveSite &&
                Id == other.Id);
        }

        /// <summary>
        /// ハッシュ値を取得します。
        /// </summary>
        public override int GetHashCode()
        {
            return (
                LiveSite.GetHashCode() ^
                Id.GetHashCode());
        }

        /// <summary>
        /// 文字列化します。
        /// </summary>
        public override string ToString()
        {
            return string.Format(
                "{0}:{1}",
                EnumEx.GetEnumLabel(LiveSite),
                Id);
        }

        /// <summary>
        /// オブジェクトの妥当性を検証します。
        /// </summary>
        public bool Validate()
        {
            if (string.IsNullOrEmpty(Id))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// オブジェクトの比較を行います。
        /// </summary>
        public static bool operator ==(VoterId lhs, VoterId rhs)
        {
            return Util.GenericEquals(lhs, rhs);
        }

        /// <summary>
        /// オブジェクトの!比較を行います。
        /// </summary>
        public static bool operator !=(VoterId lhs, VoterId rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VoterId()
        {
            LiveSite = LiveSite.Unknown;
            Id = "";
        }
    }
}
