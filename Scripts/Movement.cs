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
	[SerializeField] public int JumpForce;
	[SerializeField] int JumpCost;
	[Space]

	[Header("Movement properties")]
	[SerializeField] public int MaxSpeed;
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
	[SerializeField] bool CanFly;
	bool DashCD;
	
	bool SlideDown;

	int LocalMaxspeed;
	[HideInInspector]public float LocalAcceleration;
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
	[SerializeField] bool IsJumping;
	[SerializeField]bool IsDashing;
	bool Shit;


	Animator SelfAnim;
	SpriteRenderer SelfRenderer;
	float JumpAnimationLength;
	int lastDir=1;
	[HideInInspector]public Entity selfEntity;
	public bool IsAttack;
	float gravityScale;
	/// <summary>
	/// For flying objects only
	/// </summary>
	float MaxYVel;
	[HideInInspector]public bool AnotherInput;

	/// <summary>
	/// Only for pushing entities
	/// </summary>

	// Start is called before the first frame update
	void Start()
	{
		SelfRenderer = GetComponent<SpriteRenderer>();
		TryGetComponent(out SelfAnim);
		selfEntity = GetComponent<Entity>();
		SelfRB = GetComponent<Rigidbody2D>();
		SelfColl = GetComponent<Collider2D>();
		if(SelfAnim!=null)JumpAnimationLength = GetJumpAnimationLength();
		mass = SelfRB.mass;
		gravityScale = SelfRB.gravityScale;
	}



	// Update is called once per frame
	void FixedUpdate()
	{
		if(WallJumpAbility) Slide();
		OnGround = CheckGround();
		SetLocalAcceleration();
		if (SelfAnim != null)
		{
			SelfAnim.SetBool("On Ground", OnGround);
			SelfAnim.SetInteger("Velocity Y", (int)Mathf.Sign(SelfRB.velocity.y));
			SelfAnim.SetFloat("Speed", Mathf.Abs(SelfRB.velocity.x));
			SelfAnim.SetInteger("Dir", lastDir);
			if (Shit)
			{
				SelfAnim.SetTrigger("Jump");
				Shit = false;
			}
			if (CanFly)
			{
				SelfAnim.SetFloat("Relative Y Velocity", SelfRB.velocity.y / MaxYVel);
			}
		}
		if (First != null || Second != null || Third != null)
		{
			MoveWeaponLayer();
			if (First.Activate || Second.Activate)
			{
				IsAttack = true;
			}
			else
			{
				IsAttack = false;
			}
		}
		if (selfEntity.GettingDamage)
		{
			//StopAllCoroutines();
			IsDashing = false;
			IsJumping = false;
		}
	}
	public void Attack(Weapon weapon, string number)
	{
		if (!IsDashing)
		{
			SelfAnim.SetTrigger("Attack "+number);
			weapon.Fire(true);
		}
	}
	public void Attack(Weapon weapon, string number, bool phase)
	{
		if ((OnGround||CanFly) && !IsDashing)
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
	public bool draw;
	void Flight(float y)
	{
		float factor = 0;
		if (y > Mathf.Sqrt(2) / 2)
		{
			factor = 1;
		}
		else
		{
			factor = y / (Mathf.Sqrt(2) / 2);
		}

		float force = JumpForce*0.3f+JumpForce*0.7f*factor;
		MaxYVel= (y * 2 * MaxSpeed - MinVelocity(force));
		if (SelfRB.velocity.y < y*2*MaxSpeed-MinVelocity(force))
		{
			//print(SelfRB.velocity.y + "    " + (y * 2 * MaxSpeed - MinVelocity()));
			if(!IsJumping)Jump(force);
		}
	}
	float MinVelocity(float force)
	{
		float deltaT = Time.fixedDeltaTime;
		var g = Physics2D.gravity.magnitude;
		var Sc = gravityScale;
		float A = force - g*mass*Sc;
		float vel = A  * deltaT;
		return vel;
	}
	void SetLocalAcceleration()
	{
		if (OnGround)
		{
			LocalAcceleration = Acceleration;
		}
		else
		{
			LocalAcceleration = AirAcceleration;
		}
		if (IsDashing)
		{
			LocalAcceleration = DashSpeed;
		}
	}
	public void Move(Vector2 direction, bool SideInput, float LocalAcceleration)
	{
		if (direction != Vector2.zero) direction = direction.normalized;
		else direction = Vector2.zero;
		//if (IsAttack) direction = Vector2.zero;
		if (Sliding) direction = new Vector2(-Wall, direction.y);
		if (direction.y < -0.5)
		{
			SlideDown = true;
		}
		else
		{
			SlideDown = false;
		}
		
		if (AnotherInput&&!SideInput)
		{
			return;
		}
		
		Vector2 force = direction.x *Vector2.right * LocalAcceleration * mass;
		SelfRB.AddForce(force);
		Friction(LocalAcceleration);


		if (direction.x == 0)
		{
			if (OnGround) LocalMaxspeed = 0;
			if (Mathf.Abs(SelfRB.velocity.x) < 1f&&!selfEntity.GettingDamage)
			{
				SelfRB.velocity = new Vector2(0, SelfRB.velocity.y);
			}
		}
		else
		{
			lastDir = (int)Mathf.Sign(direction.x);
			LocalMaxspeed = MaxSpeed;
		}
		if (CanFly)
		{
			Flight(direction.y*LocalAcceleration/this.LocalAcceleration);
		}
	}
	void Friction(float LocalAcceleration)
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
	public void Jump(float force)
	{
		StartCoroutine(IeJump(force));
	}
	IEnumerator IeJump( float force)
	{
		if (JumpRemains != 0 && selfEntity.StaminaRemains >= JumpCost && !IsDashing&&!IsAttack)
		{
			Shit = true;
			IsJumping = true;
			selfEntity.StaminaRemains -= JumpCost;

			if (Mathf.Abs(SelfRB.velocity.x) < 0.1f&&!CanFly)
			{
				yield return new WaitForSeconds(JumpAnimationLength);
			}
			var xSpeed = SelfRB.velocity.x;
			SelfRB.velocity = new Vector2(xSpeed, 0);
			if (Wall == 0||!WallJumpAbility)
			{
				SelfRB.AddForce(Vector2.up * force * mass);
			}
			else
			{
				SelfRB.AddForce(new Vector2(Wall * 0.5f, 1).normalized * force * mass);
			}
			yield return new WaitForFixedUpdate();
			yield return new WaitForFixedUpdate();
			if(!CanFly)JumpRemains--;
			IsJumping = false;
		}
		StopCoroutine(IeJump(force));
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
		if (Wall == 0 || selfEntity.StaminaRemains < SlideCost || SlideDown)
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
		AnotherInput = true;
		selfEntity.Push = false;
		selfEntity.ImmuneToDamage = true;
		for (int i = 0; i < DashLength; i++)
		{
			LocalMaxspeed = DashSpeed;
			Move(Vector2.right * direction,true,LocalAcceleration*10);
			yield return new WaitForFixedUpdate();
		}
		IsDashing = false;
		AnotherInput = false;
		selfEntity.Push = true;
		selfEntity.ImmuneToDamage = false;
		LocalMaxspeed = MaxSpeed;
		yield return new WaitForSeconds(DashCoolDown);
		DashCD = false;
		yield break;
	}



}
