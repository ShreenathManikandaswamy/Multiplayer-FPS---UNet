using UnityEngine;
using System.Collections;

	/// <summary>
	/// This script is attached to the player and it
	/// ensures that every players position, rotation, and scale,
	/// are kept up to date across the network.
	///
	/// This script is closely based on a script written by M2H,
	/// and edited by AnthonyB28 ( https://github.com/AnthonyB28/FPS_Unity/blob/master/MovementUpdate.cs )
	/// </summary>
    
public class PlayerSync : MonoBehaviour {

	Vector3 lastPosition;
	Quaternion lastRotation;
	Transform myTransform;
	NetworkView networkView;
	
	Vector3 targetPosition;
	Quaternion targetRotation;
	
	[SerializeField] float posThreshold = 0.1f;
	[SerializeField] float rotThreshold = 5f;
	
	void Start ()
	{
		networkView = GetComponent<NetworkView>();
		if(networkView.isMine == true)
		{
		    myTransform = transform;
			//Make sure everyone sees the player at right location the moment he spawns 
			if(!Network.isServer)
			{
				networkView.RPC("UpdateMovement", RPCMode.OthersBuffered, myTransform.position, myTransform.rotation);
	    	}
	    }
	}
	
	[RPC]
	void SetUsername (string name)
	{
		gameObject.name = name;
	}
	
	
	void Update ()
	{
	    if (networkView.isMine)
	    {
		    SendMovement();
	    }
	    else
	    {
		    ApplyMovement();
	    }
	}
	
	void SendMovement()
	{
		if (Vector3.Distance(myTransform.position, lastPosition) >= posThreshold)
		{
			//If player has moved, send move update to other players
			//Capture the player's position before the RPC is fired off and use this
			//to determine if the player has moved in the if statement above.
			lastPosition = transform.position;
			networkView.RPC("UpdateMovement", RPCMode.OthersBuffered, myTransform.position, myTransform.rotation);
		}
		if (Quaternion.Angle(myTransform.rotation, lastRotation) >= rotThreshold)
		{
			//Capture the player's rotation before the RPC is fired off and use this
			//to determine if the player has turned in the if statement above. 
			lastRotation = transform.rotation;
			networkView.RPC("UpdateMovement", RPCMode.OthersBuffered, myTransform.position, myTransform.rotation);
		}
	}
	
	void ApplyMovement ()
	{
		transform.position = Vector3.Lerp(transform.position, targetPosition, 0.5f);
		transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 0.5f);
	}
	
	[RPC]
	void UpdateMovement (Vector3 newPosition, Quaternion newRotation)
	{
		targetPosition = newPosition;
		targetRotation = newRotation;
	}
}