/*
Copyright (C) 2025 Robyn (robyn@mamallama.dev)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SharpWebserver;
public static partial class Utilities
{
  private static readonly List<string> RefAssemblies = [];
  public static string[] ReferenceAssemblies => [.. RefAssemblies];
  public static Assembly? ResolveAssembly(object? sender, ResolveEventArgs args)
  {

    var assemblyName = new AssemblyName(args.Name);

    if (assemblyName.Name is null)
      return null;

    FileInfo load = new(Path.Combine(ListenServer.ReferenceDir, assemblyName.Name + ".dll"));
    var LoadFrom = load.FullName.Replace(ListenServer.BaseDir, string.Empty);

    Logger.LogInfo("Resolving an assembly", [
      ("Assembly", assemblyName.Name),
      ("Load From", LoadFrom),
      ("Located?", load.Exists)
    ]);

    if (load.Exists)
    {
      RefAssemblies.Add(LoadFrom);
      return Assembly.LoadFrom(load.FullName);
    }


    return null;
  }
}
