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
    internal class AlignWithHandler : ITokenHandler
    {
        public TokenType Type => TokenType.ALIGNWITH;
        public bool Handle(ref NativeWriter stream, Token token)
        {
            Debug.Assert(token.Type == TokenType.INT);

            long[] data = (long[])token.Literal;
            long alignment = Convert.ToInt64(data[0]);
            byte writeValue = Convert.ToByte(data[1]);

            while (stream.Position % alignment != 0)
            {
                stream.Write(writeValue);
            }

            return true;
        }
    }
}
