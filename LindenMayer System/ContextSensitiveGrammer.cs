using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LindenMayer_System
{
    class ContextSensitiveGrammer
    {
        char before;
        char after;
        char toReplace;
        string replaceWith;

        public string ReplaceWith
        {
            get { return replaceWith; }
            private set { }
        } 

        public ContextSensitiveGrammer( char b, char toRep, char a, string repWith)
        {
            before = b;
            after = a;
            toReplace = toRep;
            replaceWith = repWith;
        }

        public bool ContextGood(char b, char toRep, char a)
        {
            return toRep == toReplace && b == before && a == after;
        }
    }
}
