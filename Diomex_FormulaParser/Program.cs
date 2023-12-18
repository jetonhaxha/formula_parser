using System;
using System.Collections.Generic;

class FormulaParser
{
    private Dictionary<char, OperatorInfo> operators;
    private Dictionary<string, Func<object, object>> functions;
    private Dictionary<string, object> variables;

    public FormulaParser()
    {
        operators = new Dictionary<char, OperatorInfo>
        {
            {'+', new OperatorInfo { Precedence = 1, Operation = (a, b) => {
                if (a is double && b is double) return (double)a + (double)b;
                else return Convert.ToString(a) + Convert.ToString(b);
            } }},
            {'-', new OperatorInfo { Precedence = 1, Operation = (a, b) => Convert.ToDouble(a) - Convert.ToDouble(b) }},
            {'*', new OperatorInfo { Precedence = 2, Operation = (a, b) => Convert.ToDouble(a) * Convert.ToDouble(b) }},
            {'/', new OperatorInfo { Precedence = 2, Operation = (a, b) => Convert.ToDouble(a) / Convert.ToDouble(b) }}
        };

        functions = new Dictionary<string, Func<object, object>>
        {
            {"Str", (value) => Convert.ToString(value)},
            {"Val", (value) => Convert.ToDouble(value)}
        };

        variables = new Dictionary<string, object>();
    }

    public void SetVariable(string name, object value)
    {
        variables[name] = value;
    }

    private List<object> SplitFormula(string formula)
    {
        var regex = new System.Text.RegularExpressions.Regex(@"\s*([a-zA-Z]+|[0-9]+\.[0-9]+|[0-9]+|\S)\s*");
        var tokens = new List<object>();
        var matches = regex.Matches(formula);

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            tokens.Add(match.Groups[1].Value);
        }

        return tokens;
    }

    private List<object> ParseFormula(string formula)
    {
        var outputQueue = new List<object>();
        var operatorStack = new Stack<object>();

        var tokens = SplitFormula(formula);

        foreach (var token in tokens)
        {
            if (double.TryParse(token.ToString(), out var numericValue))
            {
                outputQueue.Add(numericValue);
            }
            else if (variables.ContainsKey(token.ToString()))
            {
                outputQueue.Add(variables[token.ToString()]);
            }
            else if (functions.ContainsKey(token.ToString()))
            {
                operatorStack.Push(token);
            }
            else if (token.ToString() == "(")
            {
                operatorStack.Push(token);
            }
            else if (token.ToString() == ")")
            {
                while (operatorStack.Count > 0 && operatorStack.Peek().ToString() != "(")
                {
                    outputQueue.Add(operatorStack.Pop());
                }

                // Pop the '('
                operatorStack.Pop();

                while (operatorStack.Count > 0 && functions.ContainsKey(operatorStack.Peek().ToString()))
                {
                    outputQueue.Add(operatorStack.Pop());
                }

            }
            else if (operators.ContainsKey(token.ToString()[0]))
            {
                while (operatorStack.Count > 0 && operators.ContainsKey(operatorStack.Peek().ToString()[0]) &&
                       operators[token.ToString()[0]].Precedence <= operators[operatorStack.Peek().ToString()[0]].Precedence)
                {
                    outputQueue.Add(operatorStack.Pop());
                }
                operatorStack.Push(token);
            }
            else if (token.ToString() != "\"")
            {
                outputQueue.Add(token);
            }
        }

        // Pop any remaining operators from the stack to the output queue
        while (operatorStack.Count > 0)
        {
            outputQueue.Add(operatorStack.Pop());
        }

         //Console.WriteLine("outputQueue: " + string.Join(", ", outputQueue));
        return outputQueue;
    }


    private object EvaluateRPN(List<object> rpn)
    {
        var stack = new Stack<object>();
        foreach (var token in rpn)
        {
            if (operators.ContainsKey(token.ToString()[0]))
            {
                var b = stack.Pop();
                var a = stack.Pop();
                stack.Push(operators[token.ToString()[0]].Operation(a, b));
            }
            else if (functions.ContainsKey(token.ToString()))
            {
                var arg = stack.Pop();
                stack.Push(functions[token.ToString()](arg));
            }
            else
            {
                stack.Push(token);
            }
        }

        return stack.Peek();
    }

    public object Calculate(string formula)
    {
        var rpn = ParseFormula(formula);
        return EvaluateRPN(rpn);
    }

    private class OperatorInfo
    {
        public int Precedence { get; set; }
        public Func<object, object, object> Operation { get; set; }
    }
}

class Program
{ 
    static void Main()
    {
        // New instance of the class
        var parser = new FormulaParser();

        // Create variables
        parser.SetVariable("sum", 10);
        parser.SetVariable("name", "John");

        // Examples
        var example1 = parser.Calculate("1 + 2.5 * 3");
        var example2 = parser.Calculate("((1 + 2) * (3 + 4)) + (5.2 * sum)");
        var example3 = parser.Calculate("\"Hello \" + name");
        var example4 = parser.Calculate("Str((1 + 2) * Val(\"55\")) + sum");

        Console.WriteLine("Result: " + example1);
        Console.WriteLine("Result: " + example2);
        Console.WriteLine("Result: " + example3);
        Console.WriteLine("Result: " + example4);
        // Press any key to stop application
        Console.ReadKey();
    }
}
