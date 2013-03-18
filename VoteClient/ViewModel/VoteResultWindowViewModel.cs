using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;

using Ragnarok;
using Ragnarok.ObjectModel;

namespace VoteSystem.Client.ViewModel
{
    using Protocol.Vote;
    using VoteSystem.Client.Model;

    /// <summary>
    /// 投票結果などを表示するウィンドウのモデルデータです。
    /// </summary>
    public class VoteResultWindowViewModel : NotifyObject
    {
        private Color movableBackgroundColor =
            Color.FromArgb(128, 0, 8, 80);

        /// <summary>
        /// 投票用オブジェクトを取得します。
        /// </summary>
        public VoteClient VoteClient
        {
            get;
            private set;
        }

        /// <summary>
        /// 設定オブジェクトを取得します。
        /// </summary>
        public Settings Settings
        {
            get { return Global.Settings; }
        }

        /// <summary>
        /// 移動時のウィンドウ色を取得または設定します。
        /// </summary>
        public Color VR_MovableBackgroundColor
        {
            get
            {
                return this.movableBackgroundColor;
            }
            set
            {
                if (this.movableBackgroundColor != value)
                {
                    this.movableBackgroundColor = value;

                    this.RaisePropertyChanged("MovableBackgroundColor");
                }
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VoteResultWindowViewModel(VoteClient voteClient)
        {
            VoteClient = voteClient;
        }
    }
}
