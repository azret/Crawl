namespace System.Text
{
    using System;

    public static class _Parse
    {
        static void SkipWhite(string html, ref int i, int ln)
        {
            while (i < ln && char.IsWhiteSpace(html[i]))
            {
                i++;
            }
        }

        static void ParseTag(string html, ref int i, int ln, Action<string> href)
        {
            if (i >= ln || html[i] != '<')
            {
                throw new InvalidOperationException();
            }

            i++;

            if (i < ln)
            {
                char tagType = html[i];

                switch (tagType)
                {
                    case '!':

                        {
                            i++;

                            int tagStart = i;

                            while (i < ln && char.IsLetter(html[i]))
                            {
                                i++;
                            }                            
                        }

                        break;

                    case '/':

                        i++;

                        SkipWhite(html, ref i, ln);

                        {
                            int tagStart = i;

                            while (i < ln && char.IsLetter(html[i]))
                            {
                                i++;
                            }
                        }                    

                        break;

                    default:

                        SkipWhite(html, ref i, ln);

                        {
                            int tagStart = i;

                            while (i < ln && char.IsLetter(html[i]))
                            {
                                i++;
                            }

                            if (i > tagStart)
                            {
                                SkipAttributes(
                                    html, 
                                    ref i, 
                                    ln, 
                                    html.Substring(tagStart, i - tagStart),
                                    href);
                            }

                        }

                        break;

                }

                while (i < ln && html[i] != '>')
                {
                    i++;
                }

                if (i < ln && html[i] == '>')
                {
                    i++;
                }
            } 
        }

        static void SkipAttributes(string html, ref int i, int ln, string tagName, Action<string> href)
        {
            SkipWhite(html, ref i, ln);

            while (i < ln && char.IsLetter(html[i]))
            {
                int attrStart = i;

                while (i < ln && char.IsLetter(html[i]))
                {
                    i++;
                }

                if (i > attrStart)
                {
                    string attrName = html.Substring(attrStart, i - attrStart);

                    SkipWhite(html, ref i, ln);

                    if (i < ln && html[i] == '=')
                    {
                        i++;
                    }

                    SkipWhite(html, ref i, ln);

                    if (i < ln)
                    {
                        char quote = html[i];

                        switch (quote)
                        {
                            case '"':
                            case '\'':

                                i++;

                                SkipWhite(html, ref i, ln);

                                int valueStart = i;

                                while (i < ln && html[i] != quote)
                                {
                                    i++;
                                }

                                if (i > valueStart)
                                {
                                    string attrValue = html.Substring(valueStart, i - valueStart);

                                    if ((tagName == "A" || tagName == "a") 
                                                        
                                                && string.Equals("href", attrName, StringComparison.OrdinalIgnoreCase))

                                    {
                                        if (href != null)
                                        {
                                            href(attrValue);
                                        }
                                    }
                                }

                                break;

                            default:

                                while (i < ln && char.IsLetterOrDigit(html[i]))
                                {
                                    i++;
                                }

                                break;

                        }
                    }
                }
            }
        }

        static void ParseText(string html, Action<string> emit, ref int i, int ln)
        {
            int start = i;

            i++;

            while (i < ln && html[i] != '<')
            {
                i++;
            }

            if (emit != null && i > start)
            {
                emit(html.Substring(start, i - start));
            }
        }

        public static void Strict(string html, Action<string> emit, Action<string> href)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return;
            }

            var i = 0; var ln = html.Length;

            while (i < ln)
            {
                if (html[i] == '<')
                {
                    ParseTag(html, ref i, ln, href);
                }

                else
                {
                    ParseText(html, emit, ref i, ln);
                }
            }
        }
    }
}
