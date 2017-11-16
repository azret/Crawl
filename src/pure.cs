namespace System.Text.Html
{
    using System;
          
    public static class Parser
    {
        public delegate void onEmit(int type, string tag,
            string attr, string data);

        public static string praseText(string text)
        {
            if (text == null)
            {
                return null;
            }

            int i = 0; int ln = text.Length; StringBuilder plain = new StringBuilder();

            while (i < ln)
            {
                if (text[i] == '&')
                {
                    i++;

                    if (i < ln && text[i] == '#')
                    {
                        i++;

                        int start = i;

                        while (i < ln && char.IsDigit(text[i]))
                        {
                            i++;
                        }

                        if (i > start)
                        {
                            int end = i;

                            if (i < ln && text[i] == ';')
                            {
                                i++;
                            }

                            string digits = text.Substring(start, end - start);

                            int val = 0;

                            for (int k = digits.Length - 1; k >= 0; k--)
                            {
                                int chr = digits[k] - '0';

                                int shift = (int)Math.Pow(10, (digits.Length - k - 1));

                                val = val + (shift * chr);
                            }

                            plain.Append($"{(char)val}");
                        }
                        else
                        {
                            plain.Append("&#");
                        }
                    }
                    else
                    {
                        int start = i;

                        while (i < ln && char.IsLetter(text[i]))
                        {
                            i++;
                        }

                        if (i > start)
                        {
                            int end = i;

                            if (i < ln && text[i] == ';')
                            {
                                i++;
                            }

                            string entity = text.Substring(start, end - start).ToLowerInvariant();

                            switch (entity)
                            {
                                case "nbsp":
                                    plain.Append(" ");
                                    break;
                                case "lt":
                                    plain.Append("<");
                                    break;
                                case "gt":
                                    plain.Append(">");
                                    break;
                                case "amp":
                                    plain.Append("&");
                                    break;
                                case "quot":
                                    plain.Append("\"");
                                    break;
                                case "apos":
                                    plain.Append("\'");
                                    break;
                                case "cent":
                                    plain.Append("¢");
                                    break;
                                case "pound":
                                    plain.Append("£");
                                    break;
                                case "yen":
                                    plain.Append("¥");
                                    break;
                                case "euro":
                                    plain.Append("€");
                                    break;
                                case "copy":
                                    plain.Append("©");
                                    break;
                                case "reg":
                                    plain.Append("®");
                                    break;
                                default:
                                    plain.Append($"&{entity};");
                                    break;
                            }
                        }
                        else
                        {
                            plain.Append("&");
                        }

                    }
                }
                else
                {
                    plain.Append(text[i]);
                    i++;
                }
            }

            return plain.ToString();
        }

        static void parseInstruction(string tagName, ref int i, int len, string buffer)
        {
            while (i < len && buffer[i] != '>')
            {
                while (i < len && char.IsWhiteSpace(buffer[i]))
                {
                    i++;
                }

                if (i < len && (buffer[i] == '\"' || buffer[i] == '\''))
                {
                    char q = buffer[i++];

                    int strStart = i;

                    while (i < len && buffer[i] != q)
                    {
                        i++;
                    }

                    if (i < len && buffer[i] == q)
                    {
                        i++;
                    }

                    if (i > strStart)
                    {
                    }
                }
                else
                {
                    int attrStart = i;

                    while (i < len && buffer[i] != '\"' 
                        && buffer[i] != '\'' && buffer[i] != '<' && buffer[i] != '>')
                    {
                        i++;
                    }

                    if (i > attrStart)
                    {
                    }
                }                
            }
        }

        static void parseAttributes(string tagName, ref int i, int len, string buffer,
            Action<string> href)
        {
            while (i < len && char.IsWhiteSpace(buffer[i]))
            {
                i++;
            }

            while (i < len && char.IsLetter(buffer[i]))
            {
                int start = i;

                while (i < len && char.IsLetter(buffer[i]))
                {
                    i++;
                }

                if (i > start)
                {
                    string attrName = buffer.Substring(start, i - start); string attrVal = null;

                    while (i < len && char.IsWhiteSpace(buffer[i]))
                    {
                        i++;
                    }

                    if (i < len && buffer[i] == '=')
                    {
                        i++;

                        while (i < len && char.IsWhiteSpace(buffer[i]))
                        {
                            i++;
                        }

                        if (i < len && (buffer[i] == '\"' || buffer[i] == '\''))
                        {
                            char q = buffer[i++];

                            start = i;

                            while (i < len && buffer[i] != q)
                            {
                                i++;
                            }

                            attrVal = buffer.Substring(start, i - start);

                            if (i < len && buffer[i] == q)
                            {
                                i++;
                            }
                        }
                        else
                        {
                            start = i;

                            while (i < len && !char.IsWhiteSpace(buffer[i]) && buffer[i] != '<' && buffer[i] != '>')
                            {
                                i++;
                            }

                            attrVal = buffer.Substring(start, i - start);
                        }
                    }

                    if (href != null && !string.IsNullOrWhiteSpace(attrVal)
                                && (tagName == "a" || tagName == "A") && string.Equals(attrName, "href", StringComparison.OrdinalIgnoreCase))
                    {
                        href(attrVal);
                    }
                }

                while (i < len && char.IsWhiteSpace(buffer[i]))
                {
                    i++;
                }
            }
        }

        static bool endOfScript(int i, int len, string buffer)
        {
            if (i < len && buffer[i++] == '<')
            {
                if (i < len && buffer[i++] == '/')
                {
                    if (i < len && (buffer[i] == 's' || buffer[i] == 'S'))
                    {
                        i++;
                        if (i < len && (buffer[i] == 'c' || buffer[i] == 'C'))
                        {
                            i++;
                            if (i < len && (buffer[i] == 'r' || buffer[i] == 'R'))
                            {
                                i++;
                                if (i < len && (buffer[i] == 'i' || buffer[i] == 'I'))
                                {
                                    i++;
                                    if (i < len && (buffer[i] == 'p' || buffer[i] == 'P'))
                                    {
                                        i++;
                                        if (i < len && (buffer[i] == 't' || buffer[i] == 'T'))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
        
        public static void parse(string buffer,
            Action<string> tag, Action<string> text, Action<string> href)
        {
            if (buffer == null)
            {
                return;
            }

            int i = 0; int len = buffer.Length;

            while (i >= 0 && i < len)
            {
                int start = i; char c = buffer[i++];

                switch (c)
                {
                    case '<':

                        char tagType = '\0';

                        if (i < len)
                        {
                            switch (buffer[i])
                            {
                                case 'a':
                                case 'b':
                                case 'c':
                                case 'd':
                                case 'e':
                                case 'f':
                                case 'g':
                                case 'h':
                                case 'i':
                                case 'j':
                                case 'k':
                                case 'l':
                                case 'm':
                                case 'n':
                                case 'o':
                                case 'p':
                                case 'q':
                                case 'r':
                                case 's':
                                case 't':
                                case 'u':
                                case 'v':
                                case 'w':
                                case 'x':
                                case 'y':
                                case 'z':
                                case 'A':
                                case 'B':
                                case 'C':
                                case 'D':
                                case 'E':
                                case 'F':
                                case 'G':
                                case 'H':
                                case 'I':
                                case 'J':
                                case 'K':
                                case 'L':
                                case 'M':
                                case 'N':
                                case 'O':
                                case 'P':
                                case 'Q':
                                case 'R':
                                case 'S':
                                case 'T':
                                case 'U':
                                case 'V':
                                case 'W':
                                case 'X':
                                case 'Y':
                                case 'Z':
                                    tagType = '<';
                                    break;
                                case '!':
                                    tagType = '!'; i++;
                                    break;
                                case '/':
                                    tagType = '/'; i++;
                                    break;
                            }
                        }
                         
                        if (tagType == '!' || tagType == '/' || tagType == '<')
                        {
                            int tagStart = i;

                            while (i < len && char.IsLetterOrDigit(buffer[i]))
                            {
                                i++;
                            }

                            if (i > tagStart)
                            {
                                string tagName = buffer.Substring(tagStart, i - tagStart);

                                switch (tagType)
                                {
                                    case '!':
                                        parseInstruction(tagName, ref i, len, buffer);
                                        break;

                                    case '<':
                                        parseAttributes(tagName, ref i, len, buffer, href);
                                        break;
                                }
                                
                                while (i < len && buffer[i] != '>')
                                {
                                    i++;
                                }

                                if (i < len && buffer[i] == '>')
                                {
                                    i++;
                                }

                                if (string.Equals(tagName, "SCRIPT", StringComparison.OrdinalIgnoreCase))
                                {
                                    int scriptStart = i; int scriptEnd = scriptStart;

                                    while (i < len)
                                    {
                                        scriptEnd = i;

                                        if (endOfScript(i, len, buffer))
                                        {
                                            scriptEnd = i;

                                            i += "</SCRIPT".Length;

                                            while (i < len && buffer[i] != '>')
                                            {
                                                i++;
                                            }

                                            if (i < len && buffer[i] == '>')
                                            {
                                                i++;
                                            }

                                            break;
                                        }

                                        i++;
                                    }

                                    string eval = buffer.Substring(scriptStart, scriptEnd - scriptStart);

                                }
                                else
                                {
                                    if (tag != null)
                                    {
                                        tag(tagName);
                                    }
                                }
                            }
                            else
                            {
                                while (i < len && buffer[i] != '<')
                                {
                                    i++;
                                }

                                if (text != null && tagType != '!')
                                {
                                    text(buffer.Substring(start, i - start));
                                }                                
                            }
                        }
                        else
                        {
                            while (i < len && buffer[i] != '<')
                            {
                                i++;
                            }

                            if (text != null)
                            {
                                text(buffer.Substring(start, i - start));
                            }
                        }

                        break;

                    default:

                        while (i < len && buffer[i] != '<')
                        {
                            i++;
                        }

                        if (text != null)
                        {
                            text(buffer.Substring(start, i - start));
                        }

                        break;

                }
            }
        }
                  
    }
}