using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

public class XDebug {
	public static bool ProductionEnabled = false;
	public static bool verboseRemote = false;

	public static void Log(object msg)
	{
		if (!ProductionEnabled) {
			Debug.Log(msg);
			Application.ExternalCall ("ConsoleW", msg + "");
		}
	}

	public static void Verbose(object msg)
	{
		if (!ProductionEnabled && verboseRemote) {
			Debug.Log(msg);
			Application.ExternalCall ("ConsoleW", msg + "");
		}
	}

	public static void Log(string tag, object msg)
	{
		if (!ProductionEnabled) {
			Debug.Log("[" + tag + "] " + msg);
			Application.ExternalCall ("ConsoleW", "[" + tag + "]" + msg);
		}
	}

	public static void Verbose(string tag, object msg)
	{
		if (!ProductionEnabled && verboseRemote) {
			Debug.Log("[" + tag + "] " + msg);
			Application.ExternalCall ("ConsoleW", "[" + tag + "]" + msg);
		}
	}
}
