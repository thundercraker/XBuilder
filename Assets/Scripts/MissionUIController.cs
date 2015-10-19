using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MissionUIController : MonoBehaviour {
	public RootScript PlayerXBuild;
	public Image HPBackground;
	public Image HPForeground;
	public Image ENBackground;
	public Image ENForeground;
	public GameObject ConsolePanel;

	float MaxWidth = 1f;
	float HP = 1f;
	float EN = 1f;

	bool animationInProgress = false;
	bool firstload = true;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (PlayerXBuild != null) {
			HP = PlayerXBuild.GetHealth () / PlayerXBuild.MaxHealth;
			EN = PlayerXBuild.GetHealth () / PlayerXBuild.MaxPower;
		}
	}

	void OnGUI() {
		RectTransform hpfg = HPForeground.GetComponent<RectTransform> ();
		RectTransform enfg = ENForeground.GetComponent<RectTransform> ();

		if (firstload) {
			//get the full width of the box
			RectTransform hpbg = HPBackground.GetComponent<RectTransform> ();
			Debug.Log("HP/EN bar size measured: " + hpbg.rect.width + " " + hpbg.rect.height);
			MaxWidth = hpbg.rect.width;
			hpfg.sizeDelta = new Vector2(MaxWidth,0);
			enfg.sizeDelta = new Vector2(MaxWidth,0);
			firstload = false;
		}
		//Debug.Log ("difference: " + (hpfg.rect.width - (MaxWidth * HP)));
		//hpfg.sizeDelta = new Vector2 (hpfg.rect.width - (MaxWidth * HP), 0);
		//enfg.sizeDelta = new Vector2 (MaxWidth - (MaxWidth * EN), 0);

		hpfg.sizeDelta = new Vector2 (-hpfg.rect.width, 0);
		hpfg.sizeDelta = new Vector2 (MaxWidth * HP, 0);
		enfg.sizeDelta = new Vector2 (-enfg.rect.width, 0);
		enfg.sizeDelta = new Vector2 (MaxWidth * EN, 0);
	}

	public void ConsoleToggle()
	{
		ConsolePanel.SetActive (!ConsolePanel.activeSelf);
	}
}
