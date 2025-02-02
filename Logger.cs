/*
Copyright (C) 2025 Robyn (robyn@mamallama.dev)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
*/

using System;

namespace SharpWebserver;
public static class Logger
{

  private static readonly int Padding = "WARNING".Length + 1;
  private static void LogWithStyle(ConsoleColor color, string label, object message, (string name, object? value)[]? context)
  {
    ConsoleColor orig = Console.ForegroundColor;
    Console.Write('[');
    Console.ForegroundColor = color;
    Console.Write(label);
    Console.ForegroundColor = orig;
    Console.Write(']');
    string padding = new(' ', Padding - label.Length);
    Console.Write(padding);
    Console.WriteLine(message);
    string deeperPadding = new(' ', Padding + 4);

    if (context is not null)
    {
      foreach (var (name, value) in context)
      {
        Console.Write(deeperPadding);
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(name);
        Console.ForegroundColor = orig;
        Console.WriteLine(": " + value);
      }
    }
  }
  public static void LogTrace(object message) =>
    LogWithStyle(ConsoleColor.White, "TRACE", message, null);
  public static void LogTrace(object message, (string name, object? value)[]? context = null) =>
  LogWithStyle(ConsoleColor.White, "DEBUG", message, context);
  public static void LogInfo(object message, (string name, object? value)[]? context = null) =>
    LogWithStyle(ConsoleColor.Cyan, "INFO", message, context);
  public static void LogWarning(object message, (string name, object? value)[]? context = null) =>
    LogWithStyle(ConsoleColor.Yellow, "WARNING", message, context);
  public static void LogError(object message, (string name, object? value)[]? context = null) =>
  LogWithStyle(ConsoleColor.Red, "ERROR", message, context);
}
