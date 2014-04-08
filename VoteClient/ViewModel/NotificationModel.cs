using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoteSystem.Client.ViewModel
{
    using Protocol;

    public sealed class NotificationModel
    {
        public Notification Notification
        {
            get;
            private set;
        }

        public int No
        {
            get;
            private set;
        }

        public string Text
        {
            get
            {
                if (Notification.Type == NotificationType.System)
                {
                    return Ragnarok.EnumEx.GetLabel(Notification.SystemType);
                }

                return Notification.Text;
            }
        }

        public string VoterId
        {
            get
            {
                return Notification.VoterId;
            }
        }

        public string TypeString
        {
            get
            {
                switch (Notification.Type)
                {
                    case NotificationType.Unknown:
                        return "不明";
                    case NotificationType.Important:
                        return "重要";
                    case NotificationType.System:
                        return "システム";
                    case NotificationType.Message:
                        return "☆コメント";
                    case NotificationType.Vote:
                        return "投票";
                    case NotificationType.Join:
                        return "参加";
                    case NotificationType.TimeExtend:
                        return "時間";
                    case NotificationType.Evaluation:
                        return "評価値";
                }

                return "不明";
            }
        }

        public string FromLiveString
        {
            get
            {
                if (Notification.FromLiveRoom == null ||
                    Notification.FromLiveRoom.LiveData == null)
                {
                    return string.Empty;
                }

                //Model.Live.LiveNicoClient.GetVoteSystemMessage

                var liveData = Notification.FromLiveRoom.LiveData;
                var index = Notification.FromLiveRoom.RoomIndex;

                return string.Format("{0}[{1}]", liveData.SiteName, index);
            }
        }

        public NotificationModel(Notification notification, int no)
        {
            Notification = notification;
            No = no;
        }
    }
}
