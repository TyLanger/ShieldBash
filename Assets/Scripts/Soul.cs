using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soul : MonoBehaviour {


	Transform player;
	float angle = 5;

	// Use this for initialization
	void Start () {
		player = FindObjectOfType<PlayerController> ().transform;
	}
	
	// Update is called once per frame
	void Update () {
		transform.RotateAround (player.position, Vector3.up, angle);
	}
}
