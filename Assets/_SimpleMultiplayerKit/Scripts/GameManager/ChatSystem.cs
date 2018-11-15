using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ChatSystem : MonoBehaviour {
	
	string chatString = "";							//The string of text we send to the chat box
	public string username = "";
	[SerializeField] Text chatBox;									//The chat box that shows all the chat
	[SerializeField] InputField inputBox;							//The input field, where we type our new chat
	
	GameManager gm;
	NetworkView networkView;
	
	public bool chatting = false;
	
	// Use this for initialization
	void Start () {
		gm = GameManager.Instance;
		networkView = GetComponent<NetworkView>();
		chatBox.text = chatString;
	}
	
	void Update ()
	{
		if(Network.isClient || Network.isServer)
		{
			if(Input.GetKeyUp (KeyCode.Return))		//If player presses Enter/Return while connected, open the chat field
			{
				if(!chatting)
				{
					inputBox.gameObject.SetActive(true);					//Activate the inputBox
					EventSystem.current.SetSelectedGameObject(inputBox.gameObject, null);	//Focus on the inputBox
					inputBox.ActivateInputField();							//Activate the input field
					chatting = true;
					gm.chatting = true;
				}
				else 
				{
					if(chatString != "")
					{
						//If the text box isn't empty, send the text
						SendText ();
					}
					chatting = false;
					gm.chatting = false;
				}
			}	
			if(chatting)
			{
				chatString = inputBox.text;
			}
		}
	}
	
	void SendText ()
	{
		networkView.RPC ("GetText", RPCMode.AllBuffered, "[" + username + "]: " + chatString);	//Send the text to all players
	}
	
	[RPC]
	void GetText (string newText)
	{
		chatBox.text = newText + "\n" + chatBox.text;		//Add the new text underneath the last line, and clear out the values for the next use
		inputBox.text = "";
		chatString = "";
		chatting = false;
		gm.chatting = false;
	}
}
