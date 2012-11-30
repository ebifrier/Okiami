using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

using Ragnarok;
using Ragnarok.ObjectModel;

namespace VoteSystem.Client.Model
{
    using VoteSystem.Protocol;

    /// <summary>
    /// 放送への接続/切断などを指示します。
    /// </summary>
    public enum CommenterInstructionType
    {
        /// <summary>
        /// サーバーからの要求で、放送への接続待ちキューに放送を追加します。
        /// </summary>
        Add,
        /// <summary>
        /// サーバーからの要求で、放送から切断しリストから放送を外します。
        /// </summary>
        Remove,
        /// <summary>
        /// 放送にコメントを投稿します。
        /// </summary>
        PostComment,
    }

    /// <summary>
    /// サーバーからの処理を別スレッドで処理するための命令オブジェクトです。
    /// </summary>
    internal sealed class CommenterInstruction
    {
        /// <summary>
        /// 命令の種類を取得または設定します。
        /// </summary>
        public CommenterInstructionType InstructionType
        {
            get;
            set;
        }

        /// <summary>
        /// 放送IDを取得または設定します。
        /// </summary>
        public string LiveId
        {
            get;
            set;
        }

        /// <summary>
        /// サーバーから来た通知を取得または設定します。
        /// </summary>
        public Notification Notification
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 中継したコメントの情報を保持します。
    /// </summary>
    public sealed class PostCommentData
    {
        /// <summary>
        /// コメントを投稿した放送IDを取得または設定します。
        /// </summary>
        public string LiveId
        {
            get;
            set;
        }

        /// <summary>
        /// コメント内容を取得または設定します。
        /// </summary>
        public string Text
        {
            get;
            set;
        }

        /// <summary>
        /// コメントを投稿した時刻を取得または設定します。
        /// </summary>
        public DateTime Timestamp
        {
            get;
            set;
        }

        /// <summary>
        /// コメントの色を取得または設定します。
        /// </summary>
        public System.Windows.Media.Color Color
        {
            get;
            set;
        }
    }

    /// <summary>
    /// コメンターの管理を行います。
    /// </summary>
    /// <remarks>
    /// コメンターとして接続した放送の管理、
    /// その放送へのコメント投稿などを行います。
    ///
    /// 一度サーバーからの処理をキューに保存し、
    /// そのキューを別スレッドから定期的に処理していきます。
    /// ネットワークの接続処理に想像以上の時間がかかることがあるため、
    /// このようにして本スレッドの遅延をなるべく少なくしています。
    /// </remarks>
    public sealed class CommenterManager
    {
        private readonly NotifyCollection<CommenterCommentClient> commenterClientList =
            new NotifyCollection<CommenterCommentClient>();
        private readonly NotifyCollection<PostCommentData> postCommentList =
            new NotifyCollection<PostCommentData>();
        private readonly ConcurrentQueue<CommenterInstruction> instructionQueue =
            new ConcurrentQueue<CommenterInstruction>();
        private readonly Thread thread;

        /// <summary>
        /// 接続要求が出された放送リストを取得します。
        /// </summary>
        public NotifyCollection<CommenterCommentClient> CommenterClientList
        {
            get { return this.commenterClientList; }
        }

        /// <summary>
        /// 中継されたコメント一覧を取得します。
        /// </summary>
        public NotifyCollection<PostCommentData> PostCommentList
        {
            get { return this.postCommentList; }
        }

        #region サーバーからの要求処理
        /// <summary>
        /// 放送が始まったときに呼ばれます。
        /// </summary>
        public void NotifyNewLive(LiveData liveData)
        {
            if (liveData == null || !liveData.Validate())
            {
                return;
            }

            if (liveData.Site != LiveSite.NicoNama)
            {
                return;
            }

            AddInstruction(new CommenterInstruction()
            {
                InstructionType = CommenterInstructionType.Add,
                LiveId = liveData.LiveIdString,
            });
        }

        /// <summary>
        /// 放送が終了したときに呼ばれます。
        /// </summary>
        public void NotifyClosedLive(LiveData liveData)
        {
            if (liveData == null || !liveData.Validate())
            {
                return;
            }

            if (liveData.Site != LiveSite.NicoNama)
            {
                return;
            }

            AddInstruction(new CommenterInstruction()
            {
                InstructionType = CommenterInstructionType.Remove,
                LiveId = liveData.LiveIdString,
            });
        }

        /// <summary>
        /// 指定のIDの放送に指定の内容のコメントを投稿します。
        /// </summary>
        public void PostComment(LiveData toLive, Notification notification)
        {
            if (notification == null || !notification.Validate())
            {
                return;
            }

            if (toLive == null || !toLive.Validate())
            {
                return;
            }

            AddInstruction(new CommenterInstruction()
            {
                InstructionType = CommenterInstructionType.PostComment,
                LiveId = toLive.LiveIdString,
                Notification = notification,
            });
        }
        #endregion

        #region 処理中のコメンターリスト
        /// <summary>
        /// コメンター用のコメントクライアントを追加します。
        /// </summary>
        /// <remarks>
        /// サーバーから放送開始通知がきたらオブジェクトが追加されます。
        /// 実際にコメントサーバーに接続しているかどうかは関係ありません。
        /// </remarks>
        private void AddCommentClient(CommenterCommentClient commentClient)
        {
            if (commentClient == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(commentClient.LiveId))
            {
                return;
            }

            lock (this.commenterClientList)
            {
                this.commenterClientList.Add(commentClient);
            }
        }

