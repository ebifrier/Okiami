using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Ragnarok;
using Ragnarok.Utility;
using Ragnarok.Shogi;
using Ragnarok.Net.ProtoBuf;

namespace VoteSystem.Server.VoteStrategy
{
    using VoteSystem.Protocol;
    using VoteSystem.Protocol.Vote;

    /// <summary>
    /// 将棋の投票を行います。
    /// </summary>
    internal sealed class ShogiVoteStrategy : IVoteStrategy
    {
        private readonly object SyncRoot = new object();

        private readonly VoteRoom voteRoom;
        private readonly PlayerPool playerPool = new PlayerPool();
        private readonly MoveStatistics moveStatistics = new MoveStatistics();
        private Board board = new Board();

        /// <summary>
        /// ログ表示名を取得します。
        /// </summary>
        public string LogName
        {
            get
            {
                return "将棋モード";
            }
        }

        /// <summary>
        /// 投票結果を取得します。
        /// </summary>
        public VoteCandidatePair[] GetVoteResult()
        {
            lock (SyncRoot)
            {
                var moveList = this.moveStatistics.MoveList;

                // 指し手を投票結果に変換します。
                return moveList.Select(
                    _ => new VoteCandidatePair
                    {
                        Candidate = _.Move.ToString(),
                        Point = _.Point,
                    })
                    .Where(_ => _.Validate())
                    .ToArray();
            }
        }

        /// <summary>
        /// 全コメントをミラー中かどうか取得します。
        /// </summary>
        public bool IsMirrorMode
        {
            get
            {
                return this.voteRoom.VoteModel.IsMirrorMode;
            }
        }
        
        /// <summary>
        /// 投票結果をすべてクリアします。
        /// </summary>
        public void ClearVote()
        {
            lock (SyncRoot)
            {
                this.moveStatistics.ClearVote();
            }
        }

        /// <summary>
        /// 各通知のハンドラを接続します。
        /// </summary>
        public void ConnectHandlers(PbConnection connection)
        {
            connection.AddRequestHandler<
                ShogiGetCurrentBoardRequest, ShogiGetCurrentBoardResponse>(
                HandleGetCurrentBoardRequest);
            connection.AddCommandHandler<ShogiSetCurrentBoardCommand>(
                HandleSetCurrentBoardCommand);
            connection.AddCommandHandler<ShogiGetWhaleClientListCommand>(
                HandleGetWhaleClientListCommand);
        }

        /// <summary>
        /// 各通知のハンドラを切り離します。
        /// </summary>
        public void DisconnectHandlers(PbConnection connection)
        {
            connection.RemoveHandler<ShogiGetCurrentBoardRequest>();
            connection.RemoveHandler<ShogiSetCurrentBoardCommand>();
            connection.RemoveHandler<ShogiGetWhaleClientListCommand>();
        }

        #region 現局面管理
        /// <summary>
        /// 現局面の取得リクエストを処理します。
        /// </summary>
        private void HandleGetCurrentBoardRequest(
            object sender,
            PbRequestEventArgs<ShogiGetCurrentBoardRequest,
                               ShogiGetCurrentBoardResponse> e)
        {
            lock (SyncRoot)
            {
                e.Response = new ShogiGetCurrentBoardResponse()
                {
                    Board = this.board,
                };
            }
        }

        /// <summary>
        /// 現局面の設定コマンドを処理します。
        /// </summary>
        private void HandleSetCurrentBoardCommand(
            object sender,
            PbCommandEventArgs<ShogiSetCurrentBoardCommand> e)
        {
            lock (SyncRoot)
            {
                var board = e.Command.Board;
                if (board == null || !board.Validate())
                {
                    Log.Error(this,
                        "設定された現局面が正しくありません。");
                    return;
                }

                var conn = sender as PbConnection;
                if (!this.voteRoom.IsRoomOwnerConnection(conn))
                {
                    Log.Error(this,
                        "現局面の設定はルームオーナーでないとできません。");
                    return;
                }

                SetCurrentBoard(board);
            }
        }

        /// <summary>
        /// 最終手を設定します。
        /// </summary>
        private void SetLastMove(Board newBoard)
        {
            lock (SyncRoot)
            {
                if (!newBoard.CanUndo)
                {
                    this.moveStatistics.OpponentMove = null;
                    return;
                }

                var tmpBoard = newBoard.Clone();
                var lastMove = tmpBoard.Undo();
                if (lastMove == null)
                {
                    this.moveStatistics.OpponentMove = null;
                    return;
                }

                // ZZ金 の形式に変換します。
                var move = tmpBoard.ConvertMove(lastMove, false);
                if (move == null || !move.Validate())
                {
                    this.moveStatistics.OpponentMove = null;
                    return;
                }

                this.moveStatistics.OpponentMove = move;

                // 指し手が変わることがあります。
                voteRoom.VoteModel.OnVoteResultChanged();
            }
        }

        /// <summary>
        /// 現局面を設定します。
        /// </summary>
        private void SetCurrentBoard(Board newBoard)
        {
            lock (SyncRoot)
            {
                this.board = newBoard;

                SetLastMove(newBoard);
            }

            // 現局面の変更を通知します。
            this.voteRoom.BroadcastCommand(new ShogiSetCurrentBoardCommand()
            {
                Board = this.board,
            });

            Log.Info(this,
                "現局面を設定しました。");
        }
        #endregion

        #region 大合神クジラちゃん
        /// <summary>
        /// 大合神クジラちゃんの情報ファイルを開きます。
        /// </summary>
        private string[] OpenWhaleInfoFile()
        {
            try
            {
                var home = Environment.GetEnvironmentVariable("HOME");
                if (string.IsNullOrEmpty(home))
                {
                    return null;
                }

                var filepath = Path.Combine(home, ".whale_list");
                if (!File.Exists(filepath))
                {
                    return null;
                }

                var lines = Util.ReadLines(filepath, Encoding.ASCII);
                return (lines != null ? lines.ToArray() : new string[0]);
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "大合神クジラちゃんの情報ファイルが開けませんでした。");

                return null;
            }
        }

