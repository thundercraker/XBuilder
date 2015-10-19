using UnityEngine;
using System.Collections;

public class ProjectileScript : MonoBehaviour {

	public float MaxBounds = 30f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 pos = gameObject.transform.position;
		if (Mathf.Abs (pos.x) > MaxBounds || Mathf.Abs(pos.y) > MaxBounds) {
			Destroy(gameObject);
		}
	}
}
