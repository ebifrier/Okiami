using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Ragnarok;
using Ragnarok.Utility;
using Ragnarok.Net.ProtoBuf;

namespace VoteSystem.Protocol
{
    /// <summary>
    /// プロトコルのエラーを判別します。
    /// </summary>
    public static class ErrorCode
    {
        /// <summary>
        /// エラー無し
        /// </summary>
        [LabelDescription("エラーはありません。")]
        public const int None = 0;
        /// <summary>
        /// 引数エラー
        /// </summary>
        [LabelDescription("与えられた引数が間違っています。")]
        public const int Argument = 1;
        /// <summary>
        /// 要求は処理できませんでした。
        /// </summary>
        [LabelDescription("要求は処理されませんでした。")]
        public const int Unhandled = 2;
        /// <summary>
        /// 許可がありません。
        /// </summary>
        [LabelDescription("権限がないため与えられた要求が実行できません。")]
        public const int Permission = 3;
        
        /// <summary>
        /// ログインしていません。
        /// </summary>
        [LabelDescription("ログインされていません。")]
        public const int NotLogin = 4;
        /// <summary>
        /// すでにログインしています。
        /// </summary>
        [LabelDescription("すでにログインされています。")]
        public const int AlreadyLogined = 5;

        /// <summary>
        /// 投票ルームの作成に失敗しました。
        /// </summary>
        [LabelDescription("投票ルームの作成に失敗しました。")]
        public const int CreateVoteRoom = 6;
        /// <summary>
        /// 投票ルームが見つかりません。
        /// </summary>
        [LabelDescription("指定のIDの投票ルームが見つかりません。")]
        public const int VoteRoomNotFound = 7;
        /// <summary>
        /// パスワードが一致しませんでした。
        /// </summary>
        [LabelDescription("パスワードが一致しませんでした。")]
        public const int PasswordUnmatched = 8;
        /// <summary>
        /// 投票ルームに入室していません。
        /// </summary>
        [LabelDescription("投票ルームに入室していません。")]
        public const int NotEnteringVoteRoom = 9;
        /// <summary>
        /// 投票ルームにはすでに入室しています。
        /// </summary>
        [LabelDescription("すでに投票ルームに入室しています。")]
        public const int AlreadyEnteredVoteRoom = 10;

        /// <summary>
        /// 放送ルームの操作IDが正しくありません。
        /// </summary>
        [LabelDescription("放送ルームの操作IDが正しくありません。")]
        public const int InvalidLiveOperation = 11;
        /// <summary>
        /// 指定の放送ルームはすでに存在しています。
        /// </summary>
        [LabelDescription("指定の放送ルームはすでに存在しています。")]
        public const int LiveAlreadyExists = 12;
        /// <summary>
        /// 指定の放送ルームは存在しません。
        /// </summary>
        [LabelDescription("指定の放送ルームは存在しません。")]
        public const int LiveNotExists = 13;
    }

    /// <summary>
    /// エラーコード関連のクラスです。
    /// </summary>
    public static class ErrorCodeUtil
    {
        /// <summary>
        /// エラーの概要を取得します。
        /// </summary>
        public static string GetDescription(int error)
        {
            var description = Util.GetFieldDescription(
                typeof(PbErrorCode), error);
            if (!string.IsNullOrEmpty(description))
            {
                return description;
            }

            description = Util.GetFieldDescription(
                typeof(ErrorCode), error);
            if (!string.IsNullOrEmpty(description))
            {
                return description;
            }

            return "不明なエラーです。";
        }
    }
}
