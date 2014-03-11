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

        public static NFA PlusNFA(NFA op)
        {
            AutomataStatus start = new AutomataStatus();
            AutomataStatus end = new AutomataStatus();
            start.AddTransition((char)0, op.startStatus_);
            op.endStatus_.AddTransition((char)0, end);
            op.endStatus_.AddTransition((char)0, op.startStatus_);

            NFA nfa = new NFA();
            nfa.startStatus_ = start;
            nfa.endStatus_ = end;
            return nfa;
        }

        private NFA()
        {
            startStatus_ = null;
            endStatus_ = null;
        }

        public bool Match(String s)
        {
            HashSet<AutomataStatus> source = new HashSet<AutomataStatus>();
            HashSet<AutomataStatus> target = new HashSet<AutomataStatus>();
            source.Add(startStatus_);

            bool ret = true;
            foreach(char c in s)
            {
                HashSet<AutomataStatus> source_with_no_epsilon = GetStatusWithNoEpsilon(source);
                foreach(AutomataStatus current_status in source_with_no_epsilon)
                {
                    ICollection<AutomataStatus> next_status = current_status.GetNextStatus(c);

                    if (next_status == null)
                        continue;

                    foreach(AutomataStatus ns in next_status)
                    {
                        target.Add(ns);
                    }
                }

                if (target.Count == 0)
                {
                    ret = false;
                    break;
                }

                HashSet<AutomataStatus> temp = source;
                source = target;
                target = temp;
                target.Clear();
            }

            if (ret == false)
                return false;

            source = GetStatusWithNoEpsilon(source);
            if (source.Contains(endStatus_))
                return true;
            else
                return false;
        }

        private HashSet<AutomataStatus> GetStatusWithNoEpsilon(HashSet<AutomataStatus> source)
        {
            HashSet<AutomataStatus> s1 = new HashSet<AutomataStatus>();
            HashSet<AutomataStatus> s2 = new HashSet<AutomataStatus>();

            foreach(AutomataStatus s in source)
                s1.Add(s);

            int prev_count = 0;
            while(prev_count != s1.Count)
            {
                foreach(AutomataStatus s in s1)
                {
                    s2.Add(s);
                    ICollection<AutomataStatus> next_status = s.GetNextStatus((char)0);
                    if (next_status == null)
                        continue;
                    foreach(AutomataStatus ss in next_status)
                        s2.Add(ss);
                }

                prev_count = s1.Count;
                HashSet<AutomataStatus> temp = s1;
                temp.Clear();
                s1 = s2;
                s2 = temp;
            }

            return s1;
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
            nfa_stack_ = new Stack<NFA>();
            result_nfa_ = null;
        }

        public NFA GetNFA()
        {
            if (result_nfa_ != null)
                return result_nfa_;

            nfa_stack_.Clear();
            if (!EvalS() || nfa_stack_.Count != 1)
            {
                return null;
            }

            result_nfa_ = nfa_stack_.Pop();
            return result_nfa_;
        }

        // S->AS'
        private bool EvalS()
        {
            bool success = false;
            int saved_index = current_index_;

            do
            {
                if (EvalA() == false)
                    break;

                if (EvalSp() == false)
                    break;

                success = true;
            } while (false);

            if (!success)
            {
                current_index_ = saved_index;
            }

            return success;
        }

        // S'->|A{unionNFA}S'|epsilon
        private bool EvalSp()
        {
            bool success = false;
            int saved_index = current_index_;

            // S'->|A{unionNFA}S'
            do
            {
                if (regex_.Length <= current_index_)
                    break;

                if (regex_[current_index_] != '|')
                    break;

                // S'->|A{unionNFA}S'
                current_index_++;
                if (!EvalA())
                    break;

                if (nfa_stack_.Count < 2)
                    break;
                NFA op2 = nfa_stack_.Pop();
                NFA op1 = nfa_stack_.Pop();
                NFA result = NFA.UnionNFA(op1, op2);
                if (result == null)
                    return false;
                nfa_stack_.Push(result);

                if (EvalSp() == false)
                    break;
                
                success = true;
            } while (false);

            // not S'->|A{unionNFA}S'
            // Use S'->epsilon
            if (!success)
            {
                current_index_ = saved_index;
                success = true;
            }

            return success;
        }

        // A->BA'
        private bool EvalA()
        {
            bool success = false;
            int saved_index = current_index_;

            do
            {
                if (EvalB() == false)
                    break;

                if (EvalAp() == false)
                    break;

                success = true;
            } while (false);

            if (!success)
            {
                current_index_ = saved_index;
            }

            return success;
        }

        // A'->B{concatNFA}A' | epsilon
        private bool EvalAp()
        {
            bool success = false;
            int saved_index = current_index_;

            // A'->B{concatNFA}A'
            do
            {
                if (!EvalB())
                    break;

                if (nfa_stack_.Count < 2)
                    break;
                NFA op2 = nfa_stack_.Pop();
                NFA op1 = nfa_stack_.Pop();
                NFA result = NFA.ConcatNFA(op1, op2);
                if (result == null)
                    return false;
                nfa_stack_.Push(result);

                if (!EvalAp())
                    break;

                success = true;
            } while (false);

            // not A'->B{concatNFA}A'
            // Use A'->epsilon
            if (!success)
            {
                current_index_ = saved_index;
                success = true;
            }

            return success;
        }

        // B->C*{starNFA}|C+{plusNFA}|C
        private bool EvalB()
        {
            int saved_index = current_index_;
            if (!EvalC())
            {
                current_index_ = saved_index;
                return false;
            }

            bool success = false;
            saved_index = current_index_;
            do
            {
                if (regex_.Length <= current_index_)
                    break;

                if (regex_[current_index_] != '*' && regex_[current_index_] != '+')
                    break;

                if (nfa_stack_.Count < 1)
                    break;

                NFA op = nfa_stack_.Pop();
                NFA result = null;
                if (regex_[current_index_] == '*')
                    result = NFA.StarNFA(op);
                else
                    result = NFA.PlusNFA(op);
                if (result == null)
                    return false;
                nfa_stack_.Push(result);

                current_index_++;
                success = true;
            } while (false);

            if (!success)
            {
                current_index_ = saved_index;
            }

            return true;
        }

        // C->(S)|others{createInput}
        private bool EvalC()
        {
            int saved_index = current_index_;

            if (regex_.Length <= current_index_)
                return false;

            // C->(S)
            if (regex_[current_index_++] == '(' && 
                EvalS() &&
                current_index_ < regex_.Length &&
                regex_[current_index_++] == ')')
            {
                return true;
            }

            // C->others{createInput}
            char[] SPECIAL_MARK = { '(', ')', '*', '|', '\\', '+' };
            current_index_ = saved_index;
            if (regex_[current_index_] == '\\')
            {
                if (regex_.Length <= current_index_ + 1)
                    return false;

                if (!SPECIAL_MARK.Contains(regex_[current_index_+1]))
                {
                    return false;
                }

                current_index_++;
            }
            else if (SPECIAL_MARK.Contains(regex_[current_index_]))
            {
                return false;
            }

            NFA result = NFA.CreateFromInput(regex_[current_index_]);
            if (result == null)
            {
                current_index_ = saved_index;
                return false;
            }
            nfa_stack_.Push(result);
            current_index_++;

            return true;
        }

        private String regex_;
        private int current_index_;
        Stack<NFA> nfa_stack_;
        NFA result_nfa_;
    }
}
