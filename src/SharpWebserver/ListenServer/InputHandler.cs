/*
Copyright (C) 2025 Robyn (robyn@mamallama.dev)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
*/

using System;
using System.Threading.Tasks;
using SharpWebserver.Caching;
using SharpWebserver.Config;

namespace SharpWebserver;

partial class ListenServer
{
  /// <summary>
  /// Checks if the standard input has any characters for us
  /// and tries to figure out what the user wants us to do
  /// </summary>

  private static bool ExitNow = false;
  public static async Task HandleInputAsync()
  {
    var input = Console.In;

    while (true)
    {
      var line = await input.ReadLineAsync();

      if (line is null)
        continue;

      var args = line.Split(' ');
      string command;

      if (args.Length == 1)
        command = line;
      else
      {
        command = args[0];
      }

      switch (command)
      {
        case "help":
          Logger.LogInfo("help", [
            ("help", "Show this screen"),
            ("exit", "Exit immediately, saving configs"),
            ("cache-clear", "Clear the page cache (for testing)"),
            ("ban", "Ban a client by IP address"),
            ("config-reload", "Reload the server config file(s)"),
          ]);
          break;
        case "exit":
          ExitNow = true;
          break;
        case "ban":
          if (args.Length < 2)
          {
            Logger.LogInfo("Ban", [
              ("Usage", "Ban <client IP>")
            ]);
            break;
          }
          Utilities.SecurityPolicy.BanRemote(args[1]);
          Utilities.SecurityPolicy.SaveBlockList();
          break;
        case "cache-clear":
          PageCache.ClearCache();
          Logger.LogInfo("Cache cleared");
          break;
        case "config-reload":
          Utilities.SecurityPolicy.LoadBlockList();
          if (ConfigManager.LoadConfig<SharpConfig>("SharpConfig.json") is not SharpConfig gc)
            Logger.LogWarning("Failed to reload global config");
          else
          {
            GlobalConfig = gc;
          }
          Logger.LogInfo("Configs reloaded");
          break;
        default:
          Logger.LogWarning("Unrecognized input, try `help`");
          break;
      }
    }
  }
}
