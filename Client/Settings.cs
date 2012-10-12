using System;
using System.Configuration;
using System.ComponentModel;

namespace VoteSystem.Client
{
    using VoteSystem.Client.Model;

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
        private void Settings_SettingsLoaded(object sender, SettingsLoadedEventArgs e)
        {
            if (AS_UserId == Guid.Empty)
            {
                AS_UserId = Guid.NewGuid();
            }

            ValidateAndChangeProperty();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Settings()
        {
            this.SettingsLoaded += Settings_SettingsLoaded;
        }
    }
}
