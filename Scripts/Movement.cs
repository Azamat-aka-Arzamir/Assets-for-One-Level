using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
	[HideInInspector] public float LocalAcceleration;
	[Space]


	[Header("Weapon")]
	public Weapon LastWeapon;
	[Header("Debug")]
	[SerializeField] int Wall;
	[SerializeField] public bool OnGround;
	[HideInInspector]
	public bool IsOnGround
	{
		get
		{
			return OnGround;
		}
	}
	[SerializeField] bool OnWall;
	[SerializeField] bool Sliding;
	[SerializeField] public bool IsJumping;
	[SerializeField] bool IsDashing;
	[SerializeField] bool CheckTurnConstantly;


	[HideInInspector] public Animator SelfAnim;
	SpriteRenderer SelfRenderer;
	float JumpAnimationLength;
	[HideInInspector] public Vector2Int CurrentDir = Vector2Int.right;
	[HideInInspector] public int lastDir = 1;
	[HideInInspector] public int lastDirY = 0;
	[HideInInspector] public Entity selfEntity;
	public bool IsAttack;
	float gravityScale;
	public float xVel;
    public float yVel;

    //REMEMBER
    [field: SerializeField] float cock { get; set; }
	 public bool AnotherInput;
	// Start is called before the first frame update
	void Start()
	{
		SelfRenderer = GetComponent<SpriteRenderer>();
		selfEntity = GetComponent<Entity>();
		SelfRB = GetComponent<Rigidbody2D>();
		SelfColl = GetComponent<Collider2D>();
		mass = SelfRB.mass;
		gravityScale = SelfRB.gravityScale;
	}


	float OnGroundTimer;
	// Update is called once per frame
	void FixedUpdate()
	{
		if (WallJumpAbility) Slide();
		var lastGroundCheck = OnGround;
		OnGround = CheckGround();
		if (OnGround)
		{
			OnGroundTimer += Time.deltaTime;
		}
		else
		{
			OnGroundTimer = 0;
		}
		if (!lastGroundCheck && OnGround)
		{
			LandEvent.Invoke();
			print("Landed");
		}
		if (!OnGround && !lastGroundCheck)
		{
			if (SelfRB.velocity.y > 0)
			{
				InAirUp.Invoke();
			}
			else
			{
				InAirDown.Invoke();
			}
		}
		SetLocalAcceleration();
		if (CheckTurnConstantly) CheckTurn();
		xVel = SelfRB.velocity.x;
        yVel = SelfRB.velocity.y;
    }
	void CheckTurn()
	{
		if (SelfRB.velocity.x == 0) return;
		int sign = (int)Mathf.Sign(SelfRB.velocity.x);
		if (sign != lastDir)
		{
			lastDir = sign;
			if(lastDir==1)TurnEventR.Invoke();
			if (lastDir == -1) TurnEventL.Invoke();
		}
	}

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

		float force = JumpForce * 0.3f + JumpForce * 0.7f * factor;
		var myv = y * 2 * MaxSpeed - MinVelocity(force);

		if (SelfRB.velocity.y < myv * 0.9)
		{
			
		}
		if (SelfRB.velocity.y < myv)
		{
			if (!IsJumping) Jump(force);
		}
	}
	float MinVelocity(float force)
	{
		float deltaT = Time.fixedDeltaTime;
		var g = Physics2D.gravity.magnitude;
		var Sc = gravityScale;
		float A = force - g * mass * Sc;
		float vel = A * deltaT;
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
	void OnTurned()
	{
		//SelfAnim.SetInteger("Dir", lastDir);
	}
	public UnityEvent LookUpEvent = new UnityEvent();
	public UnityEvent LookDownEvent = new UnityEvent();
	public UnityEvent MoveEvent = new UnityEvent();
	public UnityEvent StopEvent = new UnityEvent();
	public UnityEvent TurnEventL = new UnityEvent();
	public UnityEvent TurnEventR = new UnityEvent();
	public void Move(Vector2 direction, bool SideInput, float LocalAcceleration)
	{
		if (direction != Vector2.zero) direction = direction.normalized;
		else direction = Vector2.zero;
		if (Sliding) direction = new Vector2(-Wall, direction.y);
		if (direction.y < -0.5)
		{
			SlideDown = true;
		}
		else
		{
			SlideDown = false;
		}

		if (AnotherInput && !SideInput)
		{
			return;
		}

		Vector2 force = direction.x * Vector2.right * LocalAcceleration * mass;
		SelfRB.AddForce(force);
		Friction(LocalAcceleration);

		if (direction.y != 0)
		{
			lastDirY = (int)Mathf.Sign(direction.y);
			if (lastDirY == -1&&direction.y<=-0.7&&LastWeapon.weaponType==Weapon.type.gun)
			{
				LookDownEvent.Invoke();
			}
			if (lastDirY == 1 && direction.y >= 0.7 && LastWeapon.weaponType == Weapon.type.gun)
			{
				LookUpEvent.Invoke();
			}
		}
		else lastDirY = 0;

		if (direction.x == 0)
		{
			if (OnGround) LocalMaxspeed = 0;
			if (Mathf.Abs(SelfRB.velocity.x) < 1f && !selfEntity.GettingDamage)
			{
				SelfRB.velocity = new Vector2(0, SelfRB.velocity.y);
			}

		}
		else
		{

			if (lastDir != (int)Mathf.Sign(direction.x)&&!CheckTurnConstantly)
			{
				lastDir = (int)Mathf.Sign(direction.x);
				TurnEventR.Invoke();
				//TurnEventL.Invoke();
			}
			LocalMaxspeed = MaxSpeed;
		}
		if (Mathf.Abs(direction.y) > 0.1) CurrentDir = new Vector2Int(lastDir, (int)Mathf.Sign(direction.y));
		else CurrentDir = new Vector2Int(lastDir, 0);
		if (CanFly)
		{
			Flight(direction.y * LocalAcceleration / this.LocalAcceleration);
		}
		if (Mathf.Abs(SelfRB.velocity.x) < 3)
		{
			if (!IsDashing && !IsJumping && SelfRB.velocity.y == 0 && OnGroundTimer >= 2f / 8f)
			{
				StopEvent.Invoke();
			}
		}
		else
		{
			if (!IsDashing && !IsJumping&&OnGround) MoveEvent.Invoke();
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
			if (Mathf.Abs(SelfRB.velocity.y) < 5f)
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
		var hit = Physics2D.Raycast(transform.position + Vector3.down * (SelfColl.bounds.extents.y + 0.1f), Vector2.down, 1, LayerMask.GetMask("Ground"));
		if (hit.transform == null) return false;
		if (hit.transform.tag == "Ground")
		{
			return true;
		}
		else return false;
	}
	public UnityEvent JumpEvent = new UnityEvent();
	public UnityEvent LandEvent = new UnityEvent();
	public UnityEvent InAirUp = new UnityEvent();
	public UnityEvent InAirDown = new UnityEvent();
	public void Jump(float force)
	{
		StartCoroutine(IeJump(force));
	}
	IEnumerator IeJump(float force)
	{
		if (JumpRemains != 0 && selfEntity.StaminaRemains >= JumpCost && !IsDashing && !IsAttack)
		{
			IsJumping = true;
			JumpEvent.Invoke();
			selfEntity.StaminaRemains -= JumpCost;

			if (Mathf.Abs(SelfRB.velocity.x) < 0.1f && !CanFly)
			{
				yield return new WaitForSeconds(2f / 8f);
			}
			var xSpeed = SelfRB.velocity.x;
			SelfRB.velocity = new Vector2(xSpeed, 0);
			if (Wall == 0 || !WallJumpAbility)
			{
				SelfRB.AddForce(Vector2.up * force * mass);
			}
			else
			{
				SelfRB.AddForce(new Vector2(Wall * 0.5f, 1).normalized * force * mass);
			}
			yield return new WaitForFixedUpdate();
			yield return new WaitForFixedUpdate();
			if (!CanFly) JumpRemains--;
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
	bool unsuccesfulSlide;
	int lastWall;
	void Slide()
	{
		if (lastWall == 0 && Wall != 0) unsuccesfulSlide = false;
		lastWall = Wall;
		if (unsuccesfulSlide)
		{
			return;
		}

		if (Wall == 0 || selfEntity.StaminaRemains < SlideCost || SlideDown)
		{
			Sliding = false;
			unsuccesfulSlide = true;
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
	public UnityEvent dashEvent = new UnityEvent();
	IEnumerator IeDash(float direction)
	{
		dashEvent.Invoke();
		IsDashing = true;
		AnotherInput = true;
		selfEntity.Push = false;
		selfEntity.ImmuneToDamage = true;
		for (int i = 0; i < DashLength; i++)
		{
			LocalMaxspeed = DashSpeed;
			Move(Vector2.right * direction, true, LocalAcceleration * 10);
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
public class MovementContext
{
	private bool m_shieldUp=false;
	private Weapon.type m_last_weapon;
	private int m_dir;
	private int m_dir_y;
	private bool m_on_ground;
	public bool ShieldUp
	{
		get { return m_shieldUp; }
	}
	public bool OnGround
	{
		get { return m_on_ground; }
	}
	public Weapon.type LastWeapon
	{
		get { return m_last_weapon; }
	}
	public int Dir
	{
		get { return m_dir; }
	}
	public int DirY
	{
		get { return m_dir_y; }
	}
	public MovementContext(Movement m)
	{
		m_last_weapon = m.LastWeapon.weaponType;
		if (m_last_weapon == Weapon.type.shield && m.LastWeapon.Activate)
		{
			m_shieldUp = true;
		}
		m_dir = m.lastDir;
		m_dir_y = m.lastDirY;
	}
}
