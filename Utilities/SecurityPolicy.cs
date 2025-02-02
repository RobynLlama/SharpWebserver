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
