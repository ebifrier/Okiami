using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace VoteSystem.Client.View.Control
{
    /// <summary>
    /// 投票参加者Noから投票ルームのオーナーであるかを判定し、
    /// それをVisibilityの値に変換します。
    /// </summary>
    [ValueConversion(typeof(ParticipantWithVoteRoomInfo), typeof(Visibility))]
    public class VoteRoomOwnerConverter : IValueConverter
    {
        private Visibility defaultHiddenValue = Visibility.Hidden;

        /// <summary>
        /// 非表示の時の規定値を取得または設定します。
        /// </summary>
        public Visibility DefaultHiddenValue
        {
            get { return this.defaultHiddenValue; }
            set { this.defaultHiddenValue = value; }
        }

        /// <summary>
        /// ルームオーナーならば表示の値を返します。
        /// </summary>
        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            var info = (ParticipantWithVoteRoomInfo)value;

            // 参加者NoがルームオーナーのNoと一致すればその人が
            // ルームオーナーです。
            if (info.Participant.No != info.VoteRoom.OwnerNo)
            {
                return DefaultHiddenValue;
            }

            return Visibility.Visible;
        }

        /// <summary>
        /// Visibilityの値を参加者Noに変換します。
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
