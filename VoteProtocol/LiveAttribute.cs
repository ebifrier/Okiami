using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;

using ProtoBuf;
using Ragnarok;
using Ragnarok.ObjectModel;

namespace VoteSystem.Protocol
{
    /// <summary>
    /// 放送情報を保持します。
    /// </summary>
    /// <remarks>
    /// NotificationとCommentが用語として入り交じってますが、
    /// 本来は"Comment"が正しいです。放送に投稿するものをまとめて
    /// "Comment"と読んでいます。しかし、ソースの便宜的にNotificationの
    /// 列挙子などを流用しています。
    /// 
    /// ・確認コメント: 自放送から投稿された票などの確認用コメント
    /// ・ミラーコメント: 他放送から投稿された票などの確認用コメント
    /// </remarks>
    [DataContract()]
    [Serializable()]
    public class LiveAttribute : NotifyObject
    {
        private NotificationTypeMask systemCommentTypeMask =
            NotificationTypeMask.SystemAll;

        private bool isPostMirrorComment = true;
        private NotificationTypeMask mirrorCommentTypeMask =
            NotificationTypeMask.CommentCore;

        private bool isPostConfirmComment = true;
        private NotificationTypeMask confirmCommentTypeMask =
            NotificationTypeMask.None;

        #region Systemコメント
        /// <summary>
        /// 放送に投稿するシステムコメントの種類を取得または設定します。
        /// </summary>
        public NotificationTypeMask SystemCommentTypeMask
        {
            get
            {
                return this.systemCommentTypeMask;
            }
            set
            {
                value &= NotificationTypeMask.SystemAll;

                if (this.systemCommentTypeMask != value)
                {
                    this.systemCommentTypeMask = value;

                    this.RaisePropertyChanged("SystemCommentTypeMask");
                }
            }
        }

        /// <summary>
        /// Flags属性付きのenumをprotobuf-netが上手く処理できないため、
        /// シリアライズ用のプロパティを作っています。
        /// </summary>
        [DataMember(Order = 1, IsRequired = true)]
        private int SystemCommentTypeMaskForSerialize
        {
            get
            {
                return (int)SystemCommentTypeMask;
            }
            set
            {
                SystemCommentTypeMask = (NotificationTypeMask)value;
            }
        }

        /// <summary>
        /// 重要メッセージを放送に投稿するかどうかを取得または設定します。
        /// </summary>
        [DependOnProperty("SystemCommentTypeMask")]
        public bool IsPostImportantComment
        {
            get
            {
                return (SystemCommentTypeMask &
                    NotificationTypeMask.Important) != 0;
            }
            set
            {
                if (value)
                {
                    SystemCommentTypeMask |= NotificationTypeMask.Important;
                }
                else
                {
                    SystemCommentTypeMask &= ~NotificationTypeMask.Important;
                }
            }
        }

        /// <summary>
        /// システムメッセージを放送に投稿するかどうかを取得または設定します。
        /// </summary>
        [DependOnProperty("SystemCommentTypeMask")]
        public bool IsPostSystemComment
        {
            get
            {
                return (SystemCommentTypeMask &
                    NotificationTypeMask.System) != 0;
            }
            set
            {
                if (value)
                {
                    SystemCommentTypeMask |= NotificationTypeMask.System;
                }
                else
                {
                    SystemCommentTypeMask &= ~NotificationTypeMask.System;
                }
            }
        }
        #endregion

        #region Mirrorコメント
        /// <summary>
        /// 他放送から投稿された投票や参加のミラーコメントを放送に投稿するか
        /// どうかを取得または設定します。
        /// </summary>
        [DataMember(Order = 20, IsRequired = true)]
        public bool IsPostMirrorComment
        {
            get
            {
                return this.isPostMirrorComment;
            }
            set
            {
                if (this.isPostMirrorComment != value)
                {
                    this.isPostMirrorComment = value;

                    this.RaisePropertyChanged("IsPostMirrorComment");
                }
            }
        }

        /// <summary>
        /// 放送に投稿するミラーコメントの種類を取得または設定します。
        /// </summary>
        public NotificationTypeMask MirrorCommentTypeMask
        {
            get
            {
                return this.mirrorCommentTypeMask;
            }
            set
            {
                value &= NotificationTypeMask.CommentAll;

                if (this.mirrorCommentTypeMask != value)
                {
                    this.mirrorCommentTypeMask = value;

                    this.RaisePropertyChanged("MirrorCommentTypeMask");
                }
            }
        }

