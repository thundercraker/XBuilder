using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class XBStructure
{
    Dictionary<string, string> variables;

    public XBStructure()
    {
        variables = new Dictionary<string, string>();
    }

    public void Set(string key, string value)
    {
        if (variables.ContainsKey(key))
        {
            variables[key] = value;
        }
        else
        {
            variables.Add(key, value);
        }
    }

    public bool TryGetValue(string key, out string value)
    {
        return variables.TryGetValue(key, out value);
    }

	public int GetLength()
	{
		return variables.Count;
	}
}
