using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading;

using Ragnarok;
using Ragnarok.Presentation.Update;

namespace VoteSystem.Client
{
    /// <summary>
    /// 自動更新作業を行います。
    /// </summary>
    internal static class UpdateChecker
    {
        /// <summary>
        /// アップデートの確認を行います。
        /// </summary>
        public static void CheckUpdate()
        {
            var updater = new PresentationUpdater(
                "http://garnet-alice.net/programs/votesystem/update/versioninfo.xml");

            updater.Start();
        }
    }
}
