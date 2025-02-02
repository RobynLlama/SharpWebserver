# SharpWebserver

![Static Badge](https://img.shields.io/badge/Language-C%23-blue?style=flat-square&logo=sharp)

![Static Badge](https://img.shields.io/badge/License-GPLv3-orange?style=flat-square&logo=gnuemacs)

A very simple webserver that serves files from your local app data.

## Features

- Serves files like a normal webserver
- Autobans clients that try to navigate below the baseRoot
- Safe mode flag for only sending a dummy file for each request
- Processes .cs files into cached methods in memory and generates pages from them on request

## Configuration

Todo!

## Using the Interop to Create Webpages

All rendered webpage scripts should adhere to SharpWebserver.Interop.IScriptPage in order to properly compile and run. Your page renderer will be send the connected client information as well as a Dictionary of string argument names and value that represent both the GET address variables and processed POST variables if they were an encoded form.

### Example

> **Index.cs**
> 
> This example will output all client info and arguments sent to the page

```cs
using SharpWebserver.Interop;
using SharpWebserver.Clients;
using System.Text;
using System.Collections.Generic;

public class Webpage : IScriptPage
{
  public byte[] CreatePage(ConnectedClient client, Dictionary<string, string> arguments)
  {
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
    buffer.Append($"<p>Your client id is: {client.ClientID} <br />Your remote is {client.Remote}</p>");
    buffer.Append("<p>Your request sent the following arguments</p>");
    buffer.AppendLine("<ul>");
    foreach (var item in arguments)
    {
      buffer.Append("<li>");
      buffer.Append(item.Key);
      buffer.Append(" : ");
      buffer.Append(item.Value);
      buffer.AppendLine("</li>");
    }
    buffer.AppendLine("</ul>");
    buffer.Append("""
      </body>
    </html>
    """);

    return Encoding.UTF8.GetBytes(buffer.ToString());
  }
}
```

> **Form.cs**
>
> for the badgeThis example will redirect the user to `Index.cs` after they submit the form

```cs
using SharpWebserver.Interop;
using SharpWebserver.Clients;
using System.Text;
using System.Collections.Generic;

public class Webpage : IScriptPage
{
  public byte[] CreatePage(ConnectedClient client, Dictionary<string, string> arguments)
  {
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

        <h1>Submit Your Name</h1>

        <!-- Form starts here -->
        <form action="Index.cs" method="post">
            <label for="name">Enter your name:</label>
            <input type="text" id="name" name="name" required>
            <button type="submit">Submit</button>
        </form>

    </body>
    </html>
    """);

    return Encoding.UTF8.GetBytes(buffer.ToString());
  }
}

```

## Defaults

The default base directory is `%localAppData%/RobynLlama/SharpWebserver` on windows and `~/.local/share/RobynLlama/SharpWebserver` on most Linux distros. See [Special Folders](https://learn.microsoft.com/en-us/dotnet/api/system.environment.getfolderpath?view=net-8.0) on the MSDN docs for more on where this location is on your system.
