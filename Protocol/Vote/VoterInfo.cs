using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using Ragnarok;

namespace VoteSystem.Protocol.Vote
{
    /// <summary>
    /// 各投票者の情報を保持します。
    /// </summary>
    /// <remarks>
    /// 投票者一覧の受け渡しに使います。
    /// </remarks>
    [DataContract()]
    [Serializable()]
    public class VoterInfo : IEquatable<VoterInfo>
    {
        private string name;

        /// <summary>
        /// 参加した放送サイトを取得または設定します。
        /// </summary>
        [DataMember(Order = 1, IsRequired = true)]
        public LiveSite LiveSite
        {
            get;
            set;
        }

        /// <summary>
        /// 識別IDを取得または設定します。
        /// </summary>
        [DataMember(Order = 2, IsRequired = true)]
        public string Id
        {
            get;
            set;
        }

        /// <summary>
        /// 名前を取得または設定します。
        /// </summary>
        [DataMember(Order = 3, IsRequired = true)]
        public string Name
        {
            get { return this.name; }
            set
            {
                // 名前は16文字までとします。
                if (value != null && value.Length > 16)
                {
                    this.name = value.Substring(0, 16);
                }
                else
                {
                    this.name = value;
                }
            }
        }

        /// <summary>
        /// 強さを示す文字列を取得または設定します。
        /// </summary>
        [DataMember(Order = 4, IsRequired = true)]
        public string Skill
        {
            get;
            set;
        }

        /// <summary>
        /// 表示色を取得または設定します。
        /// </summary>
        [DataMember(Order = 5, IsRequired = true)]
        public NotificationColor Color
        {
            get;
            set;
        }

        /// <summary>
        /// オブジェクトの比較を行います。
        /// </summary>
        public override bool Equals(object obj)
        {
            var other = obj as VoterInfo;

            return Equals(other);
        }

        /// <summary>
        /// オブジェクトの比較を行います。
        /// </summary>
        public bool Equals(VoterInfo other)
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
        /// オブジェクトの妥当性を検証します。
        /// </summary>
        public bool Validate()
        {
            /*if (LiveSite == LiveSite.Unknown)
            {
                return false;
            }*/

            if (string.IsNullOrEmpty(Id))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// オブジェクトの比較を行います。
        /// </summary>
        public static bool operator ==(VoterInfo lhs, VoterInfo rhs)
        {
            return Util.GenericClassEquals(lhs, rhs);
        }

        /// <summary>
        /// オブジェクトの!比較を行います。
        /// </summary>
        public static bool operator !=(VoterInfo lhs, VoterInfo rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VoterInfo()
        {
            LiveSite = LiveSite.Unknown;
            Id = null;
            Name = null;
            Skill = null;
            Color = NotificationColor.Default;
        }
    }
}