        /// <summary>
        /// 大合神クジラちゃんのクライアントリストを取得します。
        /// </summary>
        private ShogiSetWhaleClientListCommand GetWhaleInfo()
        {
            try
            {
                var lines = OpenWhaleInfoFile();
                if (lines == null || !lines.Any())
                {
                    return null;
                }

                // 一行目は評価値
                double value;
                if (!double.TryParse(lines[0], out value))
                {
                    return null;
                }

                // 二行目以降は参加者の名前一覧
                var joiners = lines.Skip(1).ToList();
                foreach (var name in joiners)
                {
                    this.voteRoom.VoterListManager.AddModeCustomJoiner(name);
                }
                
                var command = new ShogiSetWhaleClientListCommand
                {
                    Value = value,
                };
                command.NameList.AddRange(joiners);

                return command;
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "大合神クジラちゃんの名前一覧の取得に失敗しました。");

                return null;
            }
        }

        /// <summary>
        /// 大合神クジラちゃんのクライアント一覧の取得要求を処理します。
        /// </summary>
        private void HandleGetWhaleClientListCommand(
            object sender,
            PbCommandEventArgs<ShogiGetWhaleClientListCommand> e)
        {
            var command = GetWhaleInfo();
            if (command == null)
            {
                return;
            }

            var conn = sender as PbConnection;
            if (conn == null)
            {
                return;
            }

            conn.SendCommand(command);
        }
        #endregion

        /// <summary>
        /// 棋力別に文字の色を分けます。
        /// </summary>
        private static NotificationColor GetPlayerColor(SkillLevel skillLevel)
        {
            if (skillLevel.Kind == SkillKind.Dan)
            {
                if (skillLevel.Grade >= 6)
                {
                    // ６,７,８,９段
                    return NotificationColor.Purple;
                }
                else if (skillLevel.Grade >= 3)
                {
                    // ３,４,５段
                    return NotificationColor.Red;
                }
                else
                {
                    // １,２段
                    return NotificationColor.Green;
                }
            }
            else if (skillLevel.Kind == SkillKind.Kyu)
            {
                if (skillLevel.Grade <= 3)
                {
                    // １,２,３級
                    return NotificationColor.Blue;
                }
                else if (skillLevel.Grade <= 6)
                {
                    // ４,５,６級
                    return NotificationColor.Yellow;
                }
                else if (skillLevel.Grade <= 9)
                {
                    // ７,８,９級
                    return NotificationColor.Cyan;
                }
                else if (skillLevel.Grade <= 12)
                {
                    // １０,１１,１２級
                    return NotificationColor.Orange;
                }
                else
                {
                    // それ以外
                    return NotificationColor.Default;
                }
            }
            else
            {
                return NotificationColor.Default;
            }
        }

        /// <summary>
        /// 参加コマンドの正規表現です。
        /// </summary>
        private static readonly Regex JoinRegex = new Regex(
            //@"^(join|sanka|参加|さんか)(\s+|@|＠)",
            @"^参加\s+",
            RegexOptions.IgnoreCase);

        /// <summary>
        /// 投票ルームにたいする各種操作を処理します。
        /// </summary>
        public void ProcessNotification(Notification notification,
                                        bool isFromVoteRoomOwner)
        {
            // 棋力登録コマンドを処理します。
            var m = JoinRegex.Match(notification.Text);
            if (m.Success)
            {
                var text = notification.Text.Substring(m.Value.Length);

                var player = ShogiParser.ParsePlayer(text);
                if (player != null)
                {
                    // ユーザーＩＤの設定は忘れずに。
                    player.PlayerId = notification.VoterId;

                    AddPlayer(player, notification);
                }

                return;
            }

            // 変化コマンドを処理します。
            /*var note = string.Empty;
            var id = -1;
            var nextId = -1;
            var moveList = ParseVariation(notification.Text, out note,
                                          out id, out nextId);
            if (moveList != null)
            {
                var command = new ShogiSendVariationCommand
                {
                    Note = note,
                    Id = id,
                    NextId = nextId,
                };
                command.MoveList.AddRange(moveList);

                this.voteRoom.BroadcastCommand(command);
            }*/

            if (IsMirrorMode)
            {
                // ミラーコメントとして各放送に送ります。
                this.voteRoom.BroadcastNotification(notification, false, true);
            }
        }

        /// <summary>
        /// 部分変化を受け入れるための処置です。
        /// </summary>
        /// <example>
        /// 例１
        /// 76歩34歩56歩54歩
        /// 
        /// 例２
        /// 76歩34歩56歩54歩 これ大丈夫か？
        /// 
        /// 例３
        /// 76歩34歩56歩54歩 $234
        /// $234 86歩24歩85歩 $235
        /// $235 25歩 まあええか
        /// </example>
        private static readonly Regex PartialVariationRegex = new Regex(
            @"^\s*(?:[$](\d+)\s+)?\s*([^$]+)\s*(?:[$](\d+))?\s*$");

        /// <summary>
        /// 変化をパースします。
        /// </summary>
        public static List<Move> ParseVariation(string text, out string note,
                                                out int id, out int nextId)
        {
            var m = PartialVariationRegex.Match(text);
            if (!m.Success)
            {
                note = null;
                id = -1;
                nextId = -1;
                return null;
            }

            id = (m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : -1);
            nextId = (m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : -1);

            // まず、文字列から指し手リストを作成します。
            var moveText = m.Groups[2].Value;
            var moveList = BoardExtension.MakeMoveList(moveText, out note);

            // 最短手数以上の変化なら、よしとします。
            if (id >= 0 || nextId >= 0 ||
                (moveList != null && moveList.Count() >= 3))
            {
                return moveList;
            }

            return null;
        }

        /// <summary>
        /// 票通知を処理します。投票時にしか呼ばれません。
        /// </summary>
        public void ProcessVoteNotification(Notification notification,
                                            bool isFromVoteRoomOwner)
        {
            var move = ParseMove(notification);
            if (move != null)
            {
                Vote(move, notification);
                return;
            }
        }

        /// <summary>
        /// ai) 53金　などを受け入れるための処置です。
        /// </summary>
        private static readonly Regex TagRegex = new Regex(
            @"^\s*\w+[)）]");

        /// <summary>
        /// 指し手をパースします。
        /// </summary>
        /// <remarks>
        /// 指し手は
        /// 　^指し手([\w]+.*)?
        /// の形式しか受理されません。
        /// </remarks>
        private Move ParseMove(Notification source)
        {
            var text = source.Text;
            
            // "ai) 指し手"を指し手として解釈するようにします。
            var m = TagRegex.Match(text);
            if (m.Success)
            {
                text = text.Substring(m.Length);
            }

            // 空白文字を削除。
            text = text.Trim();

            // 文字列を"指し手 + 全角空白 + 残り"となるように再構成します。
            // クライアント側で指し手と残りのメッセージが一意に取り出せる
            // ようにするためです。
            var whitespaceIndex = Util.IndexOfWhitespace(text);
            if (whitespaceIndex >= 0)
            {
                var moveText = text.Substring(0, whitespaceIndex);
                var afterText = text.Substring(whitespaceIndex + 1);

                text = moveText + "　" + afterText;
            }

            var move = ShogiParser.ParseMove(text, true);
            if (move == null || !move.Validate())
            {
                return null;
            }

            // 投了は常にさせることにします。
            if (this.board == null || move.IsResigned)
            {
                return move;
            }

            // 現局面から指せるかどうか調べます。
            var bm = this.board.ConvertMove(move);
            if (bm == null)
            {
                return null;
            }

            /*// 指し手の正規化を行います（打を消したり、左を追加するなど）
            var newMove = this.board.ConvertMove(bm, false);
            if (newMove == null)
            {
                return null;
            }

            newMove.OriginalText = move.OriginalText;*/
            return move;
        }

        /// <summary>
        /// プレイヤーを追加します。
        /// </summary>
        private void AddPlayer(ShogiPlayer player, Notification source)
        {
            lock (SyncRoot)
            {
                // lockはしません。
                // ここで使われている各オブジェクトはスレッドセーフです。
                var oldPlayer = this.playerPool.Get(player.PlayerId);

                // 参加希望は放送のコメントから出され、そのコメントは
                // ツールによって複数回扱われる可能性があります。
                // (ツールの再起動などで)
                // つまり、二重に参加希望が出される可能性があるため、
                // プレイヤーの参加希望時刻によって新しい参加希望か
                // どうかを区別しています。
                if (oldPlayer == null || source.Timestamp > oldPlayer.Timestamp)
                {
                    AddVoter(player, source, false);

                    Log.Info(this,
                        "プレイヤー='{0}'が参加しました。",
                        player);

                    if (!IsMirrorMode)
                    {
                        // プレイヤーの参加表明を全ユーザーに通知します。
                        var notification = new Notification()
                        {
                            Text = player + "さんが参加しました",
                            Type = NotificationType.Join,
                            Color = GetPlayerColor(player.SkillLevel),
                            VoterId = player.PlayerId,
                            VoterName = (
                                !string.IsNullOrEmpty(source.VoterName) ?
                                source.VoterName : player.Nickname),
                            FromLiveRoom = source.FromLiveRoom,
                            Timestamp = source.Timestamp,
                        };

                        this.voteRoom.BroadcastNotification(notification, true, true);
                    }
                }
            }
        }

        /// <summary>
        /// 指し手への投票を行います。
        /// </summary>
        private void Vote(Move move, Notification source)
        {
            lock (SyncRoot)
            {
                // 手番は無しに設定します。
                move.BWType = BWType.None;

                // lockはしません。
                // 個々で使われている各オブジェクトはスレッドセーフです。
                var oldMove = this.moveStatistics.GetVote(source.VoterId);

                // 実際に可能な指し手か調べます。
                /*var boardMove = this.board.ConvertMove(move);
                if (boardMove == null)
                {
                    return;
                }*/

                // 二重投票をさけるため、投票された時刻を見ています。
                // また投票がクリアされた時刻よりも新しい指し手のみを採用します。
                if (oldMove == null || source.Timestamp > oldMove.Timestamp)
                {
                    var player = GetOrAddShogiPlayer(source);

                    // 票数を更新します。
                    this.moveStatistics.Vote(player, move, source.Timestamp);

                    AddVoter(player, source, true);

                    Log.Info(this,
                        "指し手={0}が受理されました。",
                        move.ToString());

                    if (!IsMirrorMode)
                    {
                        // 投票された指し手を全ユーザーに通知します。
                        var notification = new Notification()
                        {
                            Text = move.OriginalText,
                            Type = NotificationType.Vote,
                            Color = GetPlayerColor(player.SkillLevel),
                            VoterId = source.VoterId,
                            VoterName = (
                                !string.IsNullOrEmpty(source.VoterName) ?
                                source.VoterName : player.Nickname),
                            FromLiveRoom = source.FromLiveRoom,
                            Timestamp = source.Timestamp,
                        };

                        this.voteRoom.BroadcastNotification(notification, true, true);
                    }
                }
            }
        }

        /// <summary>
        /// 棋力登録済みの参加者を捜し、もしいなければ登録します。
        /// </summary>
        private ShogiPlayer GetOrAddShogiPlayer(Notification source)
        {
            lock (SyncRoot)
            {
                var regPlayer = this.playerPool.Get(source.VoterId);

                // 指し手を投票したプレイヤーが登録されていない場合は、
                // 棋力不明としてプレイヤーを登録します。
                if (regPlayer != null)
                {
                    return regPlayer.Player;
                }

                // 棋力登録はしますが、参加者登録はしません。
                var player = new ShogiPlayer()
                {
                    PlayerId = source.VoterId,
                    Nickname = "匿名さん",
                    SkillLevel = new SkillLevel(SkillKind.Unknown, 0),
                };

                this.playerPool.Add(player, source.Timestamp);
                return player;
            }
        }

        /// <summary>
        /// 参加者を登録します。
        /// </summary>
        private void AddVoter(ShogiPlayer player, Notification source,
                              bool isAnonymous)
        {
            lock (SyncRoot)
            {
                var voter = new VoterInfo()
                {
                    LiveSite = LiveSite.NicoNama,
                    Id = player.PlayerId,
                    Name = (isAnonymous ? string.Empty : player.Nickname),
                    Skill = player.SkillLevel.ToString(),
                    Color = GetPlayerColor(player.SkillLevel),
                    LiveData = (source.FromLiveRoom != null ?
                        source.FromLiveRoom.LiveData : null),
                };

                if (isAnonymous)
                {
                    // 無名投票者リストに追加します。
                    this.voteRoom.VoterListManager.AddUnjoinedVoter(voter);
                }
                else
                {
                    // 棋力登録＆参加者登録を行います。
                    this.playerPool.Add(player, source.Timestamp);

                    this.voteRoom.VoterListManager.AddJoinedVoter(voter);
                }

                // 指し手のポイントが変わることがあります。
                this.voteRoom.VoteModel.OnVoteResultChanged();
            }
        }

        /// <summary>
        /// シグナル受信時に呼ばれます。
        /// </summary>
        private void OnSignalReceived(object sender, SignalEventArgs e)
        {
            try
            {
                Log.Info("将棋) シグナル処理を開始しました。");

                var command = GetWhaleInfo();
                if (command == null)
                {
                    return;
                }

                // 大合神クジラちゃんのクライアントリストを送信します。
                this.voteRoom.BroadcastCommand(command);
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "大合神クジラちゃんの名前一覧の取得に失敗しました。");
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ShogiVoteStrategy(VoteRoom voteRoom)
        {
            this.voteRoom = voteRoom;

            Signal.SignalReceived +=
                Util.MakeWeak<SignalEventArgs>(
                    OnSignalReceived,
                    _ => Signal.SignalReceived -= _);
        }
    }
}
