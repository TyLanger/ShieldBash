using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour {

	public PlayerController player;


	public bool isShielding()
	{
		return player.isShielding ();
	}

}
