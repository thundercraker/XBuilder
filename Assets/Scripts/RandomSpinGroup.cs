using UnityEngine;
using System.Collections;

public class RandomSpinGroup : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GameObject[] spinCandidates = GameObject.FindGameObjectsWithTag ("spincandidates");
		int seed = Random.Range (0, spinCandidates.Length);
		GameObject selectedCandidate = spinCandidates [seed];
		RandomSpin rs = selectedCandidate.GetComponent<RandomSpin> ();
		rs.isSelected = true;
		selectedCandidate.transform.Rotate(Vector3.forward * Time.deltaTime);

		//register as a target
		GameControlScript gcs = gameObject.GetComponent<GameControlScript> ();
		gcs.RegisterTarget (selectedCandidate, GameControlScript.TargetType.Destroy);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
