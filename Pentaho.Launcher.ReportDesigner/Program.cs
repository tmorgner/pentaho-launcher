using System;
using Pentaho.Launcher.Core;

namespace Pentaho.Launcher.ReportDesigner
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static int Main(string[] args)
    {
      return LauncherCore.DefaultMain(args);
    }
  }
}
