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
    internal class Int64WriterHandler : ITokenHandler
    {
        public TokenType Type => TokenType.INT64;
        public bool Handle(ref NativeWriter stream, Token token)
        {
            Debug.Assert(token.Type == TokenType.INT);
            long data;
            try
            {
                data = (long)token.Literal;
            }
            catch { return false; }

            stream.Write(data, Parser.Endian);

            return true;
        }
    }
}
