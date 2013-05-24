using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Ragnarok.Presentation;
using Ragnarok.Presentation.Control;

namespace TimeController
{
    /// <summary>
    /// TimeWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class TimeWindow : MovableWindow
    {
        public TimeWindow()
        {
            InitializeComponent();

            DataContext = Global.MainViewModel;
        }
    }
}
