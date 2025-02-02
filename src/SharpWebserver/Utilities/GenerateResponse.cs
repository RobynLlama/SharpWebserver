/*
Copyright (C) 2025 Robyn (robyn@mamallama.dev)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
*/

using System.Text;

namespace SharpWebserver;

public static partial class Utilities
{
  public static byte[] GenerateResponse(uint code, string reason, byte[] content, string contentType, bool close = false)
  {

    StringBuilder sb = new();
    sb.Append("HTTP/1.1 ");
    sb.Append(code);
    sb.Append(' ');
    sb.AppendLine(reason);
    sb.Append("Content-Type: ");
    sb.AppendLine(contentType);
    sb.AppendLine($"Content-Length: {content.Length}");
    if (close)
      sb.AppendLine("Connection: close");
    else
      sb.AppendLine("Connection: keep-alive");
    sb.AppendLine("Server: SharpServer/" + ListenServer.Version);
    sb.AppendLine(); //Blank line to separate headers from body

    //Process header
    var header = Encoding.UTF8.GetBytes(sb.ToString());
    return [.. header, .. content];
  }

  public static byte[] GenerateResponse(uint code, string reason, string content, string contentType, bool close = false)
  {
    return GenerateResponse(code, reason, Encoding.UTF8.GetBytes(content), contentType, close);
  }
}
