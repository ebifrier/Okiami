using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using Ragnarok.Net.ProtoBuf;

namespace VoteSystem.Protocol
{
    /// <summary>
    /// サーバーの基本情報を保持します。
    /// </summary>
    public static class ServerSettings
    {
        /// <summary>
        /// 通信プロトコルのバージョンです。
        /// </summary>
        public static readonly PbProtocolVersion ProtocolVersion =
            new PbProtocolVersion(2, 7, 0);

        /// <summary>
        /// 投票サーバーのアドレスです。
        /// </summary>
        public static readonly string VoteAddress =
            "garnet-alice.net";
            //"localhost";

        /// <summary>
        /// コメンターサーバーのアドレスです。
        /// </summary>
        public static readonly string CommenterAddress =
            "garnet-alice.net";
            //"localhost";

        /// <summary>
        /// 投票サーバーのポート番号です。
        /// </summary>
        public static readonly int VotePort = 38780;

        /// <summary>
        /// コメンターサーバーのポート番号です。
        /// </summary>
        public static readonly int CommenterPort = 38790;
    }
}
