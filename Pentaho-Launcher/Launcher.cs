using System;

namespace Pentaho
{
  internal class Launcher
  {
    private ProcessWrapper process;
    private readonly LauncherConfiguration configuration;

    private Launcher()
    {
      Console.Out.WriteLine("[Launcher] Starting Pentaho Launcher");
      configuration = new LauncherConfiguration();
      configuration.LoadConfiguration();
      
      Console.Out.WriteLine("[Launcher] Launcher location '{0}'.", configuration.FullPathExecutable);

      Console.CancelKeyPress += CancelHandler;
    }

    private void CancelHandler(object sender, ConsoleCancelEventArgs args)
    {
      args.Cancel = true;
      if (process != null)
      {
        process.InterruptRequested = true;
      }
    }

    private bool ValidateEnvironment()
    {
      return configuration.ValidateEnvironment();
    }

    private void RegisterExtensions()
    {
      LauncherFileRegistration registration = new LauncherFileRegistration(configuration);
      registration.RegisterExtensions();
    }

    private int LaunchExecutable()
    {
      this.process = new ProcessWrapper(configuration);
      return this.process.LaunchExecutable();
    }


    [STAThread]
    private static int Main(string[] args)
    {
      Launcher launcher = new Launcher();
      if (launcher.ValidateEnvironment() == false)
      {
        return -1;
      }
      launcher.RegisterExtensions();
      return launcher.LaunchExecutable();
    }
  }
}
