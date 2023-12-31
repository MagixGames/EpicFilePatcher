﻿using EpicFilePatcher.Common;
using EpicFilePatcher.Handlers;
using EpicFilePatcher.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace EpicFilePatcher
{
    public class Parser
    {
        public FileInfo Output;
        public FileInfo Original;

        private int position;
        private List<Token> tokens;

        private static Endian currentEndian;
        private static long currentGotoOffset;
        public static Endian Endian { get { return currentEndian; } }
        public static long GotoOffset { get { return currentGotoOffset; } }
        private bool atEnd => tokens[position].Type == TokenType.EOF;
        private Token previous => tokens[position - 1];

        public Parser(FileInfo original)
        {
            Original = original;
            Output = new FileInfo(original.FullName.Replace(original.Extension, string.Empty) + ".patched" + original.Extension);
            currentEndian = Endian.Little; // default
            currentGotoOffset = 0; // default
        }

        public void Execute(ref List<Token> tokens)
        {
            this.tokens = tokens;
            int tokensCount = tokens.Count;

            File.Copy(Original.FullName, Output.FullName, true);

            NativeWriter writer = new NativeWriter(Output.Open(FileMode.OpenOrCreate));
            if (EpicFilePatcher.WriteDebugBTs)
            {
                writer.UseDebug = true;
            }


            while (position < tokensCount)
            {
                Token nextToken = Advance();
                switch (nextToken.Type)
                {
                    case TokenType.EOF:
                        goto ExitLoop;
                    case TokenType.SLE:
                        currentEndian = Endian.Little; 
                        break;
                    case TokenType.SBE:
                        currentEndian = Endian.Big;
                        break;
                    case TokenType.OFFSET:
                        currentGotoOffset = (long) Advance().Literal;
                        break;
                    default:
                        {
                            ITokenHandler handler = TokenHandler.GetHandler(nextToken.Type);
                            if (handler == null)
                            {
                                break;
                            }

                            try
                            {
                                handler.Handle(ref writer, Advance());
                            } 
                            catch (Exception e)
                            {
                                ConsoleLogger.Log(LogType.Error, "Error handling token#" + (position - 1) + " with handler [" + handler.GetType().FullName + "]: " + e);
                            }
                        }
                        break;
                }
            }
        ExitLoop:

            // writes extra byte because bug
            // writer.Write(0x00);
            writer.Flush();

            if (EpicFilePatcher.WriteDebugBTs)
            {
                writer.OutPDBData(Output.FullName + ".bt");
            }
        }

        internal void Consume(TokenType type)
        {
            if (!Match(type))
            {
                //if (type == TokenType.SEMICOLON)
                //{
                //    throw new Exception("Expected semicolon after statement.");
                //}

                // Token actualTok = Peek();
                string actual = Peek().Type.ToString();
                string expected = type.ToString();
                //throw new CompilerError("Expected " + expected + ", found " + actual);
            }
        }
        internal void Consume(params TokenType[] types)
        {
            foreach (var type in types)
            {
                Consume(type);
            }
        }

        internal bool Check(TokenType type)
        {
            return Peek().Type == type;
        }

        internal bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        internal Token Peek()
        {
            if (atEnd)
            {
                throw new Exception("Reached EOF while parsing.");
            }
            return tokens[position];
        }
        internal Token Advance()
        {
            return tokens[position++];
        }
    }
}
