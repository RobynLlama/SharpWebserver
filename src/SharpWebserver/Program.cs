/*
Copyright (C) 2025 Robyn (robyn@mamallama.dev)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
*/

using System.Net;
using System.Net.Sockets;
using CSScriptLib;
using System;
using System.IO;
using System.Threading.Tasks;
using SharpWebserver.Config;
using System.Runtime.InteropServices;

namespace SharpWebserver;
partial class ListenServer
{
  public static string BaseDir { get; private set; } = string.Empty;
  public static string WebRoot { get; private set; } = string.Empty;
  public static string ConfigDir { get; private set; } = string.Empty;
  public static string ReferenceDir { get; private set; } = string.Empty;
  public static readonly IEvaluator ScriptRunner = CSScript.Evaluator.ReferenceAssembly(typeof(ListenServer).Assembly);
  public static bool SafeMode => GlobalConfig.SafeMode;
  public static string Version { get; private set; } = string.Empty;
  private static SharpConfig GlobalConfig = new();
  public static string LICENSE => """
  ---------------------------------------------------------------------
  SharpWebserver  Copyright (C) 2025  Robyn <Robyn@mamallama.dev>
    This program comes with ABSOLUTELY NO WARRANTY;
    This is free software, and you are welcome to redistribute it
    under certain conditions; See the included license for details.

  You should have received a copy of the GNU General Public License
  along with this program.  If not, see <https://www.gnu.org/licenses/>
  ---------------------------------------------------------------------
  
  """;

  static async Task Main(string[] args)
  {

    ProcessArguments(in args);

    if (ExitNow)
      goto EndProgram;


    Console.WriteLine(LICENSE);

    var ver = typeof(ListenServer).Assembly.GetName().Version;

    if (ver is null)
    {
      Logger.LogError("Somehow booted up without a version!");
      return;
    }

    //Only use the default if none specified
    if (BaseDir == string.Empty)
      BaseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RobynLlama", "SharpWebserver");

    WebRoot = Path.Combine(BaseDir, "www");
    ConfigDir = Path.Combine(BaseDir, "config");
    ReferenceDir = Path.Combine(BaseDir, "references");
    Version = $"v{ver.Major}.{ver.Minor}.{ver.Revision}";

    Utilities.EnsureDirectory(BaseDir);
    Utilities.EnsureDirectory(WebRoot);
    Utilities.EnsureDirectory(ConfigDir);
    Utilities.EnsureDirectory(ReferenceDir);

    //Load global config
    if (ConfigManager.LoadConfig<SharpConfig>("SharpConfig.toml") is not SharpConfig gc)
      Logger.LogInfo("Using default global config");
    else
      GlobalConfig = gc;

    Utilities.SecurityPolicy.LoadBlockList();
    CSScript.GlobalSettings.AddSearchDir(ReferenceDir);
    AppDomain.CurrentDomain.AssemblyResolve += Utilities.ResolveAssembly;

    Logger.LogInfo("Startup", [
      ("BaseDir", BaseDir),
      ("Version", Version),
      ("SafeMode", SafeMode),
    ]);

    //Define the local endpoint for the socket.
    HttpListener listener = new();

    //Set valid prefixes
    listener.Prefixes.Add($"http://127.0.0.1:{GlobalConfig.PortNumber}/");

    //Register the input handler
    var InputHandler = Task.Run(HandleInputAsync);

    try
    {
      // Start listening for client requests.
      listener.Start();
      Logger.LogInfo("Server starting up", [
        ("Bound Port", GlobalConfig.PortNumber),
        ("Status", "Waiting for connections")
      ]);

      void ContextReceivedCallback(IAsyncResult ar)
      {
        var context = listener.EndGetContext(ar);

        Logger.LogInfo("New Request", [
          ("From", context.Request.RemoteEndPoint),
          ("Document", context.Request.RawUrl),
          ("Query Count", context.Request.QueryString.Count),
          ("Has Post Data?", context.Request.HasEntityBody)
          ]);

        ServePageToClient(context);
        listener.BeginGetContext(ContextReceivedCallback, listener);
      }

      listener.BeginGetContext(ContextReceivedCallback, listener);

      #region Main Loop

      while (listener.IsListening)
      {
        if (ExitNow)
          break;

        await Task.Delay(10);
      }

      #endregion

    }
    catch (HttpListenerException ex)
    {

      Logger.LogError("The server encountered an unrecoverable error during operation", [
        ("Stack", ex)
      ]);

      if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        if (GlobalConfig.PortNumber < 1025)
        {
          Console.WriteLine();
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine("-NOTICE-");
          Console.ResetColor();
          Console.Write("  Unable to bind ports up to 1024 on Unix family OSes without superuser escalation.\n  If this error is occurring before the server even starts up fully then please try running the program again but with ");
          Console.ForegroundColor = ConsoleColor.Yellow;
          Console.Write("sudo");
          Console.ResetColor();
          Console.Write(" or your distro's equivalent superuser\n  or edit ");
          Console.ForegroundColor = ConsoleColor.Magenta;
          Console.Write("SharpConfig.toml");
          Console.ResetColor();
          Console.WriteLine(" to change the port number.\n");
        }
    }
    catch (Exception ex)
    {
      Logger.LogError("Exception!", [
        ("Type", ex.GetType().Name),
        ("Stack", ex.StackTrace)
      ]);
    }

    #region Cleanup

    //Perform cleanup and eventually save configurations here, etc
    ConfigManager.SaveConfig("SharpConfig.toml", GlobalConfig);
    Utilities.SecurityPolicy.SaveBlockList();

    #endregion

    Console.WriteLine();

  //Jump here to exit program gracefully
  EndProgram:
    Logger.LogInfo("Goodbye!");
  }
}
