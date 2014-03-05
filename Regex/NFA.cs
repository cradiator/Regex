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

        public static NFA UnionNFA(NFA op1, NFA op2)
        {
            AutomataStatus start = new AutomataStatus();
            AutomataStatus end = new AutomataStatus();

            start.AddTransition((char)0, op1.startStatus_);
            start.AddTransition((char)0, op2.startStatus_);
            op1.endStatus_.AddTransition((char)0, end);
            op2.endStatus_.AddTransition((char)0, end);

            NFA nfa = new NFA();
            nfa.startStatus_ = start;
            nfa.endStatus_ = end;
            return nfa;
        }

        public static NFA StarNFA(NFA op)
        {
            AutomataStatus start = new AutomataStatus();
            AutomataStatus end = new AutomataStatus();
            start.AddTransition((char)0, op.startStatus_);
            start.AddTransition((char)0, end);
            op.endStatus_.AddTransition((char)0, op.startStatus_);
            op.endStatus_.AddTransition((char)0, end);

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

    class NFATranslator
    {
        public NFATranslator(String regex)
        {
            regex_ = regex;
            current_index_ = 0;
            need_concat_ = false;
        }

        public NFA GetNFA()
        {
            return null;
        }

        private char GetNextChar()
        {
            char[] SPECIAL_MARK = {')', '*', '|'};
            char current_char = regex_[current_index_];

            if (SPECIAL_MARK.Contains(current_char))
            {
                current_index_++;
                return current_char;
            }
            else
            {
                if (need_concat_)
                {
                    need_concat_ = false;
                    return CONCAT_MARK;
                }

                current_index_++;
                need_concat_ = true;
                return current_char;
            }
        }

        private String regex_;
        private int current_index_;
        private bool need_concat_;

        private static char CONCAT_MARK = (char)1;
    }
}
