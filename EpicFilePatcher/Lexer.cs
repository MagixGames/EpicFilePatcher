using EpicFilePatcher.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace EpicFilePatcher
{
    public class LexerException : Exception { public LexerException(string e) : base(e) { }  }


    internal class Lexer
    {
        public FileInfo File;
        private string currentLine;
        public int position;

        public Lexer(FileInfo file) 
        {
            File = file;
        }

        public static Dictionary<string, TokenType> STRING_TO_TYPE = new Dictionary<string, TokenType>(StringComparer.OrdinalIgnoreCase)
        {
            { "write", TokenType.WRITE },

            { "int16", TokenType.INT16 },
            { "int32", TokenType.INT32 },
            { "int64", TokenType.INT64 },

            { "writestring", TokenType.WRITESTRING },
            { "writestringn", TokenType.WRITESTRING_N },
            { "append", TokenType.APPEND },

            { "include", TokenType.INCLUDE },
            { "goto", TokenType.GOTO },
            { "offset", TokenType.OFFSET },

            { "littleendian", TokenType.SLE },
            { "little", TokenType.SLE },
            { "switchlittleendian", TokenType.SLE },

            { "bigendian", TokenType.SBE },
            { "big", TokenType.SBE },
            { "switchbigendian", TokenType.SBE },
        };


        public List<Token> Tokenize()
        {
            List<Token> tokens = new List<Token>();

            StreamReader reader = new StreamReader(File.OpenRead());
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                position = 0;
                currentLine = line;
                try
                {
                    TokenizeNextLine(ref tokens);
                } 
                catch (LexerException e) 
                {
                    ConsoleLogger.Log(LogType.Error, "Lexer.cs : " + e.ToString());
                }
            }
            tokens.Add(new Token(TokenType.EOF));

            return tokens;
        }

        public void TokenizeNextLine(ref List<Token> tokens)
        {
            SkipWhitespace();
            if (EndOfContext()) return;

            if (Peek() == '/' && Peek(1) == '/')
            {
                return;
            }

            else if (Peek() == '@')
            {
                Consume();
                HandleOptions(ref tokens);

                TrySubInstruction(ref tokens);
                return;
            }

            else if (Peek() == '0' && Peek(1) == 'x')
            {
                tokens.Add(new Token(TokenType.GOTO));
                tokens.Add(ScanNumber());
                SkipWhitespace();
            }

            tokens.Add(ScanKeyword());

            HandleOperations(ref tokens);

            TrySubInstruction(ref tokens);
        }

        public void TrySubInstruction(ref List<Token> tokens)
        {
            SkipWhitespace();
            if (!(Peek() == ';')) return;

            Consume();
            currentLine = currentLine.Substring(position);
            position = 0;
            TokenizeNextLine(ref tokens);
        }

        public void HandleOperations(ref List<Token> tokens)
        {
            TokenType type = tokens.Last().Type;
            SkipWhitespace();
            switch (type)
            {
                case TokenType.INT16:
                case TokenType.INT32:
                case TokenType.INT64:

                case TokenType.GOTO:
                    {
                        tokens.Add(ScanNumber());
                    }
                    break;

                case TokenType.APPEND:
                case TokenType.WRITE:
                    {
                        List<byte> array = new List<byte>();
                        while (!EndOfContext() && NextIsHex())
                        {
                            array.Add(Convert.ToByte(ScanHexByte().Literal));
                            SkipWhitespace();
                        }
                        tokens.Add(new Token(TokenType.BYTEARRAY, array.ToArray()));
                    }
                    break;

                case TokenType.WRITESTRING_N:
                case TokenType.WRITESTRING:
                    {
                        tokens.Add(ScanString());
                    }
                    break;
            }
        }

        public void HandleOptions(ref List<Token> tokens)
        {
            Token token = ScanKeyword();
            tokens.Add(token);
            SkipWhitespace();
            switch (token.Type)
            {
                case TokenType.INCLUDE:
                    {
                        tokens.RemoveAt(tokens.Count - 1);
                        HandleInclude(ref tokens);
                    }
                    break;
                case TokenType.OFFSET:
                    {
                        tokens.Add(ScanNumber());
                    }
                    break;
                default:
                    break;
            }
        }

        public void HandleInclude(ref List<Token> tokens)
        {
            position = currentLine.IndexOf('"');
            if (position == -1)
            {
                throw new Exception("Expected path string after include.");
            }
            Token str = ScanString();
            Debug.Assert(str.Literal is string);
            try
            {
                string filePath = (string)str.Literal;
                if (filePath != null)
                {
                    tokens.AddRange(new Lexer(new FileInfo(File.Directory.FullName + "\\" + filePath)).Tokenize());
                    Debug.Assert(tokens.Last().Type == TokenType.EOF);
                    tokens.RemoveAt(tokens.Count - 1);
                }
            }
            catch
            {
                throw new Exception("Error including file " + File.Directory.FullName + "\\" + (string)str.Literal);
            }
        }


        private Token ScanNumber()
        {
            //Debug.Assert(Peek() == '0');
            if (Peek() == '0' && Peek(1) == 'x')
            {
                return ScanHex();
            }

            StringBuilder sb = new StringBuilder();
            bool isReal = false;
            while (char.IsDigit(Peek()) || Peek() == '.')
            {
                char part = Next();
                if (part == '.')
                {
                    isReal = true;
                }
                sb.Append(part);
            }
            if (isReal)
            {
                throw new LexerException("Floats aren't allowed");
            }
            return TryParseInt(sb.ToString());
        }

        private Token TryParseInt(string input, NumberStyles style = NumberStyles.Integer)
        {
            try
            {
                return new Token(TokenType.INT, long.Parse(input, style));
            }
            catch
            {
                throw new LexerException("Error parsing integer from string.");
            }
        }
        private Token ScanHex()
        {
            Debug.Assert(Peek() == '0' && Peek(1) == 'x');
            StringBuilder sb = new();
            position += 2;
            while (NextIsHex())
            {
                sb.Append(Next());
            }
            return TryParseInt(sb.ToString(), NumberStyles.HexNumber);
        }

        private Token ScanHexWithoutPrefix()
        {
            StringBuilder sb = new();
            while (NextIsHex())
            {
                sb.Append(Next());
            }
            return TryParseInt(sb.ToString(), NumberStyles.HexNumber);
        }

        private Token ScanHexByte()
        {
            StringBuilder sb = new();
            sb.Append(Next());
            sb.Append(Next());
            return TryParseInt(sb.ToString(), NumberStyles.HexNumber);
        }

        private bool NextIsHex()
        {
            char n = char.ToLower(Peek());
            return char.IsDigit(n) || (n >= ((byte)'a') && n <= ((byte)'f'));
        }

        public string ReadString()
        {
            StringBuilder sb = new StringBuilder();
            Debug.Assert(Peek() == '"');
            Consume();
            while (Peek() != '"')
            {
                if (EndOfContext()) throw new Exception("Reached end of line while scanning string.");
                sb.Append(Next());
            }
            Consume();
            return sb.ToString();
        }

        public Token ScanString()
        {
            return new Token(TokenType.STRING, ReadString());
        }


        private void SkipWhitespace()
        {
            while (Peek() == ' ' || Peek() == '\t' || Peek() == '\n' || Peek() == '\r')
            {
                Consume();
            }
        }

        private Token ScanKeyword()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Next());
            while (char.IsLetterOrDigit(Peek()) || Peek() == ':')
            {
                sb.Append(Next());
            }
            string str = sb.ToString();
            if (STRING_TO_TYPE.ContainsKey(str))
            {
                return new Token(STRING_TO_TYPE[str], str);
            }

            return new Token(TokenType.INVALID,  str);
        }

        private char Peek(int lookahead = 0)
        {
            if (position + lookahead >= currentLine.Length)
            {
                return '\0';
            }
            return currentLine[position + lookahead];
        }

        private void Consume()
        {
            position++;
        }
        private char Next()
        {
            Debug.Assert(!EndOfContext());
            return currentLine[position++];
        }
        private bool EndOfContext()
        {
            return position >= currentLine.Length;
        }
    }
}
