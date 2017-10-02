using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyController : MovementController {

	/* In parent
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
	*/


	public delegate void deathDelegate();
	public deathDelegate onDeath;

	Transform player;
	/* in parent
	public float moveSpeed = 0.5f;
	float originalMoveSpeed;
	*/

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

	/* in parent
	bool beingPulled = false;
	float pullSpeedMultiplier = 4;
	Vector3 pullTargetPos;
	*/

	public Transform weapon;
	Animator weaponAnim;
	public GameObject swordArc;

	Health health;


	// Use this for initialization
	protected override void Start () {
		base.Start ();
		player = FindObjectOfType<PlayerController> ().transform;
		spawnPoint = transform.position;
		weaponAnim = weapon.GetComponent<Animator> ();
		health = GetComponent<Health> ();
		health.onDeath += die;

	}
	
	// Update is called once per frame
	protected override void FixedUpdate () {

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
			return randomPoint;
		} else {
			// only look at the player when moving
			// and only when moving towards player
			transform.LookAt (player);
			return player.transform.position;
		}
	}

	public void die()
	{
		// reset the enemy so it can be used again when it dies
		transform.position = spawnPoint;
		attacking = false;
		rootedForAttack = false;
		health.resetHealth ();
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
