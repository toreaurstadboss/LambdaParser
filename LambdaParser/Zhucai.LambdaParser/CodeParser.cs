using System;
using System.Collections.Generic;
using System.Text;

namespace ToreAurstadIT.LambdaParser
{
    /// <summary>
    /// 解析代码
    /// </summary>
    public class CodeParser
    {
        /// <summary>
        /// Currently read index position
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Currently read characters length
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// The entire incoming code content
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Analysis ".." or @ ".." defined string
        /// </summary>
        public string DefineString { get; private set; }

        public CodeParser(string content)
        {
            this.Content = content;
        }

        /// <summary>
        ///Read the string down. (This method is the encapsulation of the read () method)
        /// </summary>
        /// <returns></returns>
        public string ReadString()
        {
            return ReadString(true);
        }

        /// <summary>
        /// Read the string down. (This method is the encapsulation of the read () method)
        /// </summary>
        /// <param name="isIgnoreWhiteSpace">是否忽略空格</param>
        /// <returns></returns>
        public string ReadString(bool isIgnoreWhiteSpace)
        {
            if (Read(true, isIgnoreWhiteSpace))
            {
                return this.Content.Substring(this.Index, this.Length);
            }
            return null;
        }

        /// <summary>
        ///Read the next symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public bool ReadSymbol(string symbol)
        {
            return ReadSymbol(symbol, true);
        }

        /// <summary>
        /// Read the next symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="throwExceptionIfError"></param>
        /// <returns></returns>
        public bool ReadSymbol(string symbol, bool throwExceptionIfError)
        {
            // Skip space
            while (char.IsWhiteSpace(this.Content[this.Index+this.Length]))
            {
                this.Length++;
            }

            if (throwExceptionIfError)
            {
                ParseException.Assert(this.Content.Substring(this.Index + this.Length, symbol.Length), symbol, this.Index);
            }
            else if (this.Content.Substring(this.Index + this.Length, symbol.Length) != symbol)
            {
                return false;
            }
            this.Index += this.Length;
            this.Length = symbol.Length;
            return true;
        }

        /// <summary>
        /// Get the next string without changing the current location
        /// </summary>
        /// <returns></returns>
        public string PeekString()
        {
            int index = this.Index;
            int length = this.Length;

            string str = ReadString(true);

            this.Index = index;
            this.Length = length;

            return str;
        }

        #region private method

