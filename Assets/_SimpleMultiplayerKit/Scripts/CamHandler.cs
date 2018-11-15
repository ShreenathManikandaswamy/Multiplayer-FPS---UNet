using UnityEngine;
using System.Collections;

public class CamHandler : MonoBehaviour
{

	GameManager gm;
	NetworkView networkView;
	
	float sensitivity = 1;
	public float minimumY = -60F;
	public float maximumY = 60F;

	float rotX = 0f;
	float rotY = 0F;
	float invert = 1;

	Vector3 look;

	Transform tParent;

	void Update()
	{
		if(!gm.paused)
			DoRotation();
	}
	void DoRotation ()
	{	
		sensitivity = gm.sensitivity;
		if (gm.inverted)
		{
			invert = 1;
		}
		else
		{
			invert = -1;
		}
		rotY += look.y * sensitivity/2 * invert;
		rotY = Mathf.Clamp(rotY, minimumY, maximumY);

		rotX = look.x*sensitivity/2;
		
		tParent.Rotate(0, rotX, 0);

		transform.localEulerAngles = new Vector3(rotY, transform.localEulerAngles.y, 0);
		
	}

	public void GetLook(Vector3 lookInput)
	{
		this.look = lookInput;
	}

	void Start ()
	{
		tParent = transform.parent;
		networkView = tParent.GetComponent <NetworkView>();
		gameObject.SetActive(networkView.isMine);
		gm = GameManager.Instance;
	}
}