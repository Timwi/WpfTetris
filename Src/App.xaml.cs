using System;
using System.Windows;
using RT.Util;

namespace WpfTetris
{
    public partial class App : Application
    {
        public static Random Rnd = new Random();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
