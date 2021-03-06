﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Ability : MonoBehaviour {

	// additional effects that can be applied to the ability in addition to damage
	public enum StatusEffect{None, Push, Pull, Knockdown, Knockup, Stun, Slow, DamageOverTime};

	// might need this
	// was going to use it to determine what parameters to pass from the player to the ability
	// when calling useAbility() or useAbility(transform, aimPoint)
	//public enum AbilityType{SpawnNew, UseExisting, Buff, Movement};

	// HideInInspector so they don't get drawn twice
	// The inspector gets drawn as normal (without these)
	// then these get drawn based on what additionalEffects is set to
	[HideInInspector]
	public StatusEffect additionalEffect;

	[HideInInspector]
	public float pushDistance = 8;
	//[HideInInspector]
	//public float pushScale = 7;
	[HideInInspector]
	public float pullDistance = 2;
	//[HideInInspector]
	//public float pullScale = 15;
	[HideInInspector]
	public float stunDuration = 2;

	// DoT
	[HideInInspector]
	public int dotDamageTick;
	[HideInInspector]
	public float dotTimeInterval, dotTotalTime;

	// Slow
	[HideInInspector]
	public float slowPercent, slowDuration;
	[HideInInspector]
	public bool decayingSlow;

	public int damage = 0;
	public float cooldown = 0;
	float timeNextReady = 0;
	protected GameObject self;
	//public AbilityType abilityType;

	public event Action abilityOver;
	public event Action<bool> aimPermission; 
	public event Action<int> slowMovement;

	public virtual void useAbility()
	{
		timeNextReady = Time.time + cooldown;
	}

	public virtual void useAbility(Transform spawnPoint, Vector3 aimPoint)
	{
		// default case is to ignore the variables and juse use the ability without them
		// if the ability wants to use the variables, it will extend this
		useAbility ();
	}

	protected void allowAiming(bool choice)
	{
		// called by the ability when the character has to stop aiming and commit to the ability
		// all it should really do is stop the aim indicator from moving
		// also maybe stop the player from rotating
		// the ability should just save where the aim point was at the moment it is needed.
		if (aimPermission != null) {
			aimPermission (choice);
		}
	}

	protected void selfSlow(int slowPercent)
	{
		// called by the ability if the ability requires the caster to slow down or stop to cast it
		if (slowMovement != null) {
			slowMovement (slowPercent);
		}
	}

	protected void abilityFinished()
	{
		// called when one ability is over
		// now another ability can be used
		if (abilityOver != null) {
			abilityOver();
		}
	}

	public void additionalEffects(GameObject g)
	{
		switch(additionalEffect)
		{
		case StatusEffect.Push:
			g.GetComponent<MovementController> ().setDisplacementLocation (transform.position + (g.transform.position - transform.position).normalized * pushDistance);
			break;
		case StatusEffect.Pull:
			g.GetComponent<MovementController> ().setDisplacementLocation (transform.position + (g.transform.position - transform.position).normalized * pullDistance);
			break;
		case StatusEffect.Stun:
			g.GetComponent<MovementController> ().stun (stunDuration);
			break;
		case StatusEffect.DamageOverTime:
			g.GetComponent<Health> ().takeDamageOverTime(dotDamageTick, dotTimeInterval, dotTotalTime);
			break;

		case StatusEffect.Slow:
			g.GetComponent<MovementController> ().slow (slowPercent, slowDuration);
			break;
		}
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
