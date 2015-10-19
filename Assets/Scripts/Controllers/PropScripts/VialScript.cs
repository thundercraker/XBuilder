using UnityEngine;
using System.Collections;

public class VialScript : MonoBehaviour {

	public float RotationY = 0.1f;
	public float Amount = 10f;
	public float Life = 3f;

	float startingZ;

	// Use this for initialization
	void Start () {
		startingZ = gameObject.transform.position.z;
		Rigidbody rb = gameObject.GetComponent<Rigidbody> ();
		rb.AddTorque (new Vector3 (0, RotationY, 0));
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 cpos = gameObject.transform.position;
		gameObject.transform.position = new Vector3 (cpos.x, cpos.y, startingZ);

		Life -= Time.deltaTime;
		if (Life <= 0) {
			Destroy(gameObject);
		}
	}

	void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.tag.Equals ("xbuildroot")) {
			Action (col.gameObject.GetComponent<RootScript>());
		}
	}

	protected virtual void Action(RootScript rootscript)
	{
		//TODO do nothing, correct implementation in inheriting implementations
	}
}
