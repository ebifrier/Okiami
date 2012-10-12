using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using Ragnarok;

namespace VoteSystem.Client.Model
{
    /// <summary>
    /// アドレスとポートの組を保持します。
    /// </summary>
    public sealed class AddressPort : IEquatable<AddressPort>
    {
        /// <summary>
        /// アドレスを取得または設定します。
        /// </summary>
        public IPAddress Address
        {
            get;
            set;
        }

        /// <summary>
        /// ポート番号を取得または設定します。
        /// </summary>
        public int Port
        {
            get;
            set;
        }

        /// <summary>
        /// オブジェクトを比較します。
        /// </summary>
        public override bool Equals(object obj)
        {
            var other = obj as AddressPort;

            return Equals(other);
        }

        /// <summary>
        /// オブジェクトを比較します。
        /// </summary>
        public bool Equals(AddressPort other)
        {
            if ((object)other == null)
            {
                return false;
            }

            if (Address == null || !Address.Equals(other.Address))
            {
                return false;
            }

            if (Port != other.Port)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// オブジェクトを比較します。
        /// </summary>
        public static bool operator ==(AddressPort x, AddressPort y)
        {
            return Util.GenericClassEquals(x, y);
        }

        /// <summary>
        /// オブジェクトを比較します。
        /// </summary>
        public static bool operator !=(AddressPort x, AddressPort y)
        {
            return !(x == y);
        }

        /// <summary>
        /// ハッシュ値を計算します。
        /// </summary>
        public override int GetHashCode()
        {
            return (
                (Address != null ? Address.GetHashCode() : 0) ^
                Port.GetHashCode());
        }

        /// <summary>
        /// 文字列化します。
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}:{1}", Address, Port);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AddressPort(IPAddress address, int port)
        {
            Address = address;
            Port = port;
        }
    }
}
