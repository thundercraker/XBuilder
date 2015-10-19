using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Arithmetic {

    private Interpreter context;

    public Arithmetic(Interpreter I)
    {
        context = I;
    }
    
    public static Dictionary<string, int> ArithOperators = new Dictionary<string, int>()
    {
        { "^", 3 },
        { "/", 2 },
        { "*", 2 },
        { "%", 1 },
        { "+", 0 },
        { "-", 0 }
    };

    public double EvaluateArithExpression(string[] tokens, int start)
    {
        tokens = context.ReplaceToValues(tokens, start);

        string line = Interpreter.GetString(tokens);
        //Debug.Log("Infix: [" + line + "]");

        InfixPrefix IP = new InfixPrefix(ArithOperators);
        string ipOut = IP.ConvertInfixToPrefix(line);

        tokens = InfixPrefix.GetTokens(ipOut);
        //Debug.Log("Prefix: " + ipOut);

        Stack<double> operands = new Stack<double>();

        for (int i = tokens.Length - 1; i > -1; i--)
        {
            string token = tokens[i];
            if (!ArithOperators.ContainsKey(token))
            {
                double op = 0;
                if (double.TryParse(token, out op))
                    operands.Push(op);
                else
                    context.ThrowError("The operand is not a number. ( " + token + " )");
            }
            else
            {
                //its an operator
                double op1 = operands.Pop();
                double op2 = operands.Pop();
                operands.Push(ComputeArith(token, op1, op2));
            }
        }
        return operands.Pop();
    }

    public double ComputeArith(string opr, double op1, double op2)
    {
        double res = 0;
        switch (opr)
        {
            case ("^"):
                res = Math.Pow(op1, op2);
                break;
            case ("/"):
                res = op1 / op2;
                break;
            case ("*"):
                res = op1 * op2;
                break;
            case ("%"):
                res = op1 % op2;
                break;
            case ("+"):
                res = op1 + op2;
                break;
            case ("-"):
                res = op1 - op2;
                break;
        }

        return res;
    }
}
