namespace System.Text
{
    using System;
    using System.Collections.Generic;

    public static class _Parse
    {
        static void SkipWhite(string BUFFER, ref int i, int ln)
        {
            while (i < ln && char.IsWhiteSpace(BUFFER[i]))
            {
                i++;
            }
        }

        static void ParseTag(string BUFFER, ref int i, int ln, 
            Action<string> href, out string tagName, out char tagType)
        {
            tagType = '\0'; tagName = null;

            if (i >= ln || BUFFER[i] != '<')
            {
                throw new InvalidOperationException();
            }

            i++;

            if (i < ln)
            {
                tagType = BUFFER[i];

                switch (tagType)
                {
                    case '!':

                        {
                            i++;

                            int tagStart = i;

                            while (i < ln && char.IsLetter(BUFFER[i]))
                            {
                                i++;
                            }

                            if (i > tagStart)
                            {
                                tagName = BUFFER.Substring(tagStart, i - tagStart);
                            }
                        }

                        break;

                    case '/':

                        i++;

                        SkipWhite(BUFFER, ref i, ln);

                        {
                            int tagStart = i;

                            while (i < ln && char.IsLetter(BUFFER[i]))
                            {
                                i++;
                            }

                            if (i > tagStart)
                            {
                                tagName = BUFFER.Substring(tagStart, i - tagStart);
                            }

                        }                    

                        break;

                    default:

                        SkipWhite(BUFFER, ref i, ln);

                        {
                            int tagStart = i;

                            while (i < ln && char.IsLetter(BUFFER[i]))
                            {
                                i++;
                            } 

                            if (i > tagStart)
                            {
                                tagName = BUFFER.Substring(tagStart, i - tagStart);

                                SkipAttributes(
                                    BUFFER, 
                                    ref i, 
                                    ln,
                                    tagName,
                                    href);
                            }

                        }

                        break;

                }

                while (i < ln && BUFFER[i] != '>')
                {
                    i++;
                }

                if (i < ln && BUFFER[i] == '>')
                {
                    i++;
                }
            } 
        }

