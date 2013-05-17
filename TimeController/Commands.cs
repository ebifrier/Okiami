using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            Global.MainViewModel.WhiteLeaveTime = span;
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
