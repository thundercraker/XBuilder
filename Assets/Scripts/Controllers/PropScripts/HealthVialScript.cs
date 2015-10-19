using UnityEngine;
using System.Collections;

public class HealthVialScript : VialScript {
	protected override void Action(RootScript rs)
	{
		rs.AddHealth (Amount);
	}
}
