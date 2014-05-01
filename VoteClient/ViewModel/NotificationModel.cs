using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ragnarok;

namespace VoteSystem.Client.ViewModel
{
    using Protocol;

    /// <summary>
    /// 投票サーバーからの通知をGUI上で表示するためのモデルクラスです。
    /// </summary>
    public sealed class NotificationModel
    {
        /// <summary>
        /// 投票サーバーから来た通知を取得します。
        /// </summary>
        public Notification Notification
        {
            get;
            private set;
        }

        /// <summary>
        /// 通知の番号を取得します。
        /// </summary>
        public int No
        {
            get;
            private set;
        }

        /// <summary>
        /// 通知の内容を取得します。
        /// </summary>
        public string Text
        {
            get
            {
                if (Notification.Type == NotificationType.System)
                {
                    return EnumEx.GetLabel(Notification.SystemType);
                }

                return Notification.Text;
            }
        }

        /// <summary>
        /// 通知を投稿した人を取得します。
        /// </summary>
        public string VoterId
        {
            get
            {
                return Notification.VoterId;
            }
        }

        /// <summary>
        /// 通知の種類を取得します。
        /// </summary>
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

        /// <summary>
        /// 通知が投稿された放送を示す文字列を取得します。
        /// </summary>
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

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NotificationModel(Notification notification, int no)
        {
            Notification = notification;
            No = no;
        }
    }
}
