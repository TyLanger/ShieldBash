using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour {

	protected float cooldown = 0;
	float timeNextReady = 0;



	public virtual void useAbility()
	{
		timeNextReady = Time.time + cooldown;
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
