using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour {

	[SerializeField] public ServerSettings advancedSettings;
	
	[System.Serializable]
	public class ServerSettings
	{
		public string gameName = "SUNKExample";		// When RefreshHostList() is called, only servers with this exact string will be displayed CTRL-F GNINFO FOR IMPORTANT INFO
		public string serverName = "Example name";	// This is the name that will be displayed when returned by RefreshHostList()
		public string serverPort = "25000";			// This is the port you'd like to use; useful to know/change if you need to port forward
		public string username = "";				// What you want your username to be
		public string password = "";				// The password for the server; needed to join a passworded server, or to create a server
		public string maxPlayers = "4";				// Maximum number of players allowed in the server; CTRL-F MPINFO FOR IMPORTANT INFO
		public bool passwordProtected = false;		// If true, require a password on server join
		public bool privateServer = false;			// If this is true, then the server will become password-protected, and only those with the password can join
		public bool dedicated = false;				// If true, the server host won't spawn as a player, but will still be joinable. A separate instance is required for the server host to join and play
		public bool showAdminMenu = false;			// If dedicated is true, this will be shown on screen; customize it to display appropriate admin functions

	}

	HostData[] hostList;							// This is where the list of servers will be stored later

	[SerializeField] Transform[] spawnZones;		// This is where we store the Transform data of the spawn points, used when spawning the players
	[SerializeField] GameObject playerObject;		// This is where we store the GameObject data of the player character; This can be transformed into an array, if you'd like to create a character-select

	[SerializeField] Button buttonPrefab;			// This is what will populate the server list (Since we can't easily create a button at run-time)
	[SerializeField] Transform buttonListHolder;	// This is what will populate the server list (Since we can't easily create a button at run-time)

	[SerializeField] Text errorText;
	
	ChatSystem chatSys;
	
	//===========
	// This is where we check NAT settings
	// Check http://docs.unity3d.com/Documentation/ScriptReference/Network.TestConnection.html for more information
	
	[SerializeField] Text testMessage;
	string testMessageText = "Test in progress.";
	string shouldEnableNatMessage = "";
	public bool doneTesting = false;
	bool probingPublicIP = false;
	ConnectionTesterStatus connectionTestResult = ConnectionTesterStatus.Undetermined;
	bool useNat = false;
	//===========

	void Awake()
	{
		chatSys = GetComponent<ChatSystem>();
		// On Awake, we want to ensure the master server is empty, and a placeholder username is chosen
		MasterServer.ClearHostList();
		testMessage.text = "Testing network connection capabilities.";
//		username = "Player"+Random.Range(0,99).ToString();	// We're randomizing a digit at the end, just to give some variety
	}
	void Update ()
	{
		testMessage.text = testMessageText;
		if (!doneTesting)
		{
			//If not testing, then run the test
			TestConnection();
		}
	}

	public void StartServer() {
		// This is where we start creating the server
		
		// All these 'else if' statements are just to warn the player that a field is incorrectly filled
		// Once all fields are appropriately filled, then the server will be created
		if (advancedSettings.serverPort == "" || int.Parse(advancedSettings.serverPort) <= 0 || int.Parse(advancedSettings.serverPort) > 65535)
		{
			errorText.text = "Error: "+"Invalid Port Number\n"+errorText.text;
		}
		else if (advancedSettings.maxPlayers == "" || int.Parse(advancedSettings.maxPlayers) <= 0)
		{
			errorText.text = "Error: "+"Invalid Max Players Number\n"+errorText.text;
		}
		else if (advancedSettings.serverName == "")
		{
			errorText.text = "Error: "+"Invalid Server Name\n"+errorText.text;
		}
		else if (advancedSettings.username == "")
		{
			errorText.text = "Error: "+"Invalid Userame\n"+errorText.text;
		}
		else if (advancedSettings.passwordProtected && advancedSettings.password == "")
		{
			errorText.text = "Error: "+"Invalid Password\n"+errorText.text;
		}
		else{
			Debug.Log("U: " + advancedSettings.username + ", SN: " + advancedSettings.serverName);
			if (advancedSettings.password != "" && advancedSettings.passwordProtected)
			{
				Network.incomingPassword = advancedSettings.password;
				Network.InitializeServer(int.Parse(advancedSettings.maxPlayers) - 1, int.Parse(advancedSettings.serverPort), useNat); 	// we check whether useNat is needed later on automatically. Again, the documentation mentioned earlier will provide better explanation
				MasterServer.RegisterHost(advancedSettings.gameName, "[P]" + advancedSettings.serverName);								// Now we register the server, using the unique gameName assigned earlier, and the server name assigned by the player.
																																		// This will display "[P]" in front of the server if there is a password
			}
			else if (advancedSettings.password == "" || !advancedSettings.passwordProtected)
			{
				// If we haven't put in a password, or if passwordProtected is false, go ahead and create a public server
				Network.InitializeServer(int.Parse(advancedSettings.maxPlayers) - 1, int.Parse(advancedSettings.serverPort), useNat);
				MasterServer.RegisterHost(advancedSettings.gameName, advancedSettings.serverName);
			}
			//If server creation successful, set the username in the ChatSystem
			chatSys.username = advancedSettings.username;
		}
	}
	
	void OnServerInitialized() {
		// Once the server is created, check if we are dedicated or not;
		// If we're dedicated, don't spawn a player, instead show the admin menu. Otherwise, spawn the player
		
		Debug.Log("Server Initialized");
		if (advancedSettings.dedicated)
		{
			advancedSettings.showAdminMenu = true;
		}else{
			SpawnPlayer();
		}
	}
	
	public void JoinServer (int serverNumber)
	{
		//Only join if we have a username
		if (advancedSettings.username != "")
		{
			if (hostList[serverNumber].passwordProtected)
			{
				Network.Connect(hostList[serverNumber], advancedSettings.password);
			}
			else
			{
				Network.Connect(hostList[serverNumber]);
			}
			chatSys.username = advancedSettings.username;
		}
		else
		{
			errorText.text = "Invalid Username\n"+errorText.text;
		}
	}
	
	void OnConnectedToServer() {
		Debug.Log("Server Joined");
		// If we successfully join the server, spawn our player object
		SpawnPlayer();
	}
	
	void SpawnPlayer()	{
		// Spawning the player. By using Network.Instantiate instead of just Instantiate, everyone on the server will have the player object instantiated (Though control should be reserved only to the owner of the NetworkView)
		int randomSpawn = Random.Range(0, spawnZones.Length-1);	// We're choosing random spawn points from an array just for convenience here - you can handle this how you like, just keep in mind: Network.Instantiate creates the object on everyone's end
		GameObject networkPlayer = Network.Instantiate(playerObject, spawnZones[randomSpawn].transform.position, Quaternion.identity, 0) as GameObject;
		networkPlayer.GetComponent<NetworkView>().RPC("SetUsername", RPCMode.AllBuffered, advancedSettings.username);
	}

	public void RefreshHostList()
	{
		// When invoked, clear out the host list of previous data, and get a fresh list
		MasterServer.ClearHostList();
		MasterServer.RequestHostList(advancedSettings.gameName);
		if (hostList != null)
		{
			Debug.Log("Hostlist != null, length = " + hostList.Length);
			// If there is data in the hostList, display each server as a button
			//First, destroy the current buttons in the list
			DestroyCurrentButtons();
			for (int i = 0; i < hostList.Length; i++)
			{
				Button serverButton = Instantiate(buttonPrefab) as Button;
				serverButton.GetComponentInChildren<Text>().text = (hostList[i].gameName + " | " + hostList[i].connectedPlayers + "/" + hostList[i].playerLimit + " players");
				serverButton.transform.parent = buttonListHolder;
				serverButton.GetComponent<ServerButtonHandler>().serverNumber = i;
				serverButton.transform.localPosition = new Vector3(-137, -5 - (30 * i), 0);	//X should actually be 5, but there is a parenting issue (Unity 4.6 is still beta, after all)
			}
		}
		else
		{
			Debug.Log("Hostlist = null");
		}
	}

	void DestroyCurrentButtons()
	{
		ServerButtonHandler[] sbh = FindObjectsOfType<ServerButtonHandler>();
		foreach (ServerButtonHandler s in sbh)
		{
			Destroy(s.gameObject);
		}
	}

	void OnMasterServerEvent(MasterServerEvent msEvent) {
		if(msEvent == MasterServerEvent.HostListReceived){
			hostList = MasterServer.PollHostList();
		}
	}

	void OnFailedToConnect (NetworkConnectionError error){
		// If we can't connect, tell the user why
		errorText.text = "Error: "+error.ToString()+"\n" +errorText.text;
	}


	void OnPlayerDisconnected(NetworkPlayer player){
		// When a player disconnects, we clean up after the player by doing the following
		Debug.Log("Cleaning up after player " + player);
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
	}

	void OnDisconnectedFromServer (NetworkDisconnection info){
		// In here, we can check why we've been disconnected, and do things accordingly.
		// In this case, we check why we DC'd, then reload the level to get back to the menu
		if(info == NetworkDisconnection.Disconnected){errorText.text = "Disconnected\n"+errorText.text;}
		else if(info == NetworkDisconnection.LostConnection){errorText.text = "LostConnection\n"+errorText.text;}
		Application.LoadLevel(1);
	}

	public void Disconnect()
	{
		Network.Disconnect();
	}
	public void Username(string sUser)
	{
		advancedSettings.username = sUser;
	}

	public void Password(string sPass)
	{
		advancedSettings.password = sPass;
	}
	public void ServerName (string sName)
	{
		advancedSettings.serverName = sName;
	}
	public void ServerPort (string sPort)
	{
		advancedSettings.serverPort = sPort;
	}
	public void Dedicated (bool sDedi)
	{
		advancedSettings.dedicated = sDedi;
	}

	public void PasswordProtected(bool sPassProt)
	{
		advancedSettings.passwordProtected = sPassProt;
	}

	public void MaxPlayers(string sMax)
	{
		advancedSettings.maxPlayers = sMax;
	}

	void TestConnection () {
		float timer = Time.time;
		// Start/Poll the connection test, report the results in a label and 
		// react to the results accordingly
		connectionTestResult = Network.TestConnection();
		switch (connectionTestResult) {
		case ConnectionTesterStatus.Error: 
			testMessageText = "Problem determining NAT capabilities";
			doneTesting = true;
			break;
			
		case ConnectionTesterStatus.Undetermined: 
			testMessageText = "Undetermined NAT capabilities";
			doneTesting = false;
			break;
			
		case ConnectionTesterStatus.PublicIPIsConnectable:
			testMessageText = "Directly connectable public IP address.";
			useNat = false;
			doneTesting = true;
			break;
			
			// This case is a bit special as we now need to check if we can 
			// circumvent the blocking by using NAT punchthrough
		case ConnectionTesterStatus.PublicIPPortBlocked:
			testMessageText = "Non-connectable public IP address (port " + advancedSettings.serverPort + " blocked), running a server is impossible.";
			useNat = false;
			// If no NAT punchthrough test has been performed on this public 
			// IP, force a test
			if (!probingPublicIP) {
				connectionTestResult = Network.TestConnectionNAT();
				probingPublicIP = true;
				testMessage.text = "Testing if blocked public IP can be circumvented";
				timer = Time.time + 10;
			}
			// NAT punchthrough test was performed but we still get blocked
			else if (Time.time > timer) {
				probingPublicIP = false; 		// reset
				useNat = true;
				doneTesting = true;
			}
			break;
		case ConnectionTesterStatus.PublicIPNoServerStarted:
			testMessageText = "Public IP address but server not initialized, "+
				"it must be started to check server accessibility. Restart "+
					"connection test when ready.";
			break;
			
		case ConnectionTesterStatus.LimitedNATPunchthroughPortRestricted:
			testMessageText = "Limited NAT punchthrough capabilities. Cannot "+
				"connect to all types of NAT servers. Running a server "+
					"is ill advised as not everyone can connect.";
			useNat = true;
			doneTesting = true;
			break;
			
		case ConnectionTesterStatus.LimitedNATPunchthroughSymmetric:
			testMessageText = "Limited NAT punchthrough capabilities. Cannot "+
				"connect to all types of NAT servers. Running a server "+
					"is ill advised as not everyone can connect.";
			useNat = true;
			doneTesting = true;
			break;
			
		case ConnectionTesterStatus.NATpunchthroughAddressRestrictedCone:
		case ConnectionTesterStatus.NATpunchthroughFullCone:
			testMessageText = "NAT punchthrough capable. Can connect to all "+
				"servers and receive connections from all clients. Enabling "+
					"NAT punchthrough functionality.";
			useNat = true;
			doneTesting = true;
			break;
			
		default: 
			testMessageText = "Error in test routine, got " + connectionTestResult;
			break;
		}
		if (doneTesting) {
			if (useNat)
				shouldEnableNatMessage = "When starting a server the NAT "+
					"punchthrough feature should be enabled (useNat parameter)";
			else
				shouldEnableNatMessage = "NAT punchthrough not needed";
			testMessage.text = shouldEnableNatMessage + "Done testing";
		}
	}
}

//	===== GNINFO =====	//
//
//	It is important to note, that any other Unity Game that uses the same Game Name as this
//	will also be displayed, though connection may not be possible. I recommend changing this
//	as soon as possible, to something with more obscurity. EG/ if your game were named Dario,
//	something like Dario2014Alpha01131a would be obscure enough that the chances of someone else
//	using it would be astronomically low. I personally use the convention GameNameDateVersion due to the high obscurity
//	
//	==================	//


//	===== MPINFO =====	//
//
//	While maxPlayers is set to X, it actually has 1 subtracted from
//	that value when used; this is because it actually treats the number
//	as an array index (where Object 1 is indexed at 0)
//	
//	Because most players would reasonably assume that making a server only
//	1 slot would mean only 1 player is allowed (instead of 1 slot for 2 players),
//	this caters to a perfectly reasonable assumption.
//
//	It's important to know this when either customizing this script, or creating one from scratch,
//	to prevent unwanted server issues
//
//	==================	//