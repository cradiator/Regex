using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regex
{
    class NFA
    {

        public static NFA CreateFromInput(char input)
        {
            AutomataStatus start = new AutomataStatus();
            AutomataStatus end = new AutomataStatus();
            start.AddTransition(input, end);
         
            NFA nfa = new NFA();
            nfa.startStatus_ = start;
            nfa.endStatus_ = end;

            return nfa;
        }

        public static NFA ConcatNFA(NFA op1, NFA op2)
        {
            AutomataStatus start = new AutomataStatus();
            AutomataStatus end = new AutomataStatus();
            start.AddTransition((char)0, op1.startStatus_);
            op2.endStatus_.AddTransition((char)0, end);
            op1.endStatus_.AddTransition((char)0, op2.startStatus_);

            NFA nfa = new NFA();
            nfa.startStatus_ = start;
            nfa.endStatus_ = end;
            return nfa;
        }

        /*
        public static NFA ConcatNFA(NFA nfa1, NFA nfa2)
        {

        }
         */

        private NFA()
        {
            startStatus_ = null;
            endStatus_ = null;
        }
        private AutomataStatus startStatus_;
        private AutomataStatus endStatus_;
    }
}
