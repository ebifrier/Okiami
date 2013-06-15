using System;
using System.ComponentModel;

using Ragnarok.Utility;

namespace VoteSystem.Client
{
    using Protocol;
    using Model;

    /// <summary>
    /// 設定を保存するオブジェクトです。
    /// </summary>
    public sealed partial class Settings
    {
        /// <summary>
        /// プロパティの値を確認し、必要なら訂正します。
        /// </summary>
        private void ValidateAndChangeProperty()
        {
            if (VoteStartSystemMessage == null)
            {
                VoteStartSystemMessage = new SystemMessage()
                {
                    CommentText = "投票を開始します"
                };
            }
            if (VoteStopSystemMessage == null)
            {
                VoteStopSystemMessage = new SystemMessage()
                {
                    CommentText = "投票を停止します"
                };
            }
            if (VotePauseSystemMessage == null)
            {
                VotePauseSystemMessage = new SystemMessage()
                {
                    CommentText = "投票を一時停止します"
                };
            }
            if (VoteEndSystemMessage == null)
            {
                VoteEndSystemMessage = new SystemMessage()
                {
                    CommentText = "投票が終了しました"
                };
            }
            if (ChangeVoteSpanSystemMessage == null)
            {
                ChangeVoteSpanSystemMessage = new SystemMessage()
                {
                    CommentText = "投票時間が変わりました"
                };
            }
        }

        /// <summary>
        /// データ読み込み後、UserIdが初期値なら値を設定します。
        /// </summary>
        protected override void OnSettingsLoaded(object sender, SettingsLoadedEventArgs e)
        {
            base.OnSettingsLoaded(sender, e);

#if !PUBLISHED
            if (AS_LoginName == "えびふらい")
            {
                AS_UserId = ProtocolUtil.SpecialGuid;
            }
            else if (AS_UserId == ProtocolUtil.SpecialGuid)
            {
                AS_UserId = Guid.NewGuid();
            }
#endif
            if (AS_UserId == Guid.Empty)
            {
                AS_UserId = Guid.NewGuid();
            }

            if (AS_NicoLiveAttribute == null)
            {
                AS_NicoLiveAttribute = new LiveAttribute();
            }

            // 確認コメントはデフォルトで投稿しない設定にします。
            AS_NicoLiveAttribute.IsPostConfirmComment = false;

            ValidateAndChangeProperty();
        }

        private void AutoSave(object sender, PropertyChangedEventArgs e)
        {
            Save();
        }

        /// <summary>
        /// プロパティ値の変更直前に呼ばれます。
        /// </summary>
        protected override void OnPropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            base.OnPropertyChanging(sender, e);

            if (e.PropertyName == "AS_NicoLiveAttribute" &&
                AS_NicoLiveAttribute != null)
            {
                AS_NicoLiveAttribute.PropertyChanged -= AutoSave;
            }
        }

        /// <summary>
        /// プロパティ値の変更後に呼ばれます。
        /// </summary>
        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(sender, e);

            if (e.PropertyName == "AS_NicoLiveAttribute" &&
                AS_NicoLiveAttribute != null)
            {
                AS_NicoLiveAttribute.PropertyChanged += AutoSave;
            }
        }
    }
}
