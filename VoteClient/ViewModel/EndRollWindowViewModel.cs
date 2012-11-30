using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

using Ragnarok.ObjectModel;

namespace VoteSystem.Client.ViewModel
{
    using Protocol.Vote;

    /// <summary>
    /// エンドロールに関するビューモデルです。
    /// </summary>
    public sealed class EndRollWindowViewModel : NotifyObject
    {
        /// <summary>
        /// 背景色を取得または設定します。
        /// </summary>
        public Brush Background
        {
            get { return GetValue<Brush>("Background"); }
            set { SetValue("Background", value); }
        }

        /// <summary>
        /// １行の高さを取得または設定します。
        /// </summary>
        public double LineHeight
        {
            get { return GetValue<double>("LineHeight"); }
            set { SetValue("LineHeight", value); }
        }

        /// <summary>
        /// エンドロールが流れる時間間隔を取得または設定します。
        /// </summary>
        public int RollTimeSeconds
        {
            get { return GetValue<int>("RollTimeSeconds"); }
            set { SetValue("RollTimeSeconds", value); }
        }

        /// <summary>
        /// エンドロールの開始と終了の何行後/何行前から文字の不透明度を
        /// １００％にするかを示す依存プロパティです。
        /// </summary>
        public int OpacityLineCount
        {
            get { return GetValue<int>("OpacityLineCount"); }
            set { SetValue("OpacityLineCount", value); }
        }

        /// <summary>
        /// エンドロールの進み具合を取得または設定します。
        /// </summary>
        public double CurrentPos
        {
            get { return GetValue<double>("CurrentPos"); }
            set { SetValue("CurrentPos", value); }
        }

        /// <summary>
        /// 最前面に表示するかどうかを取得または設定します。
        /// </summary>
        public bool Topmost
        {
            get { return GetValue<bool>("Topmost"); }
            set { SetValue("Topmost", value); }
        }

        /// <summary>
        /// ウィンドウの外枠を表示するかどうかを取得または設定します。
        /// </summary>
        public bool IsShowBorder
        {
            get { return GetValue<bool>("IsShowBorder"); }
            set { SetValue("IsShowBorder", value); }
        }

        /// <summary>
        /// ウィンドウの外枠の太さを取得または設定します。
        /// </summary>
        public double EdgeLength
        {
            get { return GetValue<double>("EdgeLength"); }
            set { SetValue("EdgeLength", value); }
        }
    }
}
