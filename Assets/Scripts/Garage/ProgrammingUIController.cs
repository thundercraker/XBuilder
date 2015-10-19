using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ProgrammingUIController : MonoBehaviour {
	public GameObject mainCanvas;
	public GameObject programmingCanvas;
	public GameObject PIF_Start, PIF_Update;
	public Text PIF_SwitchText;
	public Text autoCompleteText;
	public GameObject ScrollArea;
	public Text blockListText;
	public Text toggleButtonText;

	string activeToken;
	ACTree AutoCompleteTree;
	ScrollRect scrollRect;

	public static string TOGGLE_BUTTON_BNM = "Block Name Mode";
	public static string TOGGLE_BUTTON_ACM = "Auto Complete Mode";

	public static string TOGGLE_PIF_STARTMODE = "Switch to Update Code";
	public static string TOGGLE_PIF_UPDATEMODE = "Switch to Start Code";

	// Use this for initialization
	void Start () {
		CreateACTree ();
		scrollRect = ScrollArea.GetComponent<ScrollRect> ();

		//Set PIF start mode
		PIF_SwitchText.text = TOGGLE_PIF_STARTMODE;
		PIF_Start.SetActive(true);
		PIF_Update.SetActive(false);

		//Set auto complete mode
		toggleButtonText.text = ProgrammingUIController.TOGGLE_BUTTON_BNM;
		blockListText.gameObject.SetActive (false);
		autoCompleteText.gameObject.SetActive (true);
		scrollRect.content = autoCompleteText.gameObject.GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
	void Update () {
		//get the current token
		if (Input.inputString.Length > 0) {
			foreach (char c in Input.inputString) {
				if (c == " " [0] || c == "\r" [0] || c == "\n" [0] || c == "\t" [0]) {
					activeToken = "";
				} else if (c == "\b"[0]) {
					activeToken = (activeToken.Length > 0) ? activeToken.Substring(0, activeToken.Length - 1) : "";
				} else {
					activeToken += c.ToString ();
				}
			}
			//Debug.Log("Active Token changed to " + activeToken);
			UpdateAutoComplete(activeToken);
		}
	}

	void UpdateAutoComplete(string token)
	{
		string fulltext = "";

		List<ACTree.ACNode> leaves = new List<ACTree.ACNode>();
		AutoCompleteTree.GetLeavesOf (token, ref leaves);
		//Debug.Log ("Leaf count: " + leaves.Count);

		List<string> words = new List<string> ();
		foreach(ACTree.ACNode leaf in leaves)
		{
			words.Add(leaf.value);
		}
		words.Sort ();
		foreach (string word in words) {
			//Debug.Log ("Printing autocomplete for " + word);
			string actext = "";
			string desc = "";
			NameAndDesc[] objectDescs;
			if(reservedWordDescs.TryGetValue(word, out desc))
			{
				actext = "<b>" + word + "</b>\n" + desc;
				actext += "\n";
			} else if(objectMethodDescs.TryGetValue(word, out objectDescs)) {
				foreach(NameAndDesc od in objectDescs)
				{
					actext += "<b>" + word + "</b> for <i>" + od.name + "</i>\n" + od.desc; 
					actext += "\n";
				}
			}
			fulltext += actext + "\n";
		}
		autoCompleteText.text = fulltext;
	}

	public void ToggleText()
	{
		//nothing
		if (blockListText.gameObject.activeSelf) {
			//switch to autocomplete
			toggleButtonText.text = ProgrammingUIController.TOGGLE_BUTTON_BNM;
			blockListText.gameObject.SetActive (false);
			autoCompleteText.gameObject.SetActive (true);

			scrollRect.content = autoCompleteText.gameObject.GetComponent<RectTransform>();
		} else {
			//switch to block list
			toggleButtonText.text = ProgrammingUIController.TOGGLE_BUTTON_ACM;
			//create block list
			string blocklist = "";
			GameObject[] blocks = GameObject.FindGameObjectsWithTag("garageblock");
			foreach(GameObject block in blocks)
			{
				blocklist += block.name + "\n";
			}
			blockListText.text = blocklist;

			blockListText.gameObject.SetActive (true);
			autoCompleteText.gameObject.SetActive (false);

			scrollRect.content = blockListText.gameObject.GetComponent<RectTransform>();
		}
	}

	public void ToggleStartUpdate()
	{
		if (PIF_Start.activeSelf) {
			//switch to update
			PIF_SwitchText.text = TOGGLE_PIF_UPDATEMODE;
			PIF_Start.SetActive (false);
			PIF_Update.SetActive (true);
		} else {
			PIF_SwitchText.text = TOGGLE_PIF_STARTMODE;
			PIF_Start.SetActive(true);
			PIF_Update.SetActive(false);
		}
	}

	public void run()
	{
		//TODO
		//just make it jump to the test scene with a debug display
		//pass the stuff in the debug display back to the debug display
	}

	public void save()
	{
		//TODO
		//System.IO.File.WriteAllText ("start.yuma", PIF_Start.transform.GetChild (0).GetComponent<Text> ().text);
		//System.IO.File.WriteAllText ("update.yuma", PIF_Start.transform.GetChild (0).GetComponent<Text> ().text);
	}

	public void open()
	{
		programmingCanvas.SetActive (true);
		mainCanvas.SetActive (false);
	}

	public void close()
	{
		programmingCanvas.SetActive (false);
		mainCanvas.SetActive (true);
	}

	void CreateACTree()
	{
		List<string> reservedWords = new List<string> ();
		foreach (KeyValuePair<string, string> pair in reservedWordDescs) {
			reservedWords.Add(pair.Key);
		}

		foreach (KeyValuePair<string, NameAndDesc[]> pair in objectMethodDescs) {
			reservedWords.Add(pair.Key);
		}

		AutoCompleteTree = new ACTree (reservedWords);

		UpdateAutoComplete ("st");
	}

	public class ACTree
	{
		public class ACNode 
		{
			public string value;
			public Dictionary<char, ACNode> children;

			public ACNode(string value)
			{
				this.value = value;
				this.children = new Dictionary<char, ACNode>();
			}

			public bool IsLeaf()
			{
				return value.Length > 0;
			}

			public void Add(char letter, ACNode child)
			{
				this.children.Add (letter, child);
			}

			public bool TryGetChild(char letter, out ACNode child)
			{
				return this.children.TryGetValue(letter, out child);
			}

			public List<ACNode> GetAllChildren()
			{
				List<ACNode> nodes = new List<ACNode> ();
				foreach(KeyValuePair<char, ACNode> pair in children)
				{
					nodes.Add(pair.Value);
				}
				return nodes;
			}

			public void GetAllLeaves(ref List<ACNode> leaves)
			{
				if (!this.IsLeaf ()) {
					foreach(KeyValuePair<char, ACNode> pair in children)
					{
						if(pair.Value.IsLeaf())
						{
							leaves.Add(pair.Value);
						} else {
							pair.Value.GetAllLeaves(ref leaves);
						}
					}
				}
			}
		}

		public ACNode root;

		public ACTree(List<string> words)
		{
			root = new ACNode("");
			foreach(string word in words)
			{
				ACNode parent = root;
				ACNode child;
				int cnt = 1;
				foreach(char letter in word.ToCharArray())
				{
					if(!parent.TryGetChild(letter, out child))
					{
						string value = (cnt < word.Length) ? "" : word;
						child = new ACNode(value);
						parent.Add(letter, child);
					}
					parent = child;
					cnt++;
				}
			}
		}

		public bool GetLeavesOf(string token, ref List<ACNode> leaves)
		{
			ACNode node = root;
			foreach(char letter in token)
			{
				if(!node.TryGetChild(letter, out node))
				{
					return false; 
				}
			}
			node.GetAllLeaves (ref leaves);
			return true;
		}
	}

	public class NameAndDesc{
		public string name;
		public string desc;

		public NameAndDesc(string name, string desc)
		{
			this.name = name;
			this.desc = desc;
		}
	}

	Dictionary<string, string> reservedWordDescs = new Dictionary<string, string>()
	{
		{
			"get", "Gets the block with a particular name (eg: ROCKET R1 <- GET ROCKETBLOCK0)"
		},
		{
			"number", "Declare a number variable (eg: NUMBER N1 = 2 + 1)"
		},
		{
			"delay", "Execute a command after soe seconds of delay (eg: DELAY 2 ROCKET1 START)"
		},
		{
			"return", "Return the value from the function, will only accept a single variable (eg: RETURN VAR1)"
		},
		{
			"print", "Print/Display text on the debug console (eg: PRINT 4 + 5)"
		},
		{
			"struct", "A structure (eg: STRUCT POSITION1 X=10 Y=20) \nAccess: POSITION1[X] \nSet: POSITION1 SET X=20"
		},
		{
			"function", "Declare the start of a function \nExample" +
				"\nFUNCTION FUNC AVERAGE NUMBER X NUMBER Y\nNUMBER A = (X + Y)/2\nRETURN A"
		},
		{
			"while", "Declare the start of a loop \nExample" +
				"NUMBER X = 0\nWHILE X < 5\nX = X + 1\nPRINT X\nEND"
		},
		{
			"if", "If the start of an if block \nExample" +
				"IF X == 1\nPRINT X\nEND"
		},
		{
			"end", "End a block"
		},
		{
			"turn", "Turn by angle in degrees (eg TURN 40)"
		},
		{
			"target", "Turn the ship to aim at target, used often with the radar targets"
		}
	};

	Dictionary<string, NameAndDesc[]> objectMethodDescs = new Dictionary<string, NameAndDesc[]>()
	{
		{ "start",  new NameAndDesc[] 
			{ 
				new NameAndDesc("rocket", "Start the rocket")
			}
		},
		{ "stop",  new NameAndDesc[] 
			{ 
				new NameAndDesc("rocket", "Stop the rocket")
			}
		},
		{ "fire",  new NameAndDesc[] 
			{ 
				new NameAndDesc("missile", "Fire the weapon")
			}
		},
		{ "ping",  new NameAndDesc[] 
			{ 
				new NameAndDesc("radar", "Refresh the data provided by the radar")
			}
		}
	};
}
