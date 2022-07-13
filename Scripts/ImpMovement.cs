using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpMovement : MonoBehaviour
{
	Movement SelfMovement;
	Rigidbody2D srb;
	public float Speed;
	bool CanParabol=true;
	[SerializeField] float CoolDown;
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
	private void OnDestroy()
	{
		print("pashol nahui  "+gameObject.name+srb.velocity+"   "+srb.angularVelocity);
	}
	public void SpearAttack()
	{
		srb.AddForce(Vector2.down * 500);
		SelfMovement.Attack(SelfMovement.Second, "2");
	}
	IEnumerator IeParabolicFlight(Vector2 TargetPos)
	{

		
		Debug.DrawLine(TargetPos, transform.position, Color.blue);
		SelfMovement.AnotherInput = true;
		Vector2 LocalPos = (Vector2)transform.position - TargetPos;
		float a = LocalPos.y / (LocalPos.x * LocalPos.x);
		int i = 0;
		GetComponent<Rigidbody2D>().AddForce(-GetComponent<Rigidbody2D>().velocity / Time.fixedDeltaTime);
		srb.AddForce(new Vector2(1, 2 * a * LocalPos.x).normalized * Speed * -Mathf.Sign(LocalPos.x) / Time.fixedDeltaTime);
		//print(gameObject.name + (double)LocalPos.normalized.x);
		if (Mathf.Abs(LocalPos.normalized.x) < 0.1f)
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
			GetComponent<SpriteRenderer>().color = Color.red;
			SelfMovement.SelfAnim.SetTrigger("Attack 1");
			i++;
			Vector2 localPos = (Vector2)transform.position - TargetPos;
			if (Mathf.Sign(localPos.x) != Mathf.Sign(LocalPos.x) && localPos.y >= 10)
			{
				break;
			}
			float v = 1.1f-Mathf.Abs(localPos.x / LocalPos.x);
			srb.AddForce(-Physics2D.gravity * srb.gravityScale);
			//print(gameObject.name + "   " + -new Vector2(1, 2 * a * (localPos.x - srb.velocity.x * Time.fixedDeltaTime)).normalized * Speed * -Mathf.Sign(LocalPos.x) / Time.fixedDeltaTime);

			srb.AddForce(-new Vector2(1, 2 * a * (localPos.x - srb.velocity.x * Time.fixedDeltaTime)).normalized * Speed * - Mathf.Sign(LocalPos.x) / Time.fixedDeltaTime);





			//print(gameObject.name + "   " + new Vector2(1, 2 * a * localPos.x).normalized * Speed * -Mathf.Sign(LocalPos.x) / Time.fixedDeltaTime);
			srb.AddForce(new Vector2(1, 2 * a * localPos.x).normalized *Speed*-Mathf.Sign(LocalPos.x) / Time.fixedDeltaTime);




			if (i>3&&Mathf.Abs(srb.velocity.normalized.x - new Vector2(1, 2 * a * localPos.x).normalized.x * -Mathf.Sign(LocalPos.x)) > 0.1)
			{
				trail.enabled = false;
				GetComponent<SpriteRenderer>().color = Color.white;
				SelfMovement.AnotherInput = false;
				yield break;
			}
			if (Mathf.Abs(localPos.x) <= Mathf.Abs(srb.velocity.x) * Time.fixedDeltaTime * 2)
			{
				SelfMovement.Attack(SelfMovement.First, "1");
			}
			yield return new WaitForFixedUpdate();
		}
		trail.enabled = false;
		GetComponent<SpriteRenderer>().color = Color.white;
		SelfMovement.AnotherInput = false;
		StartCoroutine(IeCoolDown(CoolDown));
		StopCoroutine(IeParabolicFlight(TargetPos));
		yield break;
	}
	IEnumerator IeCoolDown(float time)
	{
		CanParabol = false;
		yield return new WaitForSeconds(time);
		CanParabol = true;
		StopCoroutine(IeCoolDown(CoolDown));
		yield break;
	}

}