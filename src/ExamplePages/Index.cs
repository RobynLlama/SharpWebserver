/*
The contents of this file are provided under the CC0
"No rights reserved" license in the hopes it will be
useful.

Original Author: Robyn (robyn@mamallama.dev)

Notes:
  There will be errors shown in your IDE because these
  pages are not meant to be compiled. They are examples
  you may copy into your /www/ folder for SharpWebserver
  to compile and serve at runtime (where the namespace
  won't be an issue)
*/

using System.Net;
using System.Text;
using SharpWebserver.Extensions;
using SharpWebserver.Interop;
public class Webpage : IScriptPage
{
  public byte[] CreatePage(HttpListenerRequest request)
  {

    var PostArgs = request.GetDecodedFormDataFirstOrDefault();
    var GetArgs = request.QueryString.GetFirstOrDefaultForAll();

    StringBuilder buffer = new();
    buffer.Append("""
    <html>
      <head>
        <title>Simple Server Index</title>
      </head>
      <body>
        <h1>Welcome to SharpServer</h1>
        <p>
          This is the home page, generated from Index.cs!
        </p>
    """);
    buffer.Append($"<p>Your client remote is {request.RemoteEndPoint}</p>");

    buffer.Append("<p>Your get request sent the following arguments</p>");
    buffer.AppendLine("<ul>");
    foreach (var item in GetArgs)
    {
      buffer.Append("<li>");
      buffer.Append(item.Key);
      buffer.Append(" : ");
      buffer.Append(item.Value);
      buffer.AppendLine("</li>");
    }
    buffer.AppendLine("</ul>");

    if (PostArgs is not null)
    {
      buffer.Append("<p>Your POST request sent the following arguments");
      buffer.AppendLine("<ul>");
      foreach (var item in PostArgs)
      {
        buffer.Append("<li>");
        buffer.Append(item.Key);
        buffer.Append(" : ");
        buffer.Append(item.Value);
        buffer.AppendLine("</li>");
      }
      buffer.AppendLine("</ul>");
    }

    buffer.Append("""
      </body>
    </html>
    """);

    return Encoding.UTF8.GetBytes(buffer.ToString());
  }
}
