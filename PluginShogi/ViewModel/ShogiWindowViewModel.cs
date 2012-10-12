using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.ComponentModel;

using Ragnarok;
using Ragnarok.Shogi;
using Ragnarok.Shogi.View;
using Ragnarok.NicoNico.Live;
using Ragnarok.ObjectModel;
using Ragnarok.Presentation;

namespace VoteSystem.PluginShogi.ViewModel
{
    using VoteSystem.Protocol;
    using VoteSystem.PluginShogi.Model;
    using VoteSystem.PluginShogi.View;

    /// <summary>
    /// 変化の再生状態を示します。
    /// </summary>
    public enum VariationState
    {
        /// <summary>
        /// 何もしていません。
        /// </summary>
        None,
        /// <summary>
        /// 変化再生中です。
        /// </summary>
        Playing,
    }

    /// <summary>
    /// ShogiWindowのモデルクラスです。
    /// </summary>
    public class ShogiWindowViewModel : NotifyObject
    {
        private Board currentBoard;
        private Board board;

        private readonly DispatcherTimer variationTimer;
        private bool isCheckingShowVariation = false;
        private readonly Queue<Variation> variationQueue = new Queue<Variation>();
        private Queue<BoardMove> currentVariation;
        private readonly VariationManager moveManager = new VariationManager();
        private readonly NotifyCollection<string> commentCandidates;
        private readonly CommentClient commentClient;

        /// <summary>
        /// 盤面を取得または設定します。
        /// </summary>
        public Board Board
        {
            get
            {
                return this.board;
            }
            set
            {
                using (LazyLock())
                {
                    if (this.currentBoard == value)
                    {
                        value = value.Clone();
                    }

                    if (this.board != value)
                    {
                        if (this.board != null)
                        {
                            this.board.BoardChanged -= board_BoardChanged;
                        }

                        this.board = value;

                        if (this.board != null)
                        {
                            this.board.BoardChanged += board_BoardChanged;
                        }

                        this.RaisePropertyChanged("Board");
                    }
                }
            }
        }

        /// <summary>
        /// 現局面を取得または設定します。
        /// </summary>
        public Board CurrentBoard
        {
            get
            {
                return this.currentBoard;
            }
            set
            {
                using (LazyLock())
                {
                    if (this.board == value)
                    {
                        value = value.Clone();
                    }

                    if (this.currentBoard != value)
                    {
                        this.currentBoard = value;

                        this.RaisePropertyChanged("CurrentBoard");
                    }
                }
            }
        }

        /// <summary>
        /// 現局面からの指し手を文字列として取得します。
        /// </summary>
        public string MoveTextFromCurrentBoard
        {
            get { return GetValue<string>("MoveTextFromCurrentBoard"); }
            private set { SetValue("MoveTextFromCurrentBoard", value); }
        }

        /// <summary>
        /// 変化のコメントを取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "AS_Comment")]
        public string Comment
        {
            get { return ShogiGlobal.Settings.AS_Comment; }
            set { ShogiGlobal.Settings.AS_Comment = value; }
        }

        /// <summary>
        /// コメント一覧を取得します。
        /// </summary>
        public NotifyCollection<string> CommentCandidates
        {
            get { return this.commentCandidates; }
        }

        /// <summary>
        /// 変化の管理オブジェクトを取得します。(XAML参照用)
        /// </summary>
        public VariationManager MoveManager
        {
            get { return this.moveManager; }
        }

        /// <summary>
        /// 放送URLを取得または設定します。
        /// </summary>
        public string LiveUrl
        {
            get { return GetValue<string>("LiveUrl"); }
            set { SetValue("LiveUrl", value); }
        }

        /// <summary>
        /// 放送オブジェクトを取得します。
        /// </summary>
        public CommentClient CommentClient
        {
            get { return this.commentClient; }
        }

        /// <summary>
        /// 放送に接続されているか調べます。
        /// </summary>
        //[DependOnProperty(typeof(CommentClient), "IsConnected")]
        public bool IsConnectedToLive
        {
            get { return this.commentClient.IsConnected; }
        }

        /// <summary>
        /// 手番を取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Board), "MovePriority")]
        public BWType MovePriority
        {
            get { return this.board.MovePriority; }
            set { this.board.MovePriority = value; }
        }

        /// <summary>
        /// 編集モードを取得または設定します。
        /// </summary>
        public EditMode EditMode
        {
            get { return GetValue<EditMode>("EditMode"); }
            set { SetValue("EditMode", value); }
        }

        /// <summary>
        /// 手を戻すことができるか取得します。
        /// </summary>
        [DependOnProperty(typeof(Board), "CanUndo")]
        public bool CanUndo
        {
            get
            {
                return (
                    EditMode != EditMode.NoEdit &&
                    VariationState == VariationState.None &&
                    this.board.CanUndo);
            }
        }

        /// <summary>
        /// 手を進めることができるか取得します。
        /// </summary>
        [DependOnProperty(typeof(Board), "CanRedo")]
        public bool CanRedo
        {
            get
            {
                return (
                    EditMode != EditMode.NoEdit &&
                    VariationState == VariationState.None &&
                    this.board.CanRedo);
            }
        }

        /// <summary>
        /// 変化が再生中かどうかを取得します。
        /// </summary>
        public VariationState VariationState
        {
            get { return GetValue<VariationState>("VariationState"); }
            set
            {
                SetValue("VariationState", value);

                // TODO: ウィンドウのボタン状態を変更するため。
                WpfUtil.InvalidateCommand();
            }
        }

        #region 見た目系
        /// <summary>
        /// 盤などの画像読み込みを行います。
        /// </summary>
        private BitmapImage CreateImage<T>(T value)
        {
            try
            {
                var imageUri = ImageUtil.GetImageUri(value);
                if (imageUri == null)
                {
                    return null;
                }

                var image = new BitmapImage(imageUri);
                image.Freeze();

                return image;
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "{0}: 画像の読み込みに失敗しました。", value);

                return null;
            }
        }

        /// <summary>
        /// 駒画像を取得します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_KomaImage")]
        public BitmapImage KomaImage
        {
            get { return CreateImage(ShogiGlobal.Settings.SD_KomaImage); }
        }

        /// <summary>
        /// 盤画像を取得します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_BanImage")]
        public BitmapImage BanImage
        {
            get { return CreateImage(ShogiGlobal.Settings.SD_BanImage); }
        }

        /// <summary>
        /// 駒台画像を取得します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_KomadaiImage")]
        public BitmapImage KomadaiImage
        {
            get { return CreateImage(ShogiGlobal.Settings.SD_KomadaiImage); }
        }

        /// <summary>
        /// 盤の不透明度を取得します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_BanOpacity")]
        public double BanOpacity
        {
            get { return ShogiGlobal.Settings.SD_BanOpacity; }
        }

        /// <summary>
        /// 背景画像のパスを取得します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_BackgroundPath")]
        public string BackgroundPath
        {
            get { return ShogiGlobal.Settings.SD_BackgroundPath; }
        }
        #endregion

        /// <summary>
        /// 盤面が変更される前に必要なら局面のコピーを行います。
        /// </summary>
        private void CloneOnWriteBoard()
        {
            // 現局面と表示されている局面が同一オブジェクトなら、
            // 表示用の盤面をコピーし、現局面に表示局面の
            // 変更の影響がでないようにします。
            if (ReferenceEquals(Board, CurrentBoard))
            {
                Board = CurrentBoard.Clone();
            }
        }

        /// <summary>
        /// 現局面からの指し手を取得します。
        /// </summary>
        private List<Move> BuildMoveListFromCurrentBoard()
        {
            var tmpBoard = Board.Clone();

            // 局面の手数を現局面まで戻して、局面が一致するか確認します。
            while (tmpBoard.MoveCount > CurrentBoard.MoveCount)
            {
                tmpBoard.Undo();
            }

            if (tmpBoard.MoveCount != CurrentBoard.MoveCount ||
                !tmpBoard.BoardEquals(CurrentBoard))
            {
                return null;
            }

            // 表示されている局面を戻すと現局面と一致するので、
            // 現局面からの指し手を文字列に直します。
            return Board.MakeMoveList(CurrentBoard.MoveCount, false);
        }

        /// <summary>
        /// 現局面からの指し手を更新します。
        /// </summary>
        private void UpdateMoveTextFromMoveList(IEnumerable<Move> moveList)
        {
            if (moveList == null)
            {
                MoveTextFromCurrentBoard = null;
                return;
            }

            // 表示されている局面を戻すと現局面と一致するので、
            // 現局面からの指し手を文字列に直します。
            MoveTextFromCurrentBoard = string.Join("",
                moveList.Select(move =>
                    Stringizer.ToString(move, MoveTextStyle.Simple)));
        }

        /// <summary>
        /// 現局面からの指し手を更新します。
        /// </summary>
        private void UpdateMoveTextFromCurrentBoard()
        {
            var moveList = BuildMoveListFromCurrentBoard();

            UpdateMoveTextFromMoveList(moveList);
        }

        /// <summary>
        /// １手戻します。
        /// </summary>
        public void Undo()
        {
            if (!CanUndo)
            {
                return;
            }

            CloneOnWriteBoard();
            Board.Undo();
        }

