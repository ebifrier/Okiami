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
    /// 投票時間を表示用の文字列に変換します。
    /// </summary>
    public sealed class VoteLeaveTimeConverter : IMultiValueConverter
    {
        /// <summary>
        /// 投票時間を表示用の文字列に変換します。
        /// </summary>
        public object Convert(object[] value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            try
            {
                if (value[0] == DependencyProperty.UnsetValue ||
                    value[1] == DependencyProperty.UnsetValue)
                {
                    return "停止中";
                }

                var leaveTime = (TimeSpan)value[0];
                var state = (VoteState)value[1];

                if (state == VoteState.Stop)
                {
                    return "停止中";
                }
                else if (leaveTime == TimeSpan.MaxValue)
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
                    "投票時間の変換に失敗しました。");
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
