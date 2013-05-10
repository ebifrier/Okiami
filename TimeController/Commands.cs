using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ragnarok;
using Ragnarok.Presentation;
using Ragnarok.Presentation.Command;

namespace TimeController
{
    public static class Commands
    {
        #region Show TimeWindow
        public static readonly RelayCommand ShowTimeWindow =
            new RelayCommand(ExecuteShowTimeWindow);

        private static void ExecuteShowTimeWindow()
        {
            var window = new TimeWindow
            {
            };

            window.Show();
        }
        #endregion
    }
}
