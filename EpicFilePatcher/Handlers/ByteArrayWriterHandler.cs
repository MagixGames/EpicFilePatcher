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
    internal class ByteArrayWriterHandler : ITokenHandler
    {
        public TokenType Type => TokenType.WRITE;
        public bool Handle(ref NativeWriter stream, Token token)
        {
            Debug.Assert(token.Type == TokenType.BYTEARRAY);
            byte[] data;
            try
            {
                data = (byte[])token.Literal;
            } catch { return false; }

            for (int i = 0; i < data.Length; i++)
            {
                stream.Write(data[i], Parser.Endian);
            }

            return true;
        }
    }
}