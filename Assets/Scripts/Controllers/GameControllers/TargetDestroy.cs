using UnityEngine;
using System.Collections;

public class TargetDestroy : MonoBehaviour
{

    public GameObject GameController;
    public int TargetLevel;
    public bool FailOnTrigger = false;

    private GameControlScript GC;

    // Use this for initialization
    void Start()
    {
        GC = GameController.GetComponent<GameControlScript>();
        if (GC == null)
        {
            Debug.Log("Game Controller is NULL.");
        }
        if (!FailOnTrigger)
            GC.RegisterTarget(this.gameObject, GameControlScript.TargetType.Destroy);
    }

    void OnDestroy()
    {
        //if the hitting object is the XBuild register targetapproach success
        if (FailOnTrigger)
            GC.FailTrigger();
        GC.RegisterDestroy(this.gameObject);
    }
}
