using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

using Ragnarok;
using Ragnarok.NicoNico;

namespace VoteSystem.Client.Model
{
    /// <summary>
    /// どの放送をブラウザで視聴中か判断するため、
    /// 各TCPの接続数をカウントするクラスです。
    /// </summary>
    /// <remarks>
    /// 放送をブラウザ視聴しているか判断する条件は
    /// (1) 当該放送のheartbeatが取れること。
    /// (2) 指定放送のコメントサーバーへの接続があること。
    /// です。
    /// 
    /// (2)の条件を確認するため、定期的にnetstatのコネクションを監視し、
    /// コメントサーバーへの接続があるか見ています。
    /// また、アプリ内からのコメントサーバーへの接続状況を無視するため、
    /// その接続状況も監視しています。
    /// </remarks>
    public sealed class ConnectionCounter
    {
        private static readonly IPAddress[] IPList = new IPAddress[4];
        private readonly object SyncRoot = new object();
        private Dictionary<AddressPort, int> netstatCountDic =
            new Dictionary<AddressPort, int>();
        private readonly Dictionary<AddressPort, int> commenterCountDic =
            new Dictionary<AddressPort, int>();

        /// <summary>
        /// 静的コンストラクタ
        /// </summary>
        static ConnectionCounter()
        {
            try
            {
                for (var n = 101; n <= 104; ++n)
                {
                    var address = NicoString.GetMessageServerAddress(n);
                    var ips = Dns.GetHostAddresses(address);

                    foreach (var ip in ips)
                    {
                        if (ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal ||
                            ip.IsIPv6Teredo)
                        {
                            continue;
                        }

                        // 番号nのアドレスを設定します。
                        IPList[n - 101] = ip;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // ネット接続ができないときなどはここに来ます。
                Log.ErrorException(ex,
                    "ConnectionCounterの初期化に失敗しました。");
            }
        }

        /// <summary>
        /// メッセージサーバーのドメイン名をＩＰアドレスに変換します。
        /// </summary>
        private IPAddress GetIPFromMessageAddress(string msgAddress)
        {
            var n = NicoString.GetMessageServerNumber(msgAddress);
            if (n < 101 || 104 < n)
            {
                // 不正なアドレスです。
                return null;
            }

            return IPList[n - 101];
        }

        /// <summary>
        /// 指定のアドレス＆ポートの外部アプリによる接続数を取得します。
        /// </summary>
        public int GetCount(string msgAddress, int port)
        {
            return GetCount(GetIPFromMessageAddress(msgAddress), port);
        }

        /// <summary>
        /// 指定のアドレス＆ポートの外部アプリによる接続数を取得します。
        /// </summary>
        public int GetCount(IPAddress address, int port)
        {
            if (address == null)
            {
                return -1;
            }

            // netstatのコメントサーバーへの接続数から、
            // アプリ内のコメントサーバーへの接続数を引いたものを
            // 外部アプリによる接続数としています。
            var addrPort = new AddressPort(address, port);
            var netstatCount = GetNetstatCount(addrPort);
            var commenterCount = GetCommenterCount(addrPort);

            return Math.Max(netstatCount - commenterCount, 0);
        }

        /// <summary>
        /// 各アドレス＆ポートの外部アプリによる接続数を更新します。
        /// </summary>
        public void Update()
        {
            var countDic = CreateNetstatConnCountDic();

            lock (SyncRoot)
            {
                this.netstatCountDic = countDic;
            }
        }

        #region netstat
        private const string IpPortRegexStr =
            @"(\d+[.]\d+[.]\d+[.]\d+):(\d+)";
        private static readonly Regex LineRegex = new Regex(
            string.Format(
                @"^\s*TCP\s*{0}\s*{0}\s*ESTABLISHED",
                IpPortRegexStr),
            RegexOptions.IgnoreCase);

        /// <summary>
        /// 各コメンターの接続数を取得します。
        /// </summary>
        private int GetNetstatCount(AddressPort addrPort)
        {
            if (addrPort == null)
            {
                return -1;
            }

            lock (SyncRoot)
            {
                var count = 0;
                if (!this.netstatCountDic.TryGetValue(addrPort, out count))
                {
                    return 0;
                }

                return count;
            }
        }

        /// <summary>
        /// netstatによる各コネクションの接続数を取得します。
        /// </summary>
        private Dictionary<AddressPort, int> CreateNetstatConnCountDic()
        {
            using (var process = DoNetstat())
            {
                if (process == null)
                {
                    return null;
                }

                // netstatの各行から
                // 各IPとポート番号の接続一覧を作成します。
                var dic = new Dictionary<AddressPort, int>();
                var reader = process.StandardOutput;
                var line = null as string;
                while ((line = reader.ReadLine()) != null)
                {
                    var m = LineRegex.Match(line);
                    if (!m.Success)
                    {
                        continue;
                    }

                    var ip = IPAddress.Parse(m.Groups[3].Value);
                    var port = int.Parse(m.Groups[4].Value);
                    var obj = new AddressPort(ip, port);
                    var count = 0;

                    // 指定のアドレスとポートの接続数を一つ増やします。
                    if (dic.TryGetValue(obj, out count))
                    {
                        dic[obj] = count + 1;
                    }
                    else
                    {
                        dic[obj] = 1;
                    }
                }

                return dic;
            }
        }

        /// <summary>
        /// netstatを実行します。
        /// </summary>
        private Process DoNetstat()
        {
            try
            {
                var startInfo = new ProcessStartInfo("netstat", "-n")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                };

                var process = new Process()
                {
                    StartInfo = startInfo,
                };

                process.Start();

                // プロセスの書き込みバッファがいっぱいになると、
                // そのプロセスはバッファが読み込まれるまでウェイト状態になります。
                // 呼び出し側もプロセスの実行が終わるまで待つようにすると、
                // Deadlockするため、プロセスのウェイトはしないようにします。
                // 参考）
                // http://msdn.microsoft.com/en-us/library/system.diagnostics.process.standardoutput.aspx
                //
                // process.WaitForExit();
                return process;
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "netstatの実行に失敗しました。");

                return null;
            }
        }
        #endregion

        #region コメンターの接続
        /// <summary>
        /// 各コメンターの接続数を取得します。
        /// </summary>
        private int GetCommenterCount(AddressPort addrPort)
        {
            if (addrPort == null)
            {
                return -1;
            }

            lock (this.commenterCountDic)
            {
                var count = 0;
                if (!this.commenterCountDic.TryGetValue(addrPort, out count))
                {
                    return 0;
                }

                return count;
            }
        }

        /// <summary>
        /// ニコ生メッセージにアプリ内から接続されたとき呼ばれます。
        /// </summary>
        public void Connected(string msgAddress, int port)
        {
            Connected(GetIPFromMessageAddress(msgAddress), port);
        }

        /// <summary>
        /// 指定のアドレス＆ポートの接続数を一つ増やします。
        /// </summary>
        public void Connected(IPAddress address, int port)
        {
            if (address == null)
            {
                return;
            }

            lock (this.commenterCountDic)
            {
                // 指定のアドレス＆ポートの組の接続数を一つ増やします。
                var obj = new AddressPort(address, port);
                var count = 0;

                if (this.commenterCountDic.TryGetValue(obj, out count))
                {
                    this.commenterCountDic[obj] = count + 1;
                }
                else
                {
                    this.commenterCountDic[obj] = 1;
                }
            }
        }

        /// <summary>
        /// ニコ生メッセージにアプリ内から切断されたとき呼ばれます。
        /// </summary>
        public void Disconnected(string msgAddress, int port)
        {
            Disconnected(GetIPFromMessageAddress(msgAddress), port);
        }

        /// <summary>
        /// 指定のアドレス＆ポートの接続数を一つ減らします。
        /// </summary>
        public void Disconnected(IPAddress address, int port)
        {
            if (address == null)
            {
                return;
            }

            lock (this.commenterCountDic)
            {
                // 指定のアドレス＆ポートの組の接続数を一つ減らします。
                var obj = new AddressPort(address, port);
                var count = 0;

                if (this.commenterCountDic.TryGetValue(obj, out count))
                {
                    this.commenterCountDic[obj] = count - 1;
                }
            }
        }
        #endregion
    }
}
