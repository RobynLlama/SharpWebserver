/*
Copyright (C) 2025 Robyn (robyn@mamallama.dev)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
*/

using System;
using SharpWebserver.Interop;

namespace SharpWebserver.Caching;

public class CachedPage(IScriptPage item)
{
  public readonly IScriptPage Item = item;
  public readonly DateTime CachedUntil = DateTime.Now.AddMinutes(60);
  public bool Stale => DateTime.Now > CachedUntil;
}
