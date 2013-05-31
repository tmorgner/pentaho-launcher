using System;
using Microsoft.Win32;

namespace Pentaho
{
  public class LauncherFileRegistration
  {
    private LauncherConfiguration configuration;

    public LauncherFileRegistration(LauncherConfiguration configuration)
    {
      this.configuration = configuration;
    }

    public void RegisterExtensions()
    {
      if (configuration.FullPathExecutable == null)
      {
        Console.Out.WriteLine("[Launcher] Unable to find local path. Skipping file registration.");
        return;
      }

      string[] extensions = configuration.Extensions;
      if (extensions.Length == 0)
      {
        return;
      }

      using (RegistryKey classes = Registry.CurrentUser.OpenSubKey("Software\\Classes", true))
      {
        RegisterProgramId(classes);

        for (int i = 0; i < extensions.Length; i++)
        {
          string extension = extensions[i];
          if (IsExtensionRegistered(classes, extension))
          {
            Console.Out.WriteLine("[Launcher] A registration for extension '{0}' exists. Skipping.", extension);
            continue;
          }

          RegisterExtension(classes, extension);
        }
      }
    }

    private void RegisterExtension(RegistryKey classes, string extension)
    {
      try
      {
        string programId = configuration.ProgramId;

        using (RegistryKey wrKey = classes.CreateSubKey(extension))
        {
          wrKey.SetValue("", programId);
        }
        Console.Out.WriteLine("[Launcher] A registration for extension '{0}' has been added successfully.", extension);
      }
      catch (Exception e)
      {
        Console.Out.WriteLine("[Launcher] Error on registering file extension '{0}': {1}", extension, e);
      }
    }

    private void RegisterProgramId(RegistryKey classes)
    {
      try
      {
        string programId = configuration.ProgramId;

        using (RegistryKey clsKey = classes.CreateSubKey(programId))
        {
          clsKey.SetValue("", "Pentaho Registration");
        }

        using (RegistryKey openKey = classes.CreateSubKey(programId + @"\DefaultIcon"))
        {
          openKey.SetValue("", ProcessWrapper.EncodeParameterArgument(configuration.FullPathExecutable) + ",0");
        }

        using (RegistryKey openKey = classes.CreateSubKey(programId + @"\shell\open\command"))
        {
          openKey.SetValue("", ProcessWrapper.EncodeParameterArgument(configuration.FullPathExecutable) + " %1");
        }
      }
      catch (Exception e)
      {
        Console.Out.WriteLine("[Launcher] Error on registering program-id '{0}': {1}", configuration.ProgramId, e);
      }
    }

    private bool IsExtensionRegistered(RegistryKey classes, string extension)
    {
      RegistryKey roKey = classes.OpenSubKey(extension);
      if (configuration.RepairRegistration == false && roKey != null)
      {
        return true;
      }
      return false;
    }
  }
}
