using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ragnarok;
using Ragnarok.ObjectModel;

namespace VoteSystem.PluginShogi.ViewModel
{
    /// <summary>
    /// 全体的な設定などを保存するモデルクラスです。
    /// </summary>
    public sealed class GlobalModel : NotifyObject
    {
        public bool IsEndingMode
        {
            get { return GetValue<bool>("IsEndingMode"); }
            set { SetValue("IsEndingMode", value); }
        }

        [DependOnProperty("IsEndingMode")]
        public bool IsNormalMode
        {
            get { return !IsEndingMode; }
        }
    }
}
