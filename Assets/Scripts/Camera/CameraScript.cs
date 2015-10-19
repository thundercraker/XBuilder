using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {

    public GameObject mainCamera;
    Vector3 maintain;
    public float smoothTime = 0.3f;
    public float velocityX = 0.0f;
    public float velocityY = 0.0f;
    public int cameraZDist = -25;

	// Use this for initialization
	void Start () {
        maintain = mainCamera.gameObject.transform.position - this.gameObject.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 cameraPos = mainCamera.transform.position;
        Vector3 needPos = this.gameObject.transform.position;

        float newPosX = Mathf.SmoothDamp(cameraPos.x, needPos.x, ref velocityX, smoothTime);
        float newPosY = Mathf.SmoothDamp(cameraPos.y, needPos.y, ref velocityY, smoothTime);

        mainCamera.transform.position = new Vector3(newPosX, newPosY, cameraZDist);
         
	}
}
