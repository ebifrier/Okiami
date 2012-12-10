using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;

using Ragnarok;
using Ragnarok.Utility;
using Ragnarok.ObjectModel;
using Ragnarok.Net.ProtoBuf;

namespace VoteSystem.Server
{
    using VoteSystem.Protocol;
    using VoteSystem.Protocol.Vote;

    /// <summary>
    /// 投票の票管理を行います。
    /// </summary>
    public sealed class VoteModel : NotifyObject, ILogObject
    {
        private readonly VoteRoom voteRoom;
        private readonly Dictionary<VoteMode, VoteStrategy.IVoteStrategy> strategyList =
            new Dictionary<VoteMode, VoteStrategy.IVoteStrategy>();
        private VoteStrategy.IVoteStrategy voteStrategy;
        private VoteMode voteMode = VoteMode.Shogi;
        private bool isVoteResultChanged = true;

        private readonly Dictionary<string, TimeExtendKind> timeExtendDic =
            new Dictionary<string, TimeExtendKind>();
        private readonly Queue<double> evaluationPointQueue = new Queue<double>();

        /// <summary>
        /// ログ表示名を取得します。
        /// </summary>
        public string LogName
        {
            get
            {
                return "投票オブジェクト";
            }
        }

        /// <summary>
        /// 投票モードを取得または設定します。
        /// </summary>
        public VoteMode VoteMode
        {
            get
            {
                return this.voteMode;
            }
            private set
            {
                using (LazyLock())
                {
                    if (this.voteMode != value)
                    {
                        this.voteMode = value;

                        this.RaisePropertyChanged("VoteMode");
                    }
                }
            }
        }

        /// <summary>
        /// 投票結果を取得します。
        /// </summary>
        public VoteResult VoteResult
        {
            get
            {
                var candidateList = this.voteStrategy.GetVoteResult();
                if (candidateList == null)
                {
                    return new VoteResult();
                }

                var result = new VoteResult()
                {
                    EvaluationPoint = EvaluationPoint,
                    TimeExtendPoint = TimeExtendPoint,
                    TimeStablePoint = TimeStablePoint,
                    CandidateList = candidateList,
                };

                // 投票された各候補とそのポイントを取得します。
                if (result.CandidateList == null)
                {
                    result.CandidateList = new VoteCandidatePair[0];
                }
                else if (result.CandidateList.Length > 30)
                {
                    // 送る結果は最大で３０に制限しています。
                    result.CandidateList =
                        result.CandidateList.Take(30).ToArray();
                }

                return result;
            }
        }

        /// <summary>
        /// 投票結果が変わったときに呼ばれます。
        /// </summary>
        public void OnVoteResultChanged()
        {
            isVoteResultChanged = true;
        }

        /// <summary>
        /// 投票結果が変わっていたときのみ投票結果を取得します。
        /// </summary>
        public VoteResult GetVoteResultIfChanged()
        {
            if (!this.isVoteResultChanged)
            {
                return null;
            }

            // 先に変更フラグを偽にしておきます。
            // フラグ変更と結果取得の間で投票結果が変わっても、
            // 次の投票結果取得時に余分に結果が取得されるだけです。
            this.isVoteResultChanged = false;

            return VoteResult;
        }

        /// <summary>
        /// 投票モードを変更します。
        /// </summary>
        public void ChangeMode(VoteMode mode)
        {
            using (LazyLock())
            {
                if (this.voteRoom.VoteTimeKeeper.VoteState != VoteState.Stop)
                {
                    Log.Error(this,
                        "投票停止中でないとモードの変更はできません。");
                    return;
                }

                // strategyは投票参加者などを保存するために、
                // 固定のオブジェクトを使います。
                VoteStrategy.IVoteStrategy strategy = null;
                if (!this.strategyList.TryGetValue(mode, out strategy))
                {
                    throw new NotImplementedException(
                        "与えられた投票モードはまだ実装されていません。");
                }

                // 投票戦略を変更します。
                this.voteStrategy = strategy;
                this.VoteMode = mode;

                OnVoteResultChanged();
            }
        }        

