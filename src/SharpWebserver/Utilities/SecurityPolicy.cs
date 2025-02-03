/*
Copyright (C) 2025 Robyn (robyn@mamallama.dev)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
*/

using System.Net;
using SharpWebserver.Config;

namespace SharpWebserver;

public static partial class Utilities
{
  public static class SecurityPolicy
  {
    private static ClientBlockList blockedClients = new();

    public static void SaveBlockList()
    {
      ConfigManager.SaveConfig("bannedClients.json", blockedClients);
    }
    public static void LoadBlockList()
    {
      if (ConfigManager.LoadConfig<ClientBlockList>("bannedClients.json") is ClientBlockList list)
        blockedClients = list;
    }
    public static void BanRemote(string item)
    {
      Logger.LogWarning("Enacting security policy on client", [
        ("Remote", item)
      ]);

      if (!blockedClients.BlockList.Contains(item))
        blockedClients.BlockList.Add(item);
    }
    public static void BanRemote(EndPoint remote)
    {
      string? item = remote.ToString()?.Split(':')[0];

      if (item is not null)
        BanRemote(item);
    }

    public static bool AllowClient(EndPoint remote)
    {
      string? item = remote.ToString()?.Split(':')[0];

      if (item is not null)
      {
        if (blockedClients.BlockList.Contains(item))
          return false;
      }

      return true;
    }
  }
}
