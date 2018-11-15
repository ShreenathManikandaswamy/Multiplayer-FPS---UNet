using UnityEngine;
using System.Collections;

public class SettingsManager : MonoBehaviour {
	
	public float sensitivity;
	public bool invert;

	public void Sensitivity (float sliderSens)
	{
		sensitivity = sliderSens;
	}
	
	public void ResetSensitivity ()
	{
		sensitivity = 1;
	}

	public void InvertYAxis ()
	{
		invert = !invert;
	}
}