        /// <summary>
        /// １手進めます。
        /// </summary>
        public void Redo()
        {
            if (!CanRedo)
            {
                return;
            }

            CloneOnWriteBoard();
            Board.Redo();
        }

        /// <summary>
        /// 変化を追加し、可能なら再生します。
        /// </summary>
        public bool AddVariation(IEnumerable<Move> moveList,
                                 string comment, bool autoPlay)
        {
            if (moveList == null)
            {
                return false;
            }

            // ここで変化を作成。
            var variation = Variation.Create(
                this.currentBoard,
                moveList, comment, true);
            if (variation == null)
            {
                return false;
            }

            return AddVariation(variation, autoPlay);
        }

        /// <summary>
        /// 変化を追加し、可能なら再生します。
        /// </summary>
        public bool AddVariation(Variation variation, bool autoPlay)
        {
            if (variation == null)
            {
                return false;
            }

            // 将棋コントロールに変化を追加します。
            this.moveManager.AddVariation(variation);

            // 自動再生用に変化を追加します。
            if (autoPlay)
            {
                ShowVariation(variation);
            }

            return true;
        }

        /// <summary>
        /// 変化の表示を行います。
        /// </summary>
        public void ShowVariation(Variation variation)
        {
            if (variation == null || !variation.CanShow)
            {
                return;
            }

            // ウィンドウが非表示なら変化を表示しません。
            if (ShogiGlobal.MainWindow == null)
            {
                return;
            }

            this.variationQueue.Enqueue(variation);
            BeginToShowVariation();
        }

        /// <summary>
        /// 変化を再生するか確かめ、再生する場合はそれを返します。
        /// </summary>
        private Variation CheckShowVariation()
        {
            // 再生中なら再生しません。
            if (VariationState == VariationState.Playing ||
                this.isCheckingShowVariation)
            {
                return null;
            }

            try
            {
                // ここで変化の再生フラグを立てます。
                // 確認中に再度確認するのを防ぐためです。
                this.isCheckingShowVariation = true;

                if (!this.variationQueue.Any())
                {
                    return null;
                }

                // 次の変化を取り出します。
                var variation = this.variationQueue.Dequeue();
                if (variation == null ||
                    !variation.Board.CanMoveList(variation.BoardMoveList))
                {
                    // もう一回。
                    return CheckShowVariation();
                }

                // ウィンドウが非表示なら変化を表示しません。
                if (ShogiGlobal.MainWindow == null)
                {
                    return null;
                }

                var dialog = DialogUtil.CreateDialog(
                    ShogiGlobal.MainWindow,
                    string.Format("{1}{0}{0}コメント: {2}{0}{0}を再生しますか？",
                        Environment.NewLine,
                        variation.Label,
                        variation.Comment),
                    "再生確認",
                    MessageBoxButton.YesNo,
                    MessageBoxResult.No);
                dialog.ShowDialog();
                if (dialog.ResultButton != MessageBoxResult.Yes)
                {
                    return null;
                }

                return variation;
            }
            finally
            {
                this.isCheckingShowVariation = false;
            }
        }

        /// <summary>
        /// 変化を再生します。
        /// </summary>
        private void BeginToShowVariation()
        {
            var variation = CheckShowVariation();
            if (variation == null)
            {
                return;
            }

            // ここで変化の再生フラグを立てます。
            VariationState = VariationState.Playing;
            EditMode = EditMode.NoEdit;

            //EndMovePiece();
            Board = variation.Board.Clone();

            ShogiGlobal.EffectManager.IsSimpleEffect = false;
            ShogiGlobal.EffectManager.EffectMoveCount = 0;

            this.currentVariation = new Queue<BoardMove>(variation.BoardMoveList);
            this.variationTimer.Start();
        }

        /// <summary>
        /// 変化の表示が終わったら呼ばれます。
        /// </summary>
        public void StopVariation()
        {
            if (VariationState != VariationState.Playing)
            {
                return;
            }

            VariationState = VariationState.None;
            EditMode = EditMode.Normal;

            this.currentVariation.Clear(); // 停止時に備える
            this.variationTimer.Stop();

            // 変化停止時の処理
            WpfUtil.InvalidateCommand();

            ShogiGlobal.EffectManager.IsSimpleEffect = true;
            ShogiGlobal.EffectManager.EffectMoveCount = 0;

            //Board = this.currentBoard;

            // もし次の変化があればそれも表示します。
            BeginToShowVariation();
        }

        void variationTimer_Tick(object sender, EventArgs e)
        {
            if (!this.currentVariation.Any())
            {
                StopVariation();
                return;
            }

            var move = this.currentVariation.Dequeue();
            if (move != null)
            {
                if (!this.currentVariation.Any())
                {
                    ShogiGlobal.EffectManager.SetLastEffect();
                }

                this.board.DoMove(move);
            }
        }

        void ShogiWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentBoard")
            {
                // 現局面を更新します。
                this.moveManager.SetCurrentBoard(CurrentBoard);

                UpdateMoveTextFromCurrentBoard();
            }
            else if (e.PropertyName == "Board")
            {
                UpdateMoveTextFromCurrentBoard();
            }
        }

        void board_BoardChanged(object sender, BoardChangedEventArgs e)
        {
            var moveList = BuildMoveListFromCurrentBoard();
            
            if (moveList != null)
            {
                var variation = Variation.Create(
                    this.currentBoard,
                    moveList, null, true);
                if (variation == null)
                {
                    return;
                }

                this.moveManager.AddVariation(variation);
                UpdateMoveTextFromCurrentBoard();
            }

            WpfUtil.InvalidateCommand();
        }

        void commentClient_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsConnected")
            {
                this.RaisePropertyChanged("IsConnectedToLive");
            }
        }

        #region コメントクライアント
        /// <summary>
        /// 変化通知を処理します。
        /// </summary>
        private void commentClient_HandleComment(
            object sender,
            CommentRoomReceivedEventArgs e)
        {
            var comment = e.Comment;
            if (comment == null || string.IsNullOrEmpty(comment.Text))
            {
                return;
            }

            // 投稿者コメントや運営コメントではアリーナ席以外を無視します。
            if (!comment.IsUserComment && e.RoomIndex != 0)
            {
                return;
            }

            var variation = Variation.Parse(e.Comment.Text);
            if (variation == null)
            {
                return;
            }

            WpfUtil.UIProcess(() =>
            {
                var ret = ShogiGlobal.ShogiModel.AddVariation(variation, true);
                if (!ret)
                {
                    return;
                }

                // 分かりやすくするため、再生する変化は一度、
                // 重要メッセージとして表示します。
                var postMessage = MakePostVariationComment(
                    variation.MoveList,
                    null,
                    variation.Comment);
                ShogiGlobal.VoteClient.OnNotificationReceived(
                    new Notification()
                    {
                        Type = NotificationType.Important,
                        Text = postMessage,
                        VoterId = "$system$",
                        Timestamp = Ragnarok.Net.NtpClient.GetTime(),
                    });
            });
        }

        /// <summary>
        /// 変化の投稿用コメントを作成します。
        /// このコメントは変化が投稿されたことを周知するために使われます。
        /// </summary>
        private string MakePostVariationComment(IEnumerable<Move> moveList,
                                                string name,
                                                string moveComment)
        {
            var result = new StringBuilder(128);
            var model = ShogiGlobal.ShogiModel;

            // 以下のようなコメントが作成されます。
            //   - "/変化:　コメント　by Name"
            //   - "/変化:　by Name"
            //   - "/変化:　コメント　"
            //   - "/変化:　"
            result.Append("/変化:　");
            if (!Util.IsWhiteSpaceOnly(moveComment))
            {
                result.Append(moveComment + "　");
            }
            if (!string.IsNullOrEmpty(name))
            {
                result.Append("by " + name);
            }
            
            // とりあえず改行します。
            result.AppendLine();

            var line = 1;
            var count = 0;
            var bwType = model.CurrentBoard.MovePriority;
            foreach (var move in moveList)
            {
                move.BWType = bwType;
                bwType = bwType.Toggle();

                var moveText = move.ToString();
                result.Append(moveText);

                // 指定文字数を超えたら改行します。
                if ((count += moveText.Length) >= 32)
                {
                    // 最大３行までしか表示しません。
                    if (++line >= 3)
                    {
                        result.Append("（ｒｙ");
                        break;
                    }

                    result.AppendLine();
                    count = 0;
                }
            }

            return result.ToString();
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ShogiWindowViewModel(Board board)
        {
            this.variationTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(1000),
            };

            this.commentCandidates = new NotifyCollection<string>(
                new []
                {
                    "こんな感じ？",
                    "どやあああ",
                    "う～ん",
                });

            VariationState = VariationState.None;

            this.currentBoard = board.Clone();
            this.board = board.Clone();
            this.board.BoardChanged += board_BoardChanged;

            this.commentClient = VoteSystem.Client.Global.CreateCommentClient();
            this.commentClient.IsSupressLog = true;
            this.commentClient.PropertyChanged += commentClient_PropertyChanged;
            this.commentClient.CommentReceived += commentClient_HandleComment;

            this.variationTimer.Tick += variationTimer_Tick;

            this.moveManager.SetCurrentBoard(this.currentBoard);

            this.PropertyChanged += ShogiWindowViewModel_PropertyChanged;

            this.AddDependModel(ShogiGlobal.Settings);
        }
    }
}
