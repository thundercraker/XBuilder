using UnityEngine;
using System.Collections;

public class DataPassingScript : MonoBehaviour {

	public XBuildData XBD;

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad (this.gameObject);
	}

	void Awake() {
		DontDestroyOnLoad (this.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		DontDestroyOnLoad (this.gameObject);
	}
}
