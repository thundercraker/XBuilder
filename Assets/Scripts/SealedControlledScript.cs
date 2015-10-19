using UnityEngine;
using System.Collections;

public class SealedControlledScript : ControlledScript {
	
	public sealed override void Start()
	{
		/* disallow ControlledScripts from executing anything on start
		 * these scripts can only start by having their RunScript methods called
		 */
	}
	
	public sealed override void Update()
	{
		if (isReady)
			UpdateScript ();
	}
	
	
	public sealed override void Run ()
	{
		RunScript ();
		isReady = true;
	}

	public void DebugLog(string msg)
	{
		Application.ExternalCall ("ConsoleW", msg);
	}
}
