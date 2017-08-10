using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildCollision : MonoBehaviour {

	EnemyController parentController;

	void Start() {
		parentController = GetComponentInParent<EnemyController> ();
		gameObject.SetActive (false);
	}

	void OnTriggerEnter(Collider col) {
		if (parentController == null) {
			Debug.Log ("Null");
		}
		parentController.OnChildTriggerEnter (col);
	}

	void OnTriggerExit(Collider col) {
		parentController.OnChildTriggerExit (col);
	}

	void OnTriggerStay(Collider col) {
		parentController.OnChildTriggerStay (col);
	}
}
