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
    internal class StringWriterHandler : ITokenHandler
    {
        public TokenType Type => TokenType.WRITESTRING;
        public bool Handle(ref NativeWriter stream, Token token)
        {
            Debug.Assert(token.Type == TokenType.STRING);
            string str;
            try
            {
                str = (string)token.Literal;
            }
            catch { return false; }

            stream.WriteFixedSizedString(str, str.Length);
            return true;
        }
    }
}
