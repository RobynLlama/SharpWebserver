# SharpWebserver

![Static Badge](https://img.shields.io/badge/Language-C%23-blue?style=flat-square&logo=sharp)

![Static Badge](https://img.shields.io/badge/License-GPLv3-orange?style=flat-square&logo=gnuemacs)

A very simple webserver that serves files from your local app data.

- [Features](#features)
- [Configuration](#configuration)
- [Launch Switches](#launch-switches)
  - [Options](#options)
- [Using Interactive Mode](#using-interactive-mode)
- [Using the Interop to Create Webpages](#using-the-interop-to-create-webpages)
  - [Examples](#examples)

## Features

- Serves files like a normal webserver
- Autobans clients that try to navigate below the baseRoot
- Safe mode flag for only sending a dummy file for each request
- Processes .cs files into cached methods in memory and generates pages from them on request

## Configuration

Global configuration options are stored in `./Config/SharpConfig.toml`

- `SafeMode` : If set to true the server will respond with dummy data instead of real webpages. Great for testing the extents of the server without exposing any real info.
- `PortNumber` : The port the server should bind to. On Unix family OSes ports below 1024 are *protected* and require super user permission to bind. Use `sudo` or choose a higher range port such as 8080.

The remote block list is stored in `./Config/BannedClients.toml`

Each entry is simply an IP that is prohibited from connecting to the server.

## Launch Switches

Use `./SharpServer --help` to display this information in your terminal.

### Options

- `-h`, `--help`          | Show the help screen
- `-p`, `--portable`      | Launch in portable mode using the current directory instead of appdata
- `-l`, `--license`       | output this program's license information and exit
  
> [!TIP]
>
> The default base directory when not launching in portable mode is `%localAppData%/RobynLlama/SharpWebserver` on windows and `~/.local/share/RobynLlama/SharpWebserver` on most Linux distros. See [Special Folders](https://learn.microsoft.com/en-us/dotnet/api/system.environment.getfolderpath?view=net-8.0) on the MSDN docs for more on where this location is on your system.

## Using Interactive Mode

The server continues to accept input from the terminal or console it was launched from during operation. The following commands are supported:

- `help` : Print all acceptable interactive commands
- `exit` : Save configs and exit immediately. No requests will be filled while shutting down.
- `ban` : Used like ban `IPAddress` this will immediately ban the remote associated with that IP address. Existing connections will not be closed but the client will be sent a notice they have been blocked and the connection ended on every subsequent request.
- `cache-clear` : Clears the existing webpage program cache. Great for testing and iterating on a page live.
- `config-reload` : Reloads all configs from disk without saving the current config. Be careful, any unsaved information will be lost. The block list is updated on disk after each entry so this is usually safe to use.

## Using the Interop to Create Webpages

All rendered webpage scripts should adhere to SharpWebserver.Interop.IScriptPage in order to properly compile and run. Your page renderer will be sent the connected client information as well as a Dictionary of string argument names and value that represent both the GET address variables and processed POST variables if they were an encoded form.

### Examples

See [Examples](src/ExamplePages/README.md)
