using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;

using Ragnarok;

namespace TimeController
{
    /// <summary>
    /// グローバルなオブジェクトを保持します。
    /// </summary>
    public static class Global
    {
        /// <summary>
        /// ビューモデルを取得または設定します。
        /// </summary>
        public static MainViewModel MainViewModel
        {
            get;
            private set;
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public static void Initialize()
        {
            Ragnarok.Presentation.WPFUtil.Init();

            MainViewModel = new MainViewModel();
            MainViewModel.StartTimer();
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        public static void Uninitialize()
        {
        }
    }
}
