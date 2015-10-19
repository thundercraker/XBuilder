using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FirstBossController : MonoBehaviour {
	public float YawForceX = 3f;
	public float YawForceY = 3f;
	public float DampeningForce = 200f;
	public float DampeningAccel = 3000f;
	public float Deceleration = 0.25f;
	public float ZeroVelocityTolerance = 0f;

	public Vector3 OperationalBoundsMax;
	public Vector3 OperationalBoundsMin;

	enum STATE { IDLE = 0, YAW_X = 1, YAW_Y = 2, StateCount }
	int currentState = 0;
	float lastStateChange = 0;
	bool waitForIdle = false;
	bool goodZone = true;

	float LastDischarge = 0;

	List<MissileGeneratorScript> turrets;
	// Use this for initialization
	void Start () {
		turrets = new List<MissileGeneratorScript> ();
		for(int i = 0; i < gameObject.transform.childCount; i++)
		{
			GameObject child = gameObject.transform.GetChild(i).gameObject;
			if(child.tag.Equals("bossturret"))
			{
				turrets.Add(child.GetComponent<MissileGeneratorScript>());
				Debug.Log("Added turret: " + child.gameObject.name);
			}
		}
		LastDischarge = Time.realtimeSinceStartup;
		currentState = RandomState ();
		BeginState ();
	}
	
	// Update is called once per frame
	void Update () {


		if (!InBound (OperationalBoundsMax, OperationalBoundsMin)) {
			if(goodZone)
			{
				goodZone = false;
				Rebound ();
			}
		} else {
			//Debug.Log("In zone: " + gameObject.transform.position);
			goodZone = true;
		}
		if (!waitForIdle) {


			//switch states every 5 seconds
			float time = Time.realtimeSinceStartup;
			if (time > (lastStateChange + 5f)) {
				waitForIdle = true;
			}
		} else {
			DampMovement();
			Rigidbody rb = gameObject.GetComponent<Rigidbody>();
			if(rb.velocity.x  <= (0 + ZeroVelocityTolerance) && (rb.velocity.y <= (0 + ZeroVelocityTolerance)) && goodZone)
			{
				waitForIdle = false;

				//begin new state

				int cstate = currentState;
				//Debug.Log("cstate: " + cstate + " currentState: " + currentState);
				while(cstate == currentState)
				{
					currentState = RandomState();
					//Debug.Log("Find Old: " + cstate + " cand: " + currentState);
				}
				
				BeginState();
			}
		}

		//Debug.Log (LastDischarge + " -- " + Time.realtimeSinceStartup);
		if ((Time.realtimeSinceStartup - LastDischarge) >= 2f) {
			LastDischarge = Time.realtimeSinceStartup;
			FireEverything();
		}
	}

	bool InBound(Vector3 max, Vector3 min)
	{
		Vector3 pos = gameObject.transform.position;
		//Debug.Log (pos);
		if ((pos.x < max.x && pos.y < max.y) && (pos.x > min.x && pos.y > min.y)) {
			//Debug.Log("In");
			return true;
		}
		return false;
	}

	int RandomState() 
	{
		return Random.Range (1, (int) STATE.StateCount);
	}

	void BeginState()
	{
		Debug.Log ("State changed: " + currentState);
		ConstantForce cforce = gameObject.GetComponent<ConstantForce> ();
		Rigidbody rb = gameObject.GetComponent<Rigidbody> ();
		switch (currentState) {
		case (int) STATE.IDLE:
			//cforce.relativeForce = new Vector3(0, 0, 0);
			break;
		case (int) STATE.YAW_X:
			//cforce.relativeForce = new Vector3(YawForceX, 0, 0);
			rb.velocity = new Vector3(YawForceX, 0, 0);
			break;
		case (int) STATE.YAW_Y:
			//cforce.relativeForce = new Vector3(0, YawForceY, 0);
			rb.velocity = new Vector3(0, YawForceY, 0);
			break;
		default:
			break;
		}
		Debug.Log (rb.velocity);
		lastStateChange = Time.realtimeSinceStartup;
	}

	void Rebound()
	{
		//Debug.Log ("Rebound State:" + currentState);
		ConstantForce cforce = gameObject.GetComponent<ConstantForce> ();
		Rigidbody rb = gameObject.GetComponent<Rigidbody> ();
		switch (currentState) {
		case (int) STATE.YAW_X:
			//cforce.relativeForce = new Vector3(-cforce.relativeForce.x, 0, 0);
			rb.velocity = new Vector3(-rb.velocity.x, 0, 0);
			break;
		case (int) STATE.YAW_Y:
			//cforce.relativeForce = new Vector3(0, -cforce.relativeForce.y, 0);
			rb.velocity = new Vector3(0, -rb.velocity.y, 0);
			break;
		default:
			break;
		}
	}

	void DampMovement()
	{
		//CompositeForceDamp();
		CompositeVelocityDamp();
		//Rigidbody rb = gameObject.GetComponent<Rigidbody> ();
		//rb.velocity = new Vector3 (rb.velocity.x - (Deceleration * Time.deltaTime), rb.velocity.y - (Deceleration * Time.deltaTime), 0);
	}

	public void CompositeForceDamp()
	{
		Vector3 cforce = gameObject.GetComponent<ConstantForce>().relativeForce;
		float dampSignX = Mathf.Sign(cforce.x);
		float dampSignY = Mathf.Sign(cforce.y);
		float dampx = (Mathf.Abs(cforce.x) > Mathf.Abs(DampeningForce * Time.deltaTime)) ? cforce.x - (dampSignX * DampeningForce * Time.deltaTime) : 0;
		float dampy = (Mathf.Abs(cforce.y) > Mathf.Abs(DampeningForce * Time.deltaTime)) ? cforce.y - (dampSignY * DampeningForce * Time.deltaTime) : 0;
		gameObject.GetComponent<ConstantForce>().relativeForce = new Vector3(dampx, dampy, 0);
	}

	public void CompositeVelocityDamp()
	{
		Rigidbody rb = gameObject.GetComponent<Rigidbody>();
		Vector3 velocity = rb.velocity;
		float dampSignX = Mathf.Sign(velocity.x);
		float dampSignY = Mathf.Sign(velocity.y);
		float dampx = (Mathf.Abs(velocity.x) > Mathf.Abs(DampeningAccel * Time.deltaTime)) ? velocity.x - (dampSignX * DampeningAccel * Time.deltaTime) : 0;
		float dampy = (Mathf.Abs(velocity.y) > Mathf.Abs(DampeningAccel * Time.deltaTime)) ? velocity.y - (dampSignY * DampeningAccel * Time.deltaTime) : 0;
		
		//Debug.Log("Velocity Damping " + dampx + " " + dampy);
		rb.velocity = new Vector3(dampx, dampy, 0);
	}

	//Weapons Methods
	void FireEverything()
	{
		foreach (MissileGeneratorScript t in turrets) {
			t.Fire();
		}
	}

}