using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading;

using AppLimit.NetSparkle;
using Ragnarok;

namespace VoteSystem.Client
{
    /// <summary>
    /// 自動更新作業を行います。
    /// </summary>
    internal static class UpdateChecker
    {
        /// <summary>
        /// NetSparkleのレジストリ情報を削除します。
        /// </summary>
        public static void RemoveSparkleSetting()
        {
            try
            {
                Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(
                    "Software\\co516151\\VoteClient");
            }
            catch
            {
            }
        }

        /// <summary>
        /// アップデートの確認を行います。
        /// </summary>
        public static void CheckUpdate()
        {
            var sparkle = new Sparkle(
                "http://garnet-alice.net/programs/votesystem/update/versioninfo.xml");

            //sparkle.ShowDiagnosticWindow = true;
            sparkle.checkLoopFinished += sparkle_checkLoopFinished;
            sparkle.StartLoop(true, true);
        }

        static void sparkle_checkLoopFinished(object sender, bool updateRequired)
        {
            var sparkle = (Sparkle)sender;

            sparkle.StopLoop();
        }
    }
}
