using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class DatabaseAdapter : SealedControlledScript {
	public GameObject dataPasser;
	DataPassingScript DPS;

	public static string TEST_JSON;

	protected override void RunScript()
	{
		string nl = "\\n";
		string fighter1_start =
				
				"RADAR R1 = GET RADARGARAGE10" + nl +
				"NUMBER N = 1" + nl +
				"NUMBER CNT = COUNT R1.VALUES" + nl +
				"PRINT AMOUNT OF CANDIDATES CNT" + nl +
				"WHILE N <= CNT" + nl +
				"PRINT CHECKING N, ANGLE R1.VALUES[N]" + nl +
				"IF ANGLE R1.VALUES[N] != 0" + nl +
				"PRINT THIS IS THE ONE!" + nl +
				"TARGET R1.VALUES[N]" + nl +
				"END" + nl +
				"N = N + 1" + nl +
				"END";

		TEST_JSON = "[{\"name\":\"Fighter 1\",\"id\":\"1\",\"uid\":\"1\",\"date\":\"2015-05-13\",\"data\":\"[ [ [ \"RocketGarage6\", \"null\", \"null\", \"null\", \"RocketGarage7\" ], [ \"(Basic)5\", \"(Basic)3\", \"null\", \"(Basic)4\", \"(Basic)6\" ], [ \"null\", \"(Basic)1\", \"XShip\", \"(Basic)2\", \"null\" ], [ \"null\", \"MissileGarage8\", \"RadarGarage10\", \"MissileGarage9\", \"null\" ] ], [ [ \"3\", \"0\", \"0\", \"0\", \"3\" ], [ \"1\", \"1\", \"0\", \"1\", \"1\" ], [ \"0\", \"1\", \"1\", \"1\", \"0\" ], [ \"0\", \"4\", \"5\", \"4\", \"0\" ] ], \"2\", \"2\" ]\",\"img\":\"\",\"start\":\""+fighter1_start+"\",\"update\":\"\"}"
				+ ",{\"name\":\"Empty Slot\",\"id\":\"6\",\"uid\":\"2\",\"date\":\"2015-05-13 07:44:27\",\"data\":\"\",\"img\":\"\",\"start\":\"\",\"update\":\"\"}"
				+ ",{\"name\":\"Empty Slot\",\"id\":\"7\",\"uid\":\"2\",\"date\":\"2015-05-13 07:44:31\",\"data\":\"\",\"img\":\"\",\"start\":\"\",\"update\":\"\"}]";
		DPS = dataPasser.GetComponent<DataPassingScript> ();
	}
	
	public List<XBuildData> LoadXBuilds(string json)
	{
		JSONNode result = JSON.Parse (json);
		List<XBuildData> xbuilds = new List<XBuildData> ();
		for (int i = 0; i < result.Count; i++) {
			JSONNode node = result[i];
			string val = "ID: " + node["id"] + " uid: " + node["uid"] + " data: " + node["data"];
			//Application.ExternalCall ("ConsoleW", "Loaded: " + val);
			xbuilds.Add(LoadXBuildData(node["id"], node["name"], node["date"], node["data"], node["img"], node["start"], node["update"]));
		}
		return xbuilds;
	}

	public XBuildData LoadXBuildData(string id, string name, string date, string json, string img, string start, string update)
	{
		XBuildData XBD;

		if(json != null)
		{
			if (!(json.Equals ("null", System.StringComparison.InvariantCulture)) && !(json.Equals ("", System.StringComparison.InvariantCulture))) {
				XDebug.Log ("Parsing xbuild " + name + " with data of length " + json.Length);

				JSONNode arr = JSON.Parse (json);

				JSONArray ja_names = arr [0].AsArray;
				JSONArray ja_types = arr [1].AsArray;
				float center_x = arr [2].AsFloat;
				float center_y = arr [3].AsFloat;

				string[,] names = new string[ja_names.Count, ja_names [0].Count];
				int[,] types = new int[ja_names.Count, ja_names [0].Count];

				for (int i = 0; i < ja_names.Count; i++) {
					for (int j = 0; j < ja_names[0].Count; j++) {
						names [i, j] = arr [0] [i] [j].Value;
						types [i, j] = arr [1] [i] [j].AsInt;
					}
				}
				//XDebug.Log (ja_names.Count + " " + ja_names [0].Count + " (" + ja_names [0] [0].Value + ") (" + ja_names [0] [1].Value + ") (" + ja_names [0] [2].Value + ")");
				//XDebug.Log (names.GetLength (0) + " " + names.GetLength (1));
				XBD = new XBuildData (types, names, new Vector2 (center_x, center_y));
			} else {
				XBD = new XBuildData (null, null, new Vector2 (0, 0));
			}
		} else {
			XDebug.Log ("Parsing xbuild " + name + " with no data");
			XBD = new XBuildData (null, null, new Vector2 (0, 0));
		}

		XBD.name = name;
		XBD.date = date;
		XBD.image = img;
		XBD.startCode = start;
		XBD.updateCode = update;
		XBD.db_id = id;
		return XBD;
	}
	
	public void SaveXBuildData(string IMG_DATA, string start_code, string update_code)
	{
		XDebug.Log ("Begining save..");
		XBuildData xbuild = DPS.XBD;

		JSONArray json = new JSONArray ();
		JSONArray names = new JSONArray ();
		if (xbuild.names != null) {
			for (int i = 0; i < xbuild.names.GetLength(0); i++) {
				JSONArray nameline = new JSONArray ();
				for (int j = 0; j < xbuild.names.GetLength(1); j++) {
					if (xbuild.names [i, j] != null)
						nameline.Add (xbuild.names [i, j]);
					else
						nameline.Add ("null");
				}
				names.Add (nameline);
			}
		}
		json.Add("names", names);

		JSONArray types = new JSONArray ();
		if (xbuild.types != null) {
			for (int i = 0; i < xbuild.types.GetLength(0); i++) {
				JSONArray typeline = new JSONArray ();
				for (int j = 0; j < xbuild.types.GetLength(1); j++) {
					if (xbuild.types [i, j] != null)
						typeline.Add (xbuild.types [i, j] + "");
					else
						typeline.Add ("0");
				}
				types.Add (typeline);
			}
		}
		json.Add("types", types);

		json.Add("center_x", new JSONData(xbuild.center.x));
		json.Add("center_y", new JSONData(xbuild.center.y));

		XDebug.Log ("Saving... @id" + xbuild.db_id + " " + json.ToString());
		XDebug.Log ("Start code: \n" + start_code);
		XDebug.Log ("Update Code: \n" + update_code);

		//XDebug.Log ("Image data size: \n" + IMG_DATA.Length);
		Application.ExternalCall ("SaveXBuild", xbuild.db_id, xbuild.name, json.ToString (), IMG_DATA, start_code, update_code);
	}

	public void DeleteXBuild(string xid)
	{
		XDebug.Log ("Deleting xbuild with id: " + xid);
		Application.ExternalCall ("DeleteXBuild", xid);
	}
}
