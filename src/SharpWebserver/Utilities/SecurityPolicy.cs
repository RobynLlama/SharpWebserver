/*
Copyright (C) 2025 Robyn (robyn@mamallama.dev)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
*/

using System.Collections.Generic;
using System.Net;

namespace SharpWebserver;

public static partial class Utilities
{
  private static readonly List<string> blockedClients = [];
  public static void BanRemote(EndPoint remote)
  {
    string? item = remote.ToString()?.Split(':')[0];

    Logger.LogWarning("Enacting security policy on client", [
      ("Remote", item)
    ]);

    if (item is not null)
      blockedClients.Add(item);
  }

  public static bool AllowClient(EndPoint remote)
  {
    string? item = remote.ToString()?.Split(':')[0];

    if (item is not null)
    {
      if (blockedClients.Contains(item))
        return false;
    }

    return true;
  }
}
