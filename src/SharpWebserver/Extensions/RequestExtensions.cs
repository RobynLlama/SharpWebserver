/*
Copyright (C) 2025 Robyn (robyn@mamallama.dev)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
*/

using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;

namespace SharpWebserver.Extensions;
public static class RequestExtensions
{
  public static string GetFirstOrDefaultForKey(this NameValueCollection query, string key)
  {
    if (query.GetValues(key) is not string[] values)
      return string.Empty;

    return values[0];
  }

  public static Dictionary<string, string> GetFirstOrDefaultForAll(this NameValueCollection query)
  {
    var keys = query.AllKeys;
    var data = new Dictionary<string, string>();

    foreach (var item in keys)
    {
      if (item is null)
        continue;

      data[item] = query.GetFirstOrDefaultForKey(item);
    }

    return data;
  }

  public static Dictionary<string, string>? GetDecodedFormDataFirstOrDefault(this HttpListenerRequest request)
  {
    var formData = new Dictionary<string, string>();

    //name=value1&name=value2&name=value3

    if (request.ContentType != "application/x-www-form-urlencoded" || request.ContentLength64 == 0)
      return null;

    using StreamReader data = new(request.InputStream);
    //data.BaseStream.Seek(0, SeekOrigin.Begin);

    string input = data.ReadToEnd();
    string[] pairs = input.Split('&');

    foreach (var item in pairs)
    {
      var kvp = item.Split('=');

      if (kvp.Length != 2)
        continue;

      if (formData.TryGetValue(kvp[0], out var _))
        continue;
      else
        formData[kvp[0]] = kvp[1];
    }

    return formData;
  }

  public static Dictionary<string, string[]>? GetDecodedFormData(this HttpListenerRequest request)
  {
    var formData = new Dictionary<string, List<string>>();

    //name=value1&name=value2&name=value3

    if (request.ContentType != "application/x-www-form-urlencoded" || request.ContentLength64 == 0)
      return null;

    using StreamReader data = new(request.InputStream);
    //data.BaseStream.Seek(0, SeekOrigin.Begin);

    string input = data.ReadToEnd();
    string[] pairs = input.Split('&');

    foreach (var item in pairs)
    {
      Logger.LogTrace("Splitting pair", [
        ("Input", item)
      ]);

      var kvp = item.Split('=');

      if (kvp.Length != 2)
        continue;

      List<string> addingList;

      if (formData.TryGetValue(kvp[0], out var strings))
        addingList = strings;
      else
        addingList = [];

      addingList.Add(kvp[1]);
      formData[kvp[0]] = addingList;
    }

    Dictionary<string, string[]> items = [];

    //Convert to array
    foreach (var item in formData)
      items[item.Key] = [.. item.Value];

    return items;
  }
}
