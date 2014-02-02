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
    /// VoteLeaveTimeConverterの戻り値タイプです。
    /// </summary>
    public enum ConvertToType
    {
        /// <summary>
        /// 残り時間をTimeSpan型で返します。
        /// </summary>
        Time,
        /// <summary>
        /// 残り時間を文字列で返します。
        /// </summary>
        Text,
    }

    internal class ConvertPair
    {
        /// <summary>
        /// 残り時間を取得または設定します。
        /// </summary>
        public TimeSpan? Time
        {
            get;
            private set;
        }

        /// <summary>
        /// 残り時間を示す文字列を取得または設定します。
        /// </summary>
        public string Text
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ConvertPair(TimeSpan? time, string text)
        {
            Time = time;
            Text = text;
        }
    }

    /// <summary>
    /// 投票時間を表示用の何かに変換します。
    /// </summary>
    public sealed class VoteLeaveTimeConverter : IMultiValueConverter
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
        /// 投票時間を表示用の文字列に変換します。
        /// </summary>
        public object Convert(object[] values, Type targetType,
                              object parameter, CultureInfo culture)
        {
            var obj = ConvertInternal(values);

            return (ConvertToType == ConvertToType.Time ? (object)obj.Time : obj.Text);
        }

        /// <summary>
        /// 投票時間を表示用の文字列に変換します。
        /// </summary>
        private ConvertPair ConvertInternal(object[] values)
        {
            try
            {
                if (values[0] == DependencyProperty.UnsetValue ||
                    values[1] == DependencyProperty.UnsetValue)
                {
                    return new ConvertPair(TimeSpan.MinValue, "停止中");
                }

                var leaveTime = (TimeSpan)values[0];
                var state = (VoteState)values[1];

                if (state == VoteState.Stop)
                {
                    return new ConvertPair(TimeSpan.MinValue, "停止中");
                }
                else if (leaveTime == TimeSpan.MaxValue)
                {
                    return new ConvertPair(TimeSpan.MaxValue, "無制限");
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
                    "投票時間の変換に失敗しました。");
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
