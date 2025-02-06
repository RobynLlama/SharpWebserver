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

  This page requires you to build ExampleLibrary and place
  it in your /references/ folder to demo assembly resolution
*/

using System.Net;
using System.Text;
using ExampleLibrary;
using SharpWebserver.Interop;

public class Webpage : IScriptPage
{
  public byte[] CreatePage(HttpListenerRequest request)
  {
    var thing = new MyCoolThing(5);

    StringBuilder buffer = new();
    buffer.Append("""
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>Simple Form Example</title>
    </head>
    <body>
    """);

    buffer.Append(thing.AmazingMethod);

    buffer.Append(
    """
    </body>
    </html>
    """);

    return Encoding.UTF8.GetBytes(buffer.ToString());
  }
}
