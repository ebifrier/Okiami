using System;
using System.Configuration;
using System.ComponentModel;
using System.Reflection;

using Ragnarok;
using Ragnarok.Utility;

namespace VoteSystem.PluginShogi
{
    /// <summary>
    /// 設定を保存するオブジェクトです。
    /// </summary>
    public sealed partial class Settings
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Settings()
        {
            var asm = Assembly.GetExecutingAssembly();
            SetLocationFromAssembly(asm);
        }
    }
}
