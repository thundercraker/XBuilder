using UnityEngine;
using System.Collections;

public class BaseBlockScript : MonoBehaviour {
	protected RootScript Root;

	public void SetRoot(RootScript root)
	{
		Root = root;
	}

	public RootScript GetRoot()
	{
		return Root;
	}

	//public defaults
	public float continuousPowerDraw;
	public float onUsePowerDraw;

	void Start (){}
	void Update (){}

	public virtual void UpdateChild(bool powerDrawTick) {}

	public virtual float PowerDrawOnTick ()
	{
		return continuousPowerDraw;
	}

	public virtual float PowerDrawOnUse()
	{
		return onUsePowerDraw;
	}
}