        static void SkipAttributes(string BUFFER, ref int i, int ln, string tagName, Action<string> href)
        {
            SkipWhite(BUFFER, ref i, ln);

            while (i < ln && char.IsLetter(BUFFER[i]))
            {
                int attrStart = i;

                while (i < ln && char.IsLetter(BUFFER[i]))
                {
                    i++;
                }

                if (i > attrStart)
                {
                    string attrName = BUFFER.Substring(attrStart, i - attrStart);

                    SkipWhite(BUFFER, ref i, ln);

                    if (i < ln && BUFFER[i] == '=')
                    {
                        i++;
                    }

                    SkipWhite(BUFFER, ref i, ln);

                    if (i < ln)
                    {
                        char quote = BUFFER[i];

                        switch (quote)
                        {
                            case '"':
                            case '\'':

                                i++;

                                SkipWhite(BUFFER, ref i, ln);

                                int valueStart = i;

                                while (i < ln && (BUFFER[i] != quote && BUFFER[i] != '>'))
                                {
                                    i++;
                                }

                                if (i > valueStart)
                                {
                                    string attrValue = BUFFER.Substring(valueStart, i - valueStart);

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

                                while (i < ln && char.IsLetterOrDigit(BUFFER[i]))
                                {
                                    i++;
                                }

                                break;

                        }
                    }
                }
            }
        }

        static void ParseText(string BUFFER, Action<string, string> emit, ref int i, int ln, string tagName)
        {
            int start = i;

            i++;

            while (i < ln && BUFFER[i] != '<')
            {
                i++;
            }

            if (emit != null && i > start)
            {
                emit(tagName, BUFFER.Substring(start, i - start));
            }
        }

        private class TagNode
        {
            /// <summary>
            /// Top
            /// </summary>
            public TagNode Top;

            /// <summary>
            /// Name
            /// </summary>
            public string Name;
        }

        public static void Strict(string BUFFER, Action<string, string> emit, Action<string> href)
        {
            if (string.IsNullOrWhiteSpace(BUFFER))
            {
                return;
            }

            ISet<string> SelfClosing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            SelfClosing.Add("area");
            SelfClosing.Add("base");
            SelfClosing.Add("br");
            SelfClosing.Add("col");
            SelfClosing.Add("embed");
            SelfClosing.Add("hr");
            SelfClosing.Add("img");
            SelfClosing.Add("input");
            SelfClosing.Add("keygen");
            SelfClosing.Add("meta");
            SelfClosing.Add("param");
            SelfClosing.Add("source");
            SelfClosing.Add("track");
            SelfClosing.Add("wbr");

            var i = 0; var ln = BUFFER.Length;

            TagNode STACK = null;

            while (i < ln)
            {
                if (BUFFER[i] == '<')
                {
                    string tagName = null; char tagType = '\0';

                    ParseTag(BUFFER, ref i, ln, href, out tagName, out tagType);

                    if (!string.IsNullOrWhiteSpace(tagName))
                    {
                        if (tagType == '/')
                        {
                            TagNode top = STACK;

                            while (top != null)
                            {
                                if (string.Equals(top.Name, tagName, StringComparison.OrdinalIgnoreCase))
                                {
                                    STACK = top.Top;
                                    break;
                                }
                                top = top.Top;
                            }
                        }
                        else if (tagType == '!')
                        {
                        }
                        else
                        {
                            if (!SelfClosing.Contains(tagName))
                            {
                                TagNode top = new TagNode()
                                {
                                    Top = STACK,
                                    Name = tagName
                                };

                                STACK = top;
                            }
                        }
                    }
                }

                else
                {
                    ParseText(BUFFER,
                        emit,
                        ref i, ln,
                        STACK != null ? STACK.Name : string.Empty);
                }
            }
        }

        /// <summary>
        /// Post processes the text replacing all found entities.
        /// </summary>
        public static string Text(string TEXT)
        {
            if (TEXT == null)
            {
                return null;
            }

            int i = 0; int ln = TEXT.Length; StringBuilder FRAGMENT = new StringBuilder();

            while (i < ln)
            {
                if (TEXT[i] == '&')
                {
                    i++;

                    if (i < ln && TEXT[i] == '#')
                    {
                        i++;

                        int start = i;

                        while (i < ln && char.IsDigit(TEXT[i]))
                        {
                            i++;
                        }

                        if (i > start)
                        {
                            int end = i;

                            if (i < ln && TEXT[i] == ';')
                            {
                                i++;
                            }

                            string digits = TEXT.Substring(start, end - start);

                            int val = 0;

                            for (int k = digits.Length - 1; k >= 0; k--)
                            {
                                int chr = digits[k] - '0';

                                int shift = (int)Math.Pow(10, (digits.Length - k - 1));

                                val = val + (shift * chr);
                            }

                            FRAGMENT.Append($"{(char)val}");
                        }
                        else
                        {
                            FRAGMENT.Append("&#");
                        }
                    }
                    else
                    {
                        int start = i;

                        while (i < ln && char.IsLetter(TEXT[i]))
                        {
                            i++;
                        }

                        if (i > start)
                        {
                            int end = i;

                            if (i < ln && TEXT[i] == ';')
                            {
                                i++;
                            }

                            string entity = TEXT.Substring(start, end - start).ToLowerInvariant();

                            switch (entity)
                            {
                                case "nbsp":
                                    FRAGMENT.Append(" ");
                                    break;
                                case "lt":
                                    FRAGMENT.Append("<");
                                    break;
                                case "gt":
                                    FRAGMENT.Append(">");
                                    break;
                                case "amp":
                                    FRAGMENT.Append("&");
                                    break;
                                case "quot":
                                    FRAGMENT.Append("\"");
                                    break;
                                case "apos":
                                    FRAGMENT.Append("\'");
                                    break;
                                case "cent":
                                    FRAGMENT.Append("¢");
                                    break;
                                case "pound":
                                    FRAGMENT.Append("£");
                                    break;
                                case "yen":
                                    FRAGMENT.Append("¥");
                                    break;
                                case "euro":
                                    FRAGMENT.Append("€");
                                    break;
                                case "copy":
                                    FRAGMENT.Append("©");
                                    break;
                                case "reg":
                                    FRAGMENT.Append("®");
                                    break;
                                default:
                                    FRAGMENT.Append($"&{entity};");
                                    break;
                            }
                        }
                        else
                        {
                            FRAGMENT.Append("&");
                        }

                    }
                }
                else
                {
                    FRAGMENT.Append(TEXT[i]);
                    i++;
                }
            }

            return FRAGMENT.ToString();
        }
    }
}
