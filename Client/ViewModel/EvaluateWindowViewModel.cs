using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;

using Ragnarok;
using Ragnarok.ObjectModel;

namespace VoteSystem.Client.ViewModel
{
    using VoteSystem.Client.Model;

    /// <summary>
    /// 評価値の取得元を識別します。
    /// </summary>
    public enum EvaluationPointType
    {
        /// <summary>
        /// ユーザーの評価値の平均値を使います。
        /// </summary>
        User,
        /// <summary>
        /// 手入力値を使います。
        /// </summary>
        Input,
        /// <summary>
        /// 各モード固有の値を使います。
        /// </summary>
        ModeCustom,
    }

    /// <summary>
    /// 評価値ウィンドウ用のモデルオブジェクトです。
    /// </summary>
    public class EvaluationWindowViewModel : NotifyObject, IDisposable
    {
        private EvaluationPointType pointType = EvaluationPointType.User;
        private double inputPoint = 0.0;
        private bool isShowEvaluationPoint = true;
        private List<ImageSetInfo> imageSetList;
        private ImageSetInfo selectedImageSet;
        private string currentImagePath;
        private bool disposed = false;

        /// <summary>
        /// 評価値の取得元を取得または設定します。
        /// </summary>
        public EvaluationPointType PointType
        {
            get
            {
                return this.pointType;
            }
            set
            {
                if (this.pointType != value)
                {
                    this.pointType = value;

                    this.UpdateImagePath();
                    this.RaisePropertyChanged("PointType");
                }
            }
        }

        /// <summary>
        /// ユーザーの評価値を取得または設定します。
        /// </summary>
        public double UserPoint
        {
            get
            {
                return Global.VoteClient.VoteResult.EvaluationPoint;
            }
        }

        /// <summary>
        /// 手入力の評価値を取得または設定します。
        /// </summary>
        public double InputPoint
        {
            get
            {
                return this.inputPoint;
            }
            set
            {
                if (this.inputPoint != value)
                {
                    this.inputPoint = value;

                    this.UpdateImagePath();
                    this.RaisePropertyChanged("InputPoint");
                }
            }
        }

        /// <summary>
        /// 各投票モードに固有の評価値を取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(MainModel), "ModeCustomPoint")]
        public double ModeCustomPoint
        {
            get
            {
                return Global.MainModel.ModeCustomPoint;
            }
        }

        /// <summary>
        /// 評価値を表示するかどうかを取得または設定します。
        /// </summary>
        public bool IsShowEvaluationPoint
        {
            get
            {
                return this.isShowEvaluationPoint;
            }
            set
            {
                if (this.isShowEvaluationPoint != value)
                {
                    this.isShowEvaluationPoint = value;

                    this.RaisePropertyChanged("IsShowEvaluationPoint");
                }
            }
        }

        /// <summary>
        /// 評価値を取得または設定します。
        /// </summary>
        [DependOnProperty("PointType")]
        [DependOnProperty("UserPoint")]
        [DependOnProperty("InputPoint")]
        [DependOnProperty("ModeCustomPoint")]
        public double EvaluationPoint
        {
            get
            {
                switch (this.pointType)
                {
                    case EvaluationPointType.User:
                        return UserPoint;
                    case EvaluationPointType.Input:
                        return InputPoint;
                    case EvaluationPointType.ModeCustom:
                        return ModeCustomPoint;
                }

                throw new InvalidEnumArgumentException(
                    "不正な評価値タイプです。");
            }
        }

        /// <summary>
        /// 画像セットの集合を取得します。
        /// </summary>
        public List<ImageSetInfo> ImageSetList
        {
            get
            {
                return this.imageSetList;
            }
            private set
            {
                this.imageSetList = value;
            }
        }

        /// <summary>
        /// 現在選択中の画像セットを取得または設定します。
        /// </summary>
        public ImageSetInfo SelectedImageSet
        {
            get
            {
                return this.selectedImageSet;
            }
            set
            {
                if (this.selectedImageSet != value)
                {
                    this.selectedImageSet = value;

                    this.UpdateImagePath();
                    this.RaisePropertyChanged("SelectedImageSet");
                }
            }
        }

