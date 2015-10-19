using UnityEngine;
using System.Collections;

public class GarageObjectScript : MonoBehaviour {

	bool isSelected;
	bool CustomName;
	GarageBlock garageBlock;

	public BlockObjectScript.BlockType Type;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 currentPos = this.gameObject.transform.position;
		transform.position = new Vector3(Mathf.Round(currentPos.x),
		                             Mathf.Round(currentPos.y),
		                             Mathf.Round(currentPos.z));
	}

	public GarageBlock GetGarageBlock()
	{
		return (new GarageBlock(gameObject.transform.position, gameObject.name, Type));
	}

	public bool SetName(string name)
	{
		if (!GameObject.Find (name)) {
			gameObject.name = name;
			return true;
		} else
			return false;
	}
}
