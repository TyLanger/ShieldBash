using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour {

	public int damage = 0;
	public float cooldown = 0;
	public float timeNextReady = 0;
	protected GameObject self;


	public virtual void useAbility()
	{
		timeNextReady = Time.time + cooldown;
	}

	public virtual void useAbility(Transform spawnPoint, Vector3 aimPoint)
	{

	}

	public void setSelf(GameObject _self)
	{
		self = _self;
	}

	protected bool isReady()
	{
		// if cooldown is finished
		if (Time.time > timeNextReady) {
			
			return true;
		}

		return false;	
	}
}
