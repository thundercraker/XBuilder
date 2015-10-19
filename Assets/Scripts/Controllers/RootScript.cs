using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RootScript : MonoBehaviour {

	// Use this for initialization
	public void Init()
	{
		Debug.Log ("Initializing Root Script");
		ForceList = new List<Vector3> ();
		TorqueList = new List<Vector3> ();
		childBlockList = new List<BaseBlockScript> ();
		LastTick = Time.realtimeSinceStartup;
		
		TotalPower = MaxPower;
		TotalHealth = MaxHealth;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		bool powerDrawTick = false;
		if ((Time.realtimeSinceStartup - LastTick) >= 1f) {
			powerDrawTick = true;
			LastTick = Time.realtimeSinceStartup;
		}

		UpdateChildren(powerDrawTick);

        //now begin updating self
        if (ForceList.Count > 0)
        {
            gameObject.GetComponent<ConstantForce>().relativeForce = CompoundForce();// *Time.deltaTime;
            //Debug.Log("Compound Force " + CompoundForce());

        }
        else
        {
            //Debug.Log("Clamping Force");
            CompositeForceDamp();
            CompositeVelocityDamp();
        }
        if (TorqueList.Count > 0)
        {
            gameObject.GetComponent<ConstantForce>().relativeTorque = CompoundTorque();// *Time.deltaTime;
            //Debug.Log("Compound Torque " + CompoundTorque());
        }
        else
        {
            //Debug.Log("Clamping Torque");
            CompositeTorqueDamp();
            CompositeAngularVelocityDamp();
        }

        ForceList.Clear();
        TorqueList.Clear();
	}

    public void Update()
    {
		if (TotalHealth <= 0)
			DestroyThis ();

        Vector3 pos = gameObject.transform.position;
        pos.z = 0;
        gameObject.transform.position = pos;

        //Look at updates
        if (LookAtTransform!=null)
        {
            /*var rotation = Quaternion.LookRotation(LookAtTransform.position - gameObject.transform.position, 
			                                       transform.TransformDirection(Vector3.up));
            gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, rotation, Time.deltaTime * DampeningTorque);
			*/

			Vector3 normalTarget = (LookAtTransform.position-transform.position).normalized;
			float angle = Mathf.Atan2(normalTarget.y,normalTarget.x)*Mathf.Rad2Deg;
			// rotate to angle
			Quaternion rotation = new Quaternion ();
			rotation.eulerAngles = new Vector3(0,0,angle-90);
			//transform.rotation = rotation;
			gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, rotation, Time.deltaTime);
		}
    }

	public void DestroyThis()
	{
		Debug.Log ("Triggered destruction of Root Object");
		GameController.FailTrigger ();
	}

    /* Manage the manual execution of updates on the children
     * this will allow the root object to have its update at the end
     * when children have passed along various updates
     */
	public BaseGameController GameController;
	List<BaseBlockScript> childBlockList;
    List<Vector3> ForceList;
    List<Vector3> TorqueList;
    public float DampeningForce = 20f;
    public float DampeningTorque = 20f;
    public float DampeningAccel = 3f;
    public Transform LookAtTransform = null;
	public float MaxPower = 100f;
	public float MaxHealth = 100f;
	float TotalPower = 100f;
	float TotalHealth = 100f;
	float LastTick = -1;

	public void AddChildBlock(BaseBlockScript child)
	{
		Debug.Log (child.gameObject.name);
		childBlockList.Add (child);
	}

    public void UpdateChildren(bool powerDrawTick)
    {
		if (childBlockList != null) {
			//Debug.Log ("Updating children... " + updateList.Count);
			foreach (BaseBlockScript child in childBlockList) {
				child.UpdateChild (powerDrawTick);
				if(TotalPower <= 0)
				{
					Debug.Log("Run out of power, final updated child: " + child.name);
					break;
				}
			}
		}
    }

	//Allow children to calculate the power draw and deactivate if there is no power
	public bool DrawPower(float powerDraw)
	{
		TotalPower -= powerDraw;
		if (TotalPower <= 0)
			return false;
		return true;
	}

	public void AddPower(float power)
	{
		TotalPower = ((TotalPower + power) <= MaxPower) ? TotalPower + power : MaxPower;
	}

	public void AddHealth(float health)
	{
		TotalHealth = ((TotalHealth + health) <= MaxHealth) ? TotalHealth + health : MaxHealth;
	}

	//allow read of health and power
	public float GetHealth()
	{
		return TotalHealth;
	}

	public float GetPower()
	{
		return TotalPower;
	}

    //Allow children to add a force or torque vector to be queued
    //and calculated for this frame
    public void AddForce(Vector3 force)
    {
        ForceList.Add(force);
    }

    public void AddTorque(Vector3 torque)
    {
        TorqueList.Add(torque);
    }

    public Vector3 CompoundForce()
    {
        Vector3 compound = new Vector3(0, 0, 0);
        foreach(Vector3 force in ForceList)
        {
            compound = compound + force;
        }
        compound.z = 0;
        return compound;
    }

    public Vector3 CompoundTorque()
    {
        Vector3 compound = new Vector3(0, 0, 0);
        foreach(Vector3 torque in TorqueList)
        {
            compound = compound + torque;
        }
        compound.x = 0;
        compound.y = 0;
        return compound;
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

    public void CompositeTorqueDamp()
    {
        Vector3 ctorque = gameObject.GetComponent<ConstantForce>().relativeTorque;
        float dampSignZ = Mathf.Sign(ctorque.z);
        float dampz = (Mathf.Abs(ctorque.z) > Mathf.Abs(DampeningForce * Time.deltaTime)) ? ctorque.z - (dampSignZ * DampeningForce * Time.deltaTime) : 0;
        gameObject.GetComponent<ConstantForce>().relativeTorque = new Vector3(0, 0, dampz);

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

    public void CompositeAngularVelocityDamp()
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        Vector3 velocity = rb.angularVelocity;
        float dampSignZ = Mathf.Sign(velocity.z);
        float dampz = (Mathf.Abs(velocity.z) > Mathf.Abs(DampeningAccel * Time.deltaTime)) ? velocity.z - (dampSignZ * DampeningAccel * Time.deltaTime) : 0;

        //Debug.Log("Angular Velocity Damping " + dampz);
        rb.angularVelocity = new Vector3(0, 0, dampz);
    }
}
