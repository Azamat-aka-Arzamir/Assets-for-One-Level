using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ImpMovement : MonoBehaviour
{
	Movement SelfMovement;
	Rigidbody2D srb;
	public float Speed;
	[SerializeField]bool CanParabol=true;
	[SerializeField] float CoolDown;

	public UnityEvent ClawsAttackEvent = new UnityEvent();
	public UnityEvent SpearAttackEvent = new UnityEvent();

	// Start is called before the first frame update
	void Start()
	{
		SelfMovement = GetComponent<Movement>();
		srb = GetComponent<Rigidbody2D>();
	}

	// Update is called once per frame
	void Update()
	{

	}
	public void ParabolicFlight(Vector2 TargetPos)
	{
		if(!SelfMovement.AnotherInput&&CanParabol)StartCoroutine(IeParabolicFlight(TargetPos));
	}

	public void SpearAttack()
	{
		
		SpearAttackEvent.Invoke();
		StartCoroutine(IeSpearAttack());

	}
	bool OnGND()
	{
		return SelfMovement.IsOnGround;
	}
	public IEnumerator IeSpearAttack()
	{
		GetComponent<ImpController>().CanMove = false;

		srb.velocity = Vector2.zero;
		GetComponent<Entity>().Push = false;
		//srb.AddForce(Vector2.down * 500);
		yield return new WaitUntil(OnGND);
		GetComponent<ImpController>().CanMove = true;
		GetComponent<Entity>().Push = true;
	}
	public UnityEngine.Events.UnityEvent AttackEvent = new UnityEngine.Events.UnityEvent();
	IEnumerator IeParabolicFlight(Vector2 TargetPos)
	{
		CanParabol = false;
		
		Debug.DrawLine(TargetPos, transform.position, Color.blue);
		SelfMovement.AnotherInput = true;
		Vector2 LocalPos = (Vector2)transform.position - TargetPos;
		float a = LocalPos.y / (LocalPos.x * LocalPos.x);
		int i = 0;
		Debug.DrawRay(transform.position, new Vector2(1, 2 * a * LocalPos.x).normalized*5,Color.white,5);
		if (!CheckFreeSpaceWithRay(new Vector2(1, 2 * a * LocalPos.x).normalized))
		{
			SelfMovement.AnotherInput = false;
			CanParabol = true;
			yield break;
		}

		GetComponent<Rigidbody2D>().AddForce(-GetComponent<Rigidbody2D>().velocity / Time.fixedDeltaTime);
		srb.AddForce(new Vector2(1, 2 * a * LocalPos.x).normalized * Speed * -Mathf.Sign(LocalPos.x) / Time.fixedDeltaTime);
		//print(gameObject.name + (double)LocalPos.normalized.x);
		if (Mathf.Abs(LocalPos.normalized.x) < 0.3f)
		{
			SpearAttack();
			SelfMovement.AnotherInput = false;
			StartCoroutine(IeCoolDown(CoolDown));
			StopCoroutine(IeParabolicFlight(TargetPos));
			yield break;
		}
		var trail = GetComponent<TrailRenderer>();
		trail.enabled = true;

		while (i < 1000)
		{
			AttackEvent.Invoke();
			//GetComponent<SpriteRenderer>().color = Color.red;
			i++;
			Vector2 localPos = (Vector2)transform.position - TargetPos;
			if (Mathf.Sign(localPos.x) != Mathf.Sign(LocalPos.x) && localPos.y >= 10)
			{
				transform.localRotation = Quaternion.Euler(0, 0, 0);
				break;
			}
			var angle = Mathf.Atan(2 * a * localPos.x)*180/Mathf.PI;
			transform.localRotation = Quaternion.Euler(0, 0, angle);
			float v = 1.1f-Mathf.Abs(localPos.x / LocalPos.x);
			srb.AddForce(-Physics2D.gravity * srb.gravityScale);
			//print(gameObject.name + "   " + -new Vector2(1, 2 * a * (localPos.x - srb.velocity.x * Time.fixedDeltaTime)).normalized * Speed * -Mathf.Sign(LocalPos.x) / Time.fixedDeltaTime);

			srb.AddForce(-new Vector2(1, 2 * a * (localPos.x - srb.velocity.x * Time.fixedDeltaTime)).normalized * Speed * - Mathf.Sign(LocalPos.x) / Time.fixedDeltaTime);





			//print(gameObject.name + "   " + new Vector2(1, 2 * a * localPos.x).normalized * Speed * -Mathf.Sign(LocalPos.x) / Time.fixedDeltaTime);
			srb.AddForce(new Vector2(1, 2 * a * localPos.x).normalized *Speed*-Mathf.Sign(LocalPos.x) / Time.fixedDeltaTime);




			if (i>3&&Mathf.Abs(srb.velocity.normalized.x - new Vector2(1, 2 * a * localPos.x).normalized.x * -Mathf.Sign(LocalPos.x)) > 0.1)
			{
				trail.enabled = false;
				StartCoroutine(IeCoolDown(CoolDown));
				SelfMovement.AnotherInput = false;
				transform.localRotation = Quaternion.Euler(0, 0, 0);
				yield break;
			}
			if (Mathf.Abs(localPos.x) <= Mathf.Abs(srb.velocity.x) * Time.fixedDeltaTime * 2)
			{
				ClawsAttackEvent.Invoke();
			}
			yield return new WaitForFixedUpdate();
		}
		trail.enabled = false;
		SelfMovement.AnotherInput = false;
		transform.localRotation = Quaternion.Euler(0, 0, 0);
		StartCoroutine(IeCoolDown(CoolDown));
	}
	bool CheckFreeSpaceWithRay(Vector2 direction)
	{
		var ray = Physics2D.RaycastAll(transform.position,direction,2,LayerMask.GetMask("Entity"));
		foreach(var hit in ray)
		{
			Entity ent;
			hit.collider.TryGetComponent(out ent);
			if (ent != null&&ent!=GetComponent<Entity>())
			{
				if (ent.Type == Entity.entityType.Demon)
				{
					return false;
				}
			}
		}
		return true;
	}
	IEnumerator IeCoolDown(float time)
	{
		yield return new WaitForSeconds(time);
		CanParabol = true;
	}

}