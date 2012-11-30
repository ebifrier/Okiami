using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

#if MONO
using Mono.Unix;
using Mono.Unix.Native;
#endif

using Ragnarok;

namespace VoteSystem.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Ragnarokの初期化処理を行います。
                Ragnarok.Initializer.Initialize();

#if MONO
                StartSignalThread();
#endif

                // メインの処理を開始します。
                var server = new VoteServer();
                server.AcceptLoop();
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "未処理の例外が発生しました。");
            }
        }

#if MONO
        private static Thread signalThread;

        /// <summary>
        /// 別スレッドでシグナル処理を行います。
        /// </summary>
        static void StartSignalThread()
        {
            signalThread = new Thread(SignalThread)
            {
                Name = "SignalThread",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
            };

            signalThread.Start();
        }

        /// <summary>
        /// 別スレッドでシグナル処理を行います。
        /// </summary>
        static void SignalThread()
        {
            var signals = new UnixSignal[]
            {
                new UnixSignal(Signum.SIGUSR2),
            };

            while (true)
            {
                ProcessSignal(signals);
            }
        }

        /// <summary>
        /// シグナル処理を行います。
        /// </summary>
        static void ProcessSignal(UnixSignal[] signals)
        {
            try
            {
                var index = UnixSignal.WaitAny(signals, -1);
                if (index < 0)
                {
                    return;
                }

                var signal = signals[index].Signum;
                Log.Info("シグナルを受信しました。({0})", signal);

                var voteRoomList = GlobalControl.Instance.VoteRoomList;
                foreach (var voteRoom in voteRoomList.Where(room => room != null))
                {
                    voteRoom.SignalReceived((int)signal);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "シグナル処理に失敗しました。");
            }
        }
#endif
    }
}
