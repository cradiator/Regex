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
            need_concat_ = false;
        }

        public NFA GetNFA()
        {
            Stack<NFA> operand_stack = new Stack<NFA>();
            Stack<char> operator_stack = new Stack<char>();

            bool success = false;
            char c = GetNextChar();
            while(c != EOF)
            {
                if (c == '*')
                {
                    operator_stack.Push(c);
                    success = EvalTop(operand_stack, operator_stack);
                }
                else if (c == '|')
                {
                    if (operator_stack.Count == 0)
                    {
                        operator_stack.Push(c);
                        success = true;
                    }
                    else
                    {
                        success = true;
                        while(success && operator_stack.Count != 0)
                        {
                            success = EvalTop(operand_stack, operator_stack);
                        }
                        operator_stack.Push(c);
                    }
                }
                else if (c == CONCAT_MARK)
                {
                    if (operator_stack.Count == 0 || operator_stack.Peek() == '|')
                    {
                        operator_stack.Push(c);
                        success = true;
                    }
                    else
                    {
                        success = true;
                        while (success && operator_stack.Count != 0)
                        {
                            success = EvalTop(operand_stack, operator_stack);
                        }
                        operator_stack.Push(c);
                    }
                }
                else
                {
                    success = EvalOperand(c, operand_stack);
                }

                if (success == false)
                    break;

                c = GetNextChar();
            }

            while(success && operator_stack.Count != 0)
            {
                success = EvalTop(operand_stack, operator_stack);
            }

            if (success && operand_stack.Count == 1)
            {
                return operand_stack.Pop();
            }
            else
            {
                return null;
            }
        }

        private bool EvalTop(Stack<NFA> operand_stack, Stack<char> operator_stack)
        {
            if (operand_stack.Count == 0 || operator_stack.Count == 0)
                return false;

            bool success = false;
            char op = operator_stack.Peek();
            if (op == '|')
            {
                success = EvalUnion(operand_stack);
            }
            else if (op == '*')
            {
                success = EvalStar(operand_stack);
            }
            else if (op == CONCAT_MARK)
            {
                success = EvalConcat(operand_stack);
            }
            else
            {
                success = false;
            }

            if (success)
            {
                operator_stack.Pop();
            }

            return success;
        }

        private bool EvalOperand(char input, Stack<NFA> operand_stack)
        {
            NFA nfa = NFA.CreateFromInput(input);
            if (nfa == null)
                return false;

            operand_stack.Push(nfa);
            return true;
        }

        private bool EvalConcat(Stack<NFA> operand_stack)
        {
            if (operand_stack.Count < 2)
            {
                return false;
            }

            NFA op2 = operand_stack.Pop();
            NFA op1 = operand_stack.Pop();
            NFA result = NFA.ConcatNFA(op1, op2);
            operand_stack.Push(result);

            return true;
        }

        private bool EvalUnion(Stack<NFA> operand_stack)
        {
            if (operand_stack.Count < 2)
            {
                return false;
            }

            NFA op2 = operand_stack.Pop();
            NFA op1 = operand_stack.Pop();
            NFA result = NFA.UnionNFA(op1, op2);
            operand_stack.Push(result);

            return true;
        }

        private bool EvalStar(Stack<NFA> operand_stack)
        {
            if (operand_stack.Count < 1)
            {
                return false;
            }

            NFA op = operand_stack.Pop();
            NFA result = NFA.StarNFA(op);
            operand_stack.Push(result);

            return true;
        }

        private char GetNextChar()
        {
            char[] SPECIAL_MARK = {'*', '|'};

            if (current_index_ >= regex_.Length)
            {
                return EOF;
            }

            char current_char = regex_[current_index_];

            if (current_char == '|')
            {
                current_index_++;
                need_concat_ = false;
                return current_char;
            }
            else if (current_char == '*')
            {
                current_index_++;
                need_concat_ = true;
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
        private static char EOF = (char)255;
    }
}
