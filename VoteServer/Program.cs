using System;
using System.Collections.Generic;
using System.Linq;

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
                Initializer.Initialize();

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
    }
}
