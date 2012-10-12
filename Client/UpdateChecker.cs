using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Threading;

using AppLimit.NetSparkle;
using Ragnarok;

namespace VoteSystem.Client
{
    internal static class UpdateChecker
    {
        private static bool updateDetected;

        /// <summary>
        /// 更新処理終了時に呼ばれます。
        /// </summary>
        public static event EventHandler UpdateFinished;

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
            sparkle.updateDetected += sparkle_updateDetected;
            sparkle.checkLoopFinished += sparkle_checkLoopFinished;
            sparkle.StartLoop(true, true);
        }

        /// <summary>
        /// 更新処理の終了通知を送ります。
        /// </summary>
        static void OnUpdateFinished()
        {
            var handler = UpdateFinished;

            if (handler != null)
            {
                Util.SafeCall(() =>
                    handler(null, EventArgs.Empty));
            }
        }

        static void sparkle_checkLoopFinished(object sender, bool updateRequired)
        {
            var sparkle = (Sparkle)sender;

            // 更新が必要でないなら、更新処理を終了します。
            if (!updateDetected)
            {
                sparkle.StopLoop();

                OnUpdateFinished();
            }
        }

        private static void sparkle_updateDetected(object sender, UpdateDetectedEventArgs e)
        {
            try
            {
                // 更新確認が必要なら
                updateDetected = true;
                e.NextAction = nNextUpdateAction.prohibitUpdate;

                Global.UIProcess(() =>
                {
                    var sparkle = (Sparkle)sender;
                    var item = e.LatestVersion;

                    ShowCheckForm(sparkle, item);
                });
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex, "Failed sparkle_updateDetected.");

                OnUpdateFinished();
                throw;
            }
        }

        private static void ShowCheckForm(Sparkle sparkle, NetSparkleAppCastItem item)
        {
            try
            {
                var form = new NetSparkleForm(
                    item,
                    sparkle.ApplicationIcon,
                    sparkle.ApplicationWindowIcon);
                form.TopMost = true;

                form.RemoveSkipButton();

                // show it
                var dlgResult = form.ShowDialog();
                if (dlgResult == DialogResult.Yes)
                {
                    // download the binaries
                    InitDownloadAndInstallProcess(sparkle, item);
                }
                else
                {
                    OnUpdateFinished();
                }
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex, "Failed ShowCheckForm.");

                OnUpdateFinished();
                throw;
            }
        }

        private static void InitDownloadAndInstallProcess(Sparkle sparkle, NetSparkleAppCastItem item)
        {
            try
            {
                var dlProgress = new NetSparkleDownloadProgress(
                    sparkle, item,
                    null,
                    sparkle.ApplicationIcon,
                    sparkle.ApplicationWindowIcon,
                    sparkle.EnableSilentMode);
                dlProgress.TopMost = true;

                dlProgress.FormClosed += dlProgress_FormClosed;
                dlProgress.ShowDialog();
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex, "Failed InitDownloadAndInstallProcess.");

                OnUpdateFinished();
                throw;
            }
        }

        static void dlProgress_FormClosed(object sender, FormClosedEventArgs e)
        {
            OnUpdateFinished();
        }
    }
}
