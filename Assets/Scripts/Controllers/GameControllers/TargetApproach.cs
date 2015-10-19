using UnityEngine;
using System.Collections;

public class TargetApproach : MonoBehaviour {

    public GameObject GameController;
    public int TargetLevel;
    public bool FailOnTrigger = false;

    private GameControlScript GC;

	// Use this for initialization
	void Start () {
	    GC = GameController.GetComponent<GameControlScript>();
        if(GC == null)
        {
            Debug.Log("Game Controller is NULL.");
        }
        if(!FailOnTrigger)
            GC.RegisterTarget(this.gameObject, GameControlScript.TargetType.Approach);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter(Collision col)
    {
        //if the hitting object is the XBuild register targetapproach success
        Debug.Log("Hit with: " + col.gameObject.name);
        if(InfixPrefix.equal(col.gameObject.tag, "xbuildroot"))
        {
            if(FailOnTrigger)
            {
                //Trigger the failure
                GC.FailTrigger();
            }
            GC.RegisterApproach(this.gameObject);
        }
    }
}
