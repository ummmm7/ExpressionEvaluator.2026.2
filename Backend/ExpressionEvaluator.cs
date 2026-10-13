using System.Globalization;

namespace Backend;

public static class ExpressionEvaluator
{
    public static double Evalute(string infix) => EvalutePostfix(ToPostfix(infix));

    private static List<string> ToPostfix(string infix)
    {
        var postfix = new List<string>();
        var stack = new Stack<char>();
        var expectOperand = true;
        var index = 0;

        while (index < infix.Length)
        {
            var item = infix[index];

            if (char.IsWhiteSpace(item))
            {
                index++;
                continue;
            }

            if (char.IsAsciiDigit(item) || item == '.' || IsNegativeNumber(infix, index, expectOperand))
            {
                if (!expectOperand)
                {
                    throw new InvalidOperationException("Invalid expression.");
                }

                postfix.Add(ReadNumber(infix, ref index));
                expectOperand = false;
                continue;
            }

            if (item == '(')
            {
                if (!expectOperand)
                {
                    throw new InvalidOperationException("Invalid expression.");
                }

                stack.Push(item);
            }
            else if (item == ')')
            {
                if (expectOperand)
                {
                    throw new InvalidOperationException("Invalid expression.");
                }

                while (stack.Count > 0 && stack.Peek() != '(')
                {
                    postfix.Add(stack.Pop().ToString());
                }

                if (stack.Count == 0)
                {
                    throw new InvalidOperationException("Unbalanced parentheses.");
                }

                stack.Pop();
            }
            else if (IsOperator(item))
            {
                if (expectOperand)
                {
                    throw new InvalidOperationException("Invalid expression.");
                }

                while (stack.Count > 0 && PriorityInfix(item) <= PriorityStack(stack.Peek()))
                {
                    postfix.Add(stack.Pop().ToString());
                }

                stack.Push(item);
                expectOperand = true;
            }
            else
            {
                throw new InvalidOperationException($"Invalid character '{item}'.");
            }

            index++;
        }

        if (expectOperand)
        {
            throw new InvalidOperationException("Invalid expression.");
        }

        while (stack.Count > 0)
        {
            var ope = stack.Pop();
            if (ope == '(')
            {
                throw new InvalidOperationException("Unbalanced parentheses.");
            }

            postfix.Add(ope.ToString());
        }

        return postfix;
    }

    private static bool IsNegativeNumber(string infix, int index, bool expectOperand) =>
        expectOperand &&
        infix[index] == '-' &&
        index + 1 < infix.Length &&
        (char.IsAsciiDigit(infix[index + 1]) || infix[index + 1] == '.');

    private static string ReadNumber(string infix, ref int index)
    {
        var start = index;
        if (infix[index] == '-')
        {
            index++;
        }

        while (index < infix.Length && (char.IsAsciiDigit(infix[index]) || infix[index] == '.'))
        {
            index++;
        }

        return infix.Substring(start, index - start);
    }

    private static int PriorityStack(char op) => op switch
    {
        '^' => 3,
        '*' => 2,
        '/' => 2,
        '+' => 1,
        '-' => 1,
        '(' => 0,
        _ => throw new InvalidOperationException("Invalid expression."),
    };

    private static int PriorityInfix(char op) => op switch
    {
        '^' => 4,
        '*' => 2,
        '/' => 2,
        '+' => 1,
        '-' => 1,
        '(' => 5,
        _ => throw new InvalidOperationException("Invalid expression."),
    };

    private static bool IsOperator(char item) => item == '^' || item == '*' || item == '/' || item == '+' || item == '-';

    private static double EvalutePostfix(List<string> postfix)
    {
        var stack = new Stack<double>();
        foreach (var token in postfix)
        {
            if (token.Length == 1 && IsOperator(token[0]))
            {
                if (stack.Count < 2)
                {
                    throw new InvalidOperationException("Invalid expression.");
                }

                var ope2 = stack.Pop();
                var ope1 = stack.Pop();
                stack.Push(Calculate(ope1, ope2, token[0]));
            }
            else
            {
                stack.Push(ParseNumber(token));
            }
        }

        if (stack.Count != 1)
        {
            throw new InvalidOperationException("Invalid expression.");
        }

        return stack.Pop();
    }

    private static double ParseNumber(string token)
    {
        if (!double.TryParse(token, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number))
        {
            throw new FormatException($"Invalid number '{token}'.");
        }

        return number;
    }

    private static double Calculate(double ope1, double ope2, char item)
    {
        if (item == '/' && ope2 == 0)
        {
            throw new DivideByZeroException("Division by zero.");
        }

        var result = item switch
        {
            '*' => ope1 * ope2,
            '/' => ope1 / ope2,
            '+' => ope1 + ope2,
            '-' => ope1 - ope2,
            '^' => Math.Pow(ope1, ope2),
            _ => throw new InvalidOperationException("Invalid expression."),
        };

        if (double.IsNaN(result) || double.IsInfinity(result))
        {
            throw new ArithmeticException("The result is not a valid number.");
        }

        return result;
    }
}