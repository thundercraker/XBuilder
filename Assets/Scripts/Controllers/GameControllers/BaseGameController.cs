using UnityEngine;
using System.Collections;

public class BaseGameController : MonoBehaviour {

	//success screen
	public GameObject successOverlay;
	public GameObject failOverlay;

	protected bool Success = false;
	protected bool Fail = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (!Success || !Fail) {
			CheckSuccess ();
			CheckFailure ();
			if(Success) SuccessTrigger();
			else if(Fail) FailTrigger();
		}
	}

	protected virtual void CheckSuccess(){}
	protected virtual void CheckFailure(){}
	
	public void SuccessTrigger()
	{
		successOverlay.active = true;
	}
	
	public void FailTrigger()
	{
		failOverlay.active = true;
	}
}
