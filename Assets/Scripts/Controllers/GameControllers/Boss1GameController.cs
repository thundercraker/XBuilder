using UnityEngine;
using System.Collections;

public class Boss1GameController : BaseGameController {
	public GameObject HealthVialPrefab;
	public GameObject EnergyVialPrefab;

	public Vector2 VialGenerationMax;
	public Vector2 VialGenerationMin;
	float LastTick = -1;

	void Update()
	{
		if ((Time.realtimeSinceStartup - LastTick) >= 1f) {
			int seed = Random.Range(0, 10);
			if(seed > 8)
			{
				int innerSeed = Random.Range(0, 2);
				GameObject gen = null;
				if(innerSeed == 0)
					gen = HealthVialPrefab;
				else
					gen = EnergyVialPrefab;

				Vector3 position = new Vector3(Random.Range(VialGenerationMin.x, VialGenerationMax.x), 
				                               Random.Range(VialGenerationMin.y, VialGenerationMax.y),
				                               0);

				Instantiate(gen, position, Quaternion.identity);
			}
			LastTick = Time.realtimeSinceStartup;
		}
	}
}
