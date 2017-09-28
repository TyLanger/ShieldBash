using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyController : MonoBehaviour {

	[System.Serializable]
	public struct Slow {
		public float percent;
		public float endTime;

		public Slow(float p, float duration)
		{
			percent = p;
			endTime = Time.time + duration;
		}

	};


	public delegate void deathDelegate();
	public deathDelegate onDeath;

	Transform player;
	public float moveSpeed = 0.5f;
	float originalMoveSpeed;


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

	bool beingPulled = false;
	float pullSpeedMultiplier = 4;
	Vector3 pullTargetPos;

	public Transform weapon;
	Animator weaponAnim;
	public GameObject swordArc;

	Health health;
	// for stunning, rooting, etc
	bool canAttack = true;
	bool canMove = true;

	public List<Slow> slowList;

	// Use this for initialization
	protected virtual void Start () {
		player = FindObjectOfType<PlayerController> ().transform;
		spawnPoint = transform.position;
		weaponAnim = weapon.GetComponent<Animator> ();
		health = GetComponent<Health> ();
		health.onDeath += die;
		originalMoveSpeed = moveSpeed;

	}
	
	// Update is called once per frame
	void FixedUpdate () {

		// if there is a slow in the list, do slow logic
		if (slowList.Count > 0) {

			do{
				if(slowList[0].endTime < Time.time)
				{
					// first slow had no time left
					slowList.RemoveAt(0);
				}
				else
				{
					// found a slow that has time left
					// adjust the moveSpeed based on the original move speed
					// this code is run every frame so moveSpeed *= slow makes the move speed slow down more and more every frame
					moveSpeed = originalMoveSpeed * ((100 - slowList[0].percent) / 100f);
					break;
				}
				// a slow may have been removed. Check again if there are slows in the list
			} while(slowList.Count > 0);

			// do-while either exitted with no slows left or with the break
			// check if there are no slows
			if (slowList.Count == 0) {
				// no slows left, reset move speed
				moveSpeed = originalMoveSpeed;
			}
		}

		if (rootedForAttack) {
			transform.LookAt (player);
			// if attacking, stop moving

			// moving this to its own function so it can change for different enemies
			// need to wait for attackWindUpTime seconds before you can start the swing
			/*
			if (Time.time > timeOfAttackStart + attackWindUpTime) {
				if (Time.time > timeOfNextAttack) {
					timeOfNextAttack = Time.time + timeBetweenAttacks;
					// attack;
					//Debug.Log ("Attacking!");
					Invoke("activateAttackArc", damageDelayTime);
					swordAnim.SetTrigger ("SwordTrigger");
					//attacking = false;
				}
			}
			*/
		} else if(attacking) {
			
		} else {
			// stop moving if can't move
			// can't move is set when the entity is stunned or rooted, etc.
			if (canMove || beingPulled) {
				// default to random walk when the player isn't nearby. This is governed by the EnemyAI
				if (randomWalk) {
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
					transform.position = Vector3.MoveTowards (transform.position, randomPoint, moveSpeed);
				} else {
					// only look at the player when moving
					// and only when moving towards player
					transform.LookAt (player);
					if (beingPulled) {
						// if you are being pulled, move towards the pull location
						// move faster than normal because of pullSpeedMultiplier
						// This only works smoothly if the target is already moving towards you
						// They will wait to get out of random walk before zooming towards you
						transform.position = Vector3.MoveTowards (transform.position, pullTargetPos, moveSpeed * pullSpeedMultiplier);
						if (Vector3.Distance (transform.position, pullTargetPos) < 0.1f) {
							// Once you get to the pull location, you can start moving again
							beingPulled = false;
						}
					} else {
						transform.position = Vector3.MoveTowards (transform.position, player.transform.position, moveSpeed);
					}
				}
			}
		}

	}

	public void die()
	{
		// reset the enemy so it can be used again when it dies
		transform.position = spawnPoint;
		attacking = false;
		rootedForAttack = false;
		health.resetHealth ();
		unStun ();
		endSlow ();
		onDeath ();
	}

	public void playerFound()
	{
		randomWalk = false;
	}

	public void playerLost()
	{
		randomWalk = true;
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

	public void setPullLocation(Vector3 targetPos)
	{
		// set a target to move towards
		pullTargetPos = targetPos;
		// set being Pulled to true
		// this changes the movement from following the player to moving towards this point
		beingPulled = true;
	}

	public void slow(float slowPercent, float slowDuration)
	{
		// this system only works for 1 slow at a time right now
		// only works CORRECTLY for 1 slow
		// multiple slows will stack, but only get the duration of the first slow
		//moveSpeed *= ((100 - slowPercent) / 100f);
		//Invoke ("endSlow", slowDuration);

		// add this slow to the list of slows
		slowList.Add(new Slow(slowPercent, slowDuration));
		// sort the slowList
		// y.CompareTo(x) because want bigger numbers at the start of the list
		slowList.Sort((x, y) => y.percent.CompareTo(x.percent));
	}

	void endSlow()
	{
		// this will break any subsequent slows applied
		moveSpeed = originalMoveSpeed;
	}

	public void stun(float durationOfStun)
	{
		// same as slows
		// only works for 1 stun at a time
		canMove = false;
		canAttack = false;
		Invoke ("unStun", durationOfStun);
	}

	void unStun()
	{
		canMove = true;
		canAttack = true;
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
