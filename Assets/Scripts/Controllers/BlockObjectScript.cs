using UnityEngine;
using System;
using System.Collections.Generic;

public class BlockObjectScript : SealedControlledScript
{
	public static string BLOCK_OBJECT_SCRIPT_TAG = "BlockObjectScript";

    public static string XBUILD_BLOCK_TAG = "xbuildblock";
	public static string XBUILD_ROOT_TAG = "xbuildroot";
	public static string XBUILD_PROP_TAG = "stageprop";
	public static string XBUILD_SPIN_CANDIDATES = "spincandidates";

	public static string GARAGE_BLOCK_TAG = "garageblock";

    public enum BlockType
    {
        RootTest = 0,
        NormalRed = 1,
        NormalBlue = 2,
        RocketBooster = 3,
        MissileT1 = 4,
        Radar = 5
    }

	public bool DebugMode = false;
	public bool GarageMode = false;
	public BaseGameController GameController;
    public GameObject interpreterObject;
    public GameObject mainCamera;
    public float X, Y, Z;
    public GameObject emptyPrefab;
    public GameObject prefabNormalRed;
    public GameObject prefabNormalBlue;
    public GameObject prefabRocketBooster;
    public GameObject prefabMissileType1;
    public GameObject prefabRadar;

	public XBuildData XBD;

    //Dictionary<Vector2, string> names;
    string[,] names;

    // Use this for initialization

	void Start()
	{
		string nl = "\n";
		RunScript ();
		Interpreter interpreter = interpreterObject.GetComponent<Interpreter> ();
		/*interpreter.start_code = 
			"PRINT POSITION(X) XBUILD" + nl +
			"PRINT POSITION(Y) XBUILD" + nl +
			"PRINT EXECUTE FOOBAR" + nl +
			"NUMBER N = 1" + nl +
			"NUMBER X = 0" + nl +
			"NUMBER F = 0" + nl +
			"WHILE N < 101" + nl +
			"F = 0" + nl +
			"X = N % 3" + nl + 
			"IF X == 0" + nl +
			"PRINT FOO N" + nl +
			"F = 1" + nl +
			"END" + nl +
			"X = N % 5" + nl + 
			"IF X == 0" + nl +
			"PRINT BAR N" + nl +
			"F = 1" + nl +
			"END" + nl +
			"IF F == 0" + nl +
			"PRINT N" + nl +
			"END" + nl +
			"N = N + 1" + nl +
			"END";*/
		interpreter.start_code = "TARGET target4";
		interpreter.update_code = "";

		interpreter.Run ();
	}

	void DefaultXBD()
	{
		Debug.Log ("Data passing script had no data, creating default debuging XBuild");
		/*int[,] data = new int[,] { 
			{ 3, 0, 1, 2, 1, 0, 3 },
			{ 1, 2, 1, 2, 1, 2, 1 },
			{ 0, 1, 1, 2, 1, 1, 0 },
			{ 0, 0, 4, 2, 4, 0, 0 },
			{ 0, 0, 0, 5, 0, 0, 0 }
		};
		names = new string[7, 7];
		names[0, 0] = "rocketgarage0";
		names[0, 6] = "rocketgarage1";
		names[3, 2] = "missilegarage2";
		names[3, 4] = "missilegarage3";
		names[4, 3] = "radar1";
		Vector2 center = new Vector2(3, 2);*/
		int[,] data = new int[,]{
			{ 3, 0, 3 },
			{ 4, 1, 4 }
		};
		names = new string[,]{
			{ "RocketGarage0", "", "RocketGarage1" },
			{ "MissileGarage2", "BasicGarage5", "MissileGarage3" }
		};

		Vector2 center = new Vector2(1, 1);
		XBD = new XBuildData (data, names, center);
	}

	public void OnLevelWasLoaded(int level)
	{
		XDebug.Log (level + " was loaded");
		if (level != 0) {
			RunScript();
		}
	}

    protected override void RunScript()
    {
        //Make the success Screen invivisble
        //successOverlay.active = false;

        //CreateBlock((int)BlockType.NormalRed, 2, 2, 0);

        //Test Data
        //int[,] data = new int[,] { { 2, 1, 1, 1, 2}, { 1, 0, 2, 0, 1}, { 1, 2, 2, 2, 1 },
        //{ 1, 0, 2, 0, 1}, { 2, 2, 3, 2, 2}};
        /*int[,] data = new int[,] { 
            { 3, 1, 0, 0, 0},
            { 0, 2, 1, 0, 0}, 
            { 1, 1, 1, 4, 0}, 
            { 2, 2, 2, 2, 5},
            { 1, 1, 1, 4, 0}, 
            { 0, 2, 1, 0, 0},
            { 3, 1, 0, 0, 0}
        };
        //names = new Dictionary<Vector2, string>();
        //names.Add(new Vector2(2, 0), "rocket1");
        names = new string[7, 7];
        names[0, 0] = "rocket1";
        names[6, 0] = "rocket2";
        names[2, 3] = "missiletype11";
        names[4, 3] = "missiletype12";
        names[3, 4] = "radar1";*/

		try{
			DataPassingScript dps = GameObject.Find ("XBuildData").GetComponent<DataPassingScript> ();
			XBD = dps.XBD;
			Debug.Log ("BOS Data " + dps.XBD.db_id);
			Debug.Log ("BOS StartCode " + dps.XBD.startCode);
		} catch (NullReferenceException e) {
			DefaultXBD();
		}
		names = XBD.names;

		//XDebug.Log (XBD.names);
        //
		if (XBD.types == null || XBD.names == null || XBD.center == null) {
			XDebug.Log(BLOCK_OBJECT_SCRIPT_TAG, "Empty XBuild \nTypes: " + XBD.types 
			       + "\nNames: " + XBD.names 
			       + "\nCenter: " + XBD.center);
			return;
		}

        Vector3 location = new Vector3(X, Y, Z);
		XDebug.Verbose ("Creating player XBuild at location: " + location);
		if (XBD.names != null && XBD.types != null) {
			GameObject root = CreateXBuild (XBD.name, location, XBD.center, XBD.types);
			if (!GarageMode) {
				Rigidbody RBroot = root.GetComponent<Rigidbody> ();
				RBroot.mass = root.transform.childCount;
			}
			XBD.name = XBD.names [(int)XBD.center.y, (int)XBD.center.x];

			//root XBuild must be attaced to camera
				
			if (!GarageMode)
				FocusCameraAndRunInterpreter (XBD, ref root);
		} else {
			XDebug.Log ("Empty xbuild, no data and type matrixes");
			XBD.name = "Empty Slot";
		}
    }

	public void FocusCameraAndRunInterpreter(XBuildData XBD, ref GameObject root)
	{
		CameraScript cs = root.AddComponent<CameraScript>();
		cs.mainCamera = mainCamera;
		
		//start the interpreter script
		Interpreter iscript = interpreterObject.GetComponent<Interpreter>();
		iscript.Root = root.GetComponent<RootScript>();

		iscript.start_code = XBD.startCode;
		iscript.update_code = XBD.updateCode;

		iscript.Run();
	}
	
