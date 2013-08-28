using System;
using System.Security.Permissions;

namespace Pentaho.Launcher.Core
{
  [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust", Unrestricted = false)]
  public class LauncherCore
  {
    private ProcessWrapper process;
    private readonly LauncherConfiguration configuration;

    public LauncherCore()
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

    private void ValidateEnvironment()
    {
      configuration.ValidateEnvironment();
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

    public static int DefaultMain(string[] args)
    {
      LauncherCore launcherCore = new LauncherCore();
      launcherCore.ValidateEnvironment();
      launcherCore.RegisterExtensions();
      return launcherCore.LaunchExecutable();
    }
  }
}
