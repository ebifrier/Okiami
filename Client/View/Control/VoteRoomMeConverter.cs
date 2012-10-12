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
    /// 投票参加者Noからそれが自分であるかを判定し、
    /// それをVisibilityの値に変換します。
    /// </summary>
    [ValueConversion(typeof(ParticipantWithVoteRoomInfo), typeof(Visibility))]
    public class VoteRoomMeConverter : IValueConverter
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
        /// 投票参加者NoをVisibilityの値に変換します。
        /// </summary>
        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            var info = (ParticipantWithVoteRoomInfo)value;

            // 自分であると確認するためには、投票ルームとその参加者Noが
            // 一致する必要があります。
            if (info.VoteRoom.Id != Global.VoteClient.VoteRoomId)
            {
                return DefaultHiddenValue;
            }

            if (info.Participant.No != Global.VoteClient.VoteParticipantNo)
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
