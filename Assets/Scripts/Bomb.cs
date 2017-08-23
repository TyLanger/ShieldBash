using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour {

	public Ability explosion;
	float fuseTime = 3f;

	// Use this for initialization
	void Start () {
		//explosion = GetComponentInChildren<SphereCollider> ().gameObject;
	}
	
	// Update is called once per frame
	void Update () {
		if(fuseTime < 0)
		{
			Debug.Log ("Timed explosion");
			Detonate ();
		}
		fuseTime -= Time.deltaTime;
	}

	void Detonate()
	{
		explosion.useAbility ();
		Invoke ("DestroySelf", 0.2f);
	}

	void DestroySelf()
	{
		Destroy (gameObject);
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.GetComponent<ProjectileController> () != null) {
			// object hitting the bomb is a projectile, detonate

			Detonate();
		}
	}
}
