using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyController : MovementController {

	public delegate void deathDelegate();
	public deathDelegate onDeath;

	Transform player;

	public bool attacking = false;
	public bool rootedForAttack = false;
	float timeOfNextAttack = 0;

	// can be modified by child classes
	// time enemy is rooted for while still being able to aim
	float attackWindUpTime = 0.05f;
	// time between attacks. Counts from start of attacks
	protected float timeBetweenAttacks = 0.85f;
	// time between animation starting and collider being turned on and damage being applied
	// cannot adjust aim during this time
	protected float damageDelayTime = 0.5f;
	protected int attackDamage = 40;

	bool randomWalk = true;
	float timeBetweenRandomWalks = 2;
	float timeOfNextRandomWalk = 0;
	float randomAngle = 0;
	Vector3 randomPoint;
	Vector3 spawnPoint;
	float spawnRadius = 5;

	float maxSightDistance;
	Astar astar;
	MapGen mapGen;
	List<Vector3> path;
	//public bool pathFinding = false;
	bool pathingToPlayer = false;
	bool pathingToSpawn = false;

	public Transform weapon;
	Animator weaponAnim;
	public GameObject swordArc;

	Health health;
	public int state = 0;

	// Use this for initialization
	protected override void Start () {
		base.Start ();
		if (FindObjectOfType<PlayerController> () != null) {
			player = FindObjectOfType<PlayerController> ().transform;
		}
		spawnPoint = transform.position;
		weaponAnim = weapon.GetComponent<Animator> ();
		health = GetComponent<Health> ();
		health.onDeath += die;
		astar = GetComponent<Astar> ();
		if (mapGen != null && astar != null) {
			astar.mapGen = mapGen;
		}
		if (!astar.isWalkable (spawnPoint)) {
			Debug.Log ("Bad spawn");
		}

	}

	public void updatePathfinding(MapGen _mapGen)
	{
		mapGen = _mapGen;
		if (astar != null) {
			astar.mapGen = _mapGen;
		}
	}
	
	// Update is called once per frame
	protected override void FixedUpdate () {

		if (player == null) {
			player = FindObjectOfType<PlayerController> ().transform;
		}

		if (rootedForAttack) {
			transform.LookAt (player);
			// if attacking, stop moving

		} else if(attacking) {
			
		} else {
			base.FixedUpdate();
		}

	}

	public override Vector3 getAiTargetMoveLocation ()
	{
		if (canSeeTarget (player, maxSightDistance)) {
			// can see the player, go to the player
			state = 1;
			// not pathing right now, travelling by sight
			notPathing ();
			return player.position;
		}

		// else follow the path if there is one
		if (path != null) {
			if (path.Count > 0) {
				state = 2;

				if (path.Count > 1) {
					if (canSee (path [1])) {
						// if you can see the next point, delete the current point so you go to that one instead
						path.RemoveAt (0);	
					}
				}

				if ((path.Count == 1) && (Vector3.Distance (transform.position, path [0]) < 0.3f)) {
					state = 3;
					// at the last point
					//pathFinding = false;
					path.RemoveAt (0);
					// at the last point, so not pathing anymore
					notPathing ();
				}
				else
				{
					transform.LookAt (path [0]);
					return path [0];
				}
			}
		}

		// can't see the player, no path
		// try to go towards spawn if you can see it
		if (canSee (spawnPoint)) {
			if (randomWalk) {
				//pathFinding = false;
				notPathing ();
				state = 4;
				// this can't exactly just switch to pathfinding
				// then when it is right beside a wall, it will sometimes try to path around the wall and then back to the spawn
				// that might work because it changes random walk every few seconds anyway...
				transform.LookAt (randomPoint);
				if (Time.time > timeOfNextRandomWalk) {
					// every timeBetweenRandomWalks (default 2 seconds) find a random point near the spawn point.
					// move towards that point
					timeOfNextRandomWalk = Time.time + timeBetweenRandomWalks;
					// pick a random angle
					randomAngle = Random.Range (0, 2 * Mathf.PI);
					// turn that random angle into a position on the unit circle
					// multiply by a radius to get a point inside the circle of the spawn point.
					// the spawn circle is a circle of radius spawnRadius around the spawn point
					randomPoint = spawnPoint + new Vector3 (Mathf.Cos (randomAngle), 0, Mathf.Sin (randomAngle)) * Random.Range (0, spawnRadius);
				}

				Debug.DrawLine (transform.position, randomPoint);
				return randomPoint;
			}
		}

		state = 5;
		// base case: stay where you are
		return transform.position;	
	}

	public void die()
	{
		// reset the enemy so it can be used again when it dies
		transform.position = spawnPoint;
		attacking = false;
		rootedForAttack = false;
		path = null;
		notPathing ();
		health.resetHealth ();
		onDeath ();
	}

	public void tookDamage(Vector3 pointTookDamageFrom)
	{
		//targetLocation = pointTookDamageFrom;
		// gets a path that the ai can use to get to the pathLastTookDamageFrom to investigate
		// path has lower priority than line of sight
		if (!canSeeTarget (player, maxSightDistance)) {
			Debug.Log ("Took Damage made a path");
			path = astar.FindPath (transform.position, pointTookDamageFrom);
		}
		//pathFinding = true;
	}

	bool canSee(Vector3 point)
	{
		return canSee (point, Vector3.Distance (point, transform.position));
	}

	bool canSee(Vector3 point, float maxDistance)
	{
		RaycastHit hit;
		if (Physics.Raycast (transform.position, (point - transform.position), out hit, maxDistance)) {

			// hit something
			return false;

		}
		return true;
	}

	bool canSeeTarget(Transform target, float maxDistance)
	{
		RaycastHit hit;
		if (Physics.Raycast (transform.position, (target.position - transform.position), out hit, maxDistance)) {
			if (hit.transform == target) {
				return true;
			}
		}
		return false;
	}

	public void playerFound(float sightDist)
	{
		maxSightDistance = sightDist;
		if (!canSeeTarget (player, maxSightDistance) && !pathingToPlayer) {
			// found the player, but can't see them; path to them
			Debug.Log ("PlayerFound made a path");
			path = astar.FindPath (transform.position, player.position);
			pathingToPlayer = true;
		}
		randomWalk = false;
	}

	public void playerLost()
	{
		randomWalk = true;
		// if you can't see the spawn,
		// set path back to spawn
		// and are not already pathing to spawn
		if (!canSee (spawnPoint) && !pathingToSpawn) {
			Debug.Log ("PlayerLost made a path");
			path = astar.FindPath (transform.position, spawnPoint);
			pathingToSpawn = true;
		}
	}

	void notPathing()
	{
		// clear pathing variables so the next time a path is needed,
		// a new path is created
		pathingToPlayer = false;
		pathingToSpawn = false;
	}

	bool attackCooldownOver()
	{
		return Time.time > timeOfNextAttack;
	}

	public void attackPlayer()
	{
		if (attackCooldownOver()) {
			timeOfNextAttack = Time.time + timeBetweenAttacks;

			// the enemy stops moving to start its attack
			// it can still track the player during this time
			rootedForAttack = true;

			// stop moving to cast attack for attackWindUpTime seconds
			Invoke ("startAttack", attackWindUpTime);

		}
	}

	void startAttack()
	{
		// Start animation
		// the enemy is still rooted, but now it can't track the player
		// wherever it was last looking is where it aims now
		attacking = true;
		rootedForAttack = false;
		weaponAnim.SetTrigger ("SwordTrigger");
		// Invoke damage trigger
		Invoke("activateAttackArc", damageDelayTime);
	}

	public void stopAttacking()
	{
		disableAttackArc ();
	}

	protected virtual void activateAttackArc()
	{
		
		swordArc.SetActive(true);
		Invoke ("disableAttackArc", 0.1f);
	}

	protected virtual void disableAttackArc()
	{
		attacking = false;
		swordArc.SetActive(false);
		// attack is finished, stop attacking so you can move again or queue up another attack

	}


	void OnDrawGizmos()
	{
		//Gizmos.DrawWireSphere (spawnPoint, spawnRadius);
		UnityEditor.Handles.DrawWireDisc (spawnPoint, Vector3.up, spawnRadius);


	}

	public void OnChildTriggerEnter(Collider col) {
		if (col.GetComponent<Health> () != null) {
			// hit player
			player.GetComponent<Health>().takeDamage(attackDamage);
		}
	}

	public void OnChildTriggerExit(Collider col) {

	}

	public void OnChildTriggerStay(Collider col) {

	}
}
