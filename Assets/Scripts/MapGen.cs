using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


// Most of the code from https://www.youtube.com/watch?v=v7yyZZjF1z4&list=PLFt_AvWsXl0eZgMK_DT5_biRkWXftAOf9
public class MapGen : MonoBehaviour {

	public int width;
	public int height;

	public int seed;
	public bool useRandomSeed;

	[Range(0,100)]
	public int randomFillPercent;

	public Vector3 testPoint;
	int[,] map;

	AstarNode[,] nodeMap;

	// Entity spawning
	public GameObject player;
	public int spawnX, spawnY;
	// default to bottom-left-most room
	Room playerSpawnRoom;
	public GameObject[] enemies;
	public int numEnemies;

	void Awake() {
		GenerateMap();
		/*
		if (isWalkable (testPoint)) {
			Debug.Log ("Is inside map");
			Coord c = WorldPointToCoord (testPoint);
			Debug.Log ("Coord: " + c.tileX + "," + c.tileY);
			List<Vector3> list = GetNeighbours (c);
			foreach (var item in list) {
				Debug.Log ("Neighbour: " + item);
			}
		}*/
	}

	void Start()
	{
		SpawnEntities ();
	}

	void SpawnEntities()
	{
		// spawn player(s)
		// -2 is to spawn the player on the ground plane
		// world points have their y at 2
		// for some reason.... maybe it can be fixed?
		// need to eliminte that to get the player to look like they're on the ground
		// increasing scale makes the walls deeper. The top of the wall never moves
		Instantiate(player, CoordToWorldPoint(new Coord(14,14)) + new Vector3(0, -2*transform.localScale.y, 0), transform.rotation);

		// spawn enemies
		for (int i = 0; i < numEnemies; i++) {
			int x;
			int y;
			do{
				x = UnityEngine.Random.Range (5, width - 5);
				y = UnityEngine.Random.Range (5, height - 5);
				// keep trying for a valid spot to spawn an enemy
				// don't spawn in walls (==1)
				// don't spawn too near the player
			} while (map [x, y] == 1 || ((x < 2*(spawnX+8)) && (y < 2*(spawnY+8))));
			//Debug.LogFormat ("Spawn at ({0}, {1})", x, y);
			//Coord randPoint = new Coord (UnityEngine.Random.Range (5, width - 5), UnityEngine.Random.Range (5, height - 5));

			var enemyCopy = Instantiate (enemies [0], CoordToWorldPoint (new Coord (x, y)) + new Vector3(0, -2*transform.localScale.y, 0), transform.rotation);
			enemyCopy.GetComponent<EnemyController>().updatePathfinding (this);
		}
	}

	void CarveStartRoom()
	{
		if ((spawnX > 0 && spawnX < (width - 1)) && (spawnY > 0 && spawnY < (height - 1))) {
			// spawnX and spawnY are inside the grid and not on the edge
			for (int x = spawnX; x < spawnX + 8; x++) {
				for (int y = spawnY; y < spawnY + 8; y++) {
					// 7 by 7 square
					map[x,y] = 0;
				}
			}
		}
	}

	void GenerateMap() {
		map = new int[width,height];
		nodeMap = new AstarNode[width, height];
		RandomFillMap();

		CarveStartRoom ();

		for (int i = 0; i < 5; i ++) {
			SmoothMap();
		}

		ProcessMap ();

		int borderSize = 1;
		int[,] borderedMap = new int[width + borderSize * 2,height + borderSize * 2];

		for (int x = 0; x < borderedMap.GetLength(0); x ++) {
			for (int y = 0; y < borderedMap.GetLength(1); y ++) {
				if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize) {
					borderedMap[x,y] = map[x-borderSize,y-borderSize];
					nodeMap [x-borderSize, y-borderSize] = new AstarNode (borderedMap [x, y] == 0, CoordToWorldPoint (new Coord (x-borderSize, y-borderSize)), x-borderSize, y-borderSize);
				}
				else {
					borderedMap[x,y] =1;
				}

			}
		}

		MeshGen meshGen = GetComponent<MeshGen>();
		meshGen.GenerateMesh(borderedMap, 1);

	}

	void ProcessMap() {
		List<List<Coord>> wallRegions = GetRegions (1);
		int wallThresholdSize = 50;

		foreach (List<Coord> wallRegion in wallRegions) {
			if (wallRegion.Count < wallThresholdSize) {
				foreach (Coord tile in wallRegion) {
					map[tile.tileX,tile.tileY] = 0;
				}
			}
		}

		List<List<Coord>> roomRegions = GetRegions (0);
		int roomThresholdSize = 50;
		List<Room> survivingRooms = new List<Room> ();

		foreach (List<Coord> roomRegion in roomRegions) {
			if (roomRegion.Count < roomThresholdSize) {
				foreach (Coord tile in roomRegion) {
					map[tile.tileX,tile.tileY] = 1;
				}
			}
			else {
				survivingRooms.Add(new Room(roomRegion, map));
				//survivingRooms[0]
			}
		}
		survivingRooms.Sort ();
		survivingRooms [0].isMainRoom = true;
		survivingRooms [0].isAccessibleFromMainRoom = true;

		ConnectClosestRooms (survivingRooms);
	}

	void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false) {

		List<Room> roomListA = new List<Room> ();
		List<Room> roomListB = new List<Room> ();

		if (forceAccessibilityFromMainRoom) {
			foreach (Room room in allRooms) {
				if (room.isAccessibleFromMainRoom) {
					roomListB.Add (room);
				} else {
					roomListA.Add (room);
				}
			}
		} else {
			roomListA = allRooms;
			roomListB = allRooms;
		}

		int bestDistance = 0;
		Coord bestTileA = new Coord ();
		Coord bestTileB = new Coord ();
		Room bestRoomA = new Room ();
		Room bestRoomB = new Room ();
		bool possibleConnectionFound = false;

		foreach (Room roomA in roomListA) {
			if (!forceAccessibilityFromMainRoom) {
				possibleConnectionFound = false;
				if (roomA.connectedRooms.Count > 0) {
					continue;
				}
			}

			foreach (Room roomB in roomListB) {
				if (roomA == roomB || roomA.IsConnected(roomB)) {
					continue;
				}

				for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA ++) {
					for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB ++) {
						Coord tileA = roomA.edgeTiles[tileIndexA];
						Coord tileB = roomB.edgeTiles[tileIndexB];
						int distanceBetweenRooms = (int)(Mathf.Pow (tileA.tileX-tileB.tileX,2) + Mathf.Pow (tileA.tileY-tileB.tileY,2));

						if (distanceBetweenRooms < bestDistance || !possibleConnectionFound) {
							bestDistance = distanceBetweenRooms;
							possibleConnectionFound = true;
							bestTileA = tileA;
							bestTileB = tileB;
							bestRoomA = roomA;
							bestRoomB = roomB;
						}
					}
				}
			}
			if (possibleConnectionFound && !forceAccessibilityFromMainRoom) {
				CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
			}
		}

		if (possibleConnectionFound && forceAccessibilityFromMainRoom) {
			CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
			ConnectClosestRooms(allRooms, true);
		}

		if (!forceAccessibilityFromMainRoom) {
			ConnectClosestRooms(allRooms, true);
		}
	}

	void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB) {
		Room.ConnectRooms (roomA, roomB);
		//Debug.DrawLine (CoordToWorldPoint (tileA), CoordToWorldPoint (tileB), Color.green, 100);

		List<Coord> line = GetLine (tileA, tileB);
		foreach (Coord c in line) {
			DrawCircle(c,5);
		}
	}

	void DrawCircle(Coord c, int r) {
		for (int x = -r; x <= r; x++) {
			for (int y = -r; y <= r; y++) {
				if (x*x + y*y <= r*r) {
					int drawX = c.tileX + x;
					int drawY = c.tileY + y;
					if (IsInMapRange(drawX, drawY)) {
						map[drawX,drawY] = 0;
					}
				}
			}
		}
	}

	List<Coord> GetLine(Coord from, Coord to) {
		List<Coord> line = new List<Coord> ();

		int x = from.tileX;
		int y = from.tileY;

		int dx = to.tileX - from.tileX;
		int dy = to.tileY - from.tileY;

		bool inverted = false;
		int step = Math.Sign (dx);
		int gradientStep = Math.Sign (dy);

		int longest = Mathf.Abs (dx);
		int shortest = Mathf.Abs (dy);

		if (longest < shortest) {
			inverted = true;
			longest = Mathf.Abs(dy);
			shortest = Mathf.Abs(dx);

			step = Math.Sign (dy);
			gradientStep = Math.Sign (dx);
		}

		int gradientAccumulation = longest / 2;
		for (int i =0; i < longest; i ++) {
			line.Add(new Coord(x,y));

			if (inverted) {
				y += step;
			}
			else {
				x += step;
			}

			gradientAccumulation += shortest;
			if (gradientAccumulation >= longest) {
				if (inverted) {
					x += gradientStep;
				}
				else {
					y += gradientStep;
				}
				gradientAccumulation -= longest;
			}
		}

		return line;
	}

	public AstarNode WorldPointToAstarNode(Vector3 point)
	{
		Coord c = WorldPointToCoord (point);
		// walkable if map is 0 at that point
		return nodeMap[c.tileX, c.tileY];//new AstarNode(map[c.tileX, c.tileY] == 0, CoordToWorldPoint(c), c.tileX, c.tileY);
	}

	public Vector3 AdjustToGridPoint(Vector3 point)
	{
		return CoordToWorldPoint (WorldPointToCoord (point));
	}

	Vector3 CoordToWorldPoint(Coord tile) {
		// 79
		// -64 + 0.5f + 79 = 15.5
		return new Vector3 ((-width/ 2 + .5f + tile.tileX)*transform.localScale.x, 2, (-height / 2 + .5f + tile.tileY)*transform.localScale.z);
	}

	Coord WorldPointToCoord(Vector3 pos)
	{
		if (isInMap (pos)) {
			int x;
			int z;
			x = Mathf.RoundToInt (((pos.x/transform.localScale.x)-0.5f) + (width / 2));
			z = Mathf.RoundToInt (((pos.z/transform.localScale.z)-0.5f) + (height / 2));

			x = (int)Mathf.Clamp (x, 0, map.GetLength (0));
			z = (int)Mathf.Clamp (z, 0, map.GetLength (1));
			Coord c = new Coord (x, z);	
			return c;
		}
		return new Coord ();
	}

	public bool isInMap(Vector3 point)
	{
		Vector3 mapCenter = transform.position;
		Vector3 mapBottomLeft = transform.position + new Vector3 ((-width*transform.localScale.x) / 2, 2, (-height*transform.localScale.z) / 2);
		Vector3 mapTopRight = transform.position + new Vector3 ((width*transform.localScale.x) / 2, 2, (height*transform.localScale.z) / 2);

		//Debug.LogFormat ("BottomLeft: {0} Point: {1} Top Right: {2}", mapBottomLeft, point, mapTopRight);

		if (!(point.x > mapBottomLeft.x && point.x < mapTopRight.x) && (point.z > mapBottomLeft.z && point.z < mapTopRight.z)) {
			// is outside map
			return false;
		}
		return true;
	}

	public bool isWalkable(Vector3 point)
	{
		
		if (!isInMap (point)) {
			return false;
		}

		// check if it is a wall tile
		Coord c = WorldPointToCoord(point);
		if (map [c.tileX, c.tileY] == 1) {
			// is a wall
			//Debug.Log("Coord is a wall");
			return false;
		}

		return true;
	}

	public List<AstarNode> GetNeighbours(AstarNode node)
	{
		List<AstarNode> neighbourList = new List<AstarNode> ();
		// check if the neighbours are walkable
		if (nodeMap [node.xGrid + 1, node.yGrid].walkable) {
			neighbourList.Add (nodeMap[node.xGrid + 1, node.yGrid]);
		}
		if (nodeMap [node.xGrid, node.yGrid + 1].walkable) {
			neighbourList.Add (nodeMap[ node.xGrid, node.yGrid+1]);
		}
		if (nodeMap [node.xGrid-1, node.yGrid].walkable) {
				neighbourList.Add (nodeMap[node.xGrid-1, node.yGrid]);
		}
		if (nodeMap [node.xGrid, node.yGrid-1].walkable) {
				neighbourList.Add (nodeMap[ node.xGrid, node.yGrid-1]);
		}
		return neighbourList;
	}
	/* swapping over to AstarNode instead of Vector3
	public List<Vector3> GetNeighbours(Vector3 point)
	{
		return GetNeighbours (WorldPointToCoord (point));

	}

	List<Vector3> GetNeighbours(Coord c)
	{
		List<Vector3> neighbourList = new List<Vector3> ();
		// find neighbours (4 connected) that are not walls
		if (map [c.tileX + 1, c.tileY] == 0) {
			neighbourList.Add (CoordToWorldPoint (new Coord(c.tileX + 1, c.tileY)));
		}
		if (map [c.tileX, c.tileY+1] == 0) {
			neighbourList.Add (CoordToWorldPoint (new Coord(c.tileX, c.tileY +1)));
		}
		if (map [c.tileX - 1, c.tileY] == 0) {
			neighbourList.Add (CoordToWorldPoint (new Coord(c.tileX - 1, c.tileY)));
		}
		if (map [c.tileX, c.tileY-1] == 0) {
			neighbourList.Add (CoordToWorldPoint (new Coord(c.tileX, c.tileY -1)));
		}

		if (neighbourList.Count < 1) {
			Debug.Log ("No neighbours at " + c.tileX + "," + c.tileY);
		}
		return neighbourList;
	}*/

	List<List<Coord>> GetRegions(int tileType) {
		List<List<Coord>> regions = new List<List<Coord>> ();
		int[,] mapFlags = new int[width,height];

		for (int x = 0; x < width; x ++) {
			for (int y = 0; y < height; y ++) {
				if (mapFlags[x,y] == 0 && map[x,y] == tileType) {
					List<Coord> newRegion = GetRegionTiles(x,y);
					regions.Add(newRegion);

					foreach (Coord tile in newRegion) {
						mapFlags[tile.tileX, tile.tileY] = 1;
					}
				}
			}
		}

		return regions;
	}

	List<Coord> GetRegionTiles(int startX, int startY) {
		List<Coord> tiles = new List<Coord> ();
		int[,] mapFlags = new int[width,height];
		int tileType = map [startX, startY];

		Queue<Coord> queue = new Queue<Coord> ();
		queue.Enqueue (new Coord (startX, startY));
		mapFlags [startX, startY] = 1;

		while (queue.Count > 0) {
			Coord tile = queue.Dequeue();
			tiles.Add(tile);

			for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++) {
				for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++) {
					if (IsInMapRange(x,y) && (y == tile.tileY || x == tile.tileX)) {
						if (mapFlags[x,y] == 0 && map[x,y] == tileType) {
							mapFlags[x,y] = 1;
							queue.Enqueue(new Coord(x,y));
						}
					}
				}
			}
		}
		return tiles;
	}

	bool IsInMapRange(int x, int y) {
		return x >= 0 && x < width && y >= 0 && y < height;
	}


	void RandomFillMap() {

		if (useRandomSeed) {
			seed = UnityEngine.Random.Range (0, 1000);
		}

		System.Random pseudoRandom = new System.Random(seed);

		for (int x = 0; x < width; x ++) {
			for (int y = 0; y < height; y ++) {
				if (x == 0 || x == width-1 || y == 0 || y == height -1) {
					map[x,y] = 1;
				}
				else {
					map[x,y] = (pseudoRandom.Next(0,100) < randomFillPercent)? 1: 0;
				}
			}
		}
	}

	void SmoothMap() {
		// make a new array
		// writing to the array that you are checking for neighbours poisons it as you go
		int[,] smoothMap = map;
		for (int x = 0; x < width; x ++) {
			for (int y = 0; y < height; y ++) {
				int neighbourWallTiles = GetSurroundingWallCount(x,y);

				if (neighbourWallTiles > 4)
					smoothMap[x,y] = 1;
				else if (neighbourWallTiles < 4)
					smoothMap[x,y] = 0;

			}
		}

		map = smoothMap;
	}

	int GetSurroundingWallCount(int gridX, int gridY) {
		int wallCount = 0;
		for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX ++) {
			for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY ++) {
				if (IsInMapRange(neighbourX,neighbourY)) {
					if (neighbourX != gridX || neighbourY != gridY) {
						wallCount += map[neighbourX,neighbourY];
					}
				}
				else {
					wallCount ++;
				}
			}
		}

		return wallCount;
	}

	struct Coord {
		public int tileX;
		public int tileY;

		public Coord(int x, int y) {
			tileX = x;
			tileY = y;
		}
	}


	class Room : IComparable<Room> {
		public List<Coord> tiles;
		public List<Coord> edgeTiles;
		public List<Room> connectedRooms;
		public int roomSize;
		public bool isAccessibleFromMainRoom;
		public bool isMainRoom;


		public Room() {
		}

		public Room(List<Coord> roomTiles, int[,] map) {
			tiles = roomTiles;
			roomSize = tiles.Count;
			connectedRooms = new List<Room>();

			edgeTiles = new List<Coord>();
			foreach (Coord tile in tiles) {
				for (int x = tile.tileX-1; x <= tile.tileX+1; x++) {
					for (int y = tile.tileY-1; y <= tile.tileY+1; y++) {
						if (x == tile.tileX || y == tile.tileY) {
							if (map[x,y] == 1) {
								edgeTiles.Add(tile);
							}
						}
					}
				}
			}
		}

		public void SetAccessibleFromMainRoom() {
			if (!isAccessibleFromMainRoom) {
				isAccessibleFromMainRoom = true;
				foreach (Room connectedRoom in connectedRooms) {
					connectedRoom.SetAccessibleFromMainRoom();
				}
			}
		}

		public static void ConnectRooms(Room roomA, Room roomB) {
			if (roomA.isAccessibleFromMainRoom) {
				roomB.SetAccessibleFromMainRoom ();
			} else if (roomB.isAccessibleFromMainRoom) {
				roomA.SetAccessibleFromMainRoom();
			}
			roomA.connectedRooms.Add (roomB);
			roomB.connectedRooms.Add (roomA);
		}

		public bool IsConnected(Room otherRoom) {
			return connectedRooms.Contains(otherRoom);
		}

		public int CompareTo(Room otherRoom) {
			return otherRoom.roomSize.CompareTo (roomSize);
		}
	}

}