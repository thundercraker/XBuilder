using UnityEngine;
using System.Collections;

public class RocketBoosterScript : BaseBlockScript, InterpreterInterface {
    public ParticleSystem particles;
    public float BoosterValue;
    double firingTime = -1;
	float LastTick = -1;

	// Use this for initialization
	void Start () {
        //particles.Pause();
        particles.enableEmission = false;
		LastTick = Time.realtimeSinceStartup;
        //StartCoroutine(LaunchAfterDelay(0));
        //StartCoroutine(StopAfterDelay(2));
	}

    public override void UpdateChild(bool powerDrawTick)
    {
		if (powerDrawTick)
			if(!Root.DrawPower(continuousPowerDraw))
				return;

        if (firingTime != -1)
        {
            particles.enableEmission = true;
            UpdateForce();
        }
        else
        {
            particles.enableEmission = false;
        }
    }

    public void UpdateForce()
    {
        //Find Angle
        BlockPhysics.AngleOrientation ao = BlockPhysics.CalculateAngle(Root.gameObject, this.gameObject);

        //Apply Force
        Vector3 up = Root.transform.up;
        up.z = 0;
        //up.Normalize();

        //Calculate the Booster Force into the pivot
        //This is F Sin theta
        Vector3 sint = up * BoosterValue;
        sint = Quaternion.AngleAxis((-1 * ao.sign * ao.angle), Root.gameObject.transform.forward) * sint;
    
        //Debug.Log("Angle Orientation of " + this.name + " angle " + ao.angle + " sign " + ao.sign + " Result Vector: " + sint);

        Root.AddForce(sint);
        //Apply Torque

        Vector3 forward = Root.transform.forward;
        forward.x = 0;
        forward.y = 0;
        //forward.Normalize();
        float torque = BlockPhysics.CalculateTorque(BoosterValue, Root.gameObject, this.gameObject);
        Root.AddTorque(forward * torque);
    }

    IEnumerator LaunchAfterDelay(float time)
    {
        yield return new WaitForSeconds(time);
        EnableEngine();
    }

    IEnumerator StopAfterDelay(float time)
    {
        yield return new WaitForSeconds(time);
        DisableEngine();
    }

    
    IEnumerator ChangeForceAfterDelay(float time, string force)
    {
        yield return new WaitForSeconds(time);
        BoosterValue = float.Parse(force);
        
        DisableEngine();
        EnableEngine();
    }

    public void EnableEngine()
    {
        if (firingTime != -1) return;
        Debug.Log("Engines enabled. Constant Force on " + Root.name);

        firingTime = Time.realtimeSinceStartup;
    }


    public void DisableEngine()
    {
        if (firingTime == -1) return;
        firingTime = -1;
    }

    //Methods for xBuild Interpreter
    public void command(float time, string method, string[] args)
    {
        switch(method)
        {
            case "start":
                StartCoroutine(LaunchAfterDelay(time));
                break;
            case "stop":
                StartCoroutine(StopAfterDelay(time));
                break;
        }
    }

    public void set(float time, string vars, string value)
    {
        switch(vars)
        {
            case "force":
                StartCoroutine(ChangeForceAfterDelay(time, value));
                break;
        }
    }
}
