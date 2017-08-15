using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour {

	delegate void DecisionDelegate();
	DecisionDelegate enemyAction;

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
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time > timeOfNextDecision) {
			timeOfNextDecision = Time.time +timeBetweenDecisions;
			enemyAction ();
		}
	}

	void RandomWalk() {
		enemyController.playerLost();
		if (Vector3.Distance (transform.position, playerTrans.position) < aggroDist) {
			enemyAction = MoveTowardsPlayer;
		}
	}

	void Attack() {
		enemyController.attackPlayer ();
		if (Vector3.Distance (transform.position, playerTrans.position) > attackDist) {
			enemyController.stopAttacking ();
			enemyAction = MoveTowardsPlayer;
		}
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

	void OnDrawGizmos()
	{
		UnityEditor.Handles.color = Color.red;
		UnityEditor.Handles.DrawWireDisc (transform.position, Vector3.up, aggroDist);

		UnityEditor.Handles.color = Color.yellow;
		UnityEditor.Handles.DrawWireDisc (transform.position, Vector3.up, attackDist);
	}
}
