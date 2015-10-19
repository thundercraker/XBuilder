using UnityEngine;
using System.Collections;

public class EnergyVialScript : VialScript {
	protected override void Action(RootScript rs)
	{
		rs.AddPower (Amount);
	}
}
