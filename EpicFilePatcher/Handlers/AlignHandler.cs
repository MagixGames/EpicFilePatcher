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
    internal class AlignHandler : ITokenHandler
    {
        public TokenType Type => TokenType.ALIGN;
        public bool Handle(ref NativeWriter stream, Token token)
        {
            Debug.Assert(token.Type == TokenType.INT);
            byte data;
            try
            {
                data = Convert.ToByte(token.Literal);
            }
            catch { return false; }

            stream.WritePadding(data);

            return true;
        }
    }
}
