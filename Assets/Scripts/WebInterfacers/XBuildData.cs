using UnityEngine;
using System;
using System.Collections;
//using System.Web.Script.Serialization;

public class XBuildData {

    public Vector2 center;
    public int[,] types;
    public string[,] names;
	public string name;
	public string date;
	public string image;
	public string startCode;
	public string updateCode;
	public string db_id;

	public XBuildData(int[,] tm, string[,] nm, Vector2 cen)
	{
		types = tm;
		names = nm;
		center = cen;
	}

	public void PrintXBuildData()
	{
		Debug.Log("");
		Debug.Log("Printing XBuild Data");
		Debug.Log("Type Matrix");
		PrintMatrix (types, 2);
		Debug.Log("Name Matrix");
		PrintMatrix (names, 10);
		Debug.Log("Center");
		Debug.Log(center);
		Debug.Log("");
	}

	void PrintMatrix(int[,] matrix, int space)
	{
		Debug.Log ("Printing Matrix..." + matrix.GetLength(1) + "x" + matrix.GetLength(0));
		string whole = "";
		for (int y = 0; y < matrix.GetLength(0); y++) {
			string line = "";
			for (int x = 0; x < matrix.GetLength(1); x++) {
				line = line + "<" + Buffer(matrix[y,x] + "", space) + ">";
			}
			Debug.Log(line);
			whole = whole + line + Environment.NewLine;
		}
		//Debug.Log (whole);
	}

	void PrintMatrix(string[,] matrix, int space)
	{
		Debug.Log ("Printing Matrix..." + matrix.GetLength(1) + "x" + matrix.GetLength(0));
		string whole = "";
		for (int y = 0; y < matrix.GetLength(0); y++) {
			string line = "";
			for (int x = 0; x < matrix.GetLength(1); x++) {
				line = line + "<" + Buffer(matrix[y,x] + "", space) + ">";
			}
			Debug.Log(line);
			whole = whole + line + Environment.NewLine;
		}
		//Debug.Log (whole);
	}

	string Buffer(string s, int space)
	{
		while (s.Length < space) {
			s = s + " ";
		}
		return s.Substring (0, space);
	}
    /*
	public XBuildData(string rawJson)
    {
        dynamic result = JsonValue.Parse(rawJson);
        XBS.center = result.response.center;
        XBS.data = result.response.data;
        XBS.name = result.response.name;
    }

    public string GetJSON()
    {
        JavaScriptSerializer jss = new JavaScriptSerializer();
        return jss.Serialize(this);
    }
     * */
}
