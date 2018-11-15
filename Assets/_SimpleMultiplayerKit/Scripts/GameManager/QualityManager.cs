using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QualityManager : MonoBehaviour {

	[SerializeField] Slider qualitySlider;
	public int qualityLevel;

	void Awake ()
	{
		qualitySlider.value = PlayerPrefs.GetInt("QualityLevel");
	}

	public void ChangeQuality (float qual)
	{
		if(Application.isPlaying)
		{
			qualityLevel = (int) qual;
			QualitySettings.SetQualityLevel(qualityLevel);
			PlayerPrefs.SetInt("QualityLevel", qualityLevel);
			Debug.Log("QL: " + qualityLevel + " | PP: " + PlayerPrefs.GetInt("QualityLevel"));
		}
	}
}
