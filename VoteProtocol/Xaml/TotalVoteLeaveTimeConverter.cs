using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

using Ragnarok;

namespace VoteSystem.Protocol.Xaml
{
    using Vote;

    /// <summary>
    /// 全投票時間を表示用の文字列に変換します。
    /// </summary>
    public sealed class TotalVoteLeaveTimeConverter : IMultiValueConverter
    {
        /// <summary>
        /// 全投票時間を表示用の文字列に変換します。
        /// </summary>
        public object Convert(object[] value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            try
            {
                var leaveTime = (TimeSpan)value[0];

                if (leaveTime == TimeSpan.MaxValue)
                {
                    return "無制限";
                }
                else
                {
                    var time = MathEx.Max(leaveTime, TimeSpan.Zero);

                    return string.Format("{0:D2}:{1:D2}",
                        (int)time.TotalMinutes,
                        time.Seconds);
                }
            }
            catch (Exception ex)
            {
                Util.ThrowIfFatal(ex);

                Log.ErrorException(ex,
                    "全投票時間の変換に失敗しました。");
            }

            return null;
        }

        /// <summary>
        /// 実装されていません。
        /// </summary>
        public object[] ConvertBack(object value, Type[] targetType,
                                    object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