        /// <summary>
        /// 現在の画像を取得します。
        /// </summary>
        public string CurrentImagePath
        {
            get
            {
                return this.currentImagePath;
            }
            private set
            {
                if (this.currentImagePath != value)
                {
                    this.currentImagePath = value;

                    this.RaisePropertyChanged("CurrentImagePath");
                }
            }
        }

        /// <summary>
        /// 評価値が変化したときに呼ばれます。
        /// </summary>
        private void UpdateImagePath()
        {
            if (SelectedImageSet == null)
            {
                return;
            }

            // 評価値から画像を取得します。
            var imagePath = SelectedImageSet.GetSelectedImagePath(
                EvaluationPoint);

            // 評価値より得られる画像を取得します。
            CurrentImagePath = (
                string.IsNullOrEmpty(imagePath) ?
                Resources.Image.ImageConstants.NoImageUrl :
                imagePath);

            // 選択された画像セットを保存します。
            Global.Settings.AS_ImageSetDirName = SelectedImageSet.DirectoryName;
        }

        /// <summary>
        /// 評価値が変わったときに画像を変更します。
        /// </summary>
        void VoteClient_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (SelectedImageSet == null ||
                PointType != EvaluationPointType.User)
            {
                return;
            }

            var voteClient = (VoteClient)sender;

            if (e.PropertyName == "VoteResult")
            {
                var point = voteClient.VoteResult.EvaluationPoint;
                var imagePath = SelectedImageSet.GetSelectedImagePath(point);

                // 評価値より得られる画像を取得します。
                CurrentImagePath = (
                    string.IsNullOrEmpty(imagePath) ?
                    Resources.Image.ImageConstants.NoImageUrl :
                    imagePath);

                // 評価値の更新を通知します。
                this.RaisePropertyChanged("UserPoint");
            }
        }

        void MainModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ModeCustomPoint")
            {
                if (PointType == EvaluationPointType.ModeCustom)
                {
                    UpdateImagePath();
                }

                this.RaisePropertyChanged("ModeCustomPoint");
            }
        }

        /// <summary>
        /// 画像セットのリストを作成します。
        /// </summary>
        private List<ImageSetInfo> GetImageSetList()
        {
            try
            {
                var fullpath = Path.GetFullPath(@"Data\Image");
                if (!Directory.Exists(fullpath))
                {
                    return new List<ImageSetInfo>();
                }

                // 画像ディレクトリのディレクトリ中にあるinfo.txtファイルを探し、
                // もしあればそのファイルを解析します。
                return Directory.EnumerateDirectories(fullpath)
                    .Select(dir => Path.Combine(dir, "info.json"))
                    .Where(File.Exists)
                    .Select(ImageSetInfo.Read)
                    .Where(imageSet => imageSet != null)
                    .ToList();
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "画像セット取得中にエラーが発生しました。");

                return new List<ImageSetInfo>();
            }
        }

        /// <summary>
        /// 画像リストを初期化します。
        /// </summary>
        private void InitImageSetList()
        {
            this.imageSetList = GetImageSetList();

            if (this.imageSetList.Any())
            {
                ImageSetInfo selected = null;

                // 既存の画像セットが保存されていれば、それを選択します。
                if (!string.IsNullOrEmpty(Global.Settings.AS_ImageSetDirName))
                {
                    selected = this.imageSetList.FirstOrDefault(imageSet =>
                        imageSet.DirectoryName == Global.Settings.AS_ImageSetDirName);
                }

                if (selected == null)
                {
                    selected = this.imageSetList[0];
                }

                this.selectedImageSet = selected;
            }

            UpdateImagePath();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EvaluationWindowViewModel()
        {
            InitImageSetList();

            Global.VoteClient.PropertyChanged += VoteClient_PropertyChanged;
            Global.MainModel.PropertyChanged += MainModel_PropertyChanged;
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~EvaluationWindowViewModel()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// おぶじぇくとを破棄します。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// オブジェクトを破棄します。
        /// </summary>
        protected void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                var voteClient = Global.VoteClient;
                if (voteClient != null)
                {
                    voteClient.PropertyChanged -= VoteClient_PropertyChanged;
                }

                var model = Global.MainModel;
                if (model != null)
                {
                    model.PropertyChanged -= MainModel_PropertyChanged;
                }
            }

            this.disposed = true;
        }
    }
}
