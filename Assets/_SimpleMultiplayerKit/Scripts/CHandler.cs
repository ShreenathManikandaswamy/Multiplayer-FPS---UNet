using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections;

public class CHandler : MonoBehaviour
{

	Vector3 move = Vector3.zero;
	public float moveMagnitude = 0;

	bool jump = false;
	bool sprint = false;

	CapsuleCollider collider;
	Rigidbody rigidbody;
	Vector3 velocity;
	NetworkView networkView;

	public bool onGround = false;
	IComparer rayHitComparer;

	[SerializeField]
	float moveSpeed = 10;
	[SerializeField]
	float strafeSpeed = 10;
	float baseMoveSpeed = 10;

	[SerializeField]
	float jumpPower = 10;
	float jumpCooldown = 0;
	int jumpCount = 0;
	[SerializeField]
	int maxJumpCount = 2;


	[SerializeField] AdvancedSettings advancedSettings;                 // Container for the advanced settings class , thiss allows the advanced settings to be in a foldout in the inspector

	[System.Serializable]
	public class AdvancedSettings
	{
		public float gravityMultiplier = 1;
		public PhysicMaterial zeroFriction;			// used when in motion to enable smooth movement
		public PhysicMaterial maxFriction;			// used when stationary to avoid sliding down slopes
		public float groundStickyEffect = 5f;				// power of 'stick to ground' effect - prevents bumping down slopes.
	}

	// Use this for initialization
	void Start ()
	{
		rigidbody = GetComponent<Rigidbody>();
		collider = GetComponent<CapsuleCollider>();
		rayHitComparer = new RayHitComparer();
		baseMoveSpeed = moveSpeed;
		networkView = GetComponent <NetworkView>();
		//If the networkview isn't ours, set the rigidbody to kinematic (prevents vertical jitter from jumping)
		rigidbody.isKinematic = !networkView.isMine;
	}

	void FixedUpdate()
	{
		velocity = rigidbody.velocity;
		GroundCheck();
		SetFriction();
		Movement();
		
		if(networkView.isMine)
		{
			//After velocity has been adjusted, apply it
			rigidbody.velocity = velocity;
		}
	}

	public void ReceiveInput(Vector3 moveInput, bool jumpInput, bool sprintInput)
	{
		move = moveInput;
		jump = jumpInput;
		sprint = sprintInput;
	}

	void Movement()
	{
		//Player movement
		//Clamp movement magnitude
		move = Vector3.ClampMagnitude(move, 1);
		moveMagnitude = move.magnitude;

		//rigidbody.useGravity = !onGround;
		
		if (sprint)
		{
			moveSpeed = baseMoveSpeed * 2f;
		}
		else
		{
			moveSpeed = baseMoveSpeed;
		}
		velocity = transform.forward * move.y * moveSpeed + transform.right * move.x * strafeSpeed + transform.up * velocity.y;
		if (jump && (jumpCount < maxJumpCount || (jumpCooldown < Time.time && onGround)))
		{
			velocity.y = jumpPower; //new Vector3(rigidbody.velocity.x, rigidbody.velocity.y + jumpPower, rigidbody.velocity.z);
			jump = false;
			onGround = false;
			jumpCount++;
			jumpCooldown = Time.time + 1f;
		}
	}

	void GroundCheck ()
	{
		Ray ray = new Ray(transform.position, -transform.up);
		Debug.DrawRay(transform.position, -Vector3.up);
		RaycastHit[] hits = Physics.RaycastAll(ray, collider.height * 0.6f);
		if (hits.Length > 0)
		{
			System.Array.Sort(hits, rayHitComparer);
//			Debug.Log(hits.Length);
		}

		if (velocity.y < jumpPower * .5f)
		{
			onGround = false;
			foreach (var hit in hits)
			{
				// check whether we hit a non-trigger collider (and not the character itself)
				if (!hit.collider.isTrigger)
				{
					onGround = true;
					jumpCount = 0;
				}
			}
		}
		// add extra gravity
		rigidbody.AddForce(Physics.gravity * (advancedSettings.gravityMultiplier - 1));
	}

	void SetFriction ()
	{

		if (onGround)
		{

			// set friction to low or high, depending on if we're moving
			if (move.magnitude < 0.01f)
			{
				// when not moving this helps prevent sliding on slopes:
				collider.material = advancedSettings.maxFriction;
			}
			else
			{
				// but when moving, we want no friction:
				collider.material = advancedSettings.zeroFriction;
			}
		}
		else
		{
			// while in air, we want no friction against surfaces (walls, ceilings, etc)
			collider.material = advancedSettings.zeroFriction;
		}
	}

	//used for comparing distances
	class RayHitComparer : IComparer
	{
		public int Compare (object x, object y)
		{
			return ((RaycastHit)x).distance.CompareTo(((RaycastHit)y).distance);
		}
	}
}
