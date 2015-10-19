using UnityEngine;
using System.Collections;

public class DestructibleObjectController : MonoBehaviour {
	public int HitPoints = 100;
	public int Damage = 25;

	// Use this for initialization
	public virtual void Start () {
	
	}
	
	// Update is called once per frame
	public virtual void Update () {
		if (HitPoints <= 0) {
			DestroyThis();
		}
	}

	public virtual void DestroyThis()
	{
		Destroy (this.gameObject);
	}

	void OnCollisionEnter(Collision col)
	{
		DestructibleObjectController doc = col.gameObject.GetComponent<DestructibleObjectController> ();
		if (doc != null) {
			CollisionDamage(col.gameObject);
		} else {
			//TODO handle collision with non destructible object
		}
	}

	public virtual int CollisionDamage(GameObject foreign)
	{
		//TODO: Damage based on weight
		if (foreign.tag.Equals ("missile"))
			return 20;
		return 10;
	}
}
