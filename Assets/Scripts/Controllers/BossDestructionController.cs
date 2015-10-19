using UnityEngine;
using System.Collections;

public class BossDestructionController : DestructibleObjectController {

	BaseGameController GameController;

	public override void DestroyThis()
	{
		base.DestroyThis ();
		GameController.SuccessTrigger ();
	}

}
