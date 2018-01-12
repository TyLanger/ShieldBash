using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour {

	// this is the base class for any entity that can move around
	// it is also the place for stuns and roots to take effect

	[System.Serializable]
	public struct Slow {
		public float percent;
		public float endTime;

		public Slow(float p, float duration)
		{
			percent = p;
			endTime = Time.time + duration;
		}

	};

	enum MovementType {PlayerControlled, AiControlled, BeingDisplaced };

	MovementType originalMovementType;
	MovementType currentMovementType;

	public float currentMoveSpeed;
	float originalMoveSpeed;

	Vector3 moveDirection;
	Vector3 targetLocation;


	// CC effects
	// pulls and pushes
	Vector3 displacementLocation;
	float displacementMultiplier = 4;

	// slows
	public List<Slow> slowList;

	// stuns
	bool isStunned = false;
	float stunEndTime;

	// root
	bool isRooted = false;
	float rootEndTime;

	// Use this for initialization
	protected virtual void Start () {
		originalMoveSpeed = currentMoveSpeed;
		// the original movement type is either player controller or AI controlled
		// I don't know if I can check if this class extends from PlayerController
		originalMovementType = (isPlayer()) ? MovementType.PlayerControlled : MovementType.AiControlled;
		currentMovementType = originalMovementType;
	}
	
	// Update is called once per frame
	protected virtual void FixedUpdate () {

		// stuns
		if (isStunned) {
			
			if (stunEndTime > Time.time) {
				// stun still has time left

			} else {
				// the stun has ended
				isStunned = false;
			}
		}

		// roots
		if (isRooted) {
			if (rootEndTime > Time.time) {
				// root still has time left on it

			} else {
				isRooted = false;
			}
		}

		// Slows
		// if there is a slow in the list, do slow logic
		if (slowList.Count > 0) {

			do{
				if(slowList[0].endTime < Time.time)
				{
					// first slow had no time left
					slowList.RemoveAt(0);
				}
				else
				{
					// found a slow that has time left
					// adjust the moveSpeed based on the original move speed
					// this code is run every frame so moveSpeed *= slow makes the move speed slow down more and more every frame
					currentMoveSpeed = originalMoveSpeed * ((100 - slowList[0].percent) / 100f);
					break;
				}
				// a slow may have been removed. Check again if there are slows in the list
			} while(slowList.Count > 0);

			// do-while either exitted with no slows left or with the break
			// check if there are no slows
			if (slowList.Count == 0) {
				// no slows left, reset move speed
				currentMoveSpeed = originalMoveSpeed;
			}
		}


		// movement
		switch (currentMovementType) {
		case MovementType.PlayerControlled:
			targetLocation = getPlayerTargetMoveLocation ();
			break;

		case MovementType.AiControlled:
			targetLocation = getAiTargetMoveLocation();
			break;

		case MovementType.BeingDisplaced:
			targetLocation = displacementLocation;
			// object moves faster when being displaced
			currentMoveSpeed = originalMoveSpeed * displacementMultiplier;
			if (Vector3.Distance (transform.position, targetLocation) < 0.1f) {
				// at the displacement location
				// stop being displaced and go back to normal movement
				currentMoveSpeed = originalMoveSpeed;
				currentMovementType = originalMovementType;
			}
			break;

		}

		// if you are stunned or rooted, don't move
		// but if you are being displaced, move anyway
		if (!isStunned && !isRooted || (currentMovementType == MovementType.BeingDisplaced)) {
			// multiply by deltaTime to get consistent movement
			// otherwise could get jumpy on frame skips
			// more of a problem in Update. FixedUpdate should be fine. 
			// fixedDeltaTime should always be the same (0.250 or whatever it is)
			transform.position = Vector3.MoveTowards (transform.position, targetLocation, currentMoveSpeed * Time.fixedDeltaTime);
		}
	}

	public virtual bool isPlayer()
	{
		return false;
	}



	public virtual Vector3 getAiTargetMoveLocation()
	{
		return Vector3.zero;
	}

	public virtual Vector3 getPlayerTargetMoveLocation()
	{
		// this will usually be based on input
		return transform.position + new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

		// it may change if the player uses a movement ability
	}

	public void setDisplacementLocation(Vector3 position)
	{
		displacementLocation = position;

		currentMovementType = MovementType.BeingDisplaced;
	}

	public void slow(float slowPercent, float slowDuration)
	{
		// add this slow to the list of slows
		slowList.Add(new Slow(slowPercent, slowDuration));
		// sort the slowList
		// y.CompareTo(x) because want bigger numbers at the start of the list
		slowList.Sort((x, y) => y.percent.CompareTo(x.percent));
	}

	public void stun(float stunDuration)
	{
		// hmmmmm....
		// this way will automatically space sttuns perfectly
		//stunDuration += stunTime;

		// this way should just make the stun end at stunEndTime
		// shoud work for multiple stuns
		stunEndTime = Time.time + stunDuration;
		isStunned = true;

	}

	public void root(float rootDuration)
	{
		rootEndTime = Time.time + rootDuration;
		isRooted = true;
	}
		
}
