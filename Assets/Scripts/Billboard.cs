using UnityEngine;
using System.Collections;


public class Billboard : MonoBehaviour {

	void Update () {
		transform.LookAt(Camera.main.transform);
		transform.Rotate(new Vector3(0, 180, 0));
	}
}