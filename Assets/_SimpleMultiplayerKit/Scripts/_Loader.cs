using UnityEngine;
using System.Collections;

public class _Loader : MonoBehaviour {
	
	// This scene is purely to load the main level. The reason for this is so that the GameManager is created only in this level, and not whenever the main level is loaded
	// I recommend researching/looking into Singletons to learn more about the purpose of this
	void Start () {
		if(GameManager.Instance != null)
			Application.LoadLevel(1);
	}
}
