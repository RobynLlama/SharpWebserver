/*
Copyright (C) 2025 Robyn (robyn@mamallama.dev)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
*/

using System;
using System.IO;
using System.Net;
using System.Text;
using SharpWebserver.Caching;

namespace SharpWebserver;

public partial class ListenServer
{
  private static void SendPageResponse(HttpListenerResponse response, string pageData, string mimeType, int statusCode = 200, string status = "OK")
  {
    var data = Encoding.UTF8.GetBytes(pageData);
    SendPageResponse(response, data, mimeType, statusCode, status);
  }

  private static void SendPageResponse(HttpListenerResponse response, byte[] pageData, string mimeType, int statusCode = 200, string status = "OK")
  {
    response.ContentType = mimeType;
    response.ContentLength64 = pageData.LongLength;
    response.ContentEncoding = Encoding.UTF8;
    response.StatusCode = statusCode;
    response.StatusDescription = status;
    response.SendChunked = false;
    response.OutputStream.Write(pageData, 0, pageData.Length);
    response.Close();
  }

  private static void ServePageToClient(HttpListenerContext context)
  {
    var request = context.Request;
    var response = context.Response;


    //Blocked clients
    if (!Utilities.SecurityPolicy.AllowClient(request.RemoteEndPoint))
    {
      Logger.LogWarning("Blocked client attempted to connect", [
        ("Client", request.RemoteEndPoint)
      ]);

      SendPageResponse(response, "<html><body><h2>Forbidden</h2><p>Your client is forbidden from accessing this server</p></body></html>", "Text/HTML", 403, "Forbidden");
      return;
    }

    #region Resolve Request

    var uri = request.Url;

    if (uri is null)
    {
      response.Close();
      return;
    }

    var localPath = uri.AbsolutePath;
    string resolvedPath = Path.GetFullPath(Path.Combine(WebRoot, "." + localPath));

    if (!resolvedPath.StartsWith(WebRoot))
    {
      Logger.LogWarning("Attempted to access below root", [
        ("Request", localPath),
        ("Resolved", resolvedPath),
        ("Remote", request.RemoteEndPoint)
      ]);

      Utilities.SecurityPolicy.BanRemote(request.RemoteEndPoint);
      SendPageResponse(response, "Access to this resource is forbidden. Your client information has been logged", "Text/HTML", 403, "Forbidden");
      return;
    }

    string fileName = Path.GetFileName(resolvedPath);
    if (string.IsNullOrEmpty(fileName))
    {
      resolvedPath = Path.Combine(resolvedPath, "Index.cs");
    }

    //Logger.LogInfo("Finished path resolution", [
    //("Original", localPath),
    //("Resolved", resolvedPath)
    //]);

    FileInfo file = new(resolvedPath);

    if (SafeMode)
    {
      Logger.LogTrace("Serving safe mode document");
      SendPageResponse(response, $"<h2>File Served!</h2><p>Request: {file.FullName.Replace(BaseDir, string.Empty)}</p>", "Text/HTML");
      return;
    }

    if (!file.Exists)
    {
      SendPageResponse(response, "<h2>404: File Not Found</h2><p>Resource not found</p>", "text/html", 404, "Not Found");
      return;
    }

    var name = Path.GetFileName(resolvedPath).ToLowerInvariant();
    var chunks = name.Split('.');
    string contentType;

    if (chunks.Length < 2)
      contentType = "text/plain";
    else
      contentType = Utilities.GetMimeType(chunks[1]);

    if (name.EndsWith(".cs"))
    {
      var script = PageCache.FetchPage(file);

      if (script is null)
      {
        SendPageResponse(response, "The requested resource failed to build", "text/html", 500, "Server Error");
        return;
      }

      try
      {
        var data = script.CreatePage();
        SendPageResponse(response, data, contentType);
        return;
      }
      catch (Exception ex)
      {
        SendPageResponse(response, $"The requested resource encountered an exception while running {ex.ToString().Replace("<", "&gt;")}", "text/html", 500, "Server Error");
        return;
      }

    }
    else
    {
      using var reader = file.OpenRead();
      byte[] fileContents = new byte[file.Length];
      reader.Read(fileContents, 0, (int)file.Length);
      SendPageResponse(response, fileContents, contentType);
      return;
    }

    #endregion
  }
}