        /// <summary>
        /// コメンター用のコメントクライアントを削除します。
        /// </summary>
        /// <remarks>
        /// サーバーから放送終了通知が来たら、当該の放送ＩＤを持つ
        /// オブジェクトが削除されます。
        /// </remarks>
        private void RemoveCommentClient(CommenterCommentClient commentClient)
        {
            if (commentClient == null)
            {
                return;
            }

            lock (this.commenterClientList)
            {
                // コメンターの削除を行います。
                if (!this.commenterClientList.Remove(commentClient))
                {
                    return;
                }
            }

            // 放送の切断処理を行います。
            commentClient.CommentPost -= CommenterClient_CommentPost;
            commentClient.Delete();
        }

        /// <summary>
        /// 指定のIDを持つ放送情報を探します。
        /// </summary>
        private CommenterCommentClient FindCommentClient(string liveId)
        {
            if (string.IsNullOrEmpty(liveId))
            {
                return null;
            }

            lock (this.commenterClientList)
            {
                return this.commenterClientList.FirstOrDefault(
                    commentClient => commentClient.LiveId == liveId);
            }
        }

        /// <summary>
        /// 登録されている接続オブジェクトをすべて削除します。
        /// </summary>
        private void ClearCommentClient()
        {
            lock (this.commenterClientList)
            {
                while (this.commenterClientList.Any())
                {
                    var commentClient = this.commenterClientList.First();

                    RemoveCommentClient(commentClient);
                }
            }
        }
        #endregion

        #region 投稿したコメント一覧
        /// <summary>
        /// 中継したコメントをリストに追加します。
        /// </summary>
        internal void AddPostComment(PostCommentData postComment)
        {
            if (postComment == null)
            {
                return;
            }

            Global.UIProcess(() =>
                this.postCommentList.Add(postComment));
        }

        /// <summary>
        /// 中継したコメントリストをクリアします。
        /// </summary>
        internal void ClearPostCommentList()
        {
            Global.UIProcess(() =>
                this.postCommentList.Clear());
        }

        /// <summary>
        /// コメント中継時に呼ばれます。
        /// </summary>
        private void CommenterClient_CommentPost(object sender, CommentPostEvent e)
        {
            AddPostComment(e.PostComment);
        }
        #endregion

        /// <summary>
        /// サーバーからの接続/切断要求をキューに追加します。
        /// </summary>
        private void AddInstruction(CommenterInstruction inst)
        {
            if (inst == null)
            {
                return;
            }

            this.instructionQueue.Enqueue(inst);
        }

        /// <summary>
        /// コメンターへの接続処理などを処理します。
        /// </summary>
        private void ProcessInstruction(CommenterInstruction inst)
        {
            var commentClient = FindCommentClient(inst.LiveId);

            switch (inst.InstructionType)
            {
                case CommenterInstructionType.Add:
                    // 同じIDの放送がすでにあったら、何もしない。
                    if (commentClient != null)
                    {
                        return;
                    }

                    // 条件によっては勝手に接続を始めます。
                    commentClient = new CommenterCommentClient(
                        Global.ConnectionCounter, inst.LiveId)
                    {
                        IsAllowToConnect =
                            Global.MainModel.IsNicoCommenterAutoStart,
                    };
                    commentClient.CommentPost += CommenterClient_CommentPost;

                    // 放送接続待ちリストに追加します。
                    AddCommentClient(commentClient);
                    break;

                case CommenterInstructionType.Remove:
                    if (commentClient != null)
                    {
                        RemoveCommentClient(commentClient);
                    }
                    break;

                case CommenterInstructionType.PostComment:
                    if (commentClient == null)
                    {
                        Log.Error(
                            "コメンターの放送[{0}]が見つかりませんでした。",
                            inst.LiveId);
                        break;
                    }

                    commentClient.PostComment(inst.Notification);
                    break;
            }
        }

        /// <summary>
        /// サーバーからの命令を別スレッド上で処理します。
        /// </summary>
        private void ProcessInstructionQueue()
        {
            while (true)
            {
                // ConcurrentQueueを使用
                CommenterInstruction inst;
                if (!this.instructionQueue.TryDequeue(out inst))
                {
                    break;
                }

                ProcessInstruction(inst);
            }
        }

        /// <summary>
        /// 放送リスト中の放送状態を更新します。
        /// </summary>
        private void ProcessUpdate()
        {
            lock (this.commenterClientList)
            {
                for (var i = 0; i < this.commenterClientList.Count; )
                {
                    var commentClient = this.commenterClientList[i];

                    // 何かを処理した場合は、処理をそこで打ち切ります。
                    // 残りは次の更新時に行います。
                    var processed = commentClient.Update();

                    // もしコメンターが削除されていたら、
                    // リストから削除します。
                    if (commentClient.State == CommentClientState.Deleted)
                    {
                        this.commenterClientList.RemoveAt(i);
                    }
                    else
                    {
                        ++i;
                    }
                }
            }
        }

        /// <summary>
        /// 各放送への接続/切断処理を別スレッドで行います。
        /// </summary>
        private void ThreadMain()
        {
            var i = 0;

            while (true)
            {
                try
                {
                    // 外部アプリによるブラウザ視聴状態を監視するため、
                    // 定期的にnetstatのコネクションを確認しています。
                    if (++i == 5)
                    {
                        Global.ConnectionCounter.Update();
                        i = 0;
                    }

                    ProcessInstructionQueue();

                    ProcessUpdate();
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Log.ErrorException(ex,
                        "コメンター処理に失敗しました。");
                }

                Thread.Sleep(TimeSpan.FromSeconds(1.0));
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CommenterManager()
        {
            this.thread = new Thread(ThreadMain)
            {
                Name = "CommenterThread",
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal,
            };
            this.thread.Start();
        }
    }
}
