using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RadarBlockScript : BaseBlockScript, InterpreterInterface
{
    public float radius;
    public string[] objects;
    
    public void Ping()
    {
        Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, radius);
        objects = (from col in colliders
                   where IsValid(col.gameObject)  
                   orderby Vector3.Distance(gameObject.transform.position, col.gameObject.transform.position)
                   select col.gameObject.name).ToArray();

        //objects = objects.Where(item => IsValid(item)).ToArray();
		//Debug data
		string closest = (objects.Length >= 1) ? objects [0] : "No Objects";
        Debug.Log("Radar Ping [" + gameObject.name + "] Range " + radius + " Colliders " + colliders.Length + " Closest " + closest);
        //objects = tobj.ToArray();
    }

    public bool IsValid(GameObject item)
    {
        return (!item.tag.Equals(BlockObjectScript.XBUILD_BLOCK_TAG, System.StringComparison.OrdinalIgnoreCase)
		        && !item.tag.Equals(BlockObjectScript.XBUILD_ROOT_TAG, System.StringComparison.OrdinalIgnoreCase)
		        && !item.tag.Equals(BlockObjectScript.XBUILD_PROP_TAG, System.StringComparison.OrdinalIgnoreCase));
	}

	IEnumerator PingAfterDelay(float time)
	{
		yield return new WaitForSeconds(time);
		Ping();
	}

	//Methods for xBuild Interpreter
	public void command(float time, string method, string[] args)
	{
		switch(method)
		{
		case "ping":
			StartCoroutine(PingAfterDelay(time));
			break;
		}
	}
	
	public void set(float time, string vars, string value)
	{
		switch(vars)
		{
		default:
			break;
		}
	}
}
