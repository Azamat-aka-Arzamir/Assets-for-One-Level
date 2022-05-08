using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
	Rigidbody2D SelfRB;
	Collider2D SelfColl;
	float mass;
	[SerializeField] int JumpRemains;

	[SerializeField] int Wall;
	[SerializeField] bool OnGround;
	[SerializeField] int JumpsCount;
	[SerializeField] bool WallJumpAbility;
	[SerializeField] bool DashAbility;
	[SerializeField] bool OnWall;
	[SerializeField] int DashCoolDown;
	bool DashCD;
	bool IsDashing;
	bool SlideDown;
	[SerializeField] int DashSpeed;
	[SerializeField] int DashLength;

	[SerializeField] int MaxSpeed;
	int LocalMaxspeed;
	[SerializeField] float Acceleration;
	[SerializeField] float AirAcceleration;
	float LocalAcceleration;
	[SerializeField] int JumpForce;
	[SerializeField] float SlideSpeed;

	[SerializeField] int DashCost;
	[SerializeField] int JumpCost;
	[SerializeField] int SlideCost;
	[SerializeField] int AttackCost;
	float JumpAnimationLength;
	public int Bullets;
	int lastDir;
	Entity selfEntity;

	// Start is called before the first frame update
	void Start()
	{
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
	}
	public void Move(Vector2 direction)
	{
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
		Friction();
		if (IsDashing)
		{
			return;
		}
		Vector2 force = direction * LocalAcceleration * mass;
		if (direction.x == 0)
		{
			LocalMaxspeed = 0;
			if (Mathf.Abs(SelfRB.velocity.x) < 1f)
			{
				SelfRB.velocity = new Vector2(0, SelfRB.velocity.y);
			}
		}
		else
		{
			lastDir =(int)Mathf.Sign(direction.x); 
			LocalMaxspeed = MaxSpeed;
		}
		SelfRB.AddForce(force);
	}
	void Friction()
	{
		var k = Mathf.Abs(SelfRB.velocity.x) / (LocalMaxspeed + 1f);
		if (k > 100) k = 100;
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
	public void Jump()
	{
		StartCoroutine(IeJump());
	}
	IEnumerator IeJump()
	{
		if (JumpRemains != 0 && selfEntity.StaminaRemains >= JumpCost && !IsDashing)
		{
			selfEntity.StaminaRemains -= JumpCost;
			yield return new WaitForSeconds(JumpAnimationLength);
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
		var animator = GetComponent<Animator>();
		List<AnimationClip> clips = new List<AnimationClip>();
		clips.AddRange(animator.runtimeAnimatorController.animationClips);
		if (animator.HasState(0, Animator.StringToHash("Jump"))) length = clips.Find(x => x.name == "Jump").length - 0.1f;
		return length;
	}

	public void Fire(Vector2 direction, Weapon weapon)
	{

	}
	void Slide()
	{
		if (Wall == 0 || !WallJumpAbility || selfEntity.StaminaRemains < SlideCost || SlideDown)
		{
			return;
		}
		selfEntity.StaminaRemains -= SlideCost;
		float frictionForce = -SelfRB.velocity.y / SlideSpeed * mass;
		if (frictionForce > Physics2D.gravity.y * mass) frictionForce = Physics2D.gravity.y * mass;
		if (SelfRB.velocity.y < -SlideSpeed) SelfRB.AddForce(Vector2.up * Physics2D.gravity.y * frictionForce);
	}
	public void Dash()
	{
		if (DashAbility && !DashCD && selfEntity.StaminaRemains > DashCost&&OnGround)
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