        /// <summary>
        /// Flags属性付きのenumをprotobuf-netが上手く処理できないため、
        /// シリアライズ用のプロパティを作っています。
        /// </summary>
        [DataMember(Order = 21, IsRequired = true)]
        private int MirrorCommentTypeMaskForSerialize
        {
            get
            {
                return (int)MirrorCommentTypeMask;
            }
            set
            {
                MirrorCommentTypeMask = (NotificationTypeMask)value;
            }
        }

        /// <summary>
        /// メッセージ確認コメントを放送に投稿するかどうかを取得または設定します。
        /// </summary>
        [DependOnProperty("MirrorCommentTypeMask")]
        public bool IsPostMirrorMessageComment
        {
            get
            {
                return (MirrorCommentTypeMask &
                    NotificationTypeMask.Message) != 0;
            }
            set
            {
                if (value)
                {
                    MirrorCommentTypeMask |= NotificationTypeMask.Message;
                }
                else
                {
                    MirrorCommentTypeMask &= ~NotificationTypeMask.Message;
                }
            }
        }

        /// <summary>
        /// 投票確認コメントを放送に投稿するかどうかを取得または設定します。
        /// </summary>
        [DependOnProperty("MirrorCommentTypeMask")]
        public bool IsPostMirrorVoteComment
        {
            get
            {
                return (MirrorCommentTypeMask &
                    NotificationTypeMask.Vote) != 0;
            }
            set
            {
                if (value)
                {
                    MirrorCommentTypeMask |= NotificationTypeMask.Vote;
                }
                else
                {
                    MirrorCommentTypeMask &= ~NotificationTypeMask.Vote;
                }
            }
        }

        /// <summary>
        /// 参加確認コメントを放送に投稿するかどうかを取得または設定します。
        /// </summary>
        [DependOnProperty("MirrorCommentTypeMask")]
        public bool IsPostMirrorJoinComment
        {
            get
            {
                return (MirrorCommentTypeMask &
                    NotificationTypeMask.Join) != 0;
            }
            set
            {
                if (value)
                {
                    MirrorCommentTypeMask |= NotificationTypeMask.Join;
                }
                else
                {
                    MirrorCommentTypeMask &= ~NotificationTypeMask.Join;
                }
            }
        }

        /// <summary>
        /// 延長確認コメントを放送に投稿するかどうかを取得または設定します。
        /// </summary>
        [DependOnProperty("MirrorCommentTypeMask")]
        public bool IsPostMirrorTimeExtendComment
        {
            get
            {
                return (MirrorCommentTypeMask &
                    NotificationTypeMask.TimeExtend) != 0;
            }
            set
            {
                if (value)
                {
                    MirrorCommentTypeMask |= NotificationTypeMask.TimeExtend;
                }
                else
                {
                    MirrorCommentTypeMask &= ~NotificationTypeMask.TimeExtend;
                }
            }
        }

        /// <summary>
        /// 評価値コメントを放送に投稿するかどうかを取得または設定します。
        /// </summary>
        [DependOnProperty("MirrorCommentTypeMask")]
        public bool IsPostMirrorEvaluationComment
        {
            get
            {
                return (MirrorCommentTypeMask &
                    NotificationTypeMask.Evaluation) != 0;
            }
            set
            {
                if (value)
                {
                    MirrorCommentTypeMask |= NotificationTypeMask.Evaluation;
                }
                else
                {
                    MirrorCommentTypeMask &= ~NotificationTypeMask.Evaluation;
                }
            }
        }
        #endregion

        #region Confirmコメント
        /// <summary>
        /// 自放送からポストされた投票や参加の確認用コメントを放送に投稿するか
        /// どうかを取得または設定します。
        /// </summary>
        [DataMember(Order = 10, IsRequired = true)]
        public bool IsPostConfirmComment
        {
            get
            {
                return this.isPostConfirmComment;
            }
            set
            {
                if (this.isPostConfirmComment != value)
                {
                    this.isPostConfirmComment = value;

                    this.RaisePropertyChanged("IsPostConfirmComment");
                }
            }
        }

