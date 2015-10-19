using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class XBuildParseScript : SealedControlledScript {
	public static string XBUILD_PARSE_SCRIPT_TAG = "XbuildParseScript";
	GraphNode<GarageBlock> root;

	public void GetXBuild()
	{
		XBuildData XBD = GetXBuildData(MakeGraph ());
		XBD.PrintXBuildData ();

		DataPassingScript dps = GameObject.Find ("XBuildData").GetComponent<DataPassingScript> ();

		//get old XBD data

		if (dps.XBD != null) {
			XBuildData old = dps.XBD;
			dps.XBD = XBD;
			DebugLog("Old " + old.db_id);
			dps.XBD.db_id = old.db_id;
		}
		//XDebug.Verbose("Getting JSON");
		//DatabaseAdapter adapter = this.gameObject.GetComponent<DatabaseAdapter> ();
		//adapter.SaveXBuildData ("");
	}

	Vector2[] scan = new Vector2[] { new Vector2(-1, 0), new Vector2(1, 0), 
		new Vector2(0, -1), new Vector2(0, 1) };

	Dictionary<Vector3, GraphNode<GarageBlock>> MakeGraph()
	{
		GameObject[] goBlocks = GameObject.FindGameObjectsWithTag("garageblock");

		if (goBlocks == null) {
			XDebug.Verbose(XBUILD_PARSE_SCRIPT_TAG, "Empty Xbuilder");
			return null;
		}

		List<GarageBlock> blocks = new List<GarageBlock> (); 
		Dictionary<Vector3, GarageBlock> blockSet = new Dictionary<Vector3, GarageBlock> ();

		foreach(GameObject goBlock in goBlocks)
		{
			GarageBlock gb = ((GarageObjectScript)goBlock.GetComponent<GarageObjectScript>()).GetGarageBlock();
			blockSet.Add(gb.position, gb);
			blocks.Add(gb);
		}

		GarageBlock first = ((GarageObjectScript)goBlocks[0].GetComponent<GarageObjectScript> ()).GetGarageBlock ();
		GraphNode<GarageBlock> firstNode = new GraphNode<GarageBlock> (first);
		blockSet.Remove (first.position);

		Queue<GraphNode<GarageBlock>> GQueue = new Queue<GraphNode<GarageBlock>> ();
		GQueue.Enqueue(firstNode);

		List<Dictionary<Vector3, GraphNode<GarageBlock>>> ExploredTrees = new List<Dictionary<Vector3, GraphNode<GarageBlock>>> ();

		int tree = 0;
		ExploredTrees.Add(new Dictionary<Vector3, GraphNode<GarageBlock>> ());

		int safety = 1;

		XDebug.Verbose ("First Node: " + firstNode.Value.name + " @ " + firstNode.Value.position);

		while (GQueue.Count > 0 && safety++ < 100) {
			GraphNode<GarageBlock> current = GQueue.Dequeue();


			Vector2 curpos = new Vector2(current.Value.position.x, current.Value.position.y);
			//check in all four directions
			foreach(Vector2 dir in scan)
			{
				Vector2 scanpos = new Vector2();
				scanpos.x = curpos.x + dir.x;
				scanpos.y = curpos.y + dir.y;
				foreach(GarageBlock block in blocks)
				{
					if(block.Equals(scanpos))
					{
						//create GraphNode for this block
						//add this block to adjacency list of current
						//if not explored and not in queue then add this block to the Queue
						//remove this block from the set
						GraphNode<GarageBlock> node = new GraphNode<GarageBlock> (block);
						current.Neighbors.Add(node);
						if(!ExploredTrees[tree].ContainsKey(block.position))
						{
							bool inQ = false;
							foreach(GraphNode<GarageBlock> gn in GQueue)
							{
								if(gn.Value.position.Equals(block.position))
								{
									inQ = true;
									break;
								}
							}
							//check if this block is already in the queue
							if(!inQ)
								GQueue.Enqueue(node);
						}
						break;
					}
				}
			}
			if(ExploredTrees[tree].ContainsKey(current.Value.position))
				XDebug.Verbose(XBUILD_PARSE_SCRIPT_TAG, "Err: Name " + current.Value.name + " @ " + current.Value.position);
			ExploredTrees[tree].Add(current.Value.position, current);
			blockSet.Remove(current.Value.position);

			if(GQueue.Count == 0 && blockSet.Count > 0)
			{
				XDebug.Verbose(XBUILD_PARSE_SCRIPT_TAG, "New tree...");
				//every node of a particular tree has been explored
				//but other trees might be present in the graph

				tree++;
				ExploredTrees.Add(new Dictionary<Vector3, GraphNode<GarageBlock>> ());
				foreach(KeyValuePair<Vector3, GarageBlock> pair in blockSet)
				{
					GraphNode<GarageBlock> node = new GraphNode<GarageBlock> (pair.Value);
					GQueue.Enqueue(node);
					break;
				}
			}

			//Print Queue
			XDebug.Verbose(XBUILD_PARSE_SCRIPT_TAG, "");
			XDebug.Verbose(XBUILD_PARSE_SCRIPT_TAG, "");
			XDebug.Verbose(XBUILD_PARSE_SCRIPT_TAG, "===================================================");
			PrintQueue(GQueue);
			XDebug.Verbose(XBUILD_PARSE_SCRIPT_TAG, "");
			PrintGDict(blockSet);
			XDebug.Verbose(XBUILD_PARSE_SCRIPT_TAG, "");
			PrintExploredTree(ExploredTrees[tree]);
		}

		Dictionary<Vector3, GraphNode<GarageBlock>> largest = new Dictionary<Vector3, GraphNode<GarageBlock>>();
		foreach (Dictionary<Vector3, GraphNode<GarageBlock>> dt in ExploredTrees) {
			if(dt.Count > largest.Count)
				largest = dt;
		}
		XDebug.Verbose("Largest Tree had " + largest.Count + " items");
		return largest;
	}

	XBuildData GetXBuildData(Dictionary<Vector3, GraphNode<GarageBlock>> map)
	{
		if (map == null) {
			XDebug.Verbose(XBUILD_PARSE_SCRIPT_TAG, "Empty Map");
			return new XBuildData(null, null, new Vector2(0,0));
		}

		XDebug.Verbose("Making matrix");
		float GX = 0, GY = 0, LX = 0, LY = 0;
		//GraphNode<GarageBlock> BoundGX = null, BoundGY = null, BoundLX = null, BoundGY = null;
		//find largest x and y
		foreach(KeyValuePair<Vector3, GraphNode<GarageBlock>> pair in map)
		{
			Vector3 pos = pair.Key;
			if(GX < pos.x)
			{
				GX = pos.x;
				//BoundGX = pair.Value
			}
			if(GY < pos.y)
			{
				GY = pos.y;
				//BoundGY = pair.Value
			}
			if(LX > pos.x)
			{
				LX = pos.x;
				//BoundLX = pair.Value
			}
			if(LY > pos.y)
			{
				LY = pos.y;
				//BoundLY = pair.Value
			}
		}
		XDebug.Verbose(XBUILD_PARSE_SCRIPT_TAG, "Bounds " + GX + " " + GY + " " + LX + " " + LY);

		//int crossingX = (Math.Sign (GX) != Math.Sign (LX) || GX == 0f || LX == 0f) ? 1 : 0;
		//int crossingY = (Math.Sign (GY) != Math.Sign (LY) || GY == 0f || LY == 0f) ? 1 : 0;
		int xlen = (int)(GX - LX + 1f);
		int ylen = (int)(GY - LY + 1f);
		GarageBlock[,] matrix = new GarageBlock[ylen,xlen];
		int[,] typeMatrix = new int[ylen,xlen];
		string[,] nameMatrix = new string[ylen,xlen];

		//calculate offset
		//set the block with the largest value
		int offsetX = (int) LX;
		int offsetY = (int) LY;

		int centerX = (int)xlen / 2;
		int centerY = (int)ylen / 2;

		XDebug.Verbose(XBUILD_PARSE_SCRIPT_TAG, "Offset x,y " + offsetX + ", " + offsetY);
		double least = double.MaxValue;
		Vector2 center = new Vector2 ();
		for (int y = ylen - 1; y >= 0; y--) {
			for(int x = 0; x < xlen; x++)
			{
				//is there a block at this coord
				int cX = x + offsetX;
				int cY = y + offsetY;
				GraphNode<GarageBlock> node= null;
				XDebug.Verbose("Serching " + cX + "," + cY + " for " + x + ", " + y);
				if(map.TryGetValue(new Vector3(cX, cY, 0), out node))
				{
					matrix[y,x] = node.Value;
					typeMatrix[y,x] = (int) node.Value.type;
					nameMatrix[y,x] = node.Value.name;

					if(typeMatrix[y,x] == 1 || typeMatrix[y,x] == 2)
					{
						double dist = Math.Pow((Math.Pow((double)(centerX - x), 2d) + Math.Pow((double)(centerY - y), 2d)), 0.5);
						if(dist < least)
						{
							least = dist;
							center = new Vector2(x,y);
						}
					}
					//update center
				}
			}
		}

		XBuildData XBD = new XBuildData (typeMatrix, nameMatrix, center);

		//update the name
		
		//get the name of the xbuild anything with no blocks is named Empty Slot
		if (XBD.names != null) {
			XBD.name = XBD.names [(int)center.y, (int)center.x];
		} else {
			XBD.name = "Empty Slot";
		}

		return XBD;
	}

	void PrintQueue(Queue<GraphNode<GarageBlock>> q)
	{
		XDebug.Verbose("Printing Queue...");
		foreach (GraphNode<GarageBlock> gb in q) {
			XDebug.Verbose (gb.Value.name + " @ " + gb.Value.position);
		}
	}

	void PrintGDict(Dictionary<Vector3, GarageBlock> gd)
	{
		XDebug.Verbose(XBUILD_PARSE_SCRIPT_TAG, "Printing Set of GarageBlocks...");
		foreach (KeyValuePair<Vector3, GarageBlock>pair in gd) {
			XDebug.Verbose (pair.Value.name + " @ " + pair.Key);
		}
	}

	void PrintExploredTree(Dictionary<Vector3, GraphNode<GarageBlock>> tree)
	{
		XDebug.Verbose(XBUILD_PARSE_SCRIPT_TAG, "Printing Explored tree...");
		foreach (KeyValuePair<Vector3, GraphNode<GarageBlock>>pair in tree) {
			XDebug.Verbose (pair.Value.Value.name + " @ " + pair.Key);
		}
	}
}
