using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Interpreter : MonoBehaviour {

    /* This is the Rootscript, please refer to RootScript.cs
	 *
	 * In summary, brings together the effects of the various other blocks
	 * so that can be executed in the same Update call (allows orderings and
	 * synchronization).
     */
	public RootScript Root;

	/* These are the strings that hold the actual code being run, since they are public
	 * other scripts in the scene can change them. In most cases, the code is updated by
	 * the GarageMainController when it loads the Scene
	 * 
	 * (!) Start code is also refered to as <InitCode>
	 */
	public string start_code, update_code;

	/* Since Sensor updates happen in the Update call. However, attempting to run the
	 * UpdateSensor function before the XBuild object is fully loaded 
	 */
	private bool StartSensorUpdate = false;

    // Update is called once per frame
	void Update () {
	    /* On each update cycle the data regarding the XBuilder Root (siply XBuild) must be updated
         * values such as position (x,y), velocity(x,y)
         * Furthermore, various sensing block types will have their own new data (eg: closestObject(x,y), closestEnemy(x,y))
         */
        if(StartSensorUpdate)
            UpdateSensorData();

        if (InitCodeLoadFinished && !InitCodeRunFinished)
        {
			Debug.Log("Init code starting...");
            Initialize();
            if (InitCode.Length > 0)
            {
                Interpret(InitCode.ToLowerInvariant());
            } else {
				Debug.Log ("Empty Init Code");
			}
            InitCodeRunFinished = true;
            Debug.Log("Init code interpretation finished");
        }

        if(InitCodeRunFinished && UpdateCodeLoadFinished)
        {
            if (UpdateCode.Length > 0)
            {
                Interpret(UpdateCode.ToLowerInvariant());
                UpdateFrame++;
                //Debug.Log("===================================================================");
                //Debug.Log("[" + UpdateFrame + "] Frame ended");
                //Debug.Log("===================================================================");
            }
        }
	}

    

	//These hold the internal versions of the code
	public string InitCode = "";
    public string UpdateCode = "";
    public bool InitCodeLoadFinished = false;
    public bool UpdateCodeLoadFinished = false;
    public bool InitCodeRunFinished = false;
    public int UpdateFrame = 0;

	/// <summary>
	/// Loads the Init (Start) and Update Code
	/// </summary>
    public void Run()
    {
        LoadInitCode();
        LoadUpdateCode();
    }

    void LoadInitCode()
    {
        Debug.Log("Loading Init Code..");
		InitCode = start_code + "";
        InitCodeLoadFinished = true;
        Debug.Log("Loaded Init Code..");
    }

    void LoadUpdateCode()
    {
        Debug.Log("Loading Update Code..");
		UpdateCode = update_code + "";
        UpdateCodeLoadFinished = true;
        Debug.Log("Loaded Update Code..");
    }

	
	//Numbers are stored here
	Dictionary<string, double> numbers;

	/* XBGameObjects stored here
	 * 
	 * XBGameObjects <refer to XBGameObjects.cs>
	 * XBGameObject is the class that allows a translation of a string ($name) and 
	 * and array of parameters into execution of a function with ($name)
	 * (assuming such a function is part of a blocks' script)
	 */
	Dictionary<string, XBGameObject> variables;

	/* Structures are stored here
	 * 
	 * Find implementation at [XBStructure.cs]
	 * 
	 * These are the containers in which a collection of data is held
	 * data is associative get(key) -> value. The key can be a string or number
	 */ 
	Dictionary<string, XBStructure> structures;

	/* Functions stored here
	 * 
	 * Find Function implementation at [XBFunction.cs]
	 */ 
	Dictionary<string, XBFunction> functions;

	/* GameObjects in the scene are stored here
	 * This is mostly relevant during the beginning of the scene's loading.
	 * Only Prop Objects such as asteroid that do nothing but float around and
	 * have self contained scripts (eg a script that triggers a destruction animtion
	 * by itself) are refered to by their scene names
	 */
	Dictionary<string, GameObject> objects;

	//Sensor names stored here
	List<string> sensors;

	//Logic and Arithmetic Helper Classes
	//Find in Arithmetic.cs and Logic.cs
	Logic logic;
	Arithmetic arithmetic;

    public void Initialize()
    {
        objects = new Dictionary<string, GameObject>();
        //the keyword THIS should be a GameObject refering to the Root GameObject
        objects.Add("xbuild", Root.gameObject);

        variables = new Dictionary<string, XBGameObject>();
        numbers = new Dictionary<string, double>();
        structures = new Dictionary<string, XBStructure>();
        sensors = new List<string>();

        logic = new Logic(this);
        arithmetic = new Arithmetic(this);

        StateStack = new Stack<XBInterpreterState>();
        loopStates = new Stack<LoopState>();
        BlockStates = new Stack<BlockType>();

        GameObject[] allObjs = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
        foreach (GameObject go in allObjs)
        {
            //Debug.Log("Object name: " + go.name);
            if (go.activeInHierarchy)
            { 
                if (go.name == null) continue;
				bool valid = false;
				foreach(string tag in VALID_TAGS)
				{
					if(go.CompareTag(tag))
					{
						valid = true;
						break;
					}
				}
				if(valid)
                	objects.Add(go.name.ToLower(), go);
            }
        }

        functions = new Dictionary<string, XBFunction>();
        StartSensorUpdate = true;
    }

	public static string[] VALID_TAGS = new string[]
	{
		"xbuildroot",
		"xbuildblock",
		"bossturret",
		"spincandidates",
		"approachTarget"
	};

	/* This Block refers to a block of code
	 * 0 - Not in a block
	 * 1 - simple block
	 * 2 to 4 - self explainatory
	 * 9 - SKIP MODE
	 * 
	 * When BlockType is set to SKIP, the interpreter will skip lines
	 * until it has reached in approriate END statement
	 */
    public enum BlockType
    {
        none = 0,
        block = 1,
        function = 2,
        loop = 3,
        conditional = 4,
        skip = 9
    }

    //Current line number
    int currentLineNumber;
	//The instruction that started the current block
    string blockInstruction = null;
	/* The current block
	 * Find XBBlock Implementation at [XBBlock.cs]
	 */
    XBBlock currentBlock = null;
	//Current function
    XBFunction currentFunction = null;
	//Current block type
    BlockType currentType = BlockType.none;

    /// <summary>
    /// This string will be used by functions that return a value
    /// the type of the value will have to be infered using the
	/// GetType function and TokenType enumarator
    /// </summary>
    string returnValue;

    bool returnStatement = false;

	/* InterpreterState
	 * 
	 * The InterpreterState maintains critical information about the interpreter
	 * All of these are variables you've seen before (above). However, these values
	 * change when the interpreter enters a new block. 
	 * 
	 * For example, the The variables created inside the scope of a functional block
	 * are not visible at a global scope. This functionality is implemented using the 
	 * InterpreterState
	 * 
	 * 0) Block is reached
	 * 1) Variables copied to InterpreterState
	 * 2) InterpreterState pushed into a stack
	 * 3) Block END is reached
	 * 4) Interpreter stack popped out
	 * 5) Variables reset to whatever is in the Interpreter State
	 */ 
    struct XBInterpreterState
    {
        public int currentLineNumber;
        public string blockInstruction;
        public XBBlock currentBlock;
        public XBFunction currentFunction;
        public BlockType currentType;
        public Dictionary<string, double> numbers;
        public Dictionary<string, XBFunction> functions;
        public Dictionary<string, XBGameObject> variables;
    }

    public void PushInterpreterState()
    {
        XBInterpreterState xbis = new XBInterpreterState();
        xbis.currentLineNumber = currentLineNumber;
        xbis.blockInstruction = blockInstruction;
        xbis.currentBlock = currentBlock;
        xbis.currentFunction = currentFunction;
        xbis.currentType = currentType;
        xbis.numbers = numbers;
        xbis.variables = variables;

        StateStack.Push(xbis);
    }

    public void PopInterpreterState()
    {
        XBInterpreterState xbis = StateStack.Pop();

        currentLineNumber = xbis.currentLineNumber;
        blockInstruction = xbis.blockInstruction;
        currentBlock = xbis.currentBlock;
        currentFunction = xbis.currentFunction;
        currentType = xbis.currentType;
        numbers = xbis.numbers;
        variables = xbis.variables;
    }

    Stack<XBInterpreterState> StateStack;

    Stack<BlockType> BlockStates;

    /// <summary>
    /// Pushes a blocktype into the BlockState Stack
    /// </summary>
    /// <param name="type"></param>
    /// <returns>True if the currentType was not none False if otherwise</returns>
    public bool PushBlockState(BlockType type)
    {
        bool push = false;
        if (currentType != BlockType.none)
        {
            BlockStates.Push(currentType);
            push = true;
        }
        currentType = type;
        push = false;
        return push;
    }

    public bool PopBlockState()
    {
        if (BlockStates.Count < 1)
        {
            currentType = BlockType.none;
            return false;
        }
        currentType = BlockStates.Pop();
        return true;
    }

	//Loop start lines help the interpreter return to the
	//line that the loop started on so that it can return to loop
	//These must be maintained in a stack so loops inside loops
	//can be effective tracked
    struct LoopState
    {
        public int startLine;
    }

    int escapeLevel = 0;
    LoopState loopState;
    Stack<LoopState> loopStates;

    public void UpdateSensorData()
    {
        //Update positional data


        //Update data from all sensor blocks
        foreach (string variable in sensors)
        {
            XBGameObject obj;
            variables.TryGetValue(variable, out obj);

            InterpreterDesc desc;
            objectDescs.TryGetValue(obj.datatype, out desc);

            //printDebug(pieces[0] + " is a variable of type " + obj.datatype, currentLineNumber);

            switch (obj.datatype)
            {
                case ("radar"):
				Debug.Log("Preparing radar data");
                    RadarBlockScript rbscript = obj.gameObject.GetComponent<RadarBlockScript>();
                    rbscript.Ping();
                    string name = variable + ".values";
                    structures.Remove(name);
                    if (rbscript.objects != null)
                        CreateStructureFromArray(name, rbscript.objects);
                    else
                        CreateStructure(name, new string[] {});
                    break;
            }
        }
    }

    public void Interpret(string script)
    {
        //numbers = new Dictionary<string, double>();

        string[] lines = script.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);
        currentLineNumber = 0;
        int n = 0;
        while(currentLineNumber < lines.Length && n < 1000)
        {
			if(lines[currentLineNumber].Trim().Length > 0)
            	InterpretLine(lines[currentLineNumber]);
            currentLineNumber++;
            n++;
            //Debug.Log("Cycle summary: Block type = " + currentType + " Block States " + BlockStates.Count + " Escape LVL " + escapeLevel);
            //Debug.Log("===================================================================");
        }
        //throw new Exception("LINEBREAK, n = " + n);
    }

    public void InterpretLine(string line)
    {
        //Debug.Log("Parsing: " + line + " line number " + currentLineNumber + " Block type = " + currentType + " Block States " + BlockStates.Count + " Escape LVL " + escapeLevel);

        if(line.Equals("end", System.StringComparison.OrdinalIgnoreCase))
        {
            if(currentType == BlockType.function)
            {
                functions.Add(currentFunction.name, currentFunction);
                Debug.Log("Added function " + currentFunction.name + " to repertoire.");
                currentFunction = null;

                PopBlockState();
            }

            else if(currentType == BlockType.loop)
            {
                //go back to the WHILE line
                currentLineNumber = loopState.startLine - 1;
                Debug.Log("Looping to " + currentLineNumber);

                PopBlockState();
            }

            else if (currentType == BlockType.skip)
            {
                /*if (--escapeLevel == 0)
                    PopBlockState();
                if (escapeLevel < 0)
                    throw new InterpreterException("There is an end statement without an accompanying block start statement.", currentLineNumber);*/
                if (escapeLevel > 0)
                {
                    escapeLevel--;
                }
                else
                {
                    PopBlockState();
                }
            }

            else if (currentType == BlockType.conditional)
            {
                PopBlockState();
            }

            return;
        }
        if(currentType != BlockType.none)
        {
            //still processing block
            if(currentType == BlockType.function)
            {
                currentFunction.AddLine(line);
                return;
            }
            else if (currentType == BlockType.block)
            {
                currentBlock.AddLine(line);
                return;
            }
            else if (currentType == BlockType.loop)
            {
                //do nothing and continue
                Debug.Log("Continuing loop line " + currentLineNumber);
            }
            else if (currentType == BlockType.conditional)
            {
                //do nothing and continue
                Debug.Log("Continuing conditional line " + currentLineNumber);
            }
            else if (currentType == BlockType.skip)
            {
                //skip do nothing
                string[] tokens = line.Split(' ');
                if (IsBlockStart(tokens[0]))
                    escapeLevel++;
                return;
            }
            else
            {
                //nothing
            }
            /*string[] tokens = line.Split(' ');
            if (IsBlockStart(tokens[0]))
                escapeLevel++;*/
            
            Debug.Log("Processed line in block [" + line + "]");
        }

        string[] pieces = line.Split(' ');

        double delayTime = 0;
        if(pieces[0].Equals("delay", System.StringComparison.OrdinalIgnoreCase))
        {
            //if the first word is delay
            //start coroutine with that delay
            if (numbers.ContainsKey(pieces[1]))
            {
                numbers.TryGetValue(pieces[1], out delayTime);
            }
            else
            {
                if (!double.TryParse(pieces[1], out delayTime))
                    throw (new InterpreterException("Cannot parse the delay time. [" + pieces[1] + "]", currentLineNumber));
            }

            string[] npieces = new string[pieces.Length - 2];
            for(int i = 0; i < pieces.Length - 2; i++)
            {
                npieces[i] = pieces[i + 2];
            }
            pieces = npieces;
        }

        if (pieces[0].Equals("return", System.StringComparison.OrdinalIgnoreCase))
        {
            if (delayTime > 0) 
                throw (new InterpreterException("A return statement cannot succeed a delay statement", currentLineNumber));

            returnStatement = true;

            if(pieces.Length > 2)
                throw (new InterpreterException("The return statement can only be followed by a single token", currentLineNumber));

            TokenType type = GetType(pieces[1]);

            string value = "";

            //Unknown = 0,
            //XBObject = 1,
            //SceneObject = 2,
            //Number = 3,
            //NumberValue = 4,
            //Text = 5,
            //Function = 6
            //Debug.Log("Returning raw [" + pieces[1] + "]");

            switch(type)
            {
                case (TokenType.XBObject):
                case (TokenType.SceneObject):
                case (TokenType.NumberValue):
                case (TokenType.Text):
                    returnValue = pieces[1];
                    break;
                case (TokenType.Number):
                    if(numbers.ContainsKey(pieces[1]))
                    {
                        double n = 0;
                        numbers.TryGetValue(pieces[1], out n);
                        returnValue = "" + n;
                    }
                    else
                    {
                        throw (new InterpreterException("Cannot return value, the number variable has not been initialized.", currentLineNumber));
                    }
                    break;
                case (TokenType.Function):
                    throw (new InterpreterException("Returning a function is not supported.", currentLineNumber));
                    break;
                case (TokenType.Unknown):
                    throw (new InterpreterException("Cannot return a value of unknown type.", currentLineNumber));
                    break;
            }
            //Debug.Log("Returning value [" + returnValue + "]");
            return;
        }

        if (pieces[0].Equals("print", System.StringComparison.OrdinalIgnoreCase))
        {
			pieces = ReplaceToValues(pieces, 1);
			Debug.Log("PRINT [" + GetString(pieces) + "]");
			PrintToDebugUI ("PRINT [" + GetString(pieces) + "]");
            return;
        }
        
        if(variables.ContainsKey(pieces[0]))
        {

            XBGameObject obj;
            variables.TryGetValue(pieces[0], out obj);
            
            InterpreterDesc desc;
            objectDescs.TryGetValue(obj.datatype, out desc);

            printDebug(pieces[0] + " is a variable of type " + obj.datatype, currentLineNumber);
            
            InterpreterInterface script = null;

            if (InfixPrefix.equal(obj.datatype, "rocket"))
            {
                RocketBoosterScript rbscript = obj.gameObject.GetComponent<RocketBoosterScript>();
                script = (InterpreterInterface)rbscript;
            }
            else if (InfixPrefix.equal(obj.datatype, "missile"))
            {
                MissileGeneratorScript mgscript = obj.gameObject.transform.GetChild(0).GetComponent<MissileGeneratorScript>();
                script = (InterpreterInterface)mgscript;
            }
            else
            {
                throw (new InterpreterException("The block type " + pieces[0] + " does not have a programmable interface. ", currentLineNumber));
            }

            if(Contains(desc.methods, pieces[1]))
            {
                //invoke the method
                //RKT START
                printDebug(pieces[1] + " is a method of " + obj.datatype, currentLineNumber);
                script.command((float)delayTime, pieces[1], null);
            }
            else if (Contains(desc.variables, pieces[1]))
            {
                //set variable
                //RKT FORCE 100
                printDebug(pieces[1] + " is a variable of " + obj.datatype, currentLineNumber);

                if (numbers.ContainsKey(pieces[2]))
                    numbers.TryGetValue(pieces[2], out delayTime);

                script.set((float)delayTime, pieces[1], pieces[2]);
            }
            else
            {
                throw (new InterpreterException("The block named " + pieces[0] + " of the " + obj.datatype +
                    " type does not have a function or variable named " + pieces[1], currentLineNumber));
            }
        } else if(objectDescs.ContainsKey(pieces[0]))
        {
            //check if it's a datatype
            //which means object initialization

            //get object of type
            //EG syntax: ROCKET RKT <- GETOBJECT rocketbooster

            if (variables.ContainsKey(pieces[1]))
                throw (new InterpreterException("Cannot initialize a variable that already exists", currentLineNumber));

            if(objects.ContainsKey(pieces[4]))
            {
                GameObject obj;
                objects.TryGetValue(pieces[4], out obj);
                XBGameObject xbobj;
                xbobj.gameObject = obj;
                xbobj.datatype = pieces[0];

                variables.Add(pieces[1], xbobj);

                if(pieces[0].Equals("radar", System.StringComparison.OrdinalIgnoreCase))
                {
                    sensors.Add(pieces[1]);
					UpdateSensorData();
                }
            }
            else
            {
                //there is no gameObject in the scene with this name
                throw (new NoBlockWithNameFoundError("There was no block with the name " + pieces[4], currentLineNumber));
            }
        }
        else if (numbers.ContainsKey(pieces[0]))
        {
            //Arithmetic evaluations on existing number
            if(pieces.Length < 3)
            {
                throw (new InterpreterException("There are not enough arguments for this Arithmetic operation.", currentLineNumber));
            }
            double value = arithmetic.EvaluateArithExpression(pieces, 2);
            numbers[pieces[0]] = value;
            //throw (new InterpreterException("Evaluated number " + pieces[0] + " to " + value, currentLineNumber));
        }
        else if (pieces[0].Equals("number", System.StringComparison.OrdinalIgnoreCase))
        {
            //Arithmetic evaluations on Initialization of number
            if (pieces.Length < 4)
            {
                throw (new InterpreterException("There are not enough arguments for this Arithmetic initialization.", currentLineNumber));
            }
            double value = arithmetic.EvaluateArithExpression(pieces, 3);
            numbers.Add(pieces[1], value);
            //throw (new InterpreterException("Evaluated number " + pieces[1] + " to " + value, currentLineNumber));
        }
        else if (pieces[0].Equals("struct", System.StringComparison.OrdinalIgnoreCase))
        {
            /* create a new struct
             * Format: STRUCT NAME KEY=VALUE KEY=VALUE KEY=VALUE...
             * NAME SET KEY=VALUE
             * NAME[KEY]
             */
            string[] kvs = new string[pieces.Length - 2];
            for (int i = 2; i < pieces.Length; i++)
                kvs[i - 2] = pieces[i];

            CreateStructure(pieces[1], kvs);
        }
        else if (structures.ContainsKey(pieces[0]))
        {
            switch(pieces[1].ToLowerInvariant())
            {
                case("set"):
                    if(pieces.Length < 4)
                        throw (new InterpreterException("There are not enough arguments for this set operation.", currentLineNumber));
                    XBStructure XBS;
                    structures.TryGetValue(pieces[0], out XBS);
                    string[] pair = pieces[2].Split('=');
                    if (pair.Length != 2)
                        throw new InterpreterException("Incoherent key value, must be in format (key=value) [" + pieces[2] + "]", currentLineNumber);
                    XBS.Set(pair[0], pair[1]);
                    break;
                default:
                    break;
            }
        }
        else if (functions.ContainsKey(pieces[0]))
        {
            CallFunction(pieces);
        }
        else if (IsBlockStart(pieces[0]))
        {
            if (pieces[0].Equals("function", System.StringComparison.OrdinalIgnoreCase))
            {
                if(currentType == BlockType.function)
                {
                    throw (new InterpreterException("You cannot define a function within a function.", currentLineNumber));
                }
            
                //function definition
                blockInstruction = line;

                //create the block for this
                //XBFunction(string name, string[] args, int lineNumber)
                List<string> args = new List<string>();
                if(pieces.Length > 2)
                {
                    if(pieces.Length % 2 != 0)
                    {
                        throw (new InterpreterException("The function declaration has improper arguments.", currentLineNumber));
                    }
                
                    for(int i = 2; i < pieces.Length; i = i + 2)
                    {
                        args.Add(pieces[i] + " " + pieces[i+1]);
                    }
            
                }
                if(functions.ContainsKey(pieces[1]))
                {
                    throw (new InterpreterException("The function " + pieces[1] + " has already been declared.", currentLineNumber));
                    return;
                }
                currentFunction = new XBFunction(this, pieces[1], args.ToArray(), currentLineNumber);
                PushBlockState(BlockType.function);
            }
            else if (pieces[0].Equals("if", System.StringComparison.OrdinalIgnoreCase))
            {
				pieces = ReplaceNumberValue(pieces, 0);
				Debug.Log (String.Join(",", pieces));
				pieces = ReplaceStructValue(pieces, 0);
				Debug.Log (String.Join(",", pieces));
				pieces = ReplacePrimitiveFunctionValue(pieces, 0);
				Debug.Log (String.Join(",", pieces));
                //conditional string
                if(pieces.Length < 4 && !functions.ContainsKey(pieces[1]))
                {
                    throw (new InterpreterException("[Basic Check] Invalid conditional statement.", currentLineNumber));
                }

                //get the results of each function called in the conditional statement
                bool eval = logic.EvaluateLogic(pieces, 1);
                if (!eval)
                    PushBlockState(BlockType.skip);
                else
                    PushBlockState(BlockType.conditional);
            }
            else
            {
                //WHILE LOOP
                //check if already inside a loop if so push the current LoopState into stack
                if(currentType == BlockType.loop)
                {
                    loopStates.Push(loopState);
                }

                loopState.startLine = currentLineNumber;
                
                //check the conditional
                bool eval = logic.EvaluateLogic(pieces, 1);
                if (!eval)
                {
                    PushBlockState(BlockType.skip);
                    Debug.Log("Setting type to SKIP");
                }
                else
                {
                    PushBlockState(BlockType.loop);
                    Debug.Log("Setting type to LOOP");
                }
                
            }
        }
        /* These cases cover reserved commands such as
         * TARGET - Look at a target
         * TURN - turn to angle position
         */
        else if (pieces[0].Equals("target", System.StringComparison.OrdinalIgnoreCase))
        {
			Debug.Log ("Target: " + pieces[1]);
			pieces = ReplaceToValues(pieces, 0);
			Debug.Log ("Target: " + pieces[1]);
            GameObject target;
            if (pieces[1].Equals("none", System.StringComparison.OrdinalIgnoreCase))
            {
                Root.LookAtTransform = null;
            }
            else if(objects.TryGetValue(pieces[1], out target))
            {
                Root.LookAtTransform = target.transform;
            }
            else
            {
                throw (new InterpreterException("Target object not recognized.", currentLineNumber));
            }
        }
        else if(pieces[0].Equals("turn", System.StringComparison.OrdinalIgnoreCase))
        {
           // Root.gameObject.transform.Rotate(Vector3.Forward,)
        }
        else
        {
            //Something else
            throw (new InterpreterException("[" + line + "] Syntax not recognized.", currentLineNumber));
        }
    }

    public bool IsBlockStart(string token)
    {
        return (token.Equals("while", System.StringComparison.OrdinalIgnoreCase)
            || token.Equals("if", System.StringComparison.OrdinalIgnoreCase)
            || token.Equals("function", System.StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// These functions are related to calling the functions
    /// </summary>

    public void CallFunction(string[] pieces)
    {
        //function call
        //get the arguments
        List<string> args = new List<string>();
        for (int i = 1; i < pieces.Length; i++)
        {
            if (numbers.ContainsKey(pieces[i]))
            {
                double repValue = 0;
                numbers.TryGetValue(pieces[i], out repValue);
                pieces[i] = "" + repValue;
            }
            args.Add(pieces[i]);
        }

        XBFunction func;
        functions.TryGetValue(pieces[0], out func);

        PushInterpreterState();
        currentLineNumber = 0;
        for (int j = 0; j < func.Length(); j++)
        {
            InterpretLine(func.GetLineWithParams(args.ToArray(), j));
            currentLineNumber++;
        }
        PopInterpreterState();
    }

    public void CallSystemFunction(string[] pieces)
    {

    }

    public string[] ExecuteAndReplace(string[] pieces, int start)
    {
        for (int i = start; i < pieces.Length; i++)
        {
            string token = pieces[i];
            if (functions.ContainsKey(token))
            {
                int prune = i;
                //call the function
                //compile arglist
                List<string> arglist = new List<string>();
                arglist.Add(token);
                i++;
                while (!(Logic.LogicOperators.ContainsKey(pieces[i]) || Arithmetic.ArithOperators.ContainsKey(pieces[i])))
                {
                    arglist.Add(pieces[i]);
                    i++;
                    if (i >= pieces.Length) break;
                }
                CallFunction(arglist.ToArray());

                //prune the pieces array
                string[] newPieces = new string[pieces.Length - (arglist.Count - 1)];

                int c = 0;
                for (; c < prune; c++)
                    newPieces[c] = pieces[c];
                newPieces[prune] = returnValue;
                for (int z = prune + arglist.Count; z < pieces.Length; z++)
                    newPieces[c++] = pieces[z];

                pieces = newPieces;
                i = prune;
                //Debug.Log("Execute and replace: (function " + token + " with " + (arglist.Count - 1) + " args) [" + pieces + "]");
            }
        }
        return pieces;
    }

    /// <summary>
    /// Types that the interpreter can comprehend
    /// XBObject is frame in which the interpreter stores (GameObject, DataType) touples.
    /// The DataType is neccessary to know what script to load using GetComponent
    /// </summary>
    public enum TokenType
    {
        Unknown = 0,
        XBObject = 1,
        SceneObject = 2,
        Number = 3,
        NumberValue = 4,
        Text = 5,
        Function = 6,
		Structure = 7,
		gameObject = 8
    }

    public struct TypeValue
    {
        public string value;
        public TokenType type;
    }

    /// <summary>
    /// Find out what type the given data is.
    /// </summary>
    /// <param name="name">A string literal containing the string token as it appears to the interpreter</param>
    /// <returns>The type of the string token in TokenType Enum</returns>
    public TokenType GetType(string name)
    {
        double d;
        if (name[0] == '"' && name[name.Length - 1] == '"')
        {
            return TokenType.Text;
        }
        else if (variables.ContainsKey(name))
        {
            return TokenType.XBObject;
        }
        else if (objects.ContainsKey(name))
        {
            return TokenType.XBObject;
        }
        else if (functions.ContainsKey(name))
        {
            return TokenType.Function;
        }
        else if (numbers.ContainsKey(name))
        {
            return TokenType.Number;
        }
		else if (structures.ContainsKey(name))
		{
			return TokenType.Structure;
		}
        else if (double.TryParse(name, out d))
        {
            return TokenType.NumberValue;
        }
        else
        {
            return TokenType.Unknown;
        }
    }

    /// <summary>
    /// Used by the interpreter to know what block types have what methods and variables
    /// </summary>
    Dictionary<string, InterpreterDesc> objectDescs = new Dictionary<string, InterpreterDesc>()
    {
        { "rocket", new InterpreterDesc(new string[]{ "start", "stop" }, new string[]{ "force" }) },
        { "missile", new InterpreterDesc(new string[]{ "fire" }, new string[]{ "speed" }) },
        { "radar", new InterpreterDesc(new string[]{ "ping" }, new string[]{ }) }
    };

    public static List<string> TYPES = new List<string>() { "number", "text", "object" };

    public string[] ReplaceNumberValue(string[] tokens, int start)
    {
        List<string> newtoks = new List<string>();
        for (int i = start; i < tokens.Length; i++)
        {
            if (numbers.ContainsKey(tokens[i]))
            {
                double repValue = 0;
                numbers.TryGetValue(tokens[i], out repValue);
                tokens[i] = "" + repValue;
            } else {
				Match match = Regex.Match(tokens[i], @"(.+)\[(.+)\]");
				if (match.Success)
				{
					string structName = match.Groups[1].Value;
					string key = match.Groups[2].Value;
					if (numbers.ContainsKey(key))
					{
						double repValue = 0;
						numbers.TryGetValue(key, out repValue);
						key = "" + repValue;
					}
					tokens[i] = structName + "[" + key + "]";
				}
			}
            newtoks.Add(tokens[i]);
        }
        return newtoks.ToArray();
    }

    public string[] ReplaceStructValue(string[] tokens, int start)
    {
        for (int i = start; i < tokens.Length; i++)
        {
            if(tokens[i].IndexOf('[') == -1) continue;

            Match match = Regex.Match(tokens[i], @"(.+)\[(.+)\]");

            if (match.Success)
            {
                string structName = match.Groups[1].Value;
                string key = match.Groups[2].Value;
                XBStructure XBS;
                if(structures.TryGetValue(structName, out XBS))
                {
                    string val;
                    if(XBS.TryGetValue(key, out val))
                    {
                        tokens[i] = val;
                    }
                    else
                    {
                        throw (new InterpreterException("The structure  (" + structName + ") does not have key (" + key + ")", currentLineNumber));
                    }
                }
                else
                {
                    throw (new InterpreterException("The structure  (" + structName + ") has not been initialized.", currentLineNumber));
                }
            }
        }
        return tokens;
    }

    public string[] ReplacePrimitiveFunctionValue(string[] pieces, int start)
    {
        for (int i = start; i < pieces.Length; i++)
        {
            string token = pieces[i];
            //Debug.Log("Token: " + token + " of " + GetString(pieces));
            if (ReservedPrimitiveFunctions.ContainsKey(token))
            {
                string commandToken = token;

                TokenType[] ArgTypes;
                ReservedPrimitiveFunctions.TryGetValue(token, out ArgTypes);

                int prune = i;
                //call the function
                //compile arglist
                List<string> arglist = new List<string>();
                i++;
                
                while (i <= (prune + ArgTypes.Length))
                {
                    if (i >= pieces.Length)
                        throw (new InterpreterException("The command  (" + token + ") does not have enough arguments.", currentLineNumber));
                    arglist.Add(pieces[i++]);
                }
                string ret = ResolveSystemFunctions(commandToken, arglist.ToArray());
                //Debug.Log("Primitive Return " + ret);
                //prune the pieces array
                string[] newPieces = new string[pieces.Length - arglist.Count];

                int c = 0;
                for (; c < prune; c++)
                    newPieces[c] = pieces[c];
                //Debug.Log(prune);
                newPieces[prune] = ret;
                //Debug.Log("New line with ret: " + GetString(newPieces));
                for (int z = prune + arglist.Count + 1; z < pieces.Length; z++)
                    newPieces[++c] = pieces[z];

                pieces = newPieces;
                //Debug.Log("New line: " + GetString(pieces));
                //Debug.Log("Execute and replace: (function " + token + " with " + (arglist.Count - 1) + " args) [" + pieces + "]");

                i = prune;
            }
        }
        return pieces;
    }

    public static Dictionary<string, TokenType[]> ReservedPrimitiveFunctions = new Dictionary<string, TokenType[]>() 
    {
        {"position(x)", new TokenType[]{ TokenType.XBObject } },
        {"position(y)", new TokenType[]{ TokenType.XBObject } },
        {"velocity(x)", new TokenType[]{ TokenType.XBObject } },
		{"velocity(y)", new TokenType[]{ TokenType.XBObject } },
		{"angle", new TokenType[]{ TokenType.XBObject } },
		{"count", new TokenType[]{ TokenType.Structure } },
        {"anglebwtween", new TokenType[]{ TokenType.XBObject, TokenType.XBObject } }
    };

    public string ResolveSystemFunctions(string func, string[] args)
    {
        TokenType[] ArgTypes;

        ReservedPrimitiveFunctions.TryGetValue(func, out ArgTypes);
        if(ArgTypes.Length != args.Length)
            throw (new InterpreterException("The command  (" + func + ") has not been given enough arguments. (Needed: " + args.Length + ")", currentLineNumber));
        int ac = 0;
        for (; ac < args.Length; ac++)
        {
            TokenType ExpectedType = ArgTypes[ac];
            TokenType ActualType = GetType(args[ac]);
            if(ExpectedType != ActualType)
            {
                throw (new InterpreterException("In command  (" + func + ") given argument  (" + args[ac] + ") of type " + ActualType + "  is not of expected type " + ExpectedType + ".", currentLineNumber));
            }
        }
        
        string value = "";
        switch(func.ToLowerInvariant())
        {
            //Args(1: GameObject)
            case "position(x)":
            case "position(y)":
            case "velocity(x)":
            case "velocity(y)":
			case "angle":
                GameObject go;
                if (objects.TryGetValue(args[0], out go))
                {
                    switch(func.ToLowerInvariant())
                    {
                        case "position(x)":
                            value = "" + go.transform.position.x;
                            //Debug.Log("Log " + value);
                            break;
                        case "position(y)":
                            value = "" + go.transform.position.y;
                            break;
                        case "velocity(x)":
                            Rigidbody RBX = go.GetComponent<Rigidbody>();
                            if (RBX != null)
                            {
                                value = "" + RBX.velocity.x;
                            }
                            else
                            {
                                value = "0";
                            }
                            break;
                        case "velocity(y)":
                            Rigidbody RBY = go.GetComponent<Rigidbody>();
                            if (RBY != null)
                            {
                                value = "" + RBY.velocity.y;
                            }
                            else
                            {
                                value = "0";
                            }
                            break;
						case "angle":
							value = "" + go.transform.rotation.z;
							break;
                    }
				}
                break;
			//Args(1: Structure)
			case "count":
				XBStructure structure;
				if(structures.TryGetValue(args[0], out structure))
				{
					value = "" + structure.GetLength();
				}
				break;

            //Args(1: GameObject, 2: GameObject)
            case "anglebetween":
                GameObject go1, go2;
                if (objects.TryGetValue(args[0], out go1) && objects.TryGetValue(args[1], out go2))
                {
                    value = "" + Quaternion.Angle(go1.transform.rotation, go2.transform.rotation);
                }
                break;
            default:
                break;
        }
        return value;
    }

    public string[] ReplaceToValues(string[] tokens, int start)
    {
        //Debug.Log("[Raw expression] " + GetString(tokens));
        tokens = ReplaceNumberValue(tokens, start);

        tokens = ReplaceStructValue(tokens, 0);

        tokens = ReplacePrimitiveFunctionValue(tokens, 0);

        //get the results of all functions in the Arithmetic or Logical line
        tokens = ExecuteAndReplace(tokens, 0);
        //Debug.Log("[New expression] " + GetString(tokens));
        return tokens;
    }

    /// <summary>
    /// Contains a touple of GameObject and DataType
    /// DataType will be used to know which type of script to load
    /// </summary>
    struct XBGameObject
    {
        public GameObject gameObject;
        public string datatype;
    }

    /// <summary>
    /// Find if two string tokens from the interpreter are of the same data type
    /// </summary>
    /// <param name="tok1">String token to compare</param>
    /// <param name="tok2">String token to compare</param>
    /// <returns>True if the types are same false if not</returns>
    public bool IsSameType(string tok1, string tok2)
    {
        return GetType(tok1) == GetType(tok2);
    }

    public void CreateStructureFromArray(string name, string[] array)
    {
        string[] kv = new string[array.Length];
        int cnt = 0;
        foreach(string obj in array)
        {
            kv[cnt++] = cnt + "=" + obj;
        }
        CreateStructure(name, kv);
    }

    public void CreateStructure(string name, string[] keyValue)
    {
        XBStructure XBS = new XBStructure();

        foreach(string kv in keyValue)
        {
            string[] pair = kv.Split('=');
            if (pair.Length != 2)
                throw new InterpreterException("Incoherent key value, must be in format key=value [" + kv + "]", currentLineNumber);
            XBS.Set(pair[0], pair[1]);
			Debug.Log("Set " + pair[0] + " - " + pair[1]);
        }

        structures.Add(name, XBS);
    }

    //Exceptions
    public void ThrowError(string message)
    {
        throw new InterpreterException(message, currentLineNumber);
    }

    public void ThrowError(string message, int line)
    {
        throw new InterpreterException(message, line);
    }


    public class NoBlockWithNameFoundError : InterpreterException
    {
        public NoBlockWithNameFoundError(string msg, int line) : base(msg, line) 
        {
            Debug.Log("[ERROR]" + msg + " @ LINE: " + line);
        }
    }

    public class InterpreterException : Exception
    {
        public InterpreterException(string msg, int line)
            : base(msg)
        {
            Debug.Log("[ERROR]" + msg + " @ LINE: " + line);
			PrintToDebugUI("[ERROR]" + msg + " @ LINE: " + line);
        }
    }

    public struct InterpreterDesc
    {
        public string[] methods;
        public string[] variables;

        public InterpreterDesc(string[] m, string[] v)
        {
            methods = m;
            variables = v;
        }
    }

    //helper methods
    public bool Contains(string[] arr, string key)
    {
        if (System.Array.IndexOf(arr, key) > -1)
            return true;
        else
            return false;
    }

    public static void printDebug(string msg, int ln)
    {
        Debug.Log("[DEBUG]" + msg + " @ LINE:" + ln);
		PrintToDebugUI ("[DEBUG]" + msg + " @ LINE:" + ln);
    }

    public static void printError(string msg, int ln)
    {
		Debug.Log("[ERROR]" + msg + " @ LINE:" + ln);
		PrintToDebugUI ("[ERROR]" + msg + " @ LINE:" + ln);
    }

    public static string GetString(string[] array)
    {
        string r = "";
        foreach (string s in array)
            r += s + " ";
        return r.Trim();
    }

    public static bool CompChar(char x, char y)
    {
        return char.ToLowerInvariant(x) == char.ToLowerInvariant(y);
    }

	public static void PrintToDebugUI(string d)
	{
		Text dtext = GameObject.Find("DebugText").GetComponent<Text>();
		dtext.text = dtext.text + "\n" + d;
	}
}

/* Code Manual
 *
     * (OBJECT TYPE) (OBJECT NAME) <- GETOBJECT (OBJECT NAME IN SCENE)
     * 
     * Methods Call
     * (OBJECT NAME) (METHOD NAME)
     * 
     * Method Call with Delay
     * DELAY (SECONDS) (OBJECT NAME) (METHOD NAME)
     * 
     * WAIT (SECONDS)
     * Pauses interpretation for given amount of time
     * 
     * Blocks start on the line after a block is started by a command/instruction, the block ends at END
     * An example of an instruction that starts a block is FUNCTION
     * 
     * FUNCTION (FUNCTION NAME) ARGTYPE{NUMBER,TEXT,OBJECT} (VALUE/NAME), ....
     * (OBJECT NAME) (OBJECT METHOD)
     * END
     * 
     * Example
     * FUNCTION GO_UP NUMBER FRC
     * RK FORCE FRC 
     * END
     * 
     * GO_UP FRC=10
     * 
     * Conditionals
     * IF CONDITION
     * BLOCK
     * END
     * 
     * Comparators
     * EQUALS ( == )
     * GREATER THAN ( > )
     * LESS THAN ( < )
     * AND ( && )
     * OR ( || )
     * NOT ( ! )
     * 
     * Loops
     * WHILE CONDITION
     * BLOCK
     * END
     * 
     * BREAK and CONTINUE
    */

//SAMPLE CODE

//string code = "ROCKET RK <- GETOBJ RocketBoosterBlockCube" + '\n' + "RK START" + '\n' + "DELAY 2 RK FORCE -2" + '\n' + "DELAY 4 RK STOP";
//string code = "ROCKET RK <- GETOBJ RocketBoosterBlockCube";
//string code = "ROCKET RK <- GETOBJ RocketBoosterBlockCube" + '\n' + "RK START";
//string code = "ROCKET RK <- GETOBJ RocketBoosterBlockCube2" + '\n' + "RK START";
//string code = "ROCKET RK <- GETOBJ RocketBoosterBlockCube2" + '\n' + "RK START"+ '\n' + "ROCKET RK2 <- GETOBJ RocketBoosterBlockCube2" + '\n' + "DELAY 2 RK2 START";

/*string code = 
            "ROCKET RK <- GETOBJECT ROCKET1\n" + 
            "RK START\n" + 
            "ROCKET RK2 <- GETOBJECT ROCKET2\n" + 
            "DELAY 1 RK2 START\n" +
            "FUNCTION STOPALL\n" + 
            "RK STOP\n" +
            "RK2 STOP\n" +
            "END\n" + 
            "STOPALL";*/

/*string code =
            "ROCKET RK1 <- GETOBJECT ROCKET1\n" +
            "ROCKET RK2 <- GETOBJECT ROCKET2\n" +
            "FUNCTION MANUEVER\n" +
            "RK2 START\n" +
            "DELAY 2 RK2 STOP\n" +
            "DELAY 6 RK1 START\n" +
            "DELAY 7.5 RK1 STOP\n" +
            "DELAY 9 RK1 START\n" +
            "DELAY 9 RK2 START\n" +
            "DELAY 13 RK1 STOP\n" +
            "DELAY 13 RK2 STOP\n" +
            "DELAY 14 RK1 START\n" +
            "DELAY 16 RK1 STOP\n" +
            "DELAY 17 RK1 START\n" +
            "DELAY 17 RK2 START\n" +
            "DELAY 22 RK1 STOP\n" +
            "DELAY 26 RK2 STOP\n" +
            "DELAY 27 RK1 START\n" +
            "DELAY 27 RK2 START\n" +
            "DELAY 29 RK1 STOP\n" +
            "DELAY 29 RK2 STOP\n" +
            "END\n" +
            "MANUEVER";*/

/*string code =
            "ROCKET RK1 <- GETOBJECT ROCKET1\n" +
            "ROCKET RK2 <- GETOBJECT ROCKET2\n" +
            "MISSILE M1 <- GETOBJECT missiletype11\n" +
            "MISSILE M2 <- GETOBJECT missiletype12\n" +
            "FUNCTION RK1TIME NUMBER STRT NUMBER STP\n" +
            "DELAY STRT RK1 START\n" +
            "DELAY STP RK1 STOP\n" +
            "END\n" +
            "FUNCTION RK2TIME NUMBER STRT NUMBER STP\n" +
            "DELAY STRT RK2 START\n" +
            "DELAY STP RK2 STOP\n" +
            "END\n" +
            "FUNCTION RKTIME NUMBER STRTP NUMBER STPP\n" +
            "RK1TIME STRTP STPP\n" +
            "RK2TIME STRTP STPP\n" +
            "END\n" +
            "RK2TIME 0 2\n" +
            "RKTIME 3 6\n" +
            "DELAY 7 M1 FIRE\n" +
            "RK1TIME 8 11\n" +
            "RKTIME 12 16\n" +
            "DELAY 18 M1 FIRE\n" +
            "RK1TIME 19 23\n" +
            "DELAY 25 M1 FIRE";*/

/*string code =
            "FUNCTION AVG NUMBER X NUMBER Y\n"+
            "NUMBER A = (X + Y) / 2\n" +
            "RETURN A\n" + 
            "END\n" +
            "NUMBER R = AVG 8 4\n" +
            "IF R > 5\n" +
            "PRINT R\n" +
            "END";*/

/*string code =
            "STRUCT XB ALT=50 X=100 Y=200\n" +
            "PRINT XB[X] + XB[Y]";*/

/*string code =
            "NUMBER C = 0\n" +
            "WHILE C < 3\n" +
            "IF C == 2\n" +
            "PRINT C\n" +
            "END\n" +
            "C = C + 1\n" +
            "PRINT C\n" +
            "END";*/
