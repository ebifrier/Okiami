using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using Ragnarok;

namespace VoteSystem.Protocol
{
    /// <summary>
    /// 各放送とそのルームの情報を保持します。
    /// </summary>
    /// <remarks>
    /// 放送URLに追加して、各放送ごとに複数のルームがある場合があります。
    /// </remarks>
    [DataContract()]
    [Serializable()]
    public class LiveRoomData : IEquatable<LiveRoomData>
    {
        /// <summary>
        /// 放送URLなどを取得します。
        /// </summary>
        [DataMember(Order = 1, IsRequired = true)]
        public LiveData LiveData
        {
            get;
            private set;
        }

        /// <summary>
        /// 放送ごとのルームインデックスを取得します。
        /// </summary>
        [DataMember(Order = 2, IsRequired = true)]
        public int RoomIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// オブジェクトの各プロパティ値が正しいか調べます。
        /// </summary>
        public bool Validate()
        {
            if (this.LiveData == null || !this.LiveData.Validate())
            {
                return false;
            }

            if (this.RoomIndex < 0)
            {
                return false;
            }

            return true;
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

            return Equals(obj as LiveRoomData);
        }

        /// <summary>
        /// 等値性を判断します。
        /// </summary>
        public bool Equals(LiveRoomData other)
        {
            if ((object)other == null)
            {
                return false;
            }

            if (this.LiveData != other.LiveData)
            {
                return false;
            }

            if (this.RoomIndex != other.RoomIndex)
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
                this.LiveData.GetHashCode() ^
                this.RoomIndex.GetHashCode());
        }

        /// <summary>
        /// == 演算子を実装します。
        /// </summary>
        public static bool operator ==(LiveRoomData lhs, LiveRoomData rhs)
        {
            return Util.GenericEquals(lhs, rhs);
        }

        /// <summary>
        /// != 演算子を実装します。
        /// </summary>
        public static bool operator !=(LiveRoomData lhs, LiveRoomData rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LiveRoomData(LiveData live, int roomIndex)
        {
            this.LiveData = live;
            this.RoomIndex = roomIndex;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        [Obsolete("protobuf用のコンストラクタなので、使わないでください。")]
        protected LiveRoomData()
        {
        }
    }
}