        /// <summary>
        /// Read it down. Index and Length indicate the current location.
        /// </summary>
        /// <param name="isBuildDefineString">Whether the string constant is parsed to the Define String member when the string constant is encountered in the code.</param>
        /// <param name="isIgnoreWhiteSpace">Do you ignore spaces?</param>
        /// <returns></returns>
        private bool Read(bool isBuildDefineString, bool isIgnoreWhiteSpace)
        {
            this.Index += this.Length;
            this.Length = 1;

            // Returns 0 over the end of the end
            if (this.Index == this.Content.Length)
            {
                this.Index = 0;
                return false;
            }

            // Check to blank characters, skip, continue
            if (isIgnoreWhiteSpace && char.IsWhiteSpace(this.Content, this.Index))
            {
                return Read(isBuildDefineString, isIgnoreWhiteSpace);
            }

            //Get the current letter
            char c = this.Content[this.Index];

            // Letters or underlined
            #region if (char.IsLetter(c) || c == '_' || c == '$')
            if (char.IsLetter(c) || c == '_' || c == '$')
            {
                //Look down
                for (this.Length = 1; (this.Length + this.Index) < this.Content.Length; this.Length++)
                {
                    char cInner = this.Content[this.Index + this.Length];

                    // When Char is not letters, numbers, the next line is returned
                    if ((!char.IsLetterOrDigit(cInner)) && cInner != '_')
                    {
                        return true;
                    }
                }

                return true;
            }
            #endregion

            // Beginning
            #region if (char.IsDigit(c))
            if (char.IsDigit(c))
            {
                // 找下去
                for (this.Length = 1; (this.Length + this.Index) < this.Content.Length; this.Length++)
                {
                    char cInner = this.Content[this.Index + this.Length];

                    // 当char是点时，判断其后面的字符是否数字，若不是数字，则返回
                    if (cInner == '.')
                    {
                        char nextChar = this.Content[this.Index + this.Length + 1];
                        if (!char.IsDigit(nextChar))
                        {
                            return true;
                        }
                    }

                    // 当char不是数字、mdflx(表示各种数字类型如l表示long)时返回
                    if ((!char.IsDigit(cInner)) && cInner != '.'
                        && cInner != 'M' && cInner != 'm'
                        && cInner != 'D' && cInner != 'd'
                        && cInner != 'F' && cInner != 'f'
                        && cInner != 'L' && cInner != 'l'
                        && cInner != 'X' && cInner != 'x')
                    {
                        return true;
                    }
                }

                return true;
            }
            #endregion

            // Get the next char
            char nextInner;
            if (!TryGetNextChar(false, out nextInner))
            {
                //Go to the end, return directly
                return true;
            }

            // Whether the symbol is known, some doing
            switch (c)
            {
                #region case .....
                case '.':
                case '(':
                case ')':
                case '[':
                case ']':
                case '+':
                case '-':
                case '!':
                case '~':
                case '*':
                case '%':
                case '^':
                case ':':

                case '{':
                case '}':
                case ',':
                case ';':
                case ' ':
                case '	':
                    break;

                case '=':
                    if (nextInner == '>') // =>
                    {
                        this.Length++;
                        return true;
                    }
                    break;

                case '>':
                case '<':
                    if (nextInner == c) // >>,<<
                    {
                        this.Length++;
                    }
                    break;
                case '&':
                case '|':
                case '?':
                    if (nextInner == c) // &&,||, ??
                    {
                        this.Length++;
                        return true;
                    }
                    break;
                #endregion

                #region case '/':
                case '/':
                    if (nextInner == c) // Note: //
                    {
                        const string SampleCommitEnd = "\n";
                        this.Length++;
                        int endIndex = GetStringIndex(SampleCommitEnd, this.Index + this.Length);
                        if (endIndex == -1) //Arrive at the last
                        {
                            this.Length = this.Content.Length - this.Index;
                        }
                        else
                        {
                            this.Length = endIndex - this.Index + SampleCommitEnd.Length;
                        }

                        return true;
                    }
                    else if (nextInner == '*') //Note: / ** /
                    {
                        const string MultiLineEnd = "*/";
                        this.Length++;
                        int endIndex = GetStringIndex(MultiLineEnd, this.Index + this.Length);
                        if (endIndex == -1) //Arrive at the last
                        {
                            throw new ParseNoEndException("/*", this.Index);
                        }
                        else
                        {
                            this.Length = endIndex - this.Index + MultiLineEnd.Length;
                        }

                        return true;
                    }
                    break;
                #endregion

                #region case '\'':
                case '\'':
                    for (int i = this.Index + this.Length; i < this.Content.Length; i++)
                    {
                        // If you find it, it is ignored the next one.
                        if (this.Content[i] == '\\')
                        {
                            i++;
                            continue;
                        }

                        //turn up'
                        if (this.Content[i] == '\'')
                        {
                            this.Length = i - this.Index + 1;

                            if (isBuildDefineString)
                            {
                                if (this.Length == 3)
                                {
                                    this.DefineString = this.Content.Substring(this.Index + 1, 1);
                                }
                                else if (this.Length == 4 && this.Content[this.Index + 1] == '\\')
                                {
                                    this.DefineString = GetTransformMeanChar(this.Content[this.Index + 2]).ToString();
                                }
                            }
                            return true;
                        }
                    }
                    throw new ParseNoEndException("\'", this.Index);
                #endregion

                #region case '\"':
                case '\"':
                    StringBuilder sb = null;
                    int prevIndex = this.Index + this.Length;
                    if (isBuildDefineString)
                    {
                        sb = new StringBuilder();
                    }
                    for (int i = this.Index + this.Length; i < this.Content.Length; i++)
                    {
                        //It is discovered that the next one is ignored.
                        if (this.Content[i] == '\\')
                        {
                            i++;
                            if (isBuildDefineString)
                            {
                                sb.Append(this.Content, prevIndex, i - prevIndex - 1);
                                prevIndex = i + 1;

                                char chOriginal = this.Content[i];
                                char ch = GetTransformMeanChar(chOriginal);
                                sb.Append(ch);
                            }
                            continue;
                        }

                        // discover"
                        if (this.Content[i] == '\"')
                        {
                            this.Length = i - this.Index + 1;
                            if (isBuildDefineString)
                            {
                                sb.Append(this.Content, prevIndex, i - prevIndex);
                                this.DefineString = sb.ToString();
                            }
                            return true;
                        }
                    }
                    throw new ParseNoEndException("\"", this.Index);
                #endregion

                #region case '@':
                case '@':
                    if (nextInner == '\"') // @""
                    {
                        this.Length++;
                        for (int i = this.Index + this.Length; i < this.Content.Length; i++)
                        {
                            // turn up"
                            if (this.Content[i] == '\"')
                            {
                                //Will it reach the end?
                                if ((i + 1) < this.Content.Length)
                                {
                                    // Whether it is followed?
                                    if (this.Content[i + 1] == '\"')
                                    {
                                        i++;
                                        continue;
                                    }
                                }

                                this.Length = i - this.Index + 1;

                                if (isBuildDefineString)
                                {
                                    // At present, use replacement, you can make optimization
                                    this.DefineString = this.Content.Substring(this.Index + 2, this.Length - 3).Replace("\"\"", "\"");
                                }

                                return true;
                            }
                        }
                    }
                    break;
                #endregion

                default:
                    throw new ParseUnknownException(c.ToString(), this.Index);
            }

            // Processing may follow the equal sign (=)
            #region switch (c)
            switch (c)
            {
                case '&':
                case '|':
                case '+':
                case '-':
                case '*':
                case '/':
                case '%':
                case '^':
                case '<':
                case '>':
                case '!':
                case '=':
                    if (!TryGetNextChar(false, out nextInner))
                    {
                        return true;
                    }
                    if (nextInner == '=')
                    {
                        this.Length++;
                    }
                    break;
            }
            #endregion

            return true;
        }