        /// <summary>
        /// 投票結果をすべてクリアします。
        /// </summary>
        public void ClearVote()
        {
            using (LazyLock())
            {
                // 投票結果を消す前にそれをログに残しておきます。
                var result = VoteResult;
                var strs = result.CandidateList.Select(pair =>
                    string.Format("{0}: {1}", pair.Candidate, pair.Point));

                Log.Info(this,
                    "投票結果{0}" +
                    "  {1}",
                    Environment.NewLine,
                    string.Join(Environment.NewLine + "  ", strs.ToArray()));

                this.voteStrategy.ClearVote();
                OnVoteResultChanged();
            }

            Log.Info(this,
                "投票結果がすべてクリアされました。");
        }

        /// <summary>
        /// シグナル受信時に呼ばれます。
        /// </summary>
        public void SignalReceived(int signum)
        {
            var strategy = this.voteStrategy;
            if (strategy != null)
            {
                strategy.SignalReceived(signum);
            }
        }

        #region 通知処理
        /// <summary>
        /// 投票ルームにたいする各種操作を行います。
        /// </summary>
        public void ProcessNotification(Notification notification,
                                        bool isFromVoteRoomOwner)
        {
            if (notification == null || !notification.Validate())
            {
                throw new ArgumentException("notification");
            }

            if (string.IsNullOrEmpty(notification.Text))
            {
                return;
            }

            Log.Info(this,
                "通知を処理します。(Text = '{0}')",
                notification.Text);

            if (ProcessMessage(notification, isFromVoteRoomOwner))
            {
                return;
            }
            if (ProcessTimeExtend(notification, isFromVoteRoomOwner))
            {
                return;
            }
            if (ProcessEvaluation(notification, isFromVoteRoomOwner))
            {
                return;
            }

            // 一応
            var voteStrategy = this.voteStrategy;
            if (voteStrategy != null)
            {
                // 各種通知を処理します。
                voteStrategy.ProcessNotification(
                    notification, isFromVoteRoomOwner);

                // 指し手として通知を処理します。
                // これは投票期間中にしか受理されません。
                if (this.voteRoom.VoteTimeKeeper.VoteState == VoteState.Voting)
                {
                    voteStrategy.ProcessVoteNotification(
                        notification, isFromVoteRoomOwner);
                }
            }
        }

        #region メッセージ
        /// <summary>
        /// 重要メッセージの正規表現です。
        /// </summary>
        private static readonly Regex importantRegex =
            new Regex(@"^/important\s+", RegexOptions.Compiled);

        /// <summary>
        /// 通常メッセージの正規表現です。
        /// </summary>
        private static readonly Regex messageRegex =
            new Regex(@"^(msg|拡散)\s*", RegexOptions.Compiled);

        /// <summary>
        /// メッセージを処理します。
        /// </summary>
        private bool ProcessMessage(Notification notification,
                                    bool isFromVoteRoomOwner)
        {
            // 重要メッセージを処理します。
            var m = importantRegex.Match(notification.Text);
            if (m.Success)
            {
                if (!isFromVoteRoomOwner)
                {
                    throw new InvalidOperationException(
                        "投票ルームのオーナーに重要メッセージは送れません。");
                }

                Log.Info(this,
                    "重要メッセージを処理しました。('{0}')",
                    notification.Text);

                var text = notification.Text.Substring(m.Length);
                this.voteRoom.BroadcastNotification(
                    text, NotificationType.Important, notification,
                    true, false);
                return true;
            }

            // メッセージを再度送り返します。
            m = messageRegex.Match(notification.Text);
            if (m.Success)
            {
                var text = notification.Text.Substring(m.Length);

                this.voteRoom.BroadcastNotification(
                    text, NotificationType.Message, notification,
                    true, true);
                return true;
            }

            return false;
        }
        #endregion

        #region 評価値
        /// <summary>
        /// 評価値を取得します。
        /// </summary>
        public double EvaluationPoint
        {
            get
            {
                lock (this.evaluationPointQueue)
                {
                    if (!this.evaluationPointQueue.Any())
                    {
                        return 0.0;
                    }

                    // 単純に平均した値を返します。
                    return this.evaluationPointQueue.Average();
                }
            }
        }

        /// <summary>
        /// 顔文字と評価値の対応表です。TODO
        /// </summary>
        private static readonly Dictionary<string, double> evaluationPointTextDic =
            new Dictionary<string, double>()
            {
                {"(`･ω･´)", 1000},
                {"(´･ω･`)", -500}
            };

        /// <summary>
        /// 顔文字などを評価値に直します。
        /// </summary>
        private double? ConvertToEvaluationPoint(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            // とりあえず正規化します。
            var normalized = StringNormalizer.NormalizeText(text);

            // まず数値として解析します。
            double value;
            if (double.TryParse(normalized, out value))
            {
                // パースに成功したらその値を返します。
                return value;
            }

            // 顔文字として検索します。
            lock (evaluationPointTextDic)
            {
                foreach (var pair in evaluationPointTextDic)
                {
                    if (text.StartsWith(pair.Key))
                    {
                        return pair.Value;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 評価値の設定を行います。
        /// </summary>
        private void AddEvaluationPoint(string text, Notification source)
        {
            // 評価値に直します。
            var value = ConvertToEvaluationPoint(text);
            if (value == null)
            {
                return;
            }

            // 値を9999～-9999の範囲に収めます。
            value = Math.Min(value.Value, 9999.0);
            value = Math.Max(value.Value, -9999.0);

            // 投票者と評価値をヒモ付けます。
            lock (this.evaluationPointQueue)
            {
                // 評価値が大きく変わった場合、昔の評価値が残っていると
                // 新しい評価値の計算に大きな影響が出るため、
                // 最新の評価値ｎ個の平均値を取るようにしています。
                while (this.evaluationPointQueue.Count() > 5)
                {
                    this.evaluationPointQueue.Dequeue();
                }

                this.evaluationPointQueue.Enqueue(value.Value);
            }

            OnVoteResultChanged();

            Log.Info(this,
                "評価値={0}({1})が受理されました。",
                text, value.Value);

            // コメントされた評価値を全ユーザーに通知します。
            this.voteRoom.BroadcastNotification(
                "評価値 " + value.Value.ToString(),
                NotificationType.Evaluation,
                source,
                true, true);
        }

        /// <summary>
        /// 評価値ポイントをクリアします。
        /// </summary>
        private void ClearEvaluationPoint()
        {
            lock (this.evaluationPointQueue)
            {
                this.evaluationPointQueue.Clear();
            }

            OnVoteResultChanged();

            Log.Info(this,
                "評価ポイントをクリアしました。");
        }

        /// <summary>
        /// 評価値コマンドの正規表現です。
        /// </summary>
        private static readonly Regex evaluationRegex =
            new Regex(@"^(point|評価値|評価)\s*", RegexOptions.Compiled);

        /// <summary>
        /// 評価値クリアコマンドの正規表現です。
        /// </summary>
        private static readonly Regex clearEvaluationRegex =
            new Regex(@"^/clearevaluationpoints");

        /// <summary>
        /// 評価値コマンドを処理します。
        /// </summary>
        private bool ProcessEvaluation(Notification notification,
                                       bool isFromVoteRoomOwner)
        {
            // 評価値設定コマンド
            var m = evaluationRegex.Match(notification.Text);
            if (m.Success)
            {
                var text = notification.Text.Substring(m.Length);

                AddEvaluationPoint(text, notification);
                return true;
            }

            // 評価値クリアコマンド
            m = clearEvaluationRegex.Match(notification.Text);
            if (m.Success)
            {
                if (!isFromVoteRoomOwner)
                {
                    throw new InvalidOperationException(
                        "評価値クリアは投票ルームのオーナーしか実行できません。");
                }

                ClearEvaluationPoint();
                return true;
            }

            return false;
        }
        #endregion

        #region 時間延長コマンド
        /// <summary>
        /// 時間延長に対して投じられたポイント数を取得します。
        /// </summary>
        public int TimeExtendPoint
        {
            get
            {
                lock (this.timeExtendDic)
                {
                    return this.timeExtendDic.Count(
                        pair => pair.Value == TimeExtendKind.Extend);
                }
            }
        }

        /// <summary>
        /// 時間延長拒否に対して投じられたポイント数を取得します。
        /// </summary>
        public int TimeStablePoint
        {
            get
            {
                lock (this.timeExtendDic)
                {
                    return this.timeExtendDic.Count(
                        pair => pair.Value == TimeExtendKind.Stable);
                }
            }
        }

        /// <summary>
        /// 延長要求コマンドの正規表現です。
        /// </summary>
        private static Regex ExtendRegex = new Regex(
            @"^(MOTTOMOTTO|MOTMOT|HOTTOMOTTO|HOTMOT|もっともっと|ほっともっと)(!|！)*",
            RegexOptions.IgnoreCase);

        /// <summary>
        /// 時間短縮要求コマンドの正規表現です。
        /// </summary>
        private static Regex StableRegex = new Regex(
            @"^(あ|あっ)?(、)?(もう)?(、)?(けっこうです|かっこうです|結構です|いりません|格好です|月光です)(。)?",
            RegexOptions.IgnoreCase);

        /// <summary>
        /// 時間短縮要求コマンドの正規表現です。
        /// </summary>
        private static Regex StableRegex2 = new Regex(
            @"^(主の)?遊びは(ここ|そこ)まで(だ)?(。)?",
            RegexOptions.IgnoreCase);

        /// <summary>
        /// 時間延長に関わるコメントであるか調べます。
        /// </summary>
        /// <remarks>
        /// 受け入れ可能な延長コメントの種類。
        /// 1, MOTMOT
        /// 2, もっともっと
        /// 3, ほっともっと
        /// 短縮コメント
        /// 最初に"あ、", "あっ", "もう"がついてもおｋ
        /// 5, 結構です
        /// 6, いりません
        /// </remarks>
        private TimeExtendKind? ParseTimeExtendText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            // とりあえず文字列を正規化します。
            var normalizedText = StringNormalizer.NormalizeText(
                text, NormalizeTextOption.Kana);

            if (ExtendRegex.IsMatch(normalizedText))
            {
                return TimeExtendKind.Extend;
            }

            if (StableRegex.IsMatch(normalizedText) ||
                StableRegex2.IsMatch(normalizedText))
            {
                return TimeExtendKind.Stable;
            }

            return null;
        }

        /// <summary>
        /// 時間延長に関わる票を追加します。
        /// </summary>
        private bool VoteTimeExtendDemand(Notification notification,
                                          TimeExtendKind kind)
        {
            var voterId = notification.VoterId;
            if (string.IsNullOrEmpty(voterId))
            {
                return false;
            }

            lock (this.timeExtendDic)
            {
                // 同じ値なら何も処理しません。
                /*TimeExtendKind value;
                if (this.timeExtendDic.TryGetValue(voterId, out value) &&
                    value == kind)
                {
                    return false;
                }*/

                this.timeExtendDic[voterId] = kind;
            }

            OnVoteResultChanged();

            Log.Info(this,
                "時間延長{0}を求める票が投票されました。",
                (kind == TimeExtendKind.Extend ? "" : "拒否"));

            this.voteRoom.BroadcastNotification(
                NotificationType.TimeExtend,
                notification,
                true, true);

            return true;
        }

        /// <summary>
        /// 延長要求による投票時間の調整を行います。
        /// </summary>
        private void ChangeTimeFromTimeExtendDemand(TimeExtendKind kind)
        {
            var timeKeeper = this.voteRoom.VoteTimeKeeper;

            // 5票以上集まったら、投票自体を打ち切ります。
            if (TimeStablePoint >= 5)
            {
                timeKeeper.SetVoteSpan(TimeSpan.Zero);
                return;
            }

            if (kind == TimeExtendKind.Extend)
            {
                timeKeeper.AddVoteSpan(TimeSpan.FromMinutes(2));
            }
            else
            {
                timeKeeper.AddVoteSpan(-TimeSpan.FromMinutes(2));
            }
        }

        /// <summary>
        /// 延長要求の結果をクリアします。
        /// </summary>
        public void ClearTimeExtendDemand()
        {
            lock (this.timeExtendDic)
            {
                this.timeExtendDic.Clear();
            }

            OnVoteResultChanged();

            Log.Info(this,
                "時間延長要求の結果がクリアされました。");
        }

        /// <summary>
        /// 時間延長コマンドを処理します。
        /// </summary>
        private bool ProcessTimeExtend(Notification notification,
                                       bool isFromVoteRoomOwner)
        {
            var kind = ParseTimeExtendText(notification.Text);
            if (kind != null)
            {
                if (VoteTimeExtendDemand(notification, kind.Value))
                {
                    // 評価値は一人一票です。
                    // 実際に延長要求値が変わったら、時間も変えます。
                    ChangeTimeFromTimeExtendDemand(kind.Value);
                }

                return true;
            }

            if (notification.Text == "/cleartimeextenddemand")
            {
                if (!isFromVoteRoomOwner)
                {
                    throw new InvalidOperationException(
                        "延長要求クリアは投票ルームのオーナーしか実行できません。");
                }

                ClearTimeExtendDemand();
                return true;
            }

            return false;
        }
        #endregion
        #endregion

        #region コマンド処理
        /// <summary>
        /// コネクションにコマンドなどのハンドラを登録します。
        /// </summary>
        public void ConnectHandlers(PbConnection connection)
        {
            connection.AddCommandHandler<ChangeVoteModeCommand>(
               HandleChangeVoteModeCommand);
            connection.AddCommandHandler<NotificationCommand>(
               HandleNotificationCommand);
            connection.AddCommandHandler<ClearVoteCommand>(
               HandleClearVoteCommand);

            foreach (var strategy in this.strategyList)
            {
                strategy.Value.ConnectHandlers(connection);
            }
        }

        /// <summary>
        /// コネクションからコマンドなどのハンドラを削除します。
        /// </summary>
        public void DisconnectHandlers(PbConnection connection)
        {
            foreach (var strategy in this.strategyList)
            {
                strategy.Value.DisconnectHandlers(connection);
            }

            connection.RemoveHandler<ChangeVoteModeCommand>();
            connection.RemoveHandler<NotificationCommand>();
            connection.RemoveHandler<ClearVoteCommand>();
        }

        /// <summary>
        /// 投票モードを変更します。
        /// </summary>
        private void HandleChangeVoteModeCommand(
            object sender, PbCommandEventArgs<ChangeVoteModeCommand> e)
        {
            var mode = e.Command.VoteMode;
            if (!Enum.IsDefined(typeof(VoteMode), mode))
            {
                throw new ArgumentException("e",
                    string.Format(
                        "投票モードの値({0})が正しくありません。",
                        (int)mode));
            }

            // 投票モードの変更を行います。
            ChangeMode(mode);
        }

        /// <summary>
        /// 通知メッセージを処理します。
        /// </summary>
        private void HandleNotificationCommand(
            object sender, PbCommandEventArgs<NotificationCommand> e)
        {
            var voteRoom = this.voteRoom;
            if (voteRoom == null)
            {
                throw new InvalidOperationException(
                    "投票ルームに入室していません。");
            }

            // メッセージが投票ルームのオーナーによるものかを判定します。
            // 放送のコメントすべてが通知として放送主から送られてくるため、
            // 放送主から投稿されたものだけを特別扱いする必要があります。
            var isFromVoteRoomOwner = (
                e.Command.IsFromLiveOwner &&
                voteRoom.IsRoomOwnerConnection(sender as PbConnection));

            // 受信した通知を処理します。
            ProcessNotification(
                e.Command.Notification,
                isFromVoteRoomOwner);
        }

        /// <summary>
        /// 投票結果のクリアコマンドを処理します。
        /// </summary>
        private void HandleClearVoteCommand(
            object sender, PbCommandEventArgs<ClearVoteCommand> e)
        {
            ClearVote();
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VoteModel(VoteRoom voteRoom)
        {
            this.voteRoom = voteRoom;

            // 投票モードが変わっても参加者リストを保存するために、
            // strategyは固定のオブジェクトを使用します。
            //strategyList[VoteMode.Kaos] = new VoteStrategy.KaosVoteStrategy(this);
            strategyList[VoteMode.Shogi] = new VoteStrategy.ShogiVoteStrategy(voteRoom);
            strategyList[VoteMode.Mirror] = new VoteStrategy.MirroringStrategy(voteRoom);

            // VoteRoomの初期化中にここが呼ばれるため、ここでモードを変えると
            // VoteRoomプロパティを参照することによるnullエラーが出ます。
            //ChangeMode(VoteMode.Shogi);
        }
    }
}
