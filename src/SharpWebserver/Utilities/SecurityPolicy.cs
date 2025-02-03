/*
Copyright (C) 2025 Robyn (robyn@mamallama.dev)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
*/

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharpWebserver;

public static partial class Utilities
{
  private class ClientBlockList
  {
    [JsonInclude]
    public List<string> BlockList = [];
  }

  private static ClientBlockList blockedClients = new();

  public static void SaveBlockList()
  {
    var path = Path.Combine(ListenServer.ConfigDir, "bannedClients.json");
    var config = new FileInfo(path);

    using (var writer = config.OpenWrite())
    {
      JsonSerializer.Serialize(writer, blockedClients);
      Logger.LogInfo("Wrote out bannedClients.json");
    }
  }
  public static void LoadBlockList()
  {
    var path = Path.Combine(ListenServer.ConfigDir, "bannedClients.json");
    var config = new FileInfo(path);

    if (config.Exists)
    {
      Logger.LogInfo("Refreshing block list from disk", [
        ("config-path", path)
      ]);

      using (var reader = config.OpenText())
      {
        string listData = reader.ReadToEnd();
        var thing = JsonSerializer.Deserialize<ClientBlockList>(listData);

        if (thing is not null)
        {
          Logger.LogTrace("Finished loading block list");
          blockedClients = thing;
          return;
        }

        Logger.LogError("Unable to parse data as ClientBlockList", [
          ("Size", listData.Length)
        ]);
      }
    }
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
