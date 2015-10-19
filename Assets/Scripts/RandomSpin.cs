using UnityEngine;
using System.Collections;

public class RandomSpin : MonoBehaviour {
	public bool isSelected = false;
	public float RotationDelta = 20f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (isSelected) {
			gameObject.transform.Rotate(Vector3.forward * Time.deltaTime * RotationDelta);
		}
	}
}
