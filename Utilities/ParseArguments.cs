/*
Copyright (C) 2025 Robyn (robyn@mamallama.dev)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
*/

using System;
using System.Net;
using System.Collections.Generic;

namespace SharpWebserver;

public static partial class Utilities
{
  public static void ParseArguments(Uri request, Dictionary<string, string> vars)
  {
    var args = WebUtility.UrlDecode(request.Query.Trim('?')).Split('&');
    ParseArguments(args, vars);
  }

  public static void ParseArguments(string input, Dictionary<string, string> vars)
  {
    var args = WebUtility.UrlDecode(input).Split('&');
    ParseArguments(args, vars);
  }

  public static void ParseArguments(string[] input, Dictionary<string, string> vars)
  {
    foreach (var pair in input)
    {
      var temp = pair.Split('=');

      if (temp.Length < 2)
        continue;

      //Logger.LogTrace("ArgPair", [
      //("Key", temp[0]),
      //("Value", temp[1])
      //]);

      vars[temp[0]] = temp[1];
    }

  }
}
