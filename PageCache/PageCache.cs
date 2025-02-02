using System;
using System.Collections.Generic;
using System.IO;
using SharpWebserver.Interop;

namespace SharpWebserver.Caching;

public static class PageCache
{

  private static readonly Dictionary<string, CachedPage> CachedObjects = [];

  public static IScriptPage? FetchPage(FileInfo file)
  {
    if (CachedObjects.TryGetValue(file.FullName, out var cache))
    {
      if (!cache.Stale)
      {
        Logger.LogInfo("Returning a cached builder", [
          ("Filename", file.FullName.Replace(ListenServer.BaseDir, string.Empty))
        ]);
        return cache.Item;
      }
    }

    Logger.LogInfo("Getting a fresh builder");

    try
    {
      string script;
      using (var textReader = file.OpenText())
        script = textReader.ReadToEnd();

      var builder = ListenServer.ScriptRunner.LoadCode<IScriptPage>(script);
      CachedObjects[file.FullName] = new(builder);
      return builder;
    }
    catch (Exception ex)
    {
      Logger.LogError("Script compilation error", [
        ("Exception", ex)
      ]);
      return null;
    }
  }
}
