using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour {

	delegate void DecisionDelegate();
	DecisionDelegate enemyAction;


	public TextMesh debugText;

	float timeBetweenDecisions = 0.15f;
	float timeBetweenCombatDecisions = 0.05f;
	float timeOfNextDecision = 0;
	bool inCombat = false;
	float timeOutOfCombat;
	float memoryTime = 5;

	EnemyController enemyController;

	Transform playerTrans;
	public float aggroDist = 10;
	public float attackDist = 1;


	// Use this for initialization
	void Start () {
		debugText.text = "";
		enemyAction = RandomWalk;
		if (FindObjectOfType<PlayerController> () != null) {
			playerTrans = FindObjectOfType<PlayerController> ().transform;
		}
		enemyController = GetComponent<EnemyController> ();
		//Invoke ("EnemyDecision", 2);
		enemyController.onDeath += reset;
		GetComponent<Health> ().onDamage += tookDamage;
	}
	
	// Update is called once per frame
	void Update () {
		// for testing pathing
		/*
		if (Input.GetButtonDown ("Jump")) {
			MoveTowardsPlayer ();
		}
		*/
		if (playerTrans == null) {
			playerTrans = FindObjectOfType<PlayerController> ().transform;
		}

		if (Time.time > timeOfNextDecision) {
			makeDecision ();

		}

		if (Time.time > timeOutOfCombat && inCombat) {
			Debug.Log("Anger timed out");
			inCombat = false;
		}
	}

	void makeDecision()
	{
		timeOfNextDecision = Time.time + (inCombat?timeBetweenCombatDecisions : timeBetweenDecisions);
		enemyAction ();
		debugText.text = enemyAction.Method.ToString();
		debugText.gameObject.SetActive (false);
	}

	void RandomWalk() {
		enemyController.playerLost();
		if (Vector3.Distance (transform.position, playerTrans.position) < aggroDist) {
			enemyAction = MoveTowardsPlayer;
		}
	}

	void Attack() {
		
		if (Vector3.Distance (transform.position, playerTrans.position) > attackDist) {
			enemyController.stopAttacking ();
			enemyAction = MoveTowardsPlayer;
		}
		// this needs to be after
		// in the case where last decision, the enemy could attack (so this method is being called)
		// but THIS decision time the player is too far away
		// if attackPlayer() is first, it will start the attack
		// then the if will be true
		// inside the if, stopAttacking is called which sets attacking to false
		// attackPlayer sets attacking to true after a 0.05s delay
		// therefore, the enemy gets stuck attacking forever and can't move
		timeOutOfCombat = Time.time + memoryTime;
		enemyController.attackPlayer ();
	}

	void MoveTowardsPlayer()
	{
		
		enemyController.playerFound (aggroDist);
		if (Vector3.Distance (transform.position, playerTrans.position) < attackDist) {
			enemyAction = Attack;
		} else if(Vector3.Distance (transform.position, playerTrans.position) > aggroDist && !inCombat) {
			enemyAction = RandomWalk;
		}
	}

	void tookDamage()
	{
		
		timeOutOfCombat = Time.time + memoryTime;
		if (!inCombat) {
			inCombat = true;
		}
		// if the enemy is randomly walking then takes damage from the player,
		// move towards the player
		// this is the same as when the player comes into aggro range
		// potentially should make aggro range larger
		// or have a different aggro system so when the player pulls aggro, the enemy does something. Then make this method flag the aggro
		if (enemyAction == RandomWalk) {
			// assuming the player did the damage
			// probably needs to be an event instead of a delegate to pass where the damage came from
			enemyController.tookDamage (playerTrans.position);
			enemyAction = MoveTowardsPlayer;
			// also call enemyAction immediately so there is no delay
			makeDecision ();

		}
	}

	void reset()
	{
		inCombat = false;
		timeOutOfCombat = 0;
		enemyAction = RandomWalk;
		timeOfNextDecision = Time.time + timeBetweenDecisions;
	}

	void OnDrawGizmos()
	{
		
		debugText.gameObject.SetActive (true);

		UnityEditor.Handles.color = Color.red;
		UnityEditor.Handles.DrawWireDisc (transform.position, Vector3.up, aggroDist);

		UnityEditor.Handles.color = Color.yellow;
		UnityEditor.Handles.DrawWireDisc (transform.position, Vector3.up, attackDist);
	}
}
