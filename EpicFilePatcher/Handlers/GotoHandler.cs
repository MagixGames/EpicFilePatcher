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

            stream.Position = data;

            return true;
        }
    }
}
