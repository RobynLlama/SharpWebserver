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

### Examples

See [Examples](src/ExamplePages/README.md)

## Defaults

The default base directory is `%localAppData%/RobynLlama/SharpWebserver` on windows and `~/.local/share/RobynLlama/SharpWebserver` on most Linux distros. See [Special Folders](https://learn.microsoft.com/en-us/dotnet/api/system.environment.getfolderpath?view=net-8.0) on the MSDN docs for more on where this location is on your system.
