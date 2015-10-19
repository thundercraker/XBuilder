using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/* Function Implementations
     * 
     * Function frames are defined here
     */

//Function List

//Function Frame Classes
    
class XBFunction : XBBlock
{
    struct XBFVariable
    {
        public string name;
        public string type;
    }
    //key = name, value = type
    List<XBFVariable> variables;
    public string name;

    public XBFunction(Interpreter context, string name, string[] args, int lineNumber)
        : base(lineNumber)
    {
        this.name = name;
        variables = new List<XBFVariable>();
        foreach (string arg in args)
        {
            string[] tokens = arg.Split(' ');
            if (!Interpreter.TYPES.Contains(tokens[0].ToLower()))
            {
                context.ThrowError("[Function (" + name + ") + argument + (" + tokens[1] + ")] Type " + tokens[0] + " not recognized.", lineNumber);
            }
            XBFVariable variable;
            variable.name = tokens[1];
            variable.type = tokens[0];
            variables.Add(variable);
        }
        Debug.Log("Created function frame for function " + name + " @  line " + lineNumber);
    }

    public string GetLineWithParams(string[] pars, int n)
    //pars in form of (value)
    {
        string line = GetLine(n);
        string rline = line;
        int parcnt = 0;
        foreach (string par in pars)
        {
            //par -> value of the parameter
            XBFVariable variable = variables[parcnt];
            string token = variable.name;
            line = line.Replace(token, par);
            Debug.Log("Parameter " + par);
            parcnt++;
        }
        Debug.Log("Returning line : " + line + " for " + rline);
        return line;
    }
}