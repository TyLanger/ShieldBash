using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : ProjectileController {

	public Ability explosion;
	float fuseTime = 3f;

	// Use this for initialization
	void Start () {
		//explosion = GetComponentInChildren<SphereCollider> ().gameObject;

	}

	void FixedUpdate() {
		// move the bomb along the throw arc
		transform.position = Vector3.MoveTowards (transform.position, movePosition, moveSpeed);
		child.transform.position = new Vector3 (child.transform.position.x, apexHeight * curve.Evaluate((Vector3.Distance(transform.position, movePosition)/Vector3.Distance(spawnPoint, movePosition))), child.transform.position.z);
	}

	// Update is called once per frame
	void Update () {
		if(fuseTime < 0)
		{
			//Debug.Log ("Timed explosion");
			Detonate ();
		}
		fuseTime -= Time.deltaTime;
	}

	void Detonate()
	{
		explosion.useAbility ();
		explosion.setSelf (caster);
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
