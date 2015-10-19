using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Blocks will be stored in this class
/// LineNumber is the line on which this block started
/// Lines contain the individual lines of the block
/// </summary>
public class XBBlock
{
    List<string> lines;
    int lineNumber;

    public XBBlock(int lineNumber)
    {
        lines = new List<string>();
        this.lineNumber = lineNumber;
    }

    public string Replace(int n, string old, string token)
    {
        string line = lines[n];
        return line.Replace(old, token);
    }

    public void AddLine(string line)
    {
        lines.Add(line);
    }

    public string GetLine(int n)
    {
        return lines[n];
    }

    public int LineNumber()
    {
        return lineNumber;
    }

    public int Length()
    {
        return lines.Count;
    }
}
