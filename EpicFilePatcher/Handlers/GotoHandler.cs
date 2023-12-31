﻿using EpicFilePatcher.Common;
using EpicFilePatcher.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicFilePatcher.Handlers
{
    internal class GotoHandler : ITokenHandler
    {
        public TokenType Type => TokenType.GOTO;
        public bool Handle(ref NativeWriter stream, Token token)
        {
            Debug.Assert(token.Type == TokenType.INT);
            long data;
            try
            {
                data = (long)token.Literal;
            }
            catch { return false; }

            if (data > stream.Length) 
            {
                ConsoleLogger.Log(LogType.Error, $"Tried to jump to file pos [{data.ToString("X")}], but it is bigger than the size of the file.");
                return false; 
            }

            stream.Position = data + Parser.GotoOffset;

            return true;
        }
    }
}