	int BasicCount = 0;
	GameObject CreateBlock(int type, string name, Vector2 location, GameObject root)
    //create a block at certain coordinates
    //z coordinate is normally locked as the
    //custom block building is restricted to a plane
    {
        GameObject newBlock;
		BaseBlockScript BBS = null;
        switch (type)
        {
            case ((int)BlockType.NormalRed):
                newBlock = Instantiate(prefabNormalRed) as GameObject;
				newBlock.name = "(Basic)" + BasicCount++;
                break;
            case ((int)BlockType.NormalBlue):
				newBlock = Instantiate(prefabNormalBlue) as GameObject;
				newBlock.name = "(Basic)" + BasicCount++;
                break;
            case ((int)BlockType.RocketBooster):
                newBlock = Instantiate(prefabRocketBooster) as GameObject;
				BBS = (BaseBlockScript) newBlock.GetComponent<RocketBoosterScript>();
                if (name != null)
                {
                    newBlock.name = name;
                    XDebug.Verbose(BLOCK_OBJECT_SCRIPT_TAG, "[CreateBlock Debug] Named RocketBooster block (" + name + ") @ " + location);
                }
                else
                {
					XDebug.Verbose(BLOCK_OBJECT_SCRIPT_TAG, "[CreateBlock Error] Named RocketBooster block does not have a name @ " + location);
                }
                break;
            case ((int)BlockType.MissileT1):
                newBlock = Instantiate(prefabMissileType1) as GameObject;
				if(newBlock.transform.childCount > 0)
					BBS = (BaseBlockScript) newBlock.transform.GetChild(0).gameObject.GetComponent<MissileGeneratorScript>();
                if (name != null)
                {
                    newBlock.name = name;
					XDebug.Verbose(BLOCK_OBJECT_SCRIPT_TAG, "[CreateBlock Debug] Named MissileTurretType1 block (" + name + ") @ " + location);
                }
                else
                {
					XDebug.Verbose(BLOCK_OBJECT_SCRIPT_TAG, "[CreateBlock Error] Named MissileTurretType1 block does not have a name @ " + location);
                }
                break;
            case ((int)BlockType.Radar):
				newBlock = Instantiate(prefabRadar) as GameObject;
				BBS = (BaseBlockScript) newBlock.GetComponent<RadarBlockScript>();
                if (name != null)
                {
                    newBlock.name = name;
                    XDebug.Verbose(BLOCK_OBJECT_SCRIPT_TAG, "[CreateBlock Debug] Named Radar block (" + name + ") @ " + location);
                }
                else
                {
                    XDebug.Verbose(BLOCK_OBJECT_SCRIPT_TAG, "[CreateBlock Error] Named Radar block does not have a name @ " + location);
                }
                break;
            
            default:
                newBlock = null;
                break;
        }

		if (!GarageMode && root != null && BBS != null) {
			RootScript RS = root.GetComponent<RootScript>();
			BBS.SetRoot(RS);
			RS.AddChildBlock(BBS);
		}

		if (newBlock != null) {
			foreach (Transform child in newBlock.transform) {
				child.gameObject.name = newBlock.name + " :: " + child.gameObject.name;
			}
			//give the newblock a tag
			newBlock.tag = (!GarageMode) ? XBUILD_BLOCK_TAG : GARAGE_BLOCK_TAG;
		}

        return newBlock;
    }

    /* Data format for any xBuild is 
     * a) the block closest to the center
     * b) a matrix of integers, eg:
     * [xBuild matrixes are always ODD*ODD]
     * 1,1
     * 0,1,0
     * 1,1,1
     * 0,1,0
     * Will give a plus sign of Normal Red Blocks
     */
    class Node
    {
        public GameObject gameObject;
        public Vector2 relative;
        public Vector2 absolute;

        public Node(GameObject gameObject, Vector2 relative, Vector2 absolute)
        {
            this.gameObject = gameObject;
            this.relative = relative;
            this.absolute = absolute;
        }

        public Node EmptyChild(Vector2 add)
        {
            Node child = new Node(null, this.relative, this.absolute);
            child.relative.x += add.x;
            child.relative.y += add.y;
            child.absolute.x += add.x;
            child.absolute.y += add.y;

            return child;
        }
    }

    //Define transforms of Up, Down, Left and Right
    //We will need this to scan in these directions
    Vector2[] scan = new Vector2[] { new Vector2(-1, 0), new Vector2(1, 0), 
        new Vector2(0, -1), new Vector2(0, 1) };

