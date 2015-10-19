using UnityEngine;
using System.Collections;

public class ControlledScript : MonoBehaviour {

	public virtual void Start(){}

	public virtual void Update(){}


	public virtual void Run ()
	{
		RunScript ();
		isReady = true;
	}
	
	protected bool isReady = false;
	protected virtual void RunScript(){}
	protected virtual void UpdateScript(){}
}
