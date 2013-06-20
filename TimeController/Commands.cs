using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using Ragnarok;
using Ragnarok.Utility;
using Ragnarok.Presentation;
using Ragnarok.Presentation.Command;

namespace TimeController
{
    public static class Commands
    {
        #region Show TimeWindow
        /// <summary>
        /// 時間表示用ウィンドウを表示します。
        /// </summary>
        public static readonly RelayCommand ShowTimeWindow =
            new RelayCommand(ExecuteShowTimeWindow);

        private static void ExecuteShowTimeWindow()
        {
            var window = new TimeWindow
            {
                Owner = Application.Current.MainWindow,
            };

            window.Show();
        }
        #endregion

        #region Play
        /// <summary>
        /// 先手番の残り時間を再設定します。
        /// </summary>
        public static readonly RelayCommand Play =
            new RelayCommand(ExecutePlay);

        private static void ExecutePlay()
        {
            var model = Global.MainViewModel;

            model.IsPlaying = (model.IsPlaying == null || model.IsPlaying == false);
        }
        #endregion

        #region Inc MoveCount
        /// <summary>
        /// 手数を一つ増やします。
        /// </summary>
        public static readonly RelayCommand IncMoveCount =
            new RelayCommand(ExecuteIncMoveCount);

        private static void ExecuteIncMoveCount()
        {
            Global.MainViewModel.MoveCount += 1;
        }
        #endregion

        #region Dec MoveCount
        /// <summary>
        /// 手数を一つ減らします。
        /// </summary>
        public static readonly RelayCommand DecMoveCount =
            new RelayCommand(ExecuteDecMoveCount);

        private static void ExecuteDecMoveCount()
        {
            var value = Global.MainViewModel.MoveCount - 1;

            Global.MainViewModel.MoveCount = Math.Max(0, value);
        }
        #endregion

        #region Set BlackLeaveTime
        /// <summary>
        /// 先手番の残り時間を再設定します。
        /// </summary>
        public static readonly RelayCommand<TimeSpan> SetBlackLeaveTime =
            new RelayCommand<TimeSpan>(ExecuteSetBlackLeaveTime);

        private static void ExecuteSetBlackLeaveTime(TimeSpan span)
        {
            if (span == TimeSpan.MinValue)
            {
                return;
            }

            Global.MainViewModel.BlackLeaveTime = span;
        }
        #endregion

        #region SetBlackUsedTime
        /// <summary>
        /// 先手番の思考時間を再設定します。
        /// </summary>
        public static readonly RelayCommand<TimeSpan> SetBlackUsedTime =
            new RelayCommand<TimeSpan>(ExecuteSetBlackUsedTime);

        private static void ExecuteSetBlackUsedTime(TimeSpan span)
        {
            if (span == TimeSpan.MinValue)
            {
                return;
            }

            Global.MainViewModel.BlackUsedTime = span;
        }
        #endregion

        #region Set WhiteLeaveTime
        /// <summary>
        /// 後手番の残り時間を再設定します。
        /// </summary>
        public static readonly RelayCommand<TimeSpan> SetWhiteLeaveTime =
            new RelayCommand<TimeSpan>(ExecuteSetWhiteLeaveTime);

        private static void ExecuteSetWhiteLeaveTime(TimeSpan span)
        {
            if (span == TimeSpan.MinValue)
            {
                return;
            }

            var model = Global.MainViewModel;
            model.WhiteLeaveTime = span;

            // ミリ秒以下を残り時間に合わせるためこうします。
            model.WhiteUsedTime = model.WhiteUsedTime;
        }
        #endregion

        #region SetWhiteUsedTime
        /// <summary>
        /// 後手番の思考時間を再設定します。
        /// </summary>
        public static readonly RelayCommand<TimeSpan> SetWhiteUsedTime =
            new RelayCommand<TimeSpan>(ExecuteSetWhiteUsedTime);

        private static void ExecuteSetWhiteUsedTime(TimeSpan span)
        {
            if (span == TimeSpan.MinValue)
            {
                return;
            }

            Global.MainViewModel.WhiteUsedTime = span;
        }
        #endregion

        #region Clear WhiteUsedTime
        /// <summary>
        /// 後手番の思考時間をクリアします。
        /// </summary>
        public static readonly RelayCommand ClearWhiteUsedTime =
            new RelayCommand(ExecuteClearWhiteUsedTime);

        private static void ExecuteClearWhiteUsedTime()
        {
            var model = Global.MainViewModel;

            model.WhiteUsedTime = TimeSpan.Zero;
        }
        #endregion
    }
}
