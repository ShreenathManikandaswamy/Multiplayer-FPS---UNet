using UnityEngine;
using System.Collections;

public class ServerButtonHandler : MonoBehaviour
{

	public int serverNumber = -1;
	NetworkManager nm;

	void Awake()
	{
		nm = GameManager.Instance.GetComponent<NetworkManager>();
	}

	public void JoinServer()
	{
		
		nm.JoinServer(serverNumber);
	}
}
