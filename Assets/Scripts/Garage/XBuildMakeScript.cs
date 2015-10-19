using UnityEngine;
using System.Collections;

public class XBuildMakeScript : SealedControlledScript {

	protected override void RunScript()
	{
		PutBlocks ();
	}

	private void PutBlocks()
	{
		gameObject.GetComponent<BlockObjectScript> ().Run();
	}
}