        /// <summary>
        /// Get the specified string in this.Content, return -1 means not found
        /// </summary>
        private int GetStringIndex(string str, int startIndex)
        {
            for (int i = startIndex; i < this.Content.Length; i++)
            {
                if (string.Compare(this.Content, i, str, 0, str.Length, StringComparison.Ordinal)
                    == 0)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Try getting the next character, if you have already arrived, there is no next character, return false.
        /// </summary>
        private bool TryGetNextChar(bool ignoreWhiteSpace, out char cNext)
        {
            cNext = '\0';
            for (int i = 0; i < int.MaxValue; i++)
            {
                if (this.Index + this.Length + i >= this.Content.Length)
                {
                    return false;
                }
                cNext = this.Content[this.Index + this.Length];
                if ((!ignoreWhiteSpace) || (!char.IsWhiteSpace(cNext)))
                {
                    break;
                }
            }
            return true;
        }

        private char GetTransformMeanChar(char chOriginal)
        {
            switch (chOriginal)
            {
                case '\\':
                    return '\\';

                case '\'':
                    return '\'';

                case '"':
                    return '"';

                case 'n':
                    return '\n';

                case 'r':
                    return '\r';

                case 't':
                    return '\t';

                case 'v':
                    return '\v';

                case 'b':
                    return '\b';

                case 'f':
                    return '\f';

                case 'a':
                    return '\a';

                //todo:
                //case 'x':
                //case 'X':
                //    sb.Append(Convert.ToChar();

                default:
                    throw new ParseUnknownException("\\" + chOriginal, Index);
                //return '\0';
            }
        }

        #endregion


        #region CodeParserPositionOperation

        /// <summary>
        /// Save the current location
        /// </summary>
        /// <returns></returns>
        public CodeParserPosition SavePosition()
        {
            return new MyCodeParserPosition()
            {
                Index = this.Index,
                Length = this.Length
            };
        }

        /// <summary>
        /// Restore the specified location
        /// </summary>
        /// <param name="position"></param>
        public void RevertPosition(CodeParserPosition position)
        {
            MyCodeParserPosition myPosition = (MyCodeParserPosition)position;
            this.Index = myPosition.Index;
            this.Length = myPosition.Length;
        }

        /// <summary>
        /// Restore to the initial state
        /// </summary>
        public void RevertPosition()
        {
            RevertPosition(new MyCodeParserPosition());
        }

        private class MyCodeParserPosition : CodeParserPosition
        {
            public int Index { get; set; }
            public int Length { get; set; }
        }

        #endregion
    }

    /// <summary>
    ///Code Parser Save the location to restore
    /// </summary>
    abstract public class CodeParserPosition
    {
    }
}
