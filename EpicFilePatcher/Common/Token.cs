using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicFilePatcher.Common
{
    public enum TokenType
    {
        INVALID,

        // Operations
        WRITE,
        APPEND,

        GOTO,

        // string shinanegains
        WRITESTRING,
        WRITESTRING_N,

        // options
        INCLUDE,
        SLE, // switch little endian
        SBE, // switch big endian

        // types
        STRING,
        INT,
        BYTEARRAY,

        // other
        IDENTIFIER,

        // End of file
        EOF
    }

    public class Token
    {
        public TokenType Type;
        public object? Literal;
        public Token(TokenType type, object? literal = null)
        {
            Type = type;
            Literal = literal;
        }
    }
}
