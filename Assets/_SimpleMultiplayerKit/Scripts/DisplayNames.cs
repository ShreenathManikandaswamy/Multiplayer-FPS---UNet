using UnityEngine;
using System.Collections;

public class DisplayNames : MonoBehaviour {
	
	//Attach this script directly to the NameDisplay object
	
	public Transform player;
	
	// Use this for initialization
	void Start () {
		GetComponent<TextMesh>().text = transform.root.name;	//Since the player's object is named the same as their Username,
		//we can just reference the gameObject's name.
		
		//Get the local player, so we can look at them
		GetPlayer ();
	}
	
	void GetPlayer ()
	{
		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");	//Get all the players in the server
		foreach (GameObject p in players)
		{
			if(p.GetComponent<NetworkView>().isMine)										//If this player is ours, set this as the player
			{
				player = p.transform;										//This way, it'll only afftect it on each players end
			}
		}
	}
	
	void Update ()
	{
		if(player)															//We check to see if we have a player assigned; if so, look at the player
		{
			Vector3 lookPos = 2 * transform.position - player.position;
			lookPos.y = player.position.y;
			transform.LookAt (lookPos);		//Because the TextMesh is basically backwards, we have to technically look in the opposite direction
		}
		else
		{
			//If we don't have a player, get one
			GetPlayer ();
		}
	}
}