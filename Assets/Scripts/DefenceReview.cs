using UnityEngine;
using System.Collections;

public class DefenceReview : MonoBehaviour {
	float delta = 10f;
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		this.gameObject.transform.Rotate (new Vector3 (delta * Time.deltaTime, delta * Time.deltaTime, delta * Time.deltaTime));
	}	
}