        /// <summary>
        /// 放送に投稿する確認コメントの種類を取得または設定します。
        /// </summary>
        public NotificationTypeMask ConfirmCommentTypeMask
        {
            get
            {
                return this.confirmCommentTypeMask;
            }
            set
            {
                value &= NotificationTypeMask.CommentAll;

                if (this.confirmCommentTypeMask != value)
                {
                    this.confirmCommentTypeMask = value;

                    this.RaisePropertyChanged("ConfirmCommentTypeMask");
                }
            }
        }

        /// <summary>
        /// Flags属性付きのenumをprotobuf-netが上手く処理できないため、
        /// シリアライズ用のプロパティを作っています。
        /// </summary>
        [DataMember(Order = 11, IsRequired = true)]
        private int ConfirmCommentTypeMaskForSerialize
        {
            get
            {
                return (int)ConfirmCommentTypeMask;
            }
            set
            {
                ConfirmCommentTypeMask = (NotificationTypeMask)value;
            }
        }

        /// <summary>
        /// メッセージ確認コメントを放送に投稿するかどうかを取得または設定します。
        /// </summary>
        [DependOnProperty("ConfirmCommentTypeMask")]
        public bool IsPostConfirmMessageComment
        {
            get
            {
                return (ConfirmCommentTypeMask &
                    NotificationTypeMask.Message) != 0;
            }
            set
            {
                if (value)
                {
                    ConfirmCommentTypeMask |= NotificationTypeMask.Message;
                }
                else
                {
                    ConfirmCommentTypeMask &= ~NotificationTypeMask.Message;
                }
            }
        }

        /// <summary>
        /// 投票確認コメントを放送に投稿するかどうかを取得または設定します。
        /// </summary>
        [DependOnProperty("ConfirmCommentTypeMask")]
        public bool IsPostConfirmVoteComment
        {
            get
            {
                return (ConfirmCommentTypeMask &
                    NotificationTypeMask.Vote) != 0;
            }
            set
            {
                if (value)
                {
                    ConfirmCommentTypeMask |= NotificationTypeMask.Vote;
                }
                else
                {
                    ConfirmCommentTypeMask &= ~NotificationTypeMask.Vote;
                }
            }
        }

        /// <summary>
        /// 参加確認コメントを放送に投稿するかどうかを取得または設定します。
        /// </summary>
        [DependOnProperty("ConfirmCommentTypeMask")]
        public bool IsPostConfirmJoinComment
        {
            get
            {
                return (ConfirmCommentTypeMask &
                    NotificationTypeMask.Join) != 0;
            }
            set
            {
                if (value)
                {
                    ConfirmCommentTypeMask |= NotificationTypeMask.Join;
                }
                else
                {
                    ConfirmCommentTypeMask &= ~NotificationTypeMask.Join;
                }
            }
        }

        /// <summary>
        /// 延長確認コメントを放送に投稿するかどうかを取得または設定します。
        /// </summary>
        [DependOnProperty("ConfirmCommentTypeMask")]
        public bool IsPostConfirmTimeExtendComment
        {
            get
            {
                return (ConfirmCommentTypeMask &
                    NotificationTypeMask.TimeExtend) != 0;
            }
            set
            {
                if (value)
                {
                    ConfirmCommentTypeMask |= NotificationTypeMask.TimeExtend;
                }
                else
                {
                    ConfirmCommentTypeMask &= ~NotificationTypeMask.TimeExtend;
                }
            }
        }

        /// <summary>
        /// 評価値コメントを放送に投稿するかどうかを取得または設定します。
        /// </summary>
        [DependOnProperty("ConfirmCommentTypeMask")]
        public bool IsPostConfirmEvaluationComment
        {
            get
            {
                return (ConfirmCommentTypeMask &
                    NotificationTypeMask.Evaluation) != 0;
            }
            set
            {
                if (value)
                {
                    ConfirmCommentTypeMask |= NotificationTypeMask.Evaluation;
                }
                else
                {
                    ConfirmCommentTypeMask &= ~NotificationTypeMask.Evaluation;
                }
            }
        }
        #endregion

        /// <summary>
        /// オブジェクトの妥当性を確認します。
        /// </summary>
        public bool Validate()
        {
            // TODO
            return true;
        }
    }
}
