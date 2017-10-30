namespace Crawl
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography;
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

        static void Crawl(Uri uri, int depth, ISet<Uri> visited, ISet<Uri> missing, Action<string, string, Uri> doc = null) { Crawl(uri, depth, 0, visited, missing, doc); }
        static void Crawl(Uri uri, int depth, int level, 
            ISet<Uri> visited = null,
            ISet<Uri> missing = null,
            Action<string, string, Uri> doc = null)
        { 
            if (level >= depth)
            {
                return;
            }

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

            string HTML = _Xdr.Xdr(
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
                if (missing != null)
                {
                    missing.Add(uri);
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($" [{status}]");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($" ({timer.ElapsedMilliseconds}ms)");
                Console.ResetColor();
            }

            Console.WriteLine();

            StringBuilder PLAIN = new StringBuilder();

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

            _Parse.Strict(HTML,

                (tagName, text) =>
                {
                    string clean = _Parse.Text(text);

                    if (string.Equals(tagName, "title", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrWhiteSpace(clean))
                        {
                            Console.WriteLine($"Title: {clean.Trim()}");
                        }
                    }
                    else
                    {
                        if (TextTags.Contains(tagName))
                        {
                            PLAIN.Append(clean);
                        }
                    }
                },

                (href) =>
                {
                    Uri target = GetTargetUri(uri, href);

                    if (target != null)
                    {
                        Crawl(target, depth, level + 1, visited, missing, doc);
                    }                     
                }
            );

            if (doc != null)
            {
                doc(HTML, PLAIN.ToString(), uri);
            }
        }

        static void Main(string[] args)
        {
            string url = string.Empty;

            url = GetParam("--url", args);

            if (url == null)
            {
                url = "https://www.w3.org/TR/html5/";
            }

            int depth = -1; string s = GetParam("--depth", args);

            if (string.Equals(s, "all", StringComparison.OrdinalIgnoreCase) 
                            || string.Equals(s, "inf", StringComparison.OrdinalIgnoreCase))
            {
                s = "-1";
            }

            if (!string.IsNullOrWhiteSpace(s))
            {
                if (!int.TryParse(s, out depth))
                {
                    Error($"--depth {s} is not valid.");
                }

                depth &= 0x7FFFFFFF;
            }
            else
            {
                depth = 1;
            }     

            string verbose = GetParam("--verbose", args);

            ISet<Uri> visited = new HashSet<Uri>(); ISet<Uri> missing = new HashSet<Uri>();

            System.Console.CancelKeyPress += (sender, e) =>
            {
                Process.GetCurrentProcess().Kill();
            };

            Stopwatch timer = Stopwatch.StartNew();

            try
            {
                string cache = GetParam("--cache", args);

                if (cache == null)
                {
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(cache))
                    {
                        cache = ".cache";
                    } 

                    cache = Path.GetFullPath(cache);

                    if (!Directory.Exists(cache))
                    {
                        Directory.CreateDirectory(cache);
                    }
                }

                Crawl(new Uri(url), depth, visited, missing, (html, plain, uri) =>
                {
                    if (verbose != null)
                    {
                        Console.WriteLine(plain);
                    }

                    if (cache != null)
                    {
                        string md5 = _MD5.MD5(uri.ToString());

                        File.WriteAllText(Path.Combine(cache, $"{md5}.original"), html, Encoding.UTF8);
                        File.WriteAllText(Path.Combine(cache, $"{md5}.plain"), plain, Encoding.UTF8);
                    }
                });

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
                Console.WriteLine($"Pages Visited: {visited.Count}");
                Console.WriteLine($"Missing Links: {missing.Count}");
                Console.WriteLine($"Time: {timer.ElapsedMilliseconds / 1000.0}s");
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
            string value = null;

            for (int i = 0; args != null && i < args.Length; i++)
            {
                if (String.Equals(args[i].Trim(), key, StringComparison.OrdinalIgnoreCase))
                {
                    value = string.Empty;

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

                        StringBuilder buff = new StringBuilder();

                        while (args[i + 1] != null && j < args[i + 1].Length)
                        {
                            char c = args[i + 1][j];

                            if (Char.IsWhiteSpace(c))
                            {
                                break;
                            }

                            buff.Append(c);
                            j++;
                        }

                        if (buff.Length > 0)
                        {
                            return buff.ToString();
                        }
                    }
                }
            }

            return value;
        }
    }
}