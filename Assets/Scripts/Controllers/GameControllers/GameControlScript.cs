using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameControlScript : BaseGameController {

    List<GameObject> approach_targets;
    List<GameObject> destroy_targets;

    bool init = false;

    public enum TargetType
    {
        Approach = 1,
        Destroy = 2
    }

    void Init()
    {
        approach_targets = new List<GameObject>();
        destroy_targets = new List<GameObject>();
    }

    public void RegisterTarget(GameObject o, TargetType t)
    {
        if(!init)
        {
            Init();
            init = true;
        }

        switch(t)
        {
            case (TargetType.Approach):
                Debug.Log("GameObject: " + o.name + " registered as Appraoch Target.");
                approach_targets.Add(o);
                break;
            case (TargetType.Destroy):
                Debug.Log("GameObject: " + o.name + " registered as Destroy Target.");
                destroy_targets.Add(o);
                break;
        }
    }

    public void RegisterApproach(GameObject o)
    {
        approach_targets.Remove(o);
        CheckSuccess();
    }

    public void RegisterDestroy(GameObject o)
    {
        destroy_targets.Remove(o);
        CheckSuccess();
    }

    private void CheckSuccess()
    {
        if(approach_targets.Count < 1 && destroy_targets.Count < 1)
        {
			base.Success = true;
        }
    }
}
