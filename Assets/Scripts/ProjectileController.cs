using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour {

	float moveSpeed = 0.5f;
	float maxDistance = 10f;

	Vector3 moveDirection;

	// Use this for initialization
	void Start () {
		moveDirection = transform.rotation.eulerAngles;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		transform.position = Vector3.MoveTowards(transform.position, transform.position + moveDirection, moveSpeed);
	}


}
