using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regex
{
    class Program
    {
        static void Main(string[] args)
        {
            NFATranslator t = new NFATranslator("(\\*)*");
            NFA nfa = t.GetNFA();
            bool ret = nfa.Match("");
        }
    }
}
