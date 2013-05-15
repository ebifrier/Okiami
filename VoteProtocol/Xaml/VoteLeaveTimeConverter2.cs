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
    /// 投票時間を表示用の値を変換します。
    /// </summary>
    public sealed class VoteLeaveTimeConverter2 : IMultiValueConverter
    {
        /// <summary>
        /// 投票時間を表示用の値に変換します。
        /// </summary>
        public object Convert(object[] value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            try
            {
                if (value[0] == DependencyProperty.UnsetValue ||
                    value[1] == DependencyProperty.UnsetValue)
                {
                    // 停止中とします。
                    return TimeSpan.MinValue;
                }

                var leaveTime = (TimeSpan)value[0];
                var state = (VoteState)value[1];

                if (state == VoteState.Stop || leaveTime == TimeSpan.MinValue)
                {
                    return TimeSpan.MinValue;
                }
                else if (leaveTime == TimeSpan.MaxValue)
                {
                    return TimeSpan.MaxValue;
                }
                else
                {
                    return MathEx.Max(leaveTime, TimeSpan.Zero);
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
