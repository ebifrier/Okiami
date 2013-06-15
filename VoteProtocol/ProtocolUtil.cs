using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;

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
#if !PUBLISHED
        /// <summary>
        /// ツールが落ちても主が強制的にルームオーナーになるために使います。
        /// </summary>
        public static readonly Guid SpecialGuid =
            new Guid(0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11);
#endif

        /// <summary>
        /// 全投票時間を出力するためのファイル名です。
        /// </summary>
        public static readonly string TotalVoteSpanFilePath =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "co516151/VoteClient/voteleavetime.tmp");

        /// <summary>
        /// 全投票時間をファイルに出力します。
        /// </summary>
        /// <remarks>
        /// 他ツールとの連携のために使います。
        /// </remarks>
        public static void WriteTotalVoteSpan(VoteState state,
                                              DateTime startTimeNtp,
                                              TimeSpan totalSpan)
        {
            try
            {
                var path = ProtocolUtil.TotalVoteSpanFilePath;
                var c = CultureInfo.InvariantCulture;

                using (var stream = new FileStream(path, FileMode.Create))
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine((int)state);
                    writer.WriteLine(startTimeNtp.ToString("o", c));
                    writer.WriteLine(totalSpan);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "全投票期間出力ファイルの保存に失敗しました。");
            }
        }

        /// <summary>
        /// 全投票時間などをファイルから読み込みます。
        /// </summary>
        /// <remarks>
        /// 他ツールとの連携のために使います。
        /// </remarks>
        public static bool ReadTotalVoteSpan(out VoteState state,
                                             out DateTime startTimeNtp,
                                             out TimeSpan totalSpan)
        {
            try
            {
                var path = ProtocolUtil.TotalVoteSpanFilePath;
                var c = CultureInfo.InvariantCulture;

                if (!File.Exists(path))
                {
                    state = VoteState.Stop;
                    startTimeNtp = DateTime.MinValue;
                    totalSpan = TimeSpan.Zero;
                    return false;
                }

                using (var stream = new FileStream(path, FileMode.Open))
                using (var reader = new StreamReader(stream))
                {
                    state = (VoteState)int.Parse(reader.ReadLine());
                    startTimeNtp = DateTime.ParseExact(reader.ReadLine(), "o", c);
                    totalSpan = TimeSpan.Parse(reader.ReadLine());

                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "全投票期間出力ファイルの読み込みに失敗しました。");

                state = VoteState.Stop;
                startTimeNtp = DateTime.MinValue;
                totalSpan = TimeSpan.Zero;
                return false;
            }
        }

        /// <summary>
        /// 全投票時間をファイルから読み込みます。
        /// </summary>
        /// <remarks>
        /// 他ツールとの連携のために使います。
        /// </remarks>
        public static TimeSpan ReadTotalVoteSpan()
        {
            try
            {
                VoteState state;
                DateTime startTimeNtp;
                TimeSpan totalSpan;

                if (ReadTotalVoteSpan(out state, out startTimeNtp, out totalSpan))
                {
                    return CalcTotalVoteLeaveTime(state, startTimeNtp, totalSpan);
                }
                else
                {
                    return TimeSpan.MinValue;
                }
            }
            catch
            {
                return TimeSpan.MinValue;
            }
        }

        /// <summary>
        /// 全投票時間を出力したファイルを削除します。
        /// </summary>
        public static void RemoveTotalVoteSpan()
        {
            try
            {
                var path = ProtocolUtil.TotalVoteSpanFilePath;

                File.Delete(path);
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "全投票期間出力ファイルの削除に失敗しました。");
            }
        }

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
        /// 全投票時間の残り時間を計算します。
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
                    return MathEx.Max(endTimeNtp - nowTimeNtp, TimeSpan.Zero);
                case VoteState.Pause:
                case VoteState.Stop:
                case VoteState.End:
                    // 投票が動いていないときは、とりあえず全部です。
                    return totalSpan;
            }

            return TimeSpan.Zero;
        }

        /// <summary>
        /// 投票の残り時間を計算します。
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
                    return MathEx.Max(endTimeNtp - nowTimeNtp, TimeSpan.Zero);
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
        /// 思考時間を計算します。
        /// </summary>
        public static TimeSpan CalcThinkTime(VoteState state,
                                             DateTime startTimeNtp,
                                             TimeSpan progressSpan)
        {
            return CalcThinkTime(
                state, startTimeNtp, progressSpan, TimeSpan.MinValue);
        }

        /// <summary>
        /// 思考時間を計算します。
        /// </summary>
        /// <remarks>
        /// <paramref name="voteSpan"/>は時刻のミリ秒を
        /// 合わせるために使います。
        /// そうしないと、投票時間と思考時間で時刻の
        /// 表示タイミングがずれてしまいます。
        /// </remarks>
        public static TimeSpan CalcThinkTime(VoteState state,
                                             DateTime startTimeNtp,
                                             TimeSpan progressSpan,
                                             TimeSpan voteSpan)
        {
            var nowTimeNtp = NtpClient.GetTime();
            var span = TimeSpan.Zero;
            switch (state)
            {
                case VoteState.Voting:
                    // 今までの経過時間 + (現在時刻 - 開始時刻) です。
                    span = progressSpan + (nowTimeNtp - startTimeNtp);
                    break;
                case VoteState.Pause:
                    span = progressSpan;
                    break;
                case VoteState.Stop:
                case VoteState.End:
                    return TimeSpan.Zero;
            }

            // 投票残り時間とミリ秒以下の端数を合わせます。
            // 
            // 投票時間は減る、思考時間は増えるため、
            // "ミリ秒の最大値 - ミリ秒" を思考時間に加算します。
            var millis = TimeSpan.FromMilliseconds(
                voteSpan == TimeSpan.MinValue || voteSpan == TimeSpan.MaxValue ?
                0 :
                999 - voteSpan.Milliseconds);

            return (span + millis);
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
