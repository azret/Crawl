namespace System.Text
{
    using System;

    public static class _Parse
    {
        static void SkipWhite(string BUFFER, ref int i, int ln)
        {
            while (i < ln && char.IsWhiteSpace(BUFFER[i]))
            {
                i++;
            }
        }

        static void ParseTag(string BUFFER, ref int i, int ln, Action<string> href)
        {
            if (i >= ln || BUFFER[i] != '<')
            {
                throw new InvalidOperationException();
            }

            i++;

            if (i < ln)
            {
                char tagType = BUFFER[i];

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
                                SkipAttributes(
                                    BUFFER, 
                                    ref i, 
                                    ln, 
                                    BUFFER.Substring(tagStart, i - tagStart),
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

        static void ParseText(string BUFFER, Action<string> emit, ref int i, int ln)
        {
            int start = i;

            i++;

            while (i < ln && BUFFER[i] != '<')
            {
                i++;
            }

            if (emit != null && i > start)
            {
                emit(BUFFER.Substring(start, i - start));
            }
        }

        public static void Strict(string BUFFER, Action<string> emit, Action<string> href)
        {
            if (string.IsNullOrWhiteSpace(BUFFER))
            {
                return;
            }

            var i = 0; var ln = BUFFER.Length;

            while (i < ln)
            {
                if (BUFFER[i] == '<')
                {
                    ParseTag(BUFFER, ref i, ln, href);
                }

                else
                {
                    ParseText(BUFFER, emit, ref i, ln);
                }
            }
        }
    }
}
