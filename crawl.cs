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
            if (href.StartsWith("#") || href.StartsWith("//") || href.StartsWith("http", StringComparison.OrdinalIgnoreCase))
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

        static void Crawl(Uri uri, int depth, ISet<Uri> visited, ISet<Uri> missing, Action<string, string, Uri, string> doc = null) { Crawl(uri, depth, 0, visited, missing, doc); }
        static void Crawl(Uri uri, int depth, int level, 
            ISet<Uri> visited = null,
            ISet<Uri> missing = null,
            Action<string, string, Uri, string> doc = null)
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

            string HTML = Http.Get(
                 uri,
                 "GET",
                 null,
                 null,
                 out status,
                 out error);

            if (status == 200)
            {
                StringBuilder PLAIN = new StringBuilder(); HashSet<Uri> LINKS = new HashSet<Uri>(); String TITLE = null;

                System.Text.Html.parse(HTML,

                    (tag, type) =>
                    {
                        if (type == '<' && (tag == "p" || tag == "P"))
                        {
                            if (PLAIN.Length > 0)
                            {
                                PLAIN.Append("\r\n");
                            }
                        }
                        else if (String.Equals("BR", tag, StringComparison.OrdinalIgnoreCase))
                        {
                            if (PLAIN.Length > 0)
                            {
                                PLAIN.Append("\r\n");
                            }
                        }
                        else if (tag == "h1" || tag == "H1" || tag == "h2" || tag == "H2"
                                || tag == "h3" || tag == "H3" || tag == "h4" || tag == "H4"
                                || tag == "h5" || tag == "H5" || tag == "h6" || tag == "H6")
                        {
                            if (PLAIN.Length > 0)
                            {
                                PLAIN.Append("\r\n");
                            }
                        }                        
                        else
                        {
                            if (PLAIN.Length > 0)
                            {
                                PLAIN.Append(" ");
                            }
                        }
                    },

                    (title) =>
                    {
                        if (!string.IsNullOrWhiteSpace(title))
                        {
                            TITLE = title.Trim();
                        }
                    },

                    (text) =>
                    {
                        var plain = System.Text.Html.praseText(text);

                        if (string.IsNullOrWhiteSpace(plain))
                        {
                            if (PLAIN.Length > 0)
                            {
                                PLAIN.Append(plain);
                            }
                        }
                        else
                        {
                            PLAIN.Append(plain);
                        }
                    },

                    (href) =>
                    {
                        Uri target = GetTargetUri(uri, href);

                        if (target != null)
                        {
                            LINKS.Add(target);
                        }
                    });

                if (doc != null && status == 200 && HTML.Length > 0)
                {
                    if (!string.IsNullOrWhiteSpace(TITLE))
                    {
                        PLAIN.Insert(0, $"# {TITLE}\r\n\r\n");
                    }

                    doc(HTML, PLAIN.ToString(), uri, TITLE);
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($" [{status}]");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($" ({timer.ElapsedMilliseconds}ms)");

                if (!string.IsNullOrWhiteSpace(TITLE))
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($" - {TITLE}");
                }

                Console.ResetColor();
                Console.WriteLine();

                foreach (var target in LINKS)
                {
                    Crawl(target, depth, level + 1, visited, missing, doc);
                }
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
                Console.WriteLine();

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
                string dir = GetParam("--cache", args);

                if (dir != null)
                {
                    if (string.IsNullOrWhiteSpace(dir))
                    {
                        dir = ".cache";
                    }

                    dir = Path.GetFullPath(dir);
                }

                Crawl(new Uri(url), depth, visited, missing, (html, plain, uri, title) =>
                {
                    if (verbose != null)
                    {
                        Console.WriteLine(plain);
                    }

                    if (dir != null)
                    {
                        Cache(html, plain, uri, dir, title);
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
                Error(e.ToString());
            }

            Console.ReadKey();
        }

        static void Cache(string html, string plain, Uri uri, string dir, string title)
        {
            var path = uri.AbsolutePath;

            if (string.IsNullOrWhiteSpace(path) || path == "/")
            {
                path = "index";
            }

            path = path.Replace("/", "\\").Trim('\\');

            string file = Path.Combine(dir, path);
             
            if (!Directory.Exists(Path.GetDirectoryName(file)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
            }

            path = Path.GetDirectoryName(file);

            if (!string.IsNullOrWhiteSpace(title))
            {
                title = title
                    .Replace("\\", ", ")
                    .Replace("/", ", ")
                    .Replace("-", " ")
                    .Replace("|", "I")
                    .Replace(": ", " - ")
                    .Replace(":", " ")
                    .Replace(".", " ")
                    .Replace("<", " ")
                    .Replace(">", " ")
                    .Replace("\t", " ")
                    .Replace("\n", " ")
                    .Replace("\r", " ");

                while (title.IndexOf("  ") >= 0)
                {
                    title = title.Replace("  ", " ");
                }

                try
                {
                    file = Path.ChangeExtension(Path.Combine(path, title), ".html");

                    int i = 0;

                    while (File.Exists(file))
                    {
                        file = Path.ChangeExtension(Path.Combine(path, title + $" [{i}]"), ".html");
                        i++;
                    }
                }
                catch
                {
                    file = Path.Combine(dir, path);
                }
            }
            
            File.WriteAllText(Path.ChangeExtension(file, ".html"), html, Encoding.UTF8);
            File.WriteAllText(Path.ChangeExtension(file, ".txt"), plain, Encoding.UTF8);
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