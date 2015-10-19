using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class GarageMainController : MonoBehaviour {
	public static string GARAGE_MAIN_CONTROLLER_TAG = "GarageMainController";
	public static string BUILD_STR = "1.0.0.2";
	enum Context { Main, Directions, Colors };
	Context context;

	enum Directions { Up, Down, Left, Right, None };
	Directions SelectedDirection = Directions.None;

	public enum PersistentMode { None, Draw, Erase }
	PersistentMode CurrentMode;

	enum Blocks { None, Basic, Rocket, Radar, Missile, Shield };
	enum ButtonId { GRID0, GRID1, GRID2, 
		GRID3, GRID4, GRID5, 
		GRID6, GRID7, GRID8 };

	string[] MainButtons = new string[] 
	{ "Basic", "Rocket", "Missile",
		"Radar", "None", "None",
		"None", "None", "None" };

	string[] DirectionButtons = new string[]
	{
		"", "Up", "",
		"Left", "", "Right",
		"", "Down", ""
	};

	int gridpage = 0;
	int objects = 0;

	Blocks SelectedBlock = Blocks.None;

	DatabaseAdapter databaseAdapter;
	XBuildMakeScript xbuildMakeScript;
	XBuildParseScript xbuildParseScript;

	public Text DebugText;

	public GameObject camera;
	public GameObject selectionMenuCanvas;
	public GameObject infoPanel;
	public GameObject selectionContentPanel;
	public GameObject selectionItemPrefab;
	public GameObject deleteConfirmPanel;
	public GameObject missionSelection;

	public GameObject blockBuilderCanvas;
	public GameObject blockBuilderPanel;
	public GameObject NameInput;
	public GameObject cModeExitPanel;

	public GameObject programmingCanvas;
	public InputField programmingStartTextField;
	public InputField programmingUpdateTextField;

	public GameObject BasicBlock;
	public GameObject RocketBlock;
	public GameObject MissileBlock;
	public GameObject RadarBlock;

	public DataPassingScript dataPassingObject;

	public float smoothFactor = 1.0f;

	Vector3 cameraTo;

	GarageObjectScript FocusedGarageBlock;

	public void OnLevelWasLoaded(int level)
	{
		XDebug.Log (level + " was loaded");
		if (level == 0) {
			Application.ExternalCall("AskInfo", "");
		}
	}

	// Use this for initialization
	void Start () {
		XDebug.Log ("Starting the Garage.. Build[" + BUILD_STR + "]");
		ContextSwitch (Context.Main);
		NameInput.SetActive (false);
		cModeExitPanel.SetActive (false);
		cameraTo = camera.transform.position;

		//called Controlled Scripts
		databaseAdapter = this.gameObject.GetComponent<DatabaseAdapter> ();
		xbuildMakeScript = this.gameObject.GetComponent<XBuildMakeScript> ();
		xbuildParseScript = this.gameObject.GetComponent<XBuildParseScript> ();

		//databaseAdapter.Run ();
		//xbuildMakeScript.Run ();

		//Activate Canvases
		XDebug.Log ("De/Activating canvases...");
		ModeToSelection ();

		databaseAdapter.Run ();

		//Loads a pre-defined XBuild from JSON data for debuging purposes
		//comment out in deployment builds
		LoadSelectionScreen (DatabaseAdapter.TEST_JSON);
	}

	// Update is called once per frame
	void Update () {

		if (Input.GetMouseButtonDown (0)) {
			//left click

			Vector3 _MP = Input.mousePosition;
			_MP.z = camera.transform.position.z * -1;
			//XDebug.Log ("Dir: " + SelectedDirection);
			if(CurrentMode == PersistentMode.Draw)
			{
				if(SelectedBlock!=Blocks.None && SelectedDirection != Directions.None)
				{
					//see if block generation is happening over the buttons
					//Debug.Log (EventSystem.current.currentSelectedGameObject);
					GameObject current = EventSystem.current.currentSelectedGameObject;

					if(current == null || (current != cModeExitPanel && current.transform.parent.gameObject != cModeExitPanel))
					{
						GameObject newBlock = null;
						switch(SelectedBlock)
						{
						case Blocks.Basic:
							//drop a block
							newBlock = Instantiate(BasicBlock) as GameObject;
							break;
						case Blocks.Rocket:
							//drop a block
							newBlock = Instantiate(RocketBlock) as GameObject;
							break;
						case Blocks.Missile:
							//drop a block
							newBlock = Instantiate(MissileBlock) as GameObject;
							break;
						case Blocks.Radar:
							//drop a block
							newBlock = Instantiate(RadarBlock) as GameObject;
							break;
						default:
							break;
						}
						if(newBlock != null)
						{
							newBlock.transform.position = Camera.main.ScreenToWorldPoint(_MP);
							if(newBlock.name.Contains("(Clone)"))
								newBlock.name = newBlock.name.Replace("(Clone)", "");
							//XDebug.Log("Name " + newBlock.name);
							newBlock.name = newBlock.name + objects++;
						}
					}
				}
			} else if(CurrentMode == PersistentMode.None) {
				if(SelectedBlock!=Blocks.None && SelectedDirection == Directions.None) {

				} else if(SelectedBlock==Blocks.None && SelectedDirection == Directions.None) {
					RaycastHit hit = new RaycastHit();
					Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
					if (Physics.Raycast (ray, out hit)){
						//select.tag = "none";
						//hit.collider.transform.tag = "select";
						GarageObjectScript gos = hit.transform.gameObject.GetComponent<GarageObjectScript>();
						if(gos != null)
						{
							FocusedGarageBlock = gos;
							XDebug.Log("Click on: " + gos.Type);
							if(gos.Type != BlockObjectScript.BlockType.NormalRed || gos.Type != BlockObjectScript.BlockType.NormalBlue)
							{
								InputField inf = NameInput.GetComponent<InputField>();
								inf.text = gos.name;
								inf.placeholder.GetComponent<Text>().text = gos.name; 
								NameInput.SetActive(true);
							} else {
								NameInput.SetActive(false);
							}
						}
							
					}
				}
			} else if (CurrentMode == PersistentMode.Erase)
			{
				RaycastHit hit = new RaycastHit ();
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				if (Physics.Raycast (ray, out hit)) {
					//select.tag = "none";
					//hit.collider.transform.tag = "select";
					GarageObjectScript gos = hit.transform.gameObject.GetComponent<GarageObjectScript> ();
					if (gos != null) {
						XDebug.Log ("Click on: " + gos.Type);
						Destroy (hit.collider.gameObject);
						//Lose focus of it
						LoseFocusedGarageBlock();
					}
				}
			}
		}

		if (Input.GetMouseButton (1)) {
			if (SelectedBlock != Blocks.None && SelectedDirection != Directions.None) {
				SelectedBlock = Blocks.None;
				SelectedDirection = Directions.None;
				ContextSwitch (Context.Main);
				
				GameObject panel = blockBuilderCanvas.transform.FindChild("Panel").gameObject;
				panel.SetActive (true);
			} else if (SelectedBlock != Blocks.None && SelectedDirection == Directions.None) {
				SelectedBlock = Blocks.None;
				SelectedDirection = Directions.None;
				ContextSwitch (Context.Main);
				
				GameObject panel = blockBuilderCanvas.transform.FindChild("Panel").gameObject;
				panel.SetActive (true);
			} else if (SelectedBlock == Blocks.None && SelectedDirection == Directions.None) {
				RaycastHit hit = new RaycastHit ();
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				if (Physics.Raycast (ray, out hit)) {
					//select.tag = "none";
					//hit.collider.transform.tag = "select";
					GarageObjectScript gos = hit.transform.gameObject.GetComponent<GarageObjectScript> ();
					if (gos != null) {
						XDebug.Log ("Click on: " + gos.Type);
						Destroy (hit.collider.gameObject);
						//Lose focus of it
						LoseFocusedGarageBlock();
					}
				}
			}
		}

		if(Input.GetKey(KeyCode.UpArrow) && !programmingCanvas.activeSelf){
			cameraTo = camera.transform.position;
			cameraTo.y = cameraTo.y + 5;
			XDebug.Log("up: " + cameraTo);
			//camera.transform.position = pos;
			camera.transform.position = Vector3.Lerp (camera.transform.position, cameraTo, Time.deltaTime * smoothFactor);
		}

		if(Input.GetKey(KeyCode.DownArrow) && !programmingCanvas.activeSelf){
			cameraTo = camera.transform.position;
			cameraTo.y = cameraTo.y - 5;
			XDebug.Log("up: " + cameraTo);
			camera.transform.position = Vector3.Lerp (camera.transform.position, cameraTo, Time.deltaTime * smoothFactor);
		}

		if(Input.GetKey(KeyCode.LeftArrow) && !programmingCanvas.activeSelf){
			cameraTo = camera.transform.position;
			cameraTo.x = cameraTo.x - 5;
			XDebug.Log("up: " + cameraTo);
			camera.transform.position = Vector3.Lerp (camera.transform.position, cameraTo, Time.deltaTime * smoothFactor);
		}

		if(Input.GetKey(KeyCode.RightArrow) && !programmingCanvas.activeSelf){
			cameraTo = camera.transform.position;
			cameraTo.x = cameraTo.x + 5;
			XDebug.Log("up: " + cameraTo);
			camera.transform.position = Vector3.Lerp (camera.transform.position, cameraTo, Time.deltaTime * smoothFactor);
		}
	}
		
	public void ModeToSelection()
	{
		selectionMenuCanvas.SetActive (true);
		programmingCanvas.SetActive (false);
		blockBuilderCanvas.SetActive (false);
	}

	public void ModeToBlockBuilder()
	{
		selectionMenuCanvas.SetActive (false);
		programmingCanvas.SetActive (false);
		blockBuilderCanvas.SetActive (true);
	}

	public void ModeToProgramming()
	{
		selectionMenuCanvas.SetActive (false);
		programmingCanvas.SetActive (true);
		blockBuilderCanvas.SetActive (false);
	}

	//SELECTION SCREEN METHODS
	List<XBuildData> ALL_XBUILDS;
	int SELECTED_XBUILD = -1;

	public void LoadSelectionScreen(string json)
	{
		if (json == null || json == "") {
			XDebug.Log(GARAGE_MAIN_CONTROLLER_TAG, "Empty json input received");
			return;
		}
		XDebug.Log (GARAGE_MAIN_CONTROLLER_TAG, "Received json of length " + json.Length);
		ALL_XBUILDS = databaseAdapter.LoadXBuilds (json);
		int index = 0;
		foreach (XBuildData xbuild in ALL_XBUILDS) {
			GameObject selectionItem = Instantiate(selectionItemPrefab) as GameObject;
			Text nameText = selectionItem.transform.GetChild(0).gameObject.GetComponent<Text>();
			nameText.text = xbuild.name;
			Text DateText = selectionItem.transform.GetChild(1).gameObject.GetComponent<Text>();
			DateText.text = xbuild.date;

			Button button = selectionItem.GetComponent<Button>();
			button.name = "" + index++;
			button.onClick.AddListener(delegate{ChangeSelected(button);});

			selectionItem.transform.SetParent(selectionContentPanel.transform, false);
		}
	}

	public void ChangeSelected(Button button)
	{
		Text changeName = infoPanel.transform.GetChild (0).gameObject.GetComponent<Text> ();
		int index = int.Parse(button.name);
		XDebug.Log (GARAGE_MAIN_CONTROLLER_TAG, "Selected: " + index);
		changeName.text = ALL_XBUILDS[index].name;

		string base64_img = ALL_XBUILDS [index].image;

		if (base64_img != null) {
			byte[] base64_bytes = System.Convert.FromBase64String (base64_img);
			Texture2D xbuilder_img = new Texture2D(1,1);
			xbuilder_img.LoadImage(base64_bytes);

			Image preview = infoPanel.transform.FindChild("XBuilderPreviewImage").gameObject.GetComponent<Image>();
			preview.sprite = Sprite.Create(xbuilder_img, new Rect(0,0,xbuilder_img.width,xbuilder_img.height), new Vector2(0.5f, 0.5f));
		}

		SELECTED_XBUILD = index;
		XDebug.Log ("Change to index.. " + index);
	}


	public void LoadXBuild()
	{
		if (SELECTED_XBUILD > -1) {
			dataPassingObject.XBD = ALL_XBUILDS[SELECTED_XBUILD];
			xbuildMakeScript.Run();

			programmingStartTextField.text = dataPassingObject.XBD.startCode + "";
			//XDebug.Log ("Set text to " + programmingStartTextField.text);
			programmingUpdateTextField.text = dataPassingObject.XBD.updateCode + "";

			//set the selected mission or default

			ModeToBlockBuilder();
		}
	}

	public void DeleteXBuild()
	{
		deleteConfirmPanel.SetActive (true);
	}

	public void DeleteXBuildConfirm()
	{
		if (SELECTED_XBUILD > -1) {
			databaseAdapter.DeleteXBuild (ALL_XBUILDS [SELECTED_XBUILD].db_id);
			deleteConfirmPanel.SetActive(false);
		}
	}

	public void DeleteXBuildDismiss()
	{
		deleteConfirmPanel.SetActive (false);
	}

	public void GridButtonPressed(int id)
	{
		switch (id) {
		case (int)ButtonId.GRID0:
			if(context == Context.Main)
			{
				ShowDirections(Blocks.Basic, Context.Directions);
			}
			break;
		case (int)ButtonId.GRID1:
			if(context == Context.Main)
			{
				ShowDirections(Blocks.Rocket, Context.Directions);
			} else if(context == Context.Directions) {
				DrawMode(Directions.Up);
			}
			break;
		case (int)ButtonId.GRID2:
			if(context == Context.Main)
			{
				ShowDirections(Blocks.Missile, Context.Directions);
			}
			break;
		case (int)ButtonId.GRID3:
			if(context == Context.Main)
			{
				ShowDirections(Blocks.Radar, Context.Directions);
			} else if(context == Context.Directions) {
				DrawMode(Directions.Left);
			}
			break;
		case (int)ButtonId.GRID5:
			if(context == Context.Main)
			{
				ShowDirections(Blocks.None, Context.Directions);
			} else if(context == Context.Directions) {
				DrawMode(Directions.Right);
			}
			break;
		case (int)ButtonId.GRID7:
			if(context == Context.Main)
			{
				ShowDirections(Blocks.None, Context.Directions);
			} else if(context == Context.Directions) {
				DrawMode(Directions.Down);
			}
			break;
		default:
			break;
		}
	}

	void ShowDirections(Blocks type, Context switchTo)
	{
		SelectedBlock = type;
		ContextSwitch (switchTo);
	}

	void DrawMode(Directions d)
	{
		CurrentMode = PersistentMode.Draw;
		GameObject panel = blockBuilderCanvas.transform.FindChild("Panel").gameObject;
		panel.SetActive (false);
		XDebug.Verbose ("Dires: " + d);
		SelectedDirection = d;

		cModeExitPanel.SetActive (true);
	}

	public void EraseMode()
	{
		CurrentMode = PersistentMode.Erase;	
		GameObject panel = blockBuilderCanvas.transform.FindChild("Panel").gameObject;
		panel.SetActive (false);

		DragAndDropScript dads = this.gameObject.GetComponent<DragAndDropScript> ();
		dads.isActive = false;

		cModeExitPanel.SetActive (true);
	}

	public void ExitMode()
	{
		if (CurrentMode == PersistentMode.Draw) {
			SelectedBlock = Blocks.None;
			SelectedDirection = Directions.None;
			ContextSwitch (Context.Main);
		} else {
			DragAndDropScript dads = this.gameObject.GetComponent<DragAndDropScript> ();
			dads.isActive = false;
		}

		CurrentMode = PersistentMode.None;

		GameObject panel = blockBuilderCanvas.transform.FindChild ("Panel").gameObject;
		panel.SetActive (true);
		cModeExitPanel.SetActive (false);
	}

	void ContextSwitch(Context switchTo)
	{
		context = switchTo;
		if (context == Context.Main) {
			ApplyButtonArray(MainButtons);
		} else if (context == Context.Directions) {
			ApplyButtonArray(DirectionButtons);
		} else {
			//
		}
	}

	void ApplyButtonArray(string[] buttonsArray)
	{
		string tok1 = "Grid";
		int i = 0;
		GameObject gridButton = null;
		for (; i < buttonsArray.Length; i++) {
			gridButton = blockBuilderPanel.transform.FindChild(tok1 + i).gameObject;
			Text buttonText = gridButton.transform.FindChild("Text").GetComponent<Text>();
			buttonText.text = buttonsArray[i];
		}
		//XDebug.Log (i);
		while (true) {
			Transform test = null;
			gridButton = ((test = blockBuilderPanel.transform.FindChild(tok1 + i)) != null) ? test.gameObject : null;
			if(gridButton == null) break;
			Text buttonText = gridButton.transform.FindChild("Text").GetComponent<Text>();
			buttonText.text = "";
			i++;
		}
	}

	public void UpdateBlockName()
	{
		string name = NameInput.transform.FindChild ("Input").gameObject.GetComponent<Text> ().text;
		Text status = NameInput.transform.FindChild ("Status").gameObject.GetComponent<Text>();
		if (FocusedGarageBlock.SetName (name)) {
			status.color = Color.green;
			status.text = "Name set to " + name + " successfully.";
		} else {
			status.color = Color.red;
			status.text = name + " is already in use.";
		}
	}

	public void LoseFocusedGarageBlock()
	{
		InputField inf = NameInput.GetComponent<InputField>();
		inf.text = "";
		inf.placeholder.GetComponent<Text>().text = ""; 
		NameInput.SetActive(false);
		FocusedGarageBlock = null;
	}
	
	public bool IsSamePosition(Vector3 a, Vector3 b){
		return (a.x == b.x && a.y == b.y && a.z == b.z);
	}

	public void ParseAndSaveXBuild(bool load)
	{
		XDebug.Log ("Begining save coroutine...");
		XDebug.Log ("Test","Begining save coroutine...");

		if (!load)
			StartCoroutine ("ParseScreenShotAndSave");
		else
			StartCoroutine ("ParseScreenShotSaveAndLoad");
	}

	public IEnumerator ParseScreenShotAndSave()
	{
		yield return new WaitForEndOfFrame();

		XDebug.Log ("[Test tag]", "Taking screenshot...");
		//close all canvases
		bool SMC = selectionMenuCanvas.activeSelf;
		bool BBC = blockBuilderCanvas.activeSelf;
		bool PUC = programmingCanvas.activeSelf;

		Camera camOV = camera.GetComponent<Camera>();
		RenderTexture rt = new RenderTexture(800, 600, 24);
		camOV.targetTexture = rt;
		Texture2D imgOv = new Texture2D(camOV.targetTexture.width, camOV.targetTexture.height, TextureFormat.RGB24, false);
		camOV.Render();

		RenderTexture.active = rt;
		imgOv.ReadPixels(new Rect(0,0,camOV.targetTexture.width, camOV.targetTexture.height), 0, 0);
		imgOv.Apply();
		camOV.targetTexture = null;
		RenderTexture.active = null;
		Destroy (rt);


		byte[] imgData = imgOv.EncodeToJPG();
		string imgString = System.Convert.ToBase64String(imgData);

		selectionMenuCanvas.SetActive (SMC);
		blockBuilderCanvas.SetActive (BBC);
		programmingCanvas.SetActive (PUC);

		string start_code = programmingStartTextField.text;
		string update_code = programmingUpdateTextField.text;

		dataPassingObject.XBD.startCode = start_code;
		dataPassingObject.XBD.updateCode = update_code;

		//XDebug.Log (start_code + " " + programmingStartTextField.textComponent.text + " " + programmingStartTextField.text);
		XDebug.Log ("Saving xbuilld...");
		xbuildParseScript.GetXBuild ();
		XDebug.Log ("Loaded xbuild data...");
		databaseAdapter.SaveXBuildData(imgString, start_code, update_code);
	}

	public IEnumerator ParseScreenShotSaveAndLoad()
	{
		yield return new WaitForEndOfFrame();
		
		XDebug.Log ("[Test tag]", "Taking screenshot...");
		//close all canvases
		bool SMC = selectionMenuCanvas.activeSelf;
		bool BBC = blockBuilderCanvas.activeSelf;
		bool PUC = programmingCanvas.activeSelf;
		
		Camera camOV = camera.GetComponent<Camera>();
		RenderTexture rt = new RenderTexture(800, 600, 24);
		camOV.targetTexture = rt;
		Texture2D imgOv = new Texture2D(camOV.targetTexture.width, camOV.targetTexture.height, TextureFormat.RGB24, false);
		camOV.Render();
		
		RenderTexture.active = rt;
		imgOv.ReadPixels(new Rect(0,0,camOV.targetTexture.width, camOV.targetTexture.height), 0, 0);
		imgOv.Apply();
		camOV.targetTexture = null;
		RenderTexture.active = null;
		Destroy (rt);
		
		
		byte[] imgData = imgOv.EncodeToJPG();
		string imgString = System.Convert.ToBase64String(imgData);
		
		selectionMenuCanvas.SetActive (SMC);
		blockBuilderCanvas.SetActive (BBC);
		programmingCanvas.SetActive (PUC);
		
		string start_code = programmingStartTextField.text;
		string update_code = programmingUpdateTextField.text;
		
		//XDebug.Log (start_code + " " + programmingStartTextField.textComponent.text + " " + programmingStartTextField.text);
		XDebug.Log ("Saving xbuilld...");
		xbuildParseScript.GetXBuild ();
		XDebug.Log ("Loaded xbuild data...");

		
		dataPassingObject.XBD.startCode = start_code;
		dataPassingObject.XBD.updateCode = update_code;

		databaseAdapter.SaveXBuildData(imgString, start_code, update_code);

		int loadlevel = 1;
		int.TryParse(missionSelection.GetComponent<Text> ().text, out loadlevel);
		switch (loadlevel) {
			case 1:
				Application.LoadLevel ("scene1");
				break;
			case 2:
				Application.LoadLevel ("mission2scene");
				break;
			case 3:
				Application.LoadLevel ("mission3scene");
				break;
			case 4:
				Application.LoadLevel ("mission4scene");
				break;
			default:
				Application.LoadLevel ("scene1");
				break;
		}
	}
	
	public void LoadMission()
	{
		ParseAndSaveXBuild (true);
	}
}
