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

        #region Set BlackLeaveTime
        /// <summary>
        /// 先手番の残り時間を再設定します。
        /// </summary>
        public static readonly RelayCommand<SimpleTimeSpan> SetBlackLeaveTime =
            new RelayCommand<SimpleTimeSpan>(ExecuteSetBlackLeaveTime);

        private static void ExecuteSetBlackLeaveTime(SimpleTimeSpan span)
        {
            if (span == null || !span.IsUse)
            {
                return;
            }

            var model = Global.MainViewModel;

            model.BlackLeaveTime = span.TimeSpan;
        }
        #endregion

        #region Set WhiteLeaveTime
        /// <summary>
        /// 後手番の残り時間を再設定します。
        /// </summary>
        public static readonly RelayCommand<SimpleTimeSpan> SetWhiteLeaveTime =
            new RelayCommand<SimpleTimeSpan>(ExecuteSetWhiteLeaveTime);

        private static void ExecuteSetWhiteLeaveTime(SimpleTimeSpan span)
        {
            if (span == null || !span.IsUse)
            {
                return;
            }

            var model = Global.MainViewModel;

            model.WhiteLeaveTime = span.TimeSpan;
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
