using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace VoteSystem.Client.View.Control
{
    using VoteSystem.Protocol.Vote;

    /// <summary>
    /// 投票参加者と投票ルームの組となるオブジェクトです。
    /// </summary>
    public class ParticipantWithVoteRoomInfo
    {
        /// <summary>
        /// 投票参加者の情報を取得または設定します。
        /// </summary>
        public VoteParticipantInfo Participant
        {
            get;
            set;
        }

        /// <summary>
        /// 投票ルームの情報を取得または設定します。
        /// </summary>
        public VoteRoomInfo VoteRoom
        {
            get;
            set;
        }
    }

    /// <summary>
    /// <see cref="VoteRoomInfo"/>から<see cref="ParticipantWithVoteRoomInfo"/>
    /// オブジェクトのリストに変換します。
    /// </summary>
    [ValueConversion(typeof(VoteRoomInfo), typeof(List<ParticipantWithVoteRoomInfo>))]
    public class VoteRoomWithParticipantConverter : IMultiValueConverter
    {
        /// <summary>
        /// <see cref="VoteRoomInfo"/>を<see cref="ParticipantWithVoteRoomInfo"/>
        /// のリストに変換します。
        /// </summary>
        public object Convert(object[] value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            var voteRoomInfo = (VoteRoomInfo)value[0];
            if (voteRoomInfo == null)
            {
                return new List<ParticipantWithVoteRoomInfo>();
            }

            // リストを作成します。
            return voteRoomInfo.ParticipantList.Select(participant =>
                new ParticipantWithVoteRoomInfo()
                {
                    Participant = participant,
                    VoteRoom = voteRoomInfo,
                });
        }

        /// <summary>
        /// ParticipantWithVoteRoomInfoの値を変換します。
        /// </summary>
        public object[] ConvertBack(object value, Type[] targetType,
                                    object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
