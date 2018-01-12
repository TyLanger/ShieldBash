using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Astar : MonoBehaviour {

	public MapGen mapGen;

	bool canSee(Vector3 point)
	{
		RaycastHit hit;
		if (Physics.Raycast (transform.position, (point - transform.position), out hit, Vector3.Distance (point, transform.position))) {
			
			// hit something
			return false;
			
		}
		return true;
	}

	public List<Vector3> FindPath(Vector3 start, Vector3 goal)
	{
		AstarNode startNode = mapGen.WorldPointToAstarNode (start);
		AstarNode goalNode = mapGen.WorldPointToAstarNode (goal);

		// Nodes already evaluated
		// use a hasSet because contains() is O(1)
		// all you need the closed set for is to add old items to and to check if they are already in it
		HashSet<AstarNode> closedSet = new HashSet<AstarNode>();

		// discovered, but not evaluated yet
		// heap openSet
		Heap<AstarNode> openSet = new Heap<AstarNode>(mapGen.width * mapGen.height);

		openSet.Add( startNode);

		while (openSet.Count > 0) {

			// current is the node with the lowest fScore
			// in the openSet heap, it is the first item
			AstarNode current = openSet.ReturnFirst ();

			// add to the closed set (it is now evaluated)
			closedSet.Add (current);

			// this doesn't return true, but c.pos == g.pos does
			// it works if i'm using already made AstarNodes and not just making a new one everytime

			if (current == goalNode) {
				return ReconstructPath (startNode, current);
			}

			// loop through current's neighbours
			foreach(AstarNode neighbour in mapGen.GetNeighbours (current)) {
				if (closedSet.Contains (neighbour)) {
					// already been evaluated; skip
					continue;
				}
				
				int tentative_gScore = current.gCost + costBetween (current, neighbour);
				if (tentative_gScore < neighbour.gCost || !openSet.Contains (neighbour)) {
					// update all this info for the neighbour
					neighbour.gCost = tentative_gScore;
					neighbour.hCost = costBetween (neighbour, goalNode);
					neighbour.cameFrom = current;

					// if they weren't in the openSet, add them
					if (!openSet.Contains (neighbour)) {
						//UnityEngine.Debug.Log("Add neighbour to openSet");
						openSet.Add (neighbour);
					} else {
						// if they were already in the open set,
						// this means their gScore was lower
						// update their position in the heap to reflect their new scores
						openSet.UpdateItem (neighbour);
					}
				}
			}
		}
		UnityEngine.Debug.Log ("Astar failed");
		return new List<Vector3> ();
	}

	List<Vector3> ReconstructPath(AstarNode startNode, AstarNode current)
	{
		List<Vector3> path = new List<Vector3> ();

		// this doesn't add the start node to the list, but you don't need it
		// the start node is where the agent calling for the path currently is
		// they don't need to path to where they currently are
		while (current != startNode) {
			path.Add (current.position + new Vector3 (0, -2, 0));
			current = current.cameFrom;
		}
		path.Reverse ();
		return path;
	}

	int costBetween(AstarNode start, AstarNode goal)
	{
		// manhattan distance
		// because only 4-connected
		return Mathf.RoundToInt(Mathf.Abs(start.xGrid - goal.xGrid) + Mathf.Abs(start.yGrid - goal.yGrid));
	}

	public bool isWalkable(Vector3 point)
	{
		return mapGen.isWalkable (point);
	}
}
