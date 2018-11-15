using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour
{

	CHandler ch;
	CamHandler camh;
	GameManager gm;

	Vector3 move;
	Vector3 look;

	bool attack1;
	bool attack2;

	bool interact;
	bool jump;
	bool sprint;

	// Use this for initialization
	void Start ()
	{
		//Input should only be accepted locally - the PlayerSync script will handle moving the players across the network
		enabled = GetComponent<NetworkView>().isMine;
		//Assign the components
		ch = GetComponent<CHandler>();
		camh = GetComponentInChildren<CamHandler>();
		gm = GameManager.Instance;
	}
	
	// Update is called once per frame
	void Update () {
	
		if(!gm.chatting)
		{
			if (Input.GetKeyDown(KeyCode.P))
			{
				gm.paused = gm.PauseGame();
			}
			GetInput();				//Get the necessary input, and assign it to the appropriate variables
		}
		else
		{
			move = Vector3.zero;
			look = Vector3.zero;
		}
		camh.GetLook(look);		//Send the look input in Update, to avoid jagged movement of camera when VSync is disabled
	}

	void FixedUpdate()
	{
//		if(!gm.chatting)
			SendInput();
	}

	void GetInput()
	{
		move = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
		look = new Vector3(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"), 0);

		if (Input.GetButtonDown("Jump"))
		{
			jump = true;
		}
		sprint = Input.GetKey(KeyCode.LeftShift);
	}

	void SendInput()
	{
		ch.ReceiveInput(move,jump,sprint);
		jump = false;
	}

}
