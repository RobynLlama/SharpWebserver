using System;
using System.IO;

namespace SharpWebserver;
public static partial class Utilities
{
  public static bool EnsureDirectory(string path)
  {
    DirectoryInfo info = new(path);
    Logger.LogTrace("Ensuring a path", [
      ("Path", path)
    ]);

    if (info.Exists)
    {
      Logger.LogTrace("Path exists");
      return true;
    }

    Logger.LogTrace("Creating path");
    try
    {
      info.Create();
    }
    catch (Exception ex)
    {
      Logger.LogError("Unable to ensure directory", [
        ("Path", path),
        ("Error", ex)
      ]);
    }

    return true;
  }
}
