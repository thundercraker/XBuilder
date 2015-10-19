using UnityEngine;
using System.Collections;

public class BackToGarageScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void BackToGarage()
	{
		Application.LoadLevel ("Garage");
	}
}
