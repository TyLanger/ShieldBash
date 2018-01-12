using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// most code from https://youtu.be/3Dw5d7PlcTM
public class AstarNode : IHeapItem<AstarNode> {

	public bool walkable;
	public Vector3 position;
	public int xGrid;
	public int yGrid;

	public int gCost;
	public int hCost;
	public AstarNode cameFrom;
	int HeapIndex;

	public AstarNode(bool _walkable, Vector3 pos, int x, int y)
	{
		walkable = _walkable;
		position = pos;
		xGrid = x;
		yGrid = y;
	}

	public int fCost {
		get{
			return gCost + hCost;
		}
	}

	public int heapIndex {
		get {
			return HeapIndex;
		}
		set {
			HeapIndex = value;
		}
	}

	public int CompareTo(AstarNode node)
	{
		// first compare fCosts (because that is what is important between nodes
		int compare = fCost.CompareTo (node.fCost);

		// if fCosts are equal,
		// compare hCosts
		if (compare == 0) {
			compare = hCost.CompareTo (node.hCost);
		}

		// using default int.CompareTo() is based on bigger ints having higher priority
		// in this case, the lower the costs, the higher the priority
		return -compare;
	}
}
