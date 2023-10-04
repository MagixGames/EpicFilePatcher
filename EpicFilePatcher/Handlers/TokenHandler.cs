using EpicFilePatcher.Common;
using EpicFilePatcher.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicFilePatcher.Handlers
{
    public static class TokenHandler
    {
        public static readonly Dictionary<TokenType, Type> handlers = new Dictionary<TokenType, Type>();

        public static ITokenHandler GetHandler(TokenType tokentype)
        {
            if (handlers.TryGetValue(tokentype, out Type type))
            {
                return (ITokenHandler)Activator.CreateInstance(type);
            }
            throw new Exception("No handler found for type " + tokentype);
            //return new DefaultHandler();
        }

        static TokenHandler()
        {
            var type = typeof(ITokenHandler);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);

            foreach (Type t in types)
            {
                ITokenHandler h = (ITokenHandler)Activator.CreateInstance(t);
                handlers.Add(h.Type, t);
            }
        }
    }
}
