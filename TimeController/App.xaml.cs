using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace TimeController
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Global.Initialize();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Global.Uninitialize();

            base.OnExit(e);
        }
    }
}
