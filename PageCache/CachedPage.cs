using System;
using SharpWebserver.Interop;

namespace SharpWebserver.Caching;

public class CachedPage(IScriptPage item)
{
  public readonly IScriptPage Item = item;
  public readonly DateTime CachedUntil = DateTime.Now.AddMinutes(60);
  public bool Stale => DateTime.Now > CachedUntil;
}
