using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HostLocker
{
    public static class Switcher {
        public static WindowSwitcher WindowSwitcher;

        public static void Switch(UserControl newPage) {
            WindowSwitcher.Navigate(newPage);
        }

        public static void Switch(UserControl newPage, object state) {
            WindowSwitcher.Navigate(newPage, state);
        }
    }
}
