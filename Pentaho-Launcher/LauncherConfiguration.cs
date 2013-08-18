using System;
using System.Configuration;
using System.IO;
using Microsoft.Win32;

namespace Pentaho
{
  public class LauncherConfiguration
  {
    private string[] args;
    private string executable;
    private string[] extensions;
    private bool repairRegistration;
    private string programId;
    private string fullPathExecutable;
    private string javaVersion;
    private string javaHome;
    private bool fixJavaHome;
    private string workingDirectory;

    public LauncherConfiguration()
    {
      args = Environment.GetCommandLineArgs();
    }

    public string WorkingDirectory
    {
      get { return workingDirectory; }
    }

    public string[] Args
    {
      get { return args; }
    }

    public string Executable
    {
      get { return executable; }
    }

    public string[] Extensions
    {
      get { return extensions; }
    }

    public bool RepairRegistration
    {
      get { return repairRegistration; }
    }

    public string ProgramId
    {
      get { return programId; }
    }

    public string FullPathExecutable
    {
      get { return fullPathExecutable; }
    }

    public string JavaVersion
    {
      get { return javaVersion; }
    }

    public string JavaHome
    {
      get { return javaHome; }
    }

    public bool FixJavaHome
    {
      get { return fixJavaHome; }
    }

    private string GetConfiguration(string key, string defaultValue)
    {
      try
      {
        string appSetting = ConfigurationManager.AppSettings[key];
        if (appSetting == null)
        {
          return defaultValue;
        }
        return appSetting;
      }
      catch (ConfigurationErrorsException ce)
      {
        Console.Out.WriteLine("[Launcher] Unable to query configuration entry '{0}': {1}.", key, ce);
      }
      return defaultValue;
    }

    private string[] GetConfigurationList(string key)
    {
      string value = GetConfiguration(key, null);
      if (value == null)
      {
        return new string[0];
      }
      return value.Split(';');
    }

    public void LoadConfiguration()
    {
      // Get the configuration file.
      extensions = GetConfigurationList("Extensions");
      executable = GetConfiguration("Executable", "report-designer.bat");
      javaVersion = GetConfiguration("JavaVersion", "1.6");
      fixJavaHome = "True".Equals(GetConfiguration("FixJavaHome", "True"), StringComparison.InvariantCultureIgnoreCase);
      programId = GetConfiguration("ProgramId", "PentahoReportDesigner");
      fullPathExecutable = args[0];
      workingDirectory = Path.GetDirectoryName(fullPathExecutable);
      repairRegistration = "True".Equals(GetConfiguration("AutoRepair", "False"), StringComparison.InvariantCultureIgnoreCase);

      for (int i = 1; i < args.Length; i++)
      {
        string arg = args[i];
        if ("--repair".Equals(arg))
        {
          repairRegistration = true;
          args[i] = null;
        }
      }
    }

    public void ValidateEnvironment()
    {
      if (FixJavaHome == false)
      {
        return;
      }

      string pentahoJavaHome = Environment.GetEnvironmentVariable("PENTAHO_JAVA_HOME");
      if (String.IsNullOrEmpty(pentahoJavaHome) == false)
      {
        if (Directory.Exists(pentahoJavaHome))
        {
          return;
        }
        this.javaHome = pentahoJavaHome;
        Console.Out.WriteLine("[Launcher] PENTAHO_JAVA_HOME environment variable defined, but does not point to a valid directory.");
      }

      string javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
      if (String.IsNullOrEmpty(javaHome) == false)
      {
        if (Directory.Exists(javaHome))
        {
          return;
        }
        this.javaHome = javaHome;
        Console.Out.WriteLine("[Launcher] JAVA_HOME environment variable defined, but does not point to a valid directory.");
      }

      using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\JavaSoft\Java Runtime Environment\" + javaVersion))
      {
        if (registryKey == null)
        {
          // NO JDK defined. 
          Console.Out.WriteLine("[Launcher] Neither JAVA_HOME or PENTAHO_JAVA_HOME environment variable defined and the registry does not contain a trace of a JRE {0}.", javaVersion);
          return;
        }

        string value = registryKey.GetValue("JavaHome") as string;
        if (value != null)
        {
          if (Directory.Exists(value))
          {
            Console.Out.WriteLine("[Launcher] Neither JAVA_HOME or PENTAHO_JAVA_HOME environment variable defined. Using registry default value.");
            this.javaHome = value;
            return;
          }
          Console.Out.WriteLine("[Launcher] Java location defined in your registry is pointing to a non-existing directory. Unable to continue.");
          return;
        }

        Console.Out.WriteLine("[Launcher] Neither JAVA_HOME or PENTAHO_JAVA_HOME environment variable defined and the registry does not contain a trace of a JRE {0}.", javaVersion);
      }
    }
  }
}