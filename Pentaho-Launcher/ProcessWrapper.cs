using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Pentaho
{
  public class ProcessWrapper
  {
    private readonly string[] args;
    private readonly string javaHome;
    private readonly string executable;
    private bool interruptRequested;
    private readonly string workingDirectory;

    public ProcessWrapper(LauncherConfiguration configuration)
    {
      this.workingDirectory = configuration.WorkingDirectory;
      this.executable = configuration.Executable;
      this.javaHome = configuration.JavaHome;
      this.args = Environment.GetCommandLineArgs();
    }

    public bool InterruptRequested
    {
      get { return interruptRequested; }
      set { interruptRequested = value; }
    }

    private string EncodeAllArguments()
    {
      StringBuilder builder = new StringBuilder();

      for (int i = 1; i < args.Length; i++)
      {
        string arg = args[i];
        if (i != 0)
        {
          builder.Append(" ");
        }
        builder.Append(EncodeParameterArgument(arg));
      }
      return builder.ToString();
    }


    /// <summary>
    /// Encodes an argument for passing into a program
    /// </summary>
    /// <param name="original">The value that should be received by the program</param>
    /// <returns>The value which needs to be passed to the program for the original value 
    /// to come through</returns>
    public static string EncodeParameterArgument(string original)
    {
      if (string.IsNullOrEmpty(original))
      {
        return original;
      }
      // via: http://stackoverflow.com/a/6040946

      // Backslashes are only treated as escape signal if they are followed by a quote. 
      // But if there are multiple backslashes, they get doubled up. 
      string value = Regex.Replace(original, @"(\\*)" + "\"", @"$1\$0");
      // Test whether the argument ends in a backslash, and if so, double the argument up
      // this covers the case where a argument ends on a backslash, where the next double-quote
      // would then cause the backslash to be interpreted as escaping the quote.
      value = Regex.Replace(value, @"^(.*\s.*?)(\\*)$", "\"$1$2$2\"");
      return value;
    }


    private void WaitWithInterrupt(Process process)
    {
      while (process.HasExited == false)
      {
        Console.Out.WriteLine("[Launcher] Waiting 500ms.");
        process.WaitForExit(500);

        if (interruptRequested)
        {
          if (process.CloseMainWindow() == false)
          {
            process.Kill();
          }
          Console.Out.WriteLine("[Launcher] Interrupt detected. Sending Kill-Signal and waiting.");
          process.WaitForExit();
          return;
        }
      }

      Console.Out.WriteLine("[Launcher] Process ended naturally. Waiting for final cleanup.");
      process.WaitForExit();
    }

    private void ProcessStdError(object sendingProcess, DataReceivedEventArgs outLine)
    {
      if (!String.IsNullOrEmpty(outLine.Data)) // use the output outLine.Data somehow;
      {
        Console.Error.WriteLine(outLine.Data);
      }
    }

    private void ProcessStdOut(object sendingProcess, DataReceivedEventArgs outLine)
    {
      if (!String.IsNullOrEmpty(outLine.Data)) // use the output outLine.Data somehow;
      {
        Console.Out.WriteLine(outLine.Data);
      }
    }

    public int LaunchExecutable()
    {
      int exitCode = 0;
      bool repeat = true;
      while (repeat)
      {
        Process process = new Process();
        process.StartInfo.FileName = executable;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.WorkingDirectory = workingDirectory;
        process.StartInfo.Arguments = EncodeAllArguments();
        process.OutputDataReceived += ProcessStdOut;
        process.ErrorDataReceived += ProcessStdError;
        if (javaHome != null && process.StartInfo.EnvironmentVariables.ContainsKey("JAVA_HOME") == false)
        {
          process.StartInfo.EnvironmentVariables.Add("JAVA_HOME", javaHome);
        }
        if (process.StartInfo.EnvironmentVariables.ContainsKey("PENTAHO_JAVA") == false)
        {
          process.StartInfo.EnvironmentVariables.Add("PENTAHO_JAVA", "javaw.exe");
        }

        try
        {
          // Start the process with the info we specified.
          // Call WaitForExit and then the using statement will close.
          using (process)
          {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            Console.Out.WriteLine("[Launcher] Started process, now waiting for exit.");
            WaitWithInterrupt(process);

            exitCode = process.ExitCode;
            if (exitCode != -128)
            {
              repeat = false;
            }
            else
            {
              Console.Out.WriteLine("[Launcher] Process requested restart, error code -128 received. ");
            }
          }
        }
        catch (Exception e)
        {
          Console.Out.WriteLine("[Launcher] Error on launching executable '{0}'", e);
          return -2;
        }
      }
      Console.Out.WriteLine("[Launcher] Finished with exit-code " + exitCode);
      return exitCode;
    }
 
  }
}