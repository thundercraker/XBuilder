using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Logic {

    private Interpreter context;

    public Logic(Interpreter I)
    {
        context = I;
    }

    public static Dictionary<string, int> LogicOperators = new Dictionary<string, int>()
    {
        { "==", 1 },
        { "!=", 1 },
        { ">", 1 },
        { ">=", 1 },
        { "<", 1 },
        { "<=", 1 },
        { "&&", 0 },
        { "||", 0 }
    };


    /// <summary>
    /// Evaluate a logical expression
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public bool EvaluateLogic(string[] tokens, int start)
    {
        tokens = context.ReplaceToValues(tokens, start);
        string line = Interpreter.GetString(tokens);

        //Debug.Log("Infix: " + line);

        InfixPrefix IP = new InfixPrefix(LogicOperators);
        string ipOut = IP.ConvertInfixToPrefix(line);

        tokens = InfixPrefix.GetTokens(ipOut);
        //Debug.Log("Prefix: " + ipOut);

        Stack<string> operands = new Stack<string>();

        for (int i = tokens.Length - 1; i > -1; i--)
        {
            string token = tokens[i];
            if (!LogicOperators.ContainsKey(token))
            {
                operands.Push(token);
            }
            else
            {
                //its an operator
                string op1 = operands.Pop();
                string op2 = operands.Pop();
                //Debug.Log("OP1 " + op1 + " OP2 " + op2 + " OPR" + token);
                operands.Push(ProcessCondition(token, op1, op2));
            }
            //Debug.Log("Operands stack  [" + Interpreter.GetString(operands.ToArray()) + "]");
        }
        string fin = operands.Pop();
        //Debug.Log(fin);
        return bool.Parse(fin);
    }

    /// <summary>
    /// Processes a single comparision condition
    /// </summary>
    /// <param name="cOperator">The operator ==, !=, >, >=, <, <=, ||, &&</param>
    /// <param name="operand1">Left Operand</param>
    /// <param name="operand2">Right Operand</param>
    /// <returns></returns>
    public string ProcessCondition(string cOperator, string operand1, string operand2)
    {
        switch (cOperator)
        {
            case "==":
                if (InfixPrefix.equal(operand1, operand2))
                    if (context.IsSameType(operand1, operand2))
                        return "true";
                break;
            case "!=":
                if (!InfixPrefix.equal(operand1, operand2))
                    return "true";
                else
                    if (!context.IsSameType(operand1, operand2))
                        return "true";
                break;
            case ">=":
            case "<=":
            case ">":
            case "<":
                double op1 = 0.0, op2 = 0.0;
                if (double.TryParse(operand1, out op1) && double.TryParse(operand2, out op2))
                {
                    //Debug.Log("Comparing " + op1 + " " + op2 + " " + cOperator[0]);
                    if (Interpreter.CompChar(cOperator[cOperator.Length - 1], '='))
                    {
                        if (op1 == op2)
                        {
                            return "true";
                        }
                    }
                    if (Interpreter.CompChar(cOperator[0], '>'))
                    {
                        //Debug.Log("GT " + op1 + " " + op2);
                        if (op1 > op2)
                            return "true";
                    }
                    else
                    {
                        if (op1 < op2)
                            return "true";
                    }
                }
                else
                {
                    //Comparision between non numbers
                    context.ThrowError("Comparision between two non-comparable values attempted. (" + operand1 + " " + cOperator + " " + operand2 + ")");
                }
                break;
            case "&&":
            case "||":
                bool bop1 = false, bop2 = false;
                if (bool.TryParse(operand1, out bop1) && bool.TryParse(operand2, out bop2))
                {
                    if (InfixPrefix.equal(cOperator, "&&"))
                    {
                        return (bop1 && bop2) ? "true" : "false";
                    }
                    else
                    {
                        return (bop1 || bop2) ? "true" : "false";
                    }
                }
                else
                {
                   context.ThrowError("Logical operation two non-boolean values attempted. (" + operand1 + " " + cOperator + " " + operand2 + ")");
                }
                break;
        }

        return "false";
    }
}
