using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	Transform playerTrans;
	public Vector3 offset;

	// Use this for initialization
	void Start () {
		if (FindObjectOfType<PlayerController> () != null) {
			playerTrans = FindObjectOfType<PlayerController> ().transform;
		}
		//offset = new Vector3 (0, 12, -10);
	}
	
	// Update is called once per frame
	void Update () {
		if (playerTrans == null) {
			playerTrans = FindObjectOfType<PlayerController> ().transform;
		} else {
			transform.position = playerTrans.position + offset;
		}
	}
}
