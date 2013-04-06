using System;
using System.Collections.Generic;
using System.Linq;

using Ragnarok;
using Ragnarok.Net;

namespace VoteSystem.Protocol
{
    using Vote;

    /// <summary>
    /// ユーティリティクラス。
    /// </summary>
    public static class ProtocolUtil
    {
        /// <summary>
        /// bool?から<see cref="BoolObject"/>に変換します。
        /// </summary>
        public static BoolObject ToBoolObject(bool? value)
        {
            if (value == null)
            {
                return null;
            }

            return new BoolObject()
            {
                Value = value.Value
            };
        }

        /// <summary>
        /// <see cref="BoolObject"/>からbool?に変換します。
        /// </summary>
        public static bool? ToBool(BoolObject value)
        {
            if (value == null)
            {
                return null;
            }

            return value.Value;
        }

        /// <summary>
        /// 参加者のIDをパースし正しいか確認します。
        /// </summary>
        public static Guid? ParseId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Log.Error(
                    "IDがありません。");
                return null;
            }

            Guid result;
            try
            {
                // monoにはTryParseもParseも無い場合があるため、
                // 例外を発生させて上手くパースできるか調べています。
                result = new Guid(id);
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "IDの解析に失敗しました。");
                return null;
            }

            if (result == Guid.Empty)
            {
                Log.Error(
                    "IDがGuid.Emptyと等価です。");
                return null;
            }

            return result;
        }

        /// <summary>
        /// 投票ルームのオーナー名が正しいか確認します。
        /// </summary>
        public static bool CheckVoteRoomOwnerName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Log.Error(
                    "投票ルームのオーナー名がありません。");

                return false;
            }

            if (name.Length > 16)
            {
                Log.Error(
                    "投票ルームのオーナー名が最大文字数を超えています。");

                return false;
            }

            return true;
        }

        /// <summary>
        /// 投票ルーム名が正しいか確認します。
        /// </summary>
        public static bool CheckVoteRoomName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Log.Error(
                    "投票ルーム名がありません。");

                return false;
            }

            if (name.Length > 32)
            {
                Log.Error(
                    "投票ルーム名が最大文字数を超えています。");

                return false;
            }

            return true;
        }

        /// <summary>
        /// 現在残っている全投票時間を計算します。
        /// </summary>
        public static TimeSpan CalcTotalVoteLeaveTime(VoteState state,
                                                      DateTime startTimeNtp,
                                                      TimeSpan totalSpan)
        {
            // 時間無制限
            if (totalSpan == TimeSpan.MaxValue)
            {
                return TimeSpan.MaxValue;
            }

            // 残り時間を計算します。
            var nowTimeNtp = NtpClient.GetTime();
            switch (state)
            {
                case VoteState.Voting:
                    // 終了時刻から現在時刻を減算し、残り時間を出します。
                    var endTimeNtp = startTimeNtp + totalSpan;
                    return (endTimeNtp - nowTimeNtp);
                case VoteState.Pause:
                case VoteState.Stop:
                case VoteState.End:
                    // 投票が動いていないときは、とりあえず全部です。
                    return totalSpan;
            }

            return TimeSpan.Zero;
        }

        /// <summary>
        /// 投票の残り時間を取得します。
        /// </summary>
        public static TimeSpan CalcVoteLeaveTime(VoteState state,
                                                 DateTime startTimeNtp,
                                                 TimeSpan voteSpan)
        {
            // 時間無制限
            if (voteSpan == TimeSpan.MaxValue)
            {
                return TimeSpan.MaxValue;
            }

            // 残り時間を計算します。
            var nowTimeNtp = NtpClient.GetTime();
            switch (state)
            {
                case VoteState.Voting:
                    // 終了時刻から現在時刻を減算し、残り時間を出します。
                    var endTimeNtp = startTimeNtp + voteSpan;
                    return (endTimeNtp - nowTimeNtp);
                case VoteState.Pause:
                    // 一時停止中
                    return voteSpan;
                case VoteState.Stop:
                case VoteState.End:
                    // 投票していないときは残り時間０になります。
                    return TimeSpan.Zero;
            }

            return TimeSpan.Zero;
        }

        /// <summary>
        /// 投票状態を文字列に変換します。
        /// </summary>
        public static string GetVoteStateText(VoteState state)
        {
            var label = EnumEx.GetLabel(state);
            if (label == null)
            {
                return "不明な状態";
            }

            return label;
        }

        /// <summary>
        /// 指定のタイプがマスクに含まれているか調べます。
        /// </summary>
        private static bool HasFlag(NotificationTypeMask notificationMask,
                                    NotificationType notificationType)
        {
            var value = (1 << (int)notificationType);

            return (((int)notificationMask & value) != 0);
        }

        /// <summary>
        /// ミラーコメントの印です。
        /// </summary>
        public const char MirrorCommentMark = '\u200C';

        /// <summary>
        /// 必要なら先頭に無幅空白を追加します。
        /// </summary>
        public static string MakeMirrorComment(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            if (IsMirrorComment(text))
            {
                // ミラーコメントならそのまま。
                return text;
            }
            else
            {
                return (MirrorCommentMark + text);
            }
        }

        /// <summary>
        /// ミラーコメントか判断します。
        /// </summary>
        public static bool IsMirrorComment(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            if (text[0] == MirrorCommentMark)
            {
                return true;
            }

            // たいていのコメビュではコメントの先頭が'/'であるかどうかで、
            // 読み上げるかどうかを決定しているため、
            // 読み上げたくない場合は先頭に'/'をつける必要があります。
            // ミラーコメントに読み上げない設定をつけたければ、
            // '/'コメントをミラーとして扱うしかありません。
            // また、'/'コメントにはコマンドも多いので、
            // 他放送に拡散させないためミラーコメントとして扱っています。
            return (text[0] == '/');
        }

        /// <summary>
        /// 通知をニコ生のコメントとして投稿するか調べます。
        /// </summary>
        public static bool IsPostComment(Notification notification, bool isMirrorMode,
                                         LiveAttribute attribute, LiveData myLiveData)
        {
            if (notification == null || !notification.Validate())
            {
                return false;
            }

            if (attribute == null || !attribute.Validate())
            {
                return false;
            }

            if (notification.Type == NotificationType.System ||
                notification.Type == NotificationType.Important)
            {
                return HasFlag(
                    attribute.SystemCommentTypeMask,
                    notification.Type);
            }

            // とりあえず全コメントをミラーします。
            // 同じ放送ルームから投稿されたコメントはミラーしませんが、
            // ルーム番号が分からないので、ここではその判断ができません。
            if (isMirrorMode)
            {
                return true;
            }

            // もしメッセージの送信元と送信先が同じなら
            // そこには再投稿しない可能性があります。
            // (オプションによって変わります)
            if (notification.FromLiveRoom != null &&
                notification.FromLiveRoom.LiveData == myLiveData)
            {
                // 自放送からキタ通知の場合。
                if (!attribute.IsPostConfirmComment)
                {
                    return false;
                }

                return HasFlag(
                    attribute.ConfirmCommentTypeMask,
                    notification.Type);
            }
            else
            {
                // 他放送からキタ通知の場合。
                if (!attribute.IsPostMirrorComment)
                {
                    return false;
                }

                return HasFlag(
                    attribute.MirrorCommentTypeMask,
                    notification.Type);
            }
        }
    }
}
