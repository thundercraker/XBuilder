using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class InfixPrefix {


    public Dictionary<string, int> operators;

    public InfixPrefix(Dictionary<string, int> operators)
    {
        this.operators = operators;
    }

    public static bool equal(string a, string b)
    {
        return a.Equals(b, System.StringComparison.OrdinalIgnoreCase);
    }

    public static string[] GetTokens(string line)
    {
        string infixExpression = Regex.Replace(line, @"\s+", " ");
        return infixExpression.Split();
    }

    public string ConvertInfixToPrefix(string infixExpression)
    {
        try
        {

            ValidateInfixExpression(ref infixExpression);

        }
        catch (Exception ex)
        {

            Console.WriteLine("Invalid infix expression. Error Details:{0}", ex.Message);
            return null;

        }

        Stack<string> operatorStack = new Stack<string>();
        Stack<string> operandStack = new Stack<string>();

        operatorStack.Push("(");
        infixExpression += ")";

        //Put space between every ( and )
        infixExpression = infixExpression.Replace("(", " ( ");
        infixExpression = infixExpression.Replace(")", " ) ");
        //collapse multiple space into single space
        infixExpression = Regex.Replace(infixExpression, @"\s+", " ");
        infixExpression = infixExpression.Trim();
        //Debug.Log("IP line: [" + infixExpression + "] stack " + PrintArray(operatorStack));

        string[] tokens = infixExpression.Split();

        //Debug.Log("Split: " + PrintArray(tokens));

        foreach (string ch in tokens)
        {
            //Debug.Log("Token " + ch);

            if (equal(ch,"("))
            {

                operatorStack.Push(ch);

            }
            else if (equal(ch,")"))
            {

                // Pop from operator Stack until '(' is encountered
                string poppedOperator = operatorStack.Pop();

                while (!equal(poppedOperator,"("))
                {

                    operandStack.Push(PrefixExpressionBuilder(operandStack, poppedOperator));

                    poppedOperator = operatorStack.Pop();

                }

            }
            else if (IsOperator(ch))
            {

                // Pop all operators from Operator Stack which have same or higher precedence
                string poppedOperator = operatorStack.Pop();

                bool sameOrHighPrecedence = CheckSameOrHighPrecedence(poppedOperator, ch);

                while (sameOrHighPrecedence)
                {

                    operandStack.Push(PrefixExpressionBuilder(operandStack, poppedOperator));

                    poppedOperator = operatorStack.Pop();

                    sameOrHighPrecedence = CheckSameOrHighPrecedence(poppedOperator, ch);

                }

                operatorStack.Push(poppedOperator);

                operatorStack.Push(ch);

            }
            else
            {

                operandStack.Push(ch);

            }

            //Debug.Log("operator stack " + PrintArray(operatorStack));
            //Debug.Log("operand stack " + PrintArray(operandStack));

        }

        string fin = operandStack.Pop();
        return (Regex.Replace(fin, @"\s+", " ")).Trim();

    }

    public static string PrintArray(string[] arr)
    {
        string res = "";
        foreach (string tok in arr)
        {
            res = res + " [" + tok + "]";
        }
        return res;
    }

    public static string PrintArray(Stack<string> stk)
    {
        string[] arr = stk.ToArray();
        string res = "";
        foreach(string tok in arr)
        {
            res = res + " [" + tok + "]";
        }
        return res;
    }

    /// <summary>
    /// Validates the infix expression for correctness
    /// </summary>
    /// <param name="expression">Infix expression to be validated</param>
    /// <returns>True if expression is valid</returns>
    private static void ValidateInfixExpression(ref string expression)
    {

        //expression = expression.Replace(" ", string.Empty);
        // Rule 1: '(' and ')' pair
        // Rule 2: Every two operands must have one operator in between

    }

    /// <summary>
    /// Checks if character is a listed operator or not
    /// </summary>
    /// <param name="character">Charaxter to be tested</param>
    /// <returns>False if not otherwise True</returns>
    private bool IsOperator(string character)
    {

        if (operators.ContainsKey(character))
            return true;

        return false;

    }

    /// <summary>
    /// Checks if popped operator has same or higher precedence than Current operator
    /// </summary>
    /// <param name="elementToTest">Popped operator</param>
    /// <param name="checkAgainst">Current operator in the expression</param>
    /// <returns>True if equal or higher precedence</returns>
    private bool CheckSameOrHighPrecedence(string elementToTest, string checkAgainst)
    {

        bool flag = false;

        /*switch (elementToTest)
        {

            case "==":
            case "!=":
            case ">=":
            case "<=":
            case ">":
            case "<":

                flag = true;
                break;

            case "||":
            case "&&":

                if ((checkAgainst == "&&") || (checkAgainst == "||"))
                {

                    flag = true;

                }
                break;

            default: // for any other popped element

                flag = false;
                break;

        }*/

        int pToTest = 0;
        
        int pAgainst = 0;
        if(!operators.TryGetValue(checkAgainst, out pToTest) || !operators.TryGetValue(checkAgainst, out pAgainst))
        {
            throw new Exception("Not an operator.");
        }

        if (pToTest > pAgainst)
        {
            flag = true;
        }

        return flag;

    }

    private static string PrefixExpressionBuilder(Stack<string> operandStack, string operatorChar)
    {

        string operand2 = operandStack.Pop();
        string operand1 = operandStack.Pop();

        string infixExpression = string.Format(" {0} {1} {2} ", operatorChar, operand1, operand2);

        return infixExpression;

    }
}
