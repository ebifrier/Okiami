using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoteSystem.PluginShogi.ViewModel
{
    //public delegate ;

    /// <summary>
    /// 分割された処理を直列的に書けるコルーチンです。
    /// </summary>
    public class Coroutine
    {
        private IEnumerator<bool> coroutine;

        /// <summary>
        /// コルーチンを実行します。
        /// </summary>
        public bool Execute()
        {
            if (!this.coroutine.MoveNext())
            {
                return false;
            }

            return this.coroutine.Current;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Coroutine(IEnumerable<bool> coroutine)
        {
            this.coroutine = coroutine.GetEnumerator();
        }
    }

    public class CoroutineManager
    {
    }
}
