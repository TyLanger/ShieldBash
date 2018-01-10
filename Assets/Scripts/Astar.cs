using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Astar : MonoBehaviour {

	public MapGen mapGen;

	// For Testing
	public Transform player;
	List<Vector3> path;

	// Use this for initialization
	void Start () {

	}

	void Update() {

		// For testing
		if (Input.GetButtonDown ("Jump")) {
			path = FindPath (transform.position, player.position);
		}
		if (path != null) {
			if (path.Count > 0) {
				if (path.Count > 1) {
					if (canSee (path [1])) {
						// if you can see the next point, delete the current point so you go to that one instead
						path.RemoveAt (0);	
					}
				}
				
				transform.position = Vector3.MoveTowards (transform.position, path [0], 10 * Time.deltaTime);
				if (Vector3.Distance (transform.position, path [0]) < 0.1f) {
					path.RemoveAt (0);
				}
			}
		}

	}

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
		start = mapGen.AdjustToGridPoint (start);
		//Debug.Log ("Start: " + start);
		goal = mapGen.AdjustToGridPoint (goal);
		//Debug.Log ("Goal: " + goal);
		// Nodes already evaluated
		List<Vector3> closedSet = new List<Vector3>();

		// discovered, but not evaluated yet
		List<Vector3> openSet = new List<Vector3>();
		openSet.Add( start);

		// both should be initially filled with infinity in each slot
		Dictionary<Vector3, int> gScore = new Dictionary<Vector3, int>();
		Dictionary<Vector3, int> fScore = new Dictionary<Vector3, int>();


		// cost of going from start to start is 0
		// I'm not sure if this is right....?
		gScore [start] = 0;

		//
		fScore[start] = heuristicCostEstimate(start, goal);

		// Dictionary of path
		// key is the start node
		// came from is the node it came from
		Dictionary<Vector3, Vector3> cameFrom = new Dictionary<Vector3, Vector3>();
		while (openSet.Count > 0) {
			//Debug.Log ("OpenSet > 0");

			// current node is the node in open set with the lowest fScore
			// iterate through open set, checking fScores
			Vector3 current = openSet[0];
			foreach (var open in openSet) {
				if (fScore [current] > fScore [open]) {
					current = open;
				}
			}
			//Debug.Log(current + " " + fScore[current]);
			// close enough
			// Vector3s may never be actually equal
			if (Mathf.Abs (current.x - goal.x) < 1f && Mathf.Abs (current.z - goal.z) < 1f) {
				//Debug.LogFormat ("Equal: {0} = {1}", current, goal);
				return ReconstructPath (cameFrom, current);
			}

			if (current == goal) {
				return ReconstructPath (cameFrom, current);
			}

			// remove the current from the open set (it has now been evaluated
			openSet.Remove (current);
			closedSet.Add (current);


			List<Vector3> neighbours = mapGen.GetNeighbours (current);

			//Debug.Log ("Number of neighbours: " + neighbours.Count);
			// loop through current's neighbours
			foreach(Vector3 neighbour in neighbours) {
				//Vector3 neighbour = GetNeighbour (current, i);

				if (closedSet.Contains (neighbour))
					continue;

				if (!openSet.Contains (neighbour)) {
					openSet.Add (neighbour);
				}

				int tentative_gScore = gScore [current] + costBetween (current, neighbour);
				if (!gScore.ContainsKey (neighbour)) {
					gScore.Add (neighbour, int.MaxValue);
					fScore.Add (neighbour, int.MaxValue);
				}
				if (tentative_gScore >= gScore [neighbour]) {
					// not a better path
					continue;
				}

				//Debug.Log ("Best so far");
				// this path is the best so far
				cameFrom[neighbour] = current;
				gScore [neighbour] = tentative_gScore;
				fScore [neighbour] = gScore [neighbour] + heuristicCostEstimate (neighbour, goal);

			}

		}
		Debug.Log ("Astar failed");
		return new List<Vector3> ();
	}

	List<Vector3> ReconstructPath (Dictionary<Vector3,Vector3> cameFrom, Vector3 current)
	{
		// make a list to store the path
		List<Vector3> path = new List<Vector3> ();
		// add the current point to the path
		path.Add (current + new Vector3(0, -2, 0));
		// while the current point is in cameFrom, iterate through the dictionary
		// it uses the current point as the key in the dictionary
		// the value is the next point
		// make the value the new current and keep going until the end (when current isn't in the dictionary)
		while (cameFrom.ContainsKey (current)) {
			current = cameFrom [current];
			// remove the height of 2 from the points
			path.Add (current + new Vector3(0, -2, 0));
		}
		path.Reverse ();
		return path;
	}

	int costBetween(Vector3 start, Vector3 goal)
	{
		
		return (int)Vector3.Distance (start, goal);
	}

	int heuristicCostEstimate(Vector3 start, Vector3 goal)
	{
		// manhattan distance
		//return (int)( Mathf.Abs(start.x - goal.x) + Mathf.Abs(start.z - goal.z));
		// absolute
		return costBetween(start, goal);
	}
}
