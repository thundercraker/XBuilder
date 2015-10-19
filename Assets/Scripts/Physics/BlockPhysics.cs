using UnityEngine;
using System.Collections;

public class BlockPhysics {

    public struct AngleOrientation
    {
        public float angle;
        public float sign;
    }

    public static AngleOrientation CalculateAngle(GameObject pivot, GameObject source)
    {
        Vector3 pivotPos = pivot.transform.position;
        Vector3 sourcePos = source.transform.position;

        Vector3 main = pivotPos - sourcePos;
        main.z = 0;

        //Find the angle between main and vector facing upward from source (transform.up)
        float angle = Vector3.Angle(main, source.transform.up);
        float sign = Mathf.Sign(Vector3.Cross(main, source.transform.up).z);

        AngleOrientation ao;
        ao.angle = angle;
        ao.sign = sign;
        return ao;
    }

    public static float CalculateTorque(float Force, GameObject pivot, GameObject source)
    /* The source block is where the force is
     * The pivot block is the one experiencing torque
     */
    {
        Vector3 pivotPos = pivot.transform.position;
        Vector3 sourcePos = source.transform.position;

        Vector3 main = pivotPos - sourcePos;
        main.z = 0;
        
        //Find the angle between main and vector facing upward from source (transform.up)
        float angle = Vector3.Angle(main, source.transform.up);
        float sign = Mathf.Sign(Vector3.Dot(main, source.transform.up));
        float sign2 = Mathf.Sign(Vector3.Cross(main, source.transform.up).z);

        float fAngle = Mathf.Round(Mathf.Abs(angle));

        //Calculate the perpendicular force
        float prepForce = -1 * sign2 * Mathf.Round(Mathf.Cos(fAngle) * Force);
        //Debug.Log(source.name + " main =  " + main + " Angle = " + angle + " Dot =  " + Vector3.Dot(main, source.transform.up) 
        //    + " Cross = " + Vector3.Cross(main, source.transform.up) + "final = " + prepForce + " up " + source.transform.up);

        return prepForce;
    }
	
}
