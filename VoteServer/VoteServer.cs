using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Ragnarok;
using Ragnarok.Net;
using Ragnarok.Net.ProtoBuf;

namespace VoteSystem.Server
{
    using Protocol;

    /// <summary>
    /// 投票システム用のロビーサーバーです。
    /// </summary>
    public class VoteServer : ILogObject
    {
        private Socket acceptSocket;

        /// <summary>
        /// ログ出力用の名前を取得します。
        /// </summary>
        public string LogName
        {
            get { return "投票サーバー"; }
        }

        /// <summary>
        /// アクセプトソケットを初期化します。
        /// </summary>
        private void InitAcceptSocket(IPAddress address, int port)
        {
            var socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            // ソケットを初期化します。
            var endpoint = new IPEndPoint(address, port);
            socket.Bind(endpoint);
            socket.Listen(100);

            // ソケットアドレスの再使用を可能にします。
            socket.SetSocketOption(
                SocketOptionLevel.Socket,
                SocketOptionName.ReuseAddress,
                true);

            if (this.acceptSocket != null)
            {
                this.acceptSocket.Close();
                this.acceptSocket = null;
            }

            this.acceptSocket = socket;
        }

        /// <summary>
        /// ソケットをアクセプトするためのループを実行します。
        /// </summary>
        public void AcceptLoop()
        {
            InitAcceptSocket(
                IPAddress.Any,
                ServerSettings.VotePort);

            Log.Info(this,
                "受信処理を開始しました。({0})",
                ServerSettings.VotePort);

            while (true)
            {
                try
                {
                    var client = this.acceptSocket.Accept();
                    if (client == null)
                    {
                        continue;
                    }

                    // このオブジェクトはすぐに破棄されるように見えますが、
                    // コンストラクタでソケットの非同期通信を設定する関係で
                    // すぐには削除されません。
                    new VoteParticipant(client);

                    Log.Info(this,
                        "コネクションを正しく受信しました。");
                }
                catch (Exception ex)
                {
                    Log.ErrorException(this, ex,
                        "コネクションの受信に失敗しました。");
                }
            }
        }
    }
}
