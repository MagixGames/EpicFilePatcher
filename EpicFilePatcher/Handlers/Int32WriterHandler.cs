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
    internal class Int32WriterHandler : ITokenHandler
    {
        public TokenType Type => TokenType.INT32;
        public bool Handle(ref NativeWriter stream, Token token)
        {
            Debug.Assert(token.Type == TokenType.INT);
            int data;
            
            data = Convert.ToInt32(token.Literal);

            stream.Write(data, Parser.Endian);

            return true;
        }
    }
}