    GameObject CreateXBuild(string XBuildName, Vector3 location, Vector2 center, int[,] data)
    //create a composite xBuilder
    //start at the first detected node than scan in
    //all 4 directions
    {
        Vector2 bounds = new Vector2(data.GetLength(1), data.GetLength(0));
        //parent object to put evertyhing under
        //GameObject rootObject = Instantiate(emptyPrefab, location, Quaternion.identity) as GameObject;
        //rootObject.name = "XBuild Root";

        //the first node
        Vector2 firstLocation = new Vector2((int)center.y, (int)center.x);
        string firstName = names[(int)center.y, (int)center.x];
        //names.TryGetValue(location, out firstName);
        int type = data[(int)center.y, (int)center.x];
        
		Debug.Log ("Root blocked created " + firstName + " " + firstLocation + " " + type);
		GameObject firstObject = CreateBlock(type, firstName, firstLocation, null);
        
        //make this a rigidbody
        //DestroyImmediate(firstObject.GetComponent("BoxCollider"));
		if (!GarageMode) {
			//Debug.Log (firstObject);
			firstObject.AddComponent<RootScript>();
			firstObject.AddComponent<Rigidbody> ();
			Rigidbody rb = firstObject.GetComponent<Rigidbody> ();
			rb.isKinematic = false;
			firstObject.AddComponent<ConstantForce> ();

			
			RootScript rs = firstObject.GetComponent<RootScript> ();
			rs.GameController = GameController;
			rs.Init ();
		}

        //firstObject.transform.parent = rootObject.transform;
        firstObject.transform.localPosition = location;//new Vector3(0,0,0);
        firstObject.name = XBuildName;
        firstObject.tag = (!GarageMode) ? XBUILD_ROOT_TAG : GARAGE_BLOCK_TAG;
        data[(int)center.y, (int)center.x] = 0;


        Node first = new Node(firstObject, new Vector2(0, 0), center);
        Queue<Node> nodes = new Queue<Node>();
        nodes.Enqueue(first);

        int count = 0;

        while (nodes.Count != 0 && count < 100)
        {
            Node parent = nodes.Dequeue();
            //XDebug.Log(BLOCK_OBJECT_SCRIPT_TAG, "Parent Relative Location " + parent.relative + " Absolute " + parent.absolute);
            //4 directional scan
            //up
            for (int i = 0; i < scan.Length; i++)
            {
                Node child = parent.EmptyChild(scan[i]);
                if (checkBounds(child, bounds))
                {
                    //XDebug.Log(BLOCK_OBJECT_SCRIPT_TAG, "Bounds " + bounds + "Child Absolute " + child.absolute);
                    int childType = data[(int)child.absolute.y, (int)child.absolute.x];
                    if (childType != 0)
                    {
                        //check if block has a name
                        Vector2 childLocation = new Vector2((int)child.absolute.x, (int)child.absolute.y);
                        string childName = names[(int)child.absolute.y, (int)child.absolute.x];
							
                        GameObject childObject = CreateBlock(childType, childName, childLocation, firstObject);

                        childObject.transform.parent = firstObject.transform;
                        childObject.transform.localPosition = child.relative;
                        if (childName == null)
                            childObject.name = firstObject.name + "_" + count + "_" + i;

                        child.gameObject = childObject;
                        nodes.Enqueue(child);
                        //XDebug.Log(BLOCK_OBJECT_SCRIPT_TAG, "Created child @ relative " + child.relative);
                        //clear from the matrix
                        data[(int)child.absolute.y, (int)child.absolute.x] = 0;
                    }
                    else
                    {
                        //XDebug.Log(BLOCK_OBJECT_SCRIPT_TAG, "Empty Tile @ absolute " + child.absolute);
                    }
                }
            }
            count++;
        }

        return firstObject;
    }

    private bool checkBounds(Node node, Vector2 bound)
    {
        return (node.absolute.x < 0 || node.absolute.x >= bound.x
            || node.absolute.y < 0 || node.absolute.y >= bound.y) ? false : true;
    }
}