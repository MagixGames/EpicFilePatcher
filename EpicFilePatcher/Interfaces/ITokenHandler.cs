using EpicFilePatcher.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicFilePatcher.Interfaces
{
    public interface ITokenHandler
    {
        TokenType Type { get; }
        bool Handle(ref NativeWriter stream, Token token);
    }
}
