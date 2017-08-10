using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageText : MonoBehaviour {


	float fade = 0.01f;
	TextMesh myTextMesh;

	// Use this for initialization
	void Start () {
		myTextMesh = GetComponent<TextMesh> ();	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		myTextMesh.color = myTextMesh.color + new Color(0,0,0, -fade);
		if (myTextMesh.color.a < 0.1f) {
			Destroy (gameObject);
		}
	}
}
