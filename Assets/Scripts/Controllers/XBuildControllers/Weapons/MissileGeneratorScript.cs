using UnityEngine;
using System.Collections;

public class MissileGeneratorScript : BaseBlockScript, InterpreterInterface {

    public GameObject prefabMissile;
    public GameObject angleObject;
    public float speed;
    Quaternion quat;

	// Use this for initialization
	void Start () {
        quat = Quaternion.AngleAxis(90, Vector3.left);
        //StartCoroutine(FireAfterDelay(2));
	}

    IEnumerator FireAfterDelay(float time)
    {
        yield return new WaitForSeconds(time);
        Fire();
    }

    IEnumerator ChangeSpeedAfterDelay(float time, string spd)
    {
        yield return new WaitForSeconds(time);

        speed = float.Parse(spd);
    }

    public GameObject Fire()
    {
        GameObject createdMissile = Instantiate(prefabMissile, this.gameObject.transform.position, Quaternion.identity) as GameObject;
        Debug.Log("Created missile @ " + this.gameObject.transform.position);
        Rigidbody rb = createdMissile.GetComponent<Rigidbody>();
        rb.velocity = speed * angleObject.transform.up;
        //createdMissile.transform.direction = this.gameObject.transform.forward;
        return createdMissile;
    }

    //Methods for xBuild Interpreter
    public void command(float time, string method, string[] args)
    {
        switch (method)
        {
            case "fire":
                StartCoroutine(FireAfterDelay(time));
                break;
        }
    }

    public void set(float time, string vars, string value)
    {
        switch (vars)
        {
            case "speed":
                StartCoroutine(ChangeSpeedAfterDelay(time, value));
                break;
        }
    }
}
