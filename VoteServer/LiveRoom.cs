using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Ragnarok;
using Ragnarok.Net;

namespace VoteSystem.Server
{
    using Protocol;
    using Protocol.Vote;

    /// <summary>
    /// 生放送別に用意されるオブジェクトで、投票ルームに複数集約されます。
    /// </summary>
    /// <remarks>
    /// 投票された票やプレイヤーの参加希望などは、
    /// ある生放送からコメントされたものが他の放送に再コメントされることがあります。
    /// 
    /// 各放送にコメントされる確認コメントなどは一つであることが望ましいので
    /// ユーザーを放送別に管理し、各放送ごとにコメントが一つだけ
    /// 投稿されるようにしています。
    /// 
    /// コメンター)
    /// コメンターとは放送にコメントを投稿するための各アカウントのことで、
    /// 投稿規制を回避するために使います。
    /// コメンターは各放送ごとにまとめられ、また各投票ルームごとにも
    /// まとめられています。
    /// </remarks>
    public class LiveRoom : ILogObject
    {
        /// <summary>
        /// ミラーコメントの印（無幅空白）です。
        /// </summary>
        public const char MirrorCommentMark = ProtocolUtil.MirrorCommentMark;
        /// <summary>
        /// 確認コメントの印（可視）です。
        /// </summary>
        public const string ComfirmCommentPrefix = "!! ";
        /// <summary>
        /// ミラーコメントの印（可視）です。
        /// </summary>
        public const string MirrorCommentPrefix = "! ";

        private readonly LiveData liveData;
        private readonly VoteParticipant liveOwner;
        private LiveAttribute attribute;

        /// <summary>
        /// ログ出力用の名前を取得します。
        /// </summary>
        public string LogName
        {
            get
            {
                return string.Format(
                    "LiveRoom[\"{0}\"]",
                    this.liveData);
            }
        }

        /// <summary>
        /// 放送ＩＤを取得します。
        /// </summary>
        public LiveData LiveData
        {
            get { return this.liveData; }
        }

        /// <summary>
        /// 放送ルームの属性を取得または設定します。
        /// </summary>
        public LiveAttribute Attribute
        {
            get { return this.attribute; }
            set
            {
                if (value == null || !value.Validate())
                {
                    return;
                }

                // システムメッセージはコメンターからは送信しません。
                this.attribute = value;
                this.attribute.SystemCommentTypeMask = 0;
            }
        }

#if false
        /// <summary>
        /// 全コメントをミラーするモードかどうかを取得します。
        /// </summary>
        public bool IsMirrorMode
        {
            get
            {
                // 全コメントをミラーするモードなら
                // コメントの最初にミラーコメントのマークである'!'をつけません。
                var voteRoom = this.liveOwner.VoteRoom;
                if (voteRoom == null)
                {
                    return false;
                }

                return voteRoom.VoteModel.IsMirrorMode;
            }
        }

        /// <summary>
        /// 通知のテキストを修正します。
        /// </summary>
        private Notification ModifyNotification(Notification notification,
                                                int roomIndex)
        {
            var text = notification.Text;
            if (string.IsNullOrEmpty(text))
            {
                return notification;
            }

            if (ProtocolUtil.IsMirrorComment(text))
            {
                return notification;
            }

            // コテハン名がつくため、@と＠を別の文字に置き換えます。
            text = text.Replace("@", "\u24D0");
            text = text.Replace("＠", "\u24D0");

            // 投票コメントの場合、確認コメントなら票部分のみを
            // ミラーコメントなら全部を投稿します。
            if (notification.FromLiveRoom != null &&
                notification.FromLiveRoom.LiveData == this.LiveData &&
                notification.FromLiveRoom.RoomIndex == roomIndex)
            {
                // 票部分と残りの部分は全幅空白で仕切られています。
                if (notification.Type == NotificationType.Vote)
                {
                    var delimIndex = text.IndexOf('　');
                    if (delimIndex >= 0)
                    {
                        text = text.Substring(0, delimIndex);
                    }
                }

                // 全コメントのミラー時は確認コメントは送りません。
                if (IsMirrorMode)
                {
                    return null;
                }

                // 先頭に無幅空白を挿入します。
                text = string.Format(
                    "{0}{1}{2}",
                    MirrorCommentMark,
                    (IsMirrorMode ? "" : ComfirmCommentPrefix),
                    text);
            }
            else
            {
                text = string.Format(
                    "{0}{1}{2}",
                    MirrorCommentMark,
                    (IsMirrorMode ? "" : MirrorCommentPrefix),
                    text);
            }

            // 文字列部分を置き換えます。
            return notification.Clone()
                .Apply(_ => _.Text = text);
        }
#endif

        /// <summary>
        /// 放送ルームを閉じます。
        /// </summary>
        public void Close()
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LiveRoom(VoteParticipant owner, LiveData liveData)
        {
            this.liveOwner = owner;
            this.liveData = liveData;
            this.attribute = new LiveAttribute();
        }
    }
}
