using UnityEngine;
using System.Collections;

public class GarageMainUIController : SealedControlledScript {
	public GarageMainController GMC;

	public GameObject blockBuilderCanvas;
	public GameObject blockBuilderPanel;
	
	public GameObject NameInput;
	
	public GameObject programmingCanvas;
	public GameObject programmingTextField;

	protected override void RunScript()
	{
		NameInput.SetActive (false);
	}
}
