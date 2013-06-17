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
    using Protocol.Commenter;

    /// <summary>
    /// コメント用の参加者オブジェクトを扱います。
    /// </summary>
    internal class Commenter
    {
        /// <summary>
        /// 投稿用のアカウントを取得または設定します。
        /// </summary>
        public VoteParticipant Participant
        {
            get;
            set;
        }

        /// <summary>
        /// コメンターのルーム番号を取得または設定します。
        /// </summary>
        public int RoomIndex
        {
            get;
            set;
        }

        /// <summary>
        /// コメント投稿が可能かどうかを取得または設定します。
        /// </summary>
        public bool PostCommentEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// 放送を視聴中かどうかを取得または設定します。
        /// </summary>
        public bool IsWatching
        {
            get;
            set;
        }
    }

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
    public class LiveRoom : ILogObject, IDisposable
    {
        /// <summary>
        /// コメンターの最大数です。全部屋の合計値がこれを超えないようにします。
        /// </summary>
        public const int CommenterMaxCount = 100;
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

        private readonly object SyncRoot = new object();
        private readonly LiveData liveData;
        private readonly VoteParticipant liveOwner;
        private readonly HashSet<VoteParticipant> alreadySentNotifySet =
            new HashSet<VoteParticipant>();
        private readonly List<LinkedList<Commenter>> commenterSetList =
            new List<LinkedList<Commenter>>();
        private LiveAttribute attribute;
        private Timer timer;
        private bool disposed = false;

        /// <summary>
        /// ログ出力用の名前を取得します。
        /// </summary>
        public string LogName
        {
            get
            {
                return string.Format(
                    "ライブルーム[\"{0}\"]",
                    this.liveData);
            }
        }

        /// <summary>
        /// 放送ＩＤを取得します。
        /// </summary>
        public LiveData LiveData
        {
            get
            {
                return this.liveData;
            }
        }

        /// <summary>
        /// 放送ルームの属性を取得または設定します。
        /// </summary>
        public LiveAttribute Attribute
        {
            get
            {
                return this.attribute;
            }
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

        /// <summary>
        /// コメンターの数を取得します。
        /// </summary>
        public int CommenterCount
        {
            get
            {
                lock (SyncRoot)
                {
                    return this.commenterSetList.Sum(
                        commenterList => commenterList.Count);
                }
            }
        }

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
                return (voteRoom != null && voteRoom.VoteModel.IsMirrorMode);
            }
        }

        /// <summary>
        /// 投票ルームが変更されたときに呼ばれます。
        /// </summary>
        internal void OnVoteRoomChanged(VoteRoom voteRoom)
        {
            if (this.liveOwner == null)
            {
                return;
            }

            // 投票ルームからの退室時には、コメンターを全部削除します。
            ClearCommenter();
        }

        /// <summary>
        /// コメンターが切断されたときに呼ばれます。
        /// </summary>
        private void Commenter_Disconnected(object sender,
                                            DisconnectEventArgs e)
        {
            var commenter = sender as VoteParticipant;

            if (commenter != null)
            {
                RemoveCommenter(commenter);
            }
        }

        /// <summary>
        /// 放送終了通知を送ります。
        /// </summary>
        private void SendNotifyClosedLive(VoteParticipant participant)
        {
            if (participant == null)
            {
                return;
            }

            // 閉じる前に放送終了の通知を送ります。
            var command = new NotifyClosedLiveCommand()
            {
                Live = this.liveData,
            };

            participant.SendCommand(command, false);
        }

        /// <summary>
        /// コメンターを追加します。
        /// </summary>
        public void AddCommenter(VoteParticipant participant, int roomIndex)
        {
            if (participant == null)
            {
                return;
            }

            lock (SyncRoot)
            {
                if (roomIndex < 0 || this.commenterSetList.Count <= roomIndex)
                {
                    Log.Error(this,
                        "ルーム番号が正しくありません。(値={0})", roomIndex);
                    return;
                }

                participant.Disconnected += Commenter_Disconnected;

                this.commenterSetList[roomIndex].AddLast(new Commenter()
                {
                    Participant = participant,
                    RoomIndex = roomIndex,
                    PostCommentEnabled = false,
                    IsWatching = false,
                });

                Log.Debug(this, "コメンターが追加されました。");
            }
        }

        /// <summary>
        /// コメンターを削除します。
        /// </summary>
        public void RemoveCommenter(VoteParticipant participant)
        {
            lock (SyncRoot)
            {
                var commenter = FindCommenter(participant);

                if (commenter != null)
                {
                    RemoveCommenter(commenter);
                }
            }
        }

        /// <summary>
        /// コメンターを削除します。
        /// </summary>
        private void RemoveCommenter(Commenter commenter)
        {
            lock (SyncRoot)
            {
                // オブジェクトがない場合は、そのまま返ります。
                if (commenter == null)
                {
                    return;
                }

                // コメンターはルーム番号ごとに管理しています。
                var index = commenter.RoomIndex;
                if (!this.commenterSetList[index].Remove(commenter))
                {
                    return;
                }

                // 削除時にここからも削除。
                this.alreadySentNotifySet.Remove(commenter.Participant);

                commenter.Participant.Disconnected -= Commenter_Disconnected;

                Log.Debug(this, "コメンターが削除されました。");
            }
        }

        /// <summary>
        /// 指定の参加者を持つコメンターオブジェクトを取得します。
        /// </summary>
        private Commenter FindCommenter(VoteParticipant participant)
        {
            if (participant == null)
            {
                return null;
            }

            lock (SyncRoot)
            {
                /*return this.commenterSetList.Select(
                    commenterList => commenterList.FirstOrDefault(
                        commenter => commenter.Participant == participant))
                    .FirstOrDefault(ret => ret != null);*/

                foreach (var commenterList in this.commenterSetList)
                {
                    // コメンターはルームごとに分かれて管理されています。
                    var ret = commenterList.FirstOrDefault(
                        commenter => commenter.Participant == participant);

                    if (ret != null)
                    {
                        return ret;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// 登録された全コメンターを削除します。
        /// </summary>
        public void ClearCommenter()
        {
            lock (SyncRoot)
            {
                // 放送開始通知を送った全放送に終了通知を送ります。
                foreach (var participant in this.alreadySentNotifySet)
                {
                    SendNotifyClosedLive(participant);
                }

                Log.Debug(
                    "NotifyClosedLiveCommandをすべての放送参加者({0})に送信しました。",
                    this.alreadySentNotifySet.Count());

                foreach (var commenterList in this.commenterSetList)
                {
                    // イベントだけ外して一斉削除します。
                    foreach (var commenter in commenterList)
                    {
                        commenter.Participant.Disconnected -= Commenter_Disconnected;
                    }

                    commenterList.Clear();
                }

                this.alreadySentNotifySet.Clear();
            }
        }

        /// <summary>
        /// リストの先頭にあるコメンターを最後に移動します。
        /// </summary>
        /// <remarks>
        /// 登録されたコメンターの順番を入れ替えます。
        /// 投稿可能なコメンターが見つかった時点で処理を終了します。
        /// </remarks>
        private Commenter RotateCommenter(int roomIndex)
        {
            lock (SyncRoot)
            {
                var commenterList = this.commenterSetList[roomIndex];
                if (!commenterList.Any())
                {
                    return null;
                }

                var count = commenterList.Count;
                while (--count >= 0)
                {
                    // 先頭のコメンターを最後に持って行きます。
                    var commenter = commenterList.First();
                    commenterList.RemoveFirst();
                    commenterList.AddLast(commenter);

                    // 先頭のコメンターが投稿可能状態なら
                    // 処理をやめ、そのコメンターからコメントを投稿します。
                    var tmp = commenterList.First;
                    if (tmp.Value.PostCommentEnabled)
                    {
                        return tmp.Value;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// 状態を更新します
        /// </summary>
        public void SetState(VoteParticipant participant,
                             bool canPostComment,
                             bool isWatching)
        {
            var commenter = FindCommenter(participant);
            if (commenter == null)
            {
                return;
            }

            // コメントを投稿可能な状態にします。
            commenter.PostCommentEnabled = canPostComment;
            commenter.IsWatching = isWatching;
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

            // 文字列部分を置き換えます。
            Notification cloned = notification.Clone();

            // コテハン名がつくため、@と＠を別の文字に置き換えます。
            cloned.Text = cloned.Text.Replace("@", "\u24D0");
            cloned.Text = cloned.Text.Replace("＠", "\u24D0");

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
                cloned.Text = string.Format(
                    "{0}{1}{2}",
                    MirrorCommentMark,
                    (IsMirrorMode ? "" : ComfirmCommentPrefix),
                    text);
                return cloned;
            }
            else
            {
                cloned.Text = string.Format(
                    "{0}{1}{2}",
                    MirrorCommentMark,
                    (IsMirrorMode ? "" : MirrorCommentPrefix),
                    text);
                return cloned;
            }
        }

        /// <summary>
        /// コメント投稿用のメッセージを送信します。
        /// </summary>
        public void SendNotificationForPost(Notification notification)
        {
            if (LiveData.Site != LiveSite.NicoNama)
            {
                return;
            }

            // 通知を送信しない設定になっていたら、そのまま帰ります。
            if (notification.Type == NotificationType.System ||
                notification.Type == NotificationType.Important)
            {
                return;
            }

            // 投稿する設定になっていなければ帰ります。
            if (!ProtocolUtil.IsPostComment(
                notification, IsMirrorMode, Attribute, LiveData))
            {
                return;
            }

            try
            {
                lock (SyncRoot)
                {
                    for (var i = 0; i < this.commenterSetList.Count; ++i)
                    {
                        // 投稿可能なコメンターを取得します。
                        var commenter = RotateCommenter(i);

                        // もし投稿可能なコメンターが無い場合は
                        // コメントのミラーを諦めます。
                        if (commenter == null)
                        {
                            continue;
                        }

                        var modified = ModifyNotification(notification, i);
                        if (modified == null)
                        {
                            continue;
                        }

                        // 通知は放送＆ルーム番号ごとに調整します。
                        var command = new NotificationForPostCommand
                        {
                            Notification = modified,
                            ToLive = LiveData,
                        };

                        commenter.Participant.SendCommand(command);
                        commenter.PostCommentEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorException(this, ex,
                    "投稿用メッセージのブロードキャスト中にエラーが発生しました。");
            }
        }

        /// <summary>
        /// 放送ルームを閉じます。
        /// </summary>
        public void Close()
        {
            // 登録してあるコメンターを全部削除します。
            ClearCommenter();
        }

        /// <summary>
        /// 投票ルーム内の参加者からコメンターを募集します。
        /// </summary>
        /// <remarks>
        /// 数十秒ごとに一度呼ばれ、放送に所属するコメンターの数を
        /// 調整します。
        /// </remarks>
        private void UpdateCommenter(object state)
        {
            var voteRoom = this.liveOwner.VoteRoom;
            if (voteRoom == null)
            {
                return;
            }

            lock (SyncRoot)
            {
                for (var i = 0; i < voteRoom.ParticipantCount; ++i)
                {
                    var participant = voteRoom.GetParticipant(i);
                    if (participant == null)
                    {
                        continue;
                    }

                    // 既に放送開始通知を送っていたらもう送りません。
                    if (alreadySentNotifySet.Contains(participant) ||
                        !participant.IsUseAsNicoCommenter)
                    {
                        continue;
                    }

                    // コメンターの最大数を超えないようにします。
                    var count = this.commenterSetList.Sum(_ => _.Count());
                    if (count > CommenterMaxCount)
                    {
                        continue;
                    }

                    var command = new NotifyNewLiveCommand()
                    {
                        Live = this.liveData,
                    };

                    participant.SendCommand(command);
                    this.alreadySentNotifySet.Add(participant);
                }
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LiveRoom(VoteParticipant owner, LiveData liveData)
        {
            this.liveOwner = owner;
            this.liveData = liveData;
            this.attribute = new LiveAttribute();
            this.commenterSetList = new List<LinkedList<Commenter>>();

            // ニコ生のルーム番号は最大でも
            // アリーナ、立ち見Ａ，Ｂ，Ｃの４つです。
            for (var i = 0; i < 4; ++i)
            {
                this.commenterSetList.Add(new LinkedList<Commenter>());
            }

            this.timer = new Timer(
                UpdateCommenter,
                null,
                TimeSpan.FromSeconds(1.0),
                TimeSpan.FromSeconds(20.0));
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~LiveRoom()
        {
            Dispose(false);
        }

        /// <summary>
        /// オブジェクトを破棄します。
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        /// <summary>
        /// オブジェクトを破棄します。
        /// </summary>
        protected void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.timer != null)
                    {
                        this.timer.Dispose();
                        this.timer = null;
                    }

                    Close();
                }

                this.disposed = true;
            }
        }
    }
}
