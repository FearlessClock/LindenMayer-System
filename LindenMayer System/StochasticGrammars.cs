using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LindenMayer_System
{
    struct StochasticGrammar
    {
        public char toReplace;
        public char[] replaceBy1;
        public char[] replaceBy2;
        public char[] replaceBy3;
        public double chanceToReplace1;
        public double chanceToReplace2;

        public StochasticGrammar(char toRep, string repBy1, string repBy2, double chance)
        {
            toReplace = toRep;
            replaceBy1 = repBy1.ToArray<char>();
            replaceBy2 = repBy2.ToArray<char>();
            replaceBy3 = new char[0];
            chanceToReplace1 = chance;
            chanceToReplace2 = 2;
        }
        public StochasticGrammar(char toRep, string repBy1, string repBy2, string repBy3, double chance1, double chance2)
        {
            toReplace = toRep;
            replaceBy1 = repBy1.ToArray<char>();
            replaceBy2 = repBy2.ToArray<char>();
            replaceBy3 = repBy3.ToArray<char>();
            chanceToReplace1 = chance1;
            if (chance1 > chance2)
                chance2 = chance1;
            chanceToReplace2 = chance2;

        }

        public char[] GetReplacer(double rand)
        {
            if (rand > chanceToReplace2)
            {
                return replaceBy3;
            }
            else if (rand > chanceToReplace1)
                return replaceBy2;
            else
                return replaceBy1;
        }
    }
}
