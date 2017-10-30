﻿namespace Crawl
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Text;

    unsafe class _Crawl
    {
        static void Log(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        static void Error(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        static Uri GetTargetUri(Uri uri, string href)
        {
            if (href.StartsWith("//") || href.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            string ext = null;

            try
            {
                ext = Path.GetExtension(href);
            }
            catch
            {
                return null;
            }

            if (string.Equals(ext, ".htm") ||
                string.Equals(ext, ".html") ||
                string.Equals(ext, ".shtml"))
            {
                if (href.StartsWith("/"))
                {
                    return new Uri(uri, href);
                }
                else
                {
                    return new Uri(uri, href);
                }
            }

            return null;
        }

        static void Crawl(Uri uri, ISet<Uri> visited = null, Action<StringBuilder> doc = null)
        {
            if (visited == null)
            {
                visited = new HashSet<Uri>();
            }

            if (visited.Contains(uri))
            {
                return;
            }

            visited.Add(uri);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"GET {uri}");
            Console.ResetColor();

            Exception error = null; int status; Stopwatch timer = Stopwatch.StartNew();

            string data = _Xdr.Xdr(
                 uri,
                 "GET",
                 null,
                 null,
                 out status,
                 out error);

            if (status == 200)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($" [{status}]");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($" ({timer.ElapsedMilliseconds}ms)");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($" [{status}]");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($" ({timer.ElapsedMilliseconds}ms)");
                Console.ResetColor();
            }

            Console.WriteLine();

            StringBuilder DOC = new StringBuilder();

            ISet<string> TextTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            TextTags.Add("a");
            TextTags.Add("span");
            TextTags.Add("p");
            TextTags.Add("article");
            TextTags.Add("div");
            TextTags.Add("h1");
            TextTags.Add("h2");
            TextTags.Add("h3");
            TextTags.Add("h4");
            TextTags.Add("h5");
            TextTags.Add("h6");
            TextTags.Add("i");
            TextTags.Add("b");
            TextTags.Add("em");
            TextTags.Add("s");
            TextTags.Add("strong");
            TextTags.Add("q");

            _Parse.Strict(data,

                (tagName, text) =>
                {
                    if (string.Equals(tagName, "title", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"Title: {text.Trim()}");
                    }
                    else
                    {
                        if (TextTags.Contains(tagName))
                        {
                            DOC.Append(text);
                        }
                    }
                },

                (href) =>
                {
                    Uri target = GetTargetUri(uri, href);

                    if (target != null)
                    {
                        Crawl(target, visited, doc);
                    }                     
                }
            );

            if (doc != null)
            {
                doc(DOC);
            }
        }

        static void Main(string[] args)
        {
            string uri = string.Empty;

            uri = GetParam("--uri", args);

            if (uri == null)
            {
                uri = "https://google.com";
            }

            string verbose = GetParam("--verbose", args);

            ISet<Uri> visited = new HashSet<Uri>();

            System.Console.CancelKeyPress += (sender, e) =>
            {
                Process.GetCurrentProcess().Kill();
            };

            Stopwatch timer = Stopwatch.StartNew();

            try
            {
                Crawl(new Uri(uri), visited, (doc) =>
                {
                     
                });

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
                Console.WriteLine($"Visited: {visited.Count}");
                Console.WriteLine($"Elasped: {timer.ElapsedMilliseconds / 1000.0}s");
                Console.WriteLine();
                Console.ResetColor();
                Console.WriteLine("Done.");
            }
            catch (Exception e)
            {
                Error(e.Message);
            }

            Console.ReadKey();
        }

        static string GetParam(string key, string[] args)
        {
            for (int i = 0; args != null && i < args.Length; i++)
            {
                if (String.Equals(args[i].Trim(), key, StringComparison.OrdinalIgnoreCase))
                {
                    if (i < args.Length - 1)
                    {
                        int j = 0;

                        while (args[i + 1] != null && j < args[i + 1].Length)
                        {
                            char c = args[i + 1][j];

                            if (!Char.IsWhiteSpace(c))
                            {
                                break;
                            }

                            j++;
                        }

                        StringBuilder value = new StringBuilder();

                        while (args[i + 1] != null && j < args[i + 1].Length)
                        {
                            char c = args[i + 1][j];

                            if (Char.IsWhiteSpace(c))
                            {
                                break;
                            }

                            value.Append(c);
                            j++;
                        }

                        if (value.Length > 0)
                        {
                            return value.ToString();
                        }
                    }
                }
            }

            return null;
        }
    }
}