using EpicFilePatcher.Common;
using EpicFilePatcher.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicFilePatcher.Handlers
{
    internal class Int16WriterHandler : ITokenHandler
    {
        public TokenType Type => TokenType.INT16;
        public bool Handle(ref NativeWriter stream, Token token)
        {
            Debug.Assert(token.Type == TokenType.INT);
            short data;
            try
            {
                data = Convert.ToInt16(token.Literal);
            }
            catch { return false; }

            stream.Write(data, Parser.Endian);

            return true;
        }
    }
}
