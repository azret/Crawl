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
    }
}
