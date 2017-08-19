using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour {

	delegate void DecisionDelegate();
	DecisionDelegate enemyAction;

	public TextMesh debugText;

	float timeBetweenDecisions = 0.15f;
	float timeOfNextDecision = 0;

	EnemyController enemyController;

	Transform playerTrans;
	public float aggroDist = 10;
	public float attackDist = 1;


	// Use this for initialization
	void Start () {
		enemyAction = RandomWalk;
		playerTrans = FindObjectOfType<PlayerController> ().transform;
		enemyController = GetComponent<EnemyController> ();
		//Invoke ("EnemyDecision", 2);
		enemyController.onDeath += reset;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time > timeOfNextDecision) {
			timeOfNextDecision = Time.time +timeBetweenDecisions;
			enemyAction ();
			debugText.text = enemyAction.Method.ToString();
		}

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
		enemyController.attackPlayer ();
	}

	void MoveTowardsPlayer()
	{
		enemyController.playerFound ();
		if (Vector3.Distance (transform.position, playerTrans.position) < attackDist) {
			enemyAction = Attack;
		} else if(Vector3.Distance (transform.position, playerTrans.position) > aggroDist) {
			enemyAction = RandomWalk;
		}

	}

	void reset()
	{
		enemyAction = RandomWalk;
	}

	void OnDrawGizmos()
	{
		UnityEditor.Handles.color = Color.red;
		UnityEditor.Handles.DrawWireDisc (transform.position, Vector3.up, aggroDist);

		UnityEditor.Handles.color = Color.yellow;
		UnityEditor.Handles.DrawWireDisc (transform.position, Vector3.up, attackDist);
	}
}
