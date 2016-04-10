using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LindenMayer_System
{
    /// <summary>
    /// Regular Grammer struct is a struct used to store a certain production rule
    /// </summary>
    struct RegularGrammer
    {
        public char toReplace;
        public char[] replaceBy;

        /// <summary>
        /// A regular grammer A -> B rule
        /// </summary>
        /// <param name="toRep">The char that will be replaced if found</param>
        /// <param name="repBy">The string that will replace it</param>
        public RegularGrammer(char toRep, string repBy)
        {
            toReplace = toRep;
            replaceBy = repBy.ToArray<char>();
        }
    }
}
