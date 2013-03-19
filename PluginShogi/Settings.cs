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
        /// エフェクトの個別の使用フラグを取得します。
        /// </summary>
        public bool HasEffectFlag(Effects.EffectFlag flag)
        {
            if (!SD_IsUseEffect)
            {
                return false;
            }

            return ((SD_EffectFlag & flag) != 0);
        }

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
