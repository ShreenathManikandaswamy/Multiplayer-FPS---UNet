using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{

	// How you use the game manager is up to you - just keep in mind that this will be on each client,
	// so try to avoid putting any conflicting code in here

	// The included script ensures there will only be one GameManager in your scene at any given time

	SettingsManager settings;

	public float sensitivity = 1f;
	public bool inverted = false;

	static GameManager instance = null;

	public static GameManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = (GameManager) FindObjectOfType (typeof (GameManager));
			}
			return instance;
		}
	}

	bool showNetworkMenu = true;
	[SerializeField] GameObject networkMenu;

	bool showPauseMenu = false;
	public bool paused = false;
	public bool chatting = false;

	[SerializeField] GameObject pauseMenu;
	[SerializeField] GameObject pauseOptions;
	[SerializeField] GameObject pauseSettings;
	[SerializeField] GameObject chatBox;

	void Awake ()
	{
		if (Instance != this)
		{
			Destroy (gameObject);
			Debug.Log ("DestroyedObjectPersist");
		}
		else
		{
			DontDestroyOnLoad (gameObject);
		}
		settings = GetComponent<SettingsManager> ();
	}

	void Update ()
	{
		if (Network.isServer || Network.isClient)
		{
			showPauseMenu = paused;
			showNetworkMenu = false;
			chatBox.SetActive (true);
		}
		else
		{
			showPauseMenu = false;
			showNetworkMenu = true;
			chatBox.SetActive (false);
		}

		if (showPauseMenu || showNetworkMenu)
		{
			if (!pauseOptions.activeSelf && !pauseSettings.activeSelf)
			{
				pauseOptions.SetActive (true);
				pauseSettings.SetActive (false);
			}
			LockCursor (true);
		}
		else
		{
			LockCursor (false);
		}
		pauseMenu.SetActive (showPauseMenu);
		networkMenu.SetActive (showNetworkMenu);
		sensitivity = settings.sensitivity;
		inverted = settings.invert;
	}

	void LockCursor (bool visible)
	{
		//Because cursor locking changes in Unity 5, this'll automatically switch to the appropriate one, depending on your version of Unity
		#if UNITY_5
		Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
		Cursor.visible = visible;
		#else
			Screen.lockCursor = !visible;
			Cursor.visible = visible;
		#endif
	}

	public bool PauseGame()
	{
		return !paused;
	}

	public void Resume ()
	{
		paused = PauseGame();
	}

	public void Quit ()
	{
		paused = PauseGame();
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#elif UNITY_WEBPLAYER
			Application.LoadLevel(1);
		#else
			Application.Quit();
		#endif
	}
}
