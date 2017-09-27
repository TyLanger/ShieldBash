using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour {

	public float moveSpeed = 1f;
	public float maxDistance = 20f;
	protected float apexHeight = 2;
	public float timeAlive = 2f;

	protected Vector3 spawnPoint;
	protected Vector3 movePosition;
	bool moving = false;

	int damage = 65;

	public AnimationCurve curve;
	//Animator anim;
	public GameObject child;
	protected GameObject caster;
	Ability parentAbility;

	// Use this for initialization
	void Awake () {
		//anim = GetComponentInChildren<Animator> (); 
		spawnPoint = transform.position;
		//child = GetComponentInChildren<MeshRenderer> ().gameObject;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (moving) {
			if (timeAlive < 0) {
				maxTimeReached ();
			}
			if (Vector3.Distance (transform.position, movePosition) < 0.1f) {
				GetComponent<SphereCollider> ().enabled = false;
			}
			timeAlive -= Time.deltaTime;
			transform.position = Vector3.MoveTowards (transform.position, movePosition, moveSpeed);
			child.transform.position = new Vector3 (child.transform.position.x, apexHeight * curve.Evaluate((Vector3.Distance(transform.position, movePosition)/Vector3.Distance(spawnPoint, movePosition))), child.transform.position.z);
		}

	}

	public void moveTo(Vector3 position, GameObject self)
	{
		caster = self;
		moving = true;
		if (Vector3.Distance (position, spawnPoint) > maxDistance) {
			
			Vector3 newPoint = position - spawnPoint;
			position = spawnPoint + Vector3.ClampMagnitude (newPoint, maxDistance);
		}
		movePosition = position;
	}

	void maxTimeReached()
	{
		
		Destroy (gameObject);
	}

	void reflect(GameObject reflector)
	{
		caster = reflector;
		moveSpeed *= -1;
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.GetComponent<Health> () != null) {
			if (col.gameObject != caster) {
				col.GetComponent<Health> ().takeDamage (damage);
				parentAbility.additionalEffects (col.gameObject);
				maxTimeReached ();
			}
		} else if (col.CompareTag ("Obstacle")) {
			// don't go through walls
			maxTimeReached ();
		} else if (col.CompareTag ("Shield")) {
			if(col.GetComponent<Shield>().isShielding())
			{
				reflect (col.gameObject.transform.parent.gameObject);
			}
		}
	}

	public void setDamage(int d)
	{
		damage = d;
	}

	public void setAbility(Ability a)
	{
		// set the ability that fired this projectile
		// it has the code to do the additional effects (stun, slow, etc.)
		parentAbility = a;
	}

	void OnDrawGizmos()
	{
		//Gizmos.DrawSphere (transform.position, 1f);
		Gizmos.DrawWireSphere (transform.position, 0.5f);
	}

}
