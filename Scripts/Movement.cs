using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
	Rigidbody2D SelfRB;
	Collider2D SelfColl;
	float mass;
	[Header("Jump properties")]
	[SerializeField] int JumpRemains;
	[SerializeField] int JumpsCount;
	[SerializeField] int JumpForce;
	[SerializeField] int JumpCost;
	[Space]

	[Header("Movement properties")]
	[SerializeField] int MaxSpeed;
	[SerializeField] float Acceleration;
	[SerializeField] float AirAcceleration;
	[SerializeField] float SlideSpeed;
	[SerializeField] int SlideCost;
	[Space]

	[Header("Dash properties")]
	[SerializeField] int DashSpeed;
	[SerializeField] int DashLength;
	[SerializeField] int DashCost;
	[SerializeField] int DashCoolDown;
	[Space]

	[Header("Abilities")]
	[SerializeField] bool WallJumpAbility;
	[SerializeField] bool DashAbility;
	bool DashCD;
	bool IsDashing;
	bool SlideDown;

	int LocalMaxspeed;
	float LocalAcceleration;
	[Space]

	[Header("Weapon")]
	[Tooltip("First weapon is usually sword")]
	[SerializeField] public Weapon First;
	[Tooltip("First weapon is usually shield")]
	[SerializeField] public Weapon Second;
	[Tooltip("First weapon is usually gun")]
	[SerializeField] public Weapon Third;
	[Header("Debug")]
	[SerializeField] int Wall;
	[SerializeField] bool OnGround;
	[SerializeField] bool OnWall;
	[SerializeField] bool Sliding;


	Animator SelfAnim;
	SpriteRenderer SelfRenderer;
	float JumpAnimationLength;
	int lastDir=1;
	Entity selfEntity;
	public bool IsAttack;

	// Start is called before the first frame update
	void Start()
	{
		SelfRenderer = GetComponent<SpriteRenderer>();
		SelfAnim = GetComponent<Animator>();
		selfEntity = GetComponent<Entity>();
		SelfRB = GetComponent<Rigidbody2D>();
		SelfColl = GetComponent<Collider2D>();
		JumpAnimationLength = GetJumpAnimationLength();
		mass = SelfRB.mass;
	}


	// Update is called once per frame
	void FixedUpdate()
	{
		Slide();
		OnGround = CheckGround();
		SelfAnim.SetBool("On Ground", OnGround);
		if (First.Activate||Second.Activate)
		{
			IsAttack = true;
		}
		else
		{
			IsAttack = false;
		}
		SelfAnim.SetInteger("Velocity Y", (int)Mathf.Sign(SelfRB.velocity.y));
		MoveWeaponLayer();
	}
	public void Attack(Weapon weapon, string number)
	{
		if (OnGround && !IsDashing)
		{
			SelfAnim.SetTrigger("Attack "+number);
			weapon.Fire(true);
		}
	}
	public void Attack(Weapon weapon, string number, bool phase)
	{
		if (OnGround && !IsDashing)
		{
			weapon.Fire(phase);
			SelfAnim.SetBool("Attack " + number,weapon.Activate);
		}
	}

	void MoveWeaponLayer()
	{
		int a = 0;
		if (SelfRenderer.sprite.ToString().StartsWith("L"))
		{
			a = -1;
		}
		else a = 1;
		if (First != null) First.transform.localPosition = Vector3.forward * First.StartZ * a;
		if (Second != null) Second.transform.localPosition = Vector3.forward * Second.StartZ * a;
		if (Third != null) Third.transform.localPosition = Vector3.forward * Third.StartZ * a;
	}

	public void Move(Vector2 direction)
	{
		if (IsAttack) direction = Vector2.zero;
		if (Sliding) direction = new Vector2(-Wall, direction.y);
		if (direction.y < -0.5)
		{
			SlideDown = true;
		}
		else
		{
			SlideDown = false;
		}

		if (OnGround)
		{
			LocalAcceleration = Acceleration;
		}
		else
		{
			LocalAcceleration = AirAcceleration;
		}
		SelfAnim.SetFloat("Speed", Mathf.Abs(SelfRB.velocity.x));
		SelfAnim.SetInteger("Dir", lastDir);

		if (IsDashing)
		{
			return;
		}
		Vector2 force = direction * LocalAcceleration * mass;
		SelfRB.AddForce(force);
		Friction();


		if (direction.x == 0)
		{
			if (OnGround) LocalMaxspeed = 0;
			if (Mathf.Abs(SelfRB.velocity.x) < 1f)
			{
				SelfRB.velocity = new Vector2(0, SelfRB.velocity.y);
			}
		}
		else
		{
			lastDir = (int)Mathf.Sign(direction.x);
			LocalMaxspeed = MaxSpeed;
		}
	}
	void Friction()
	{
		float maxFriction;
		if (LocalMaxspeed == 0) maxFriction = MaxSpeed;
		else maxFriction = LocalMaxspeed;
		var k = Mathf.Abs(SelfRB.velocity.x) / (maxFriction);
		if (k > LocalAcceleration) k = LocalAcceleration;
		Vector2 frictionForce = Vector2.right * mass * LocalAcceleration * -Mathf.Sign(SelfRB.velocity.x) * k;
		SelfRB.AddForce(frictionForce);
	}
	bool CheckGround()
	{
		ContactPoint2D[] contacts = new ContactPoint2D[10];
		SelfRB.GetContacts(contacts);
		if (contacts[0].collider == null)
		{
			Wall = 0;
			if(Mathf.Abs(SelfRB.velocity.y)<5f)
			{
				return CheckForGroundViaRay();
			}
			return false;
		}
		foreach (var contact in contacts)
		{
			if (contact.collider == null)
			{
				break;
			}
			if (contact.collider.tag == "Ground")
			{
				if (contact.normal.y > 0.9)
				{
					Wall = 0;
					JumpRemains = JumpsCount;
					return true;
				}
				else if (contact.normal.y == 0)
				{
					Wall = (int)contact.normal.x;
				}
			}
		}
		return false;
	}
	bool CheckForGroundViaRay()
	{
		var hit = Physics2D.Raycast(transform.position + Vector3.down * (SelfColl.bounds.extents.y+0.1f),Vector2.down,1,LayerMask.GetMask("Ground"));
		if (hit.transform == null) return false;
		if (hit.transform.tag == "Ground")
		{
			return true;
		}
		else return false;
	}
	public void Jump(float relativeForce)
	{
		StartCoroutine(IeJump());
	}
	IEnumerator IeJump()
	{
		if (JumpRemains != 0 && selfEntity.StaminaRemains >= JumpCost && !IsDashing&&!IsAttack)
		{
			SelfAnim.SetTrigger("Jump");
			selfEntity.StaminaRemains -= JumpCost;

			if (Mathf.Abs(SelfRB.velocity.x) < 0.1f)
			{
				yield return new WaitForSeconds(JumpAnimationLength);
			}
			var xSpeed = SelfRB.velocity.x;
			SelfRB.velocity = new Vector2(xSpeed, 0);
			if (Wall == 0)
			{
				SelfRB.AddForce(Vector2.up * JumpForce * mass);
			}
			else
			{
				SelfRB.AddForce(new Vector2(Wall * 0.5f, 1).normalized * JumpForce * mass);
			}
			yield return new WaitForFixedUpdate();
			yield return new WaitForFixedUpdate();
			JumpRemains--;
		}
		StopCoroutine(IeJump());
		yield break;
	}
	float GetJumpAnimationLength()
	{
		float length = 0;
		List<AnimationClip> clips = new List<AnimationClip>();
		clips.AddRange(SelfAnim.runtimeAnimatorController.animationClips);
		if (SelfAnim.HasState(0, Animator.StringToHash("Jump L"))) length = clips.Find(x => x.name == "Jump L").length - 0.1f;
		return length;
	}
	public static float GetAnimationLength(string name, GameObject obj)
	{
		var anim = obj.GetComponent<Animator>();
		float length = 0;
		List<AnimationClip> clips = new List<AnimationClip>();
		clips.AddRange(anim.runtimeAnimatorController.animationClips);
		if (anim.HasState(0, Animator.StringToHash(name))) length = clips.Find(x => x.name == name).length - 0.01f;
		return length;
	}
	/// <summary>
	/// Get current animation state length
	/// </summary>
	/// <param name="obj"></param>
	/// <returns></returns>
	public static float GetAnimationLength(GameObject obj)
	{
		var anim = obj.GetComponent<Animator>();
		return anim.GetCurrentAnimatorStateInfo(0).length;
	}

	void Slide()
	{
		if (Wall == 0 || !WallJumpAbility || selfEntity.StaminaRemains < SlideCost || SlideDown)
		{
			Sliding = false;
			return;
		}
		Sliding = true;
		selfEntity.StaminaRemains -= SlideCost;
		float frictionForce = -SelfRB.velocity.y / SlideSpeed * mass;
		if (frictionForce > Physics2D.gravity.y * mass) frictionForce = Physics2D.gravity.y * mass;
		if (SelfRB.velocity.y < -SlideSpeed) SelfRB.AddForce(Vector2.up * Physics2D.gravity.y * frictionForce);
	}
	public void Dash()
	{
		if (DashAbility && !DashCD && selfEntity.StaminaRemains > DashCost && OnGround)
		{
			selfEntity.StaminaRemains -= DashCost;
			DashCD = true;
			StartCoroutine(IeDash(lastDir));
		}
	}
	IEnumerator IeDash(float direction)
	{
		IsDashing = true;
		for (int i = 0; i < DashLength; i++)
		{
			LocalMaxspeed = DashSpeed;
			SelfRB.AddForce(Vector2.right * Mathf.Sign(direction) * 50);
			yield return new WaitForFixedUpdate();
		}
		IsDashing = false;
		LocalMaxspeed = MaxSpeed;
		yield return new WaitForSeconds(DashCoolDown);
		DashCD = false;
		yield break;
	}

}
