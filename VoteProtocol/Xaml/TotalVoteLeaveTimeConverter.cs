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
        /// 最終的に得る値の型を取得または設定します。
        /// </summary>
        public ConvertToType ConvertToType
        {
            get;
            set;
        }

        /// <summary>
        /// 全投票時間を表示用の文字列に変換します。
        /// </summary>
        public object Convert(object[] values, Type targetType,
                              object parameter, CultureInfo culture)
        {
            var obj = ConvertInternal(values);

            return (ConvertToType == ConvertToType.Time ? (object)obj.Time : obj.Text);
        }

        /// <summary>
        /// 全投票時間を表示用の文字列に変換します。
        /// </summary>
        private ConvertPair ConvertInternal(object[] values)
        {
            try
            {
                var leaveTime = (TimeSpan)values[0];
                if (leaveTime == TimeSpan.MinValue)
                {
                    throw new ArgumentException(
                        "持ち時間に負数は設定できません。");
                }

                if (leaveTime == TimeSpan.MaxValue)
                {
                    return new ConvertPair(leaveTime, "無制限");
                }
                else
                {
                    var time = MathEx.Max(leaveTime, TimeSpan.Zero);

                    return new ConvertPair(
                        time,
                        string.Format("{0:D2}:{1:D2}:{2:D2}",
                            (int)time.TotalHours,
                            time.Minutes,
                            time.Seconds));
                }
            }
            catch (Exception ex)
            {
                Util.ThrowIfFatal(ex);

                Log.ErrorException(ex,
                    "全投票時間の変換に失敗しました。");
            }

            return new ConvertPair(null, null);
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
