using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ImpController : MonoBehaviour
{
	public Hive MyHive;
	public int number;
	public bool SeeEnemy;
	public bool InHive;
	Movement SelfMovement;
	[HideInInspector]public ImpMovement SelfImpAdd;
	[HideInInspector]public Entity SelfEntity;
	public Vector2 Target;
	float RaysCount;
	[SerializeField] float x;
	private float rayNumber;
	Rigidbody2D SelfRB;
	float RayNumber
	{
		set
		{
			if (value < RaysCount / x)
			{
				rayNumber = value;
			}
			else
			{
				rayNumber -= (RaysCount / x - 0.2f);
			}
		}
		get
		{
			return rayNumber;
		}
	}
	int simpleRayNumber;
	int SimpleRayNumber
	{
		get
		{
			return simpleRayNumber;
		}
		set
		{
			if (value < 18)
			{
				simpleRayNumber = value;
			}
			else
			{
				simpleRayNumber = 0;
				AnalyzeWalls();
			}
		}
	}
	[SerializeField] float RayAngle;
	[SerializeField]
	int SightRadius;
	public Entity TargetEntity;
	[HideInInspector] public Vector2 LastTargetPos;
	List<Wall> Walls = new List<Wall>();
	[SerializeField] Wall Wall = new Wall();
	[SerializeField] bool SeeWall;
	[SerializeField] int WallsCount;
	[SerializeField] int AttentionTime;
	Dictionary<string, bool> WallDirs = new Dictionary<string, bool>();
	public string Corridor;
	int GlobalJumpForce;
	public bool CanMove = true;
	[SerializeField] float WallCheckingRayLength = 10;
	[SerializeField] Vector2 control;

	// Start is called before the first frame update
	void Start()
	{
		SelfMovement = GetComponent<Movement>();
		SelfEntity = GetComponent<Entity>();
		SelfImpAdd = GetComponent<ImpMovement>();
		InitializeWalls();
		SelfRB = GetComponent<Rigidbody2D>();
		GlobalJumpForce = SelfMovement.JumpForce;
		StartCoroutine(DisconnectTimer());
	}
	void InitializeWalls()
	{
		WallDirs.Add("Up", false);
		WallDirs.Add("Right", false);
		WallDirs.Add("Down", false);
		WallDirs.Add("Left", false);
	}
	void ClearWalls()
	{
		WallDirs.Clear();
		InitializeWalls();
	}

	public bool stop;
	public bool a;
	// Update is called once per frame
	void FixedUpdate()
	{
		if (stop)
		{
			stop = false;
			SelfRB.velocity = Vector2.zero;
		}
		if (a)
		{
			a = false;
			SelfRB.velocity = Vector2.zero;
			ParabAttack();
		}
		Sight();
		SimpleSight();
		if(CanMove)MoveTowardsTarget(Target);
		if (MyHive != null && MyHive.Hurricane) SelfMovement.selfEntity.Push = false;
		else SelfMovement.selfEntity.Push = true;
	}
	void CheckForBackStab()
	{

	}
	void MoveTowardsTarget(Vector2 target)
	{
		if (SelfMovement.IsOnGround) SelfMovement.Jump(SelfMovement.JumpForce);
		if (Corridor != "")
		{
			if (Corridor == "Horizontal")
			{
				//SelfMovement.JumpForce = GlobalJumpForce / 3;
				SelfMovement.Move(Vector2.right * Mathf.Sign(SelfRB.velocity.x), false,SelfMovement.LocalAcceleration);
				control = Vector2.right * Mathf.Sign(SelfRB.velocity.x)*SelfMovement.LocalAcceleration;
			}
			if (Corridor == "Vertical")
			{
				SelfMovement.Move(Vector2.up * Mathf.Sign(SelfRB.velocity.y), false,SelfMovement.LocalAcceleration);
				control = Vector2.up * Mathf.Sign(SelfRB.velocity.y) * SelfMovement.LocalAcceleration;
			}
		}
		else if (SeeWall)
		{
			//SelfMovement.JumpForce = GlobalJumpForce;
			SelfMovement.Move((target - (Vector2)transform.position).normalized * (Wall.Distance / WallCheckingRayLength) + Wall.Direction * (1 - (Wall.Distance / WallCheckingRayLength)), false,SelfMovement.LocalAcceleration);
			control = (target - (Vector2)transform.position).normalized * (Wall.Distance / WallCheckingRayLength) + Wall.Direction * (1 - (Wall.Distance / WallCheckingRayLength)) * SelfMovement.LocalAcceleration;
			//Debug.DrawRay(transform.position, ((target - (Vector2)transform.position).normalized * (Wall.Distance / WallCheckingRayLength) + Wall.Direction).normalized*10, Color.cyan);
		}
		else if ((target - (Vector2)transform.position).magnitude > 0.5)
		{
			//SelfMovement.JumpForce = GlobalJumpForce;
			var f = (target - (Vector2)transform.position).magnitude / 5;
			if (f > 1) f = 1;
			SelfMovement.Move((target - (Vector2)transform.position).normalized, false, f*SelfMovement.LocalAcceleration);
			control = (target - (Vector2)transform.position).normalized * f * SelfMovement.LocalAcceleration;
		}
		else
		{
			//SelfMovement.JumpForce = GlobalJumpForce;

			if (TargetEntity != null && (!InHive||!MyHive.Hurricane))
			{
				ParabAttack();
			}
		}
		var b = (LastTargetPos - (Vector2)transform.position);
		if (b.magnitude>10&&b.magnitude < 20 && Mathf.Abs(b.x) / Mathf.Abs(b.y) > 0.5f && Mathf.Abs(b.x) / Mathf.Abs(b.y) < 2 && b.y < 0 && (Wall.Distance > 1 || !SeeWall))
		{
			if (TargetEntity != null && (!InHive || !MyHive.Hurricane))
			{
				ParabAttack();
			}
		}

	}
	public void ParabAttack()
	{
		SelfImpAdd.ParabolicFlight(LastTargetPos);
	}
	private void Update()
	{
		SightAngle();
	}
	void Sight()
	{
		RaysCount = 360 / RayAngle;
		//Debug.DrawLine(transform.position, Target,Color.blue);
		var vec = -LastTargetPos + (Vector2)transform.position;
		vec = vec.normalized;
		vec = new Vector2(-vec.x, vec.y);
		Vector2 ray = new Vector2(Mathf.Cos((RayNumber * RayAngle + Vector2.SignedAngle(vec, Vector2.right) - RaysCount / x * RayAngle / 2) * Mathf.PI / 180), Mathf.Sin((RayNumber * RayAngle + Vector2.SignedAngle(vec, Vector2.right) - RaysCount / x * RayAngle / 2) * Mathf.PI / 180));
		ray *= SightRadius;


		RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, ray, SightRadius);
		if (hits.Length == 4)
		{
			//Debug.DrawRay(transform.position, ray, Color.red, 0.1f);
		}
		foreach (var hit in hits)
		{
			Entity hitEntity;
			if (hit.collider.gameObject.TryGetComponent(out hitEntity))
			{
				if (hitEntity.Type == Entity.entityType.Human)
				{
					EnemyDetection(hitEntity);
				}
				else if(hitEntity.Type == Entity.entityType.Demon)
				{
					ImpController imp;
					hitEntity.TryGetComponent(out imp);
					if (imp != null&&imp!=this)
					{
						if (imp.InHive && !InHive)
						{
							ConnectToHive(imp.MyHive);
						}
						else if(!imp.InHive && !InHive)
						{
							CreateHive(imp);
						}
						else if (imp.InHive && InHive)
						{
							if (MyHive != imp.MyHive&&MyHive.Imps.Count>=imp.MyHive.Imps.Count)
							{
								UniteHives(MyHive, imp.MyHive);
							}
						}
					}
				}
			}
			if (hit.collider.tag == "Ground")
			{
				break;
			}
		}

		RayNumber++;
	}
	void SimpleSight()
	{
		Vector2 ray = new Vector2(Mathf.Cos((SimpleRayNumber * 20) * Mathf.PI / 180), Mathf.Sin((SimpleRayNumber * 20) * Mathf.PI / 180));
		ray *= WallCheckingRayLength;
		//Debug.DrawRay(transform.position, ray, Color.white);
		RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, ray, WallCheckingRayLength);
		foreach (var hit in hits)
		{
			if (hit.collider.tag == "Ground")
			{
				Wall a = new Wall((-hit.point + (Vector2)transform.position).normalized, hit.distance, hit.normal);
				Walls.Add(a);

				WallsCount++;
				SeeWall = true;
				break;
			}
		}
		SimpleRayNumber++;
	}
	void AnalyzeWalls()
	{
		if (WallsCount == 0)
		{
			Corridor = "";
			SeeWall = false;
			Walls.Clear();
		}
		else if (WallsCount == 1)
		{
			Wall = Walls[0];
			Corridor = "";
		}
		else
		{
			Wall = SetActiveWall();
			foreach (var wall in Walls)
			{
				if (Vector2.Angle(-Vector2.up, wall.Normal) <= 10)
				{
					WallDirs["Up"] = true;
				}
				if (Vector2.Angle(-Vector2.left, wall.Normal) <= 10)
				{
					WallDirs["Left"] = true;
				}
				if (Vector2.Angle(-Vector2.right, wall.Normal) <= 10)
				{
					WallDirs["Right"] = true;
				}
				if (Vector2.Angle(-Vector2.down, wall.Normal) <= 10)
				{
					WallDirs["Down"] = true;
				}
			}
			if (WallDirs["Left"] == true && WallDirs["Right"] == true && WallDirs["Up"] == false && WallDirs["Down"] == false)
			{
				Corridor = "Vertical";
			}
			else if (WallDirs["Up"] == true && WallDirs["Down"] == true && WallDirs["Left"] == false && WallDirs["Right"] == false)
			{
				Corridor = "Horizontal";
			}
			else
			{
				Corridor = "";
			}
		}
		Walls.Clear();
		ClearWalls();
		WallsCount = 0;
	}
	void UniteHives(Hive biggerHive, Hive smallerHive)
	{
		var dist = biggerHive.transform.position - smallerHive.transform.position;
		if (dist.magnitude > 0.6f * SightRadius)
		{
			return;
		}
		var a = new List<ImpController>();
		a.AddRange(smallerHive.Imps);
		foreach(var imp in a)
		{
			imp.DisconnectFromHive();
			imp.ConnectToHive(biggerHive);
		}
	}
	void CreateHive(ImpController imp)
	{
		var GHive = new GameObject();
		Hive Hive = GHive.AddComponent<Hive>();
		Hive.Initializing = true;
		GHive.name = "Hive of " + gameObject.name + " and " + imp.name;
		GHive.transform.position = (imp.transform.position + transform.position) / 2;
		ConnectToHive(Hive);
		imp.ConnectToHive(Hive);
		Hive.Initializing = false;
	}
	void ConnectToHive(Hive hive)
	{
		if (InHive) return;
		var dist = (Vector2)hive.transform.position - (Vector2)transform.position;
		var hit = Physics2D.Raycast(transform.position, dist, 20, LayerMask.GetMask("Ground"));
		if (hit && hit.distance < dist.magnitude)
		{
			return;
		}
		InHive = true;
		MyHive = hive;
		hive.Imps.Add(this);
		hive.SomeoneConnected.Invoke(this);
	}
	public IEnumerator DisconnectTimer()
	{
		while (true)
		{
			var a = Random.Range(1.8f, 2.2f);
			yield return new WaitForSeconds(a);
			CheckForDisconnectionFromHive();
		}
	}
	public void CheckForDisconnectionFromHive()
	{
		if (InHive)
		{
			var dist = (Vector2)MyHive.transform.position - (Vector2)transform.position;
			if (dist.magnitude > 20)
			{
				DisconnectFromHive();
			}
			else
			{
				var hit = Physics2D.Raycast(transform.position, dist, 20, LayerMask.GetMask("Ground"));
				if (hit && hit.distance < dist.magnitude)
				{
					DisconnectFromHive();
				}
			}

		}
	}
	public void DisconnectFromHive()
	{
		MyHive.SomeoneDisconnected.Invoke(this);
		InHive = false;
		MyHive.Imps.Remove(this);
		MyHive = null;
	}
	Wall SetActiveWall()
	{
		var minDist = 10f;
		Wall ActiveWall = new Wall();
		foreach (var wall in Walls)
		{
			{
				if (wall.Distance < minDist)
				{
					minDist = wall.Distance;
					ActiveWall.Normal = wall.Normal;
				}
				ActiveWall.Direction += wall.Direction;
			}
		}
		ActiveWall.Direction = ActiveWall.Direction.normalized;
		ActiveWall.Distance = minDist;

		return ActiveWall;
	}
	void CheckForCorridor()
	{

	}
	void InspectWall(Vector2 point)
	{
		//var toWall = point - transform.position;
	}
	void SightAngle()
	{
		if (TargetEntity == null)
		{
			x = 1;
		}
		else
		{
			var dist = (LastTargetPos - (Vector2)transform.position).magnitude;
			x = dist;
		}
	}
	void EnemyDetection(Entity ent)
	{
		SeeEnemy = true;
		TargetEntity = ent;
		LastTargetPos = TargetEntity.transform.position;
		if (!InHive) Target = LastTargetPos + Vector2.up * 10 + Mathf.Sign(transform.position.x - LastTargetPos.x) * Vector2.right * 15;
		StopCoroutine(Attention(AttentionTime));
		StartCoroutine(Attention(AttentionTime));
	}
	IEnumerator Attention(int cd)
	{
		for (int i = 0; i < cd; i++)
		{
			if (i == 2) SeeEnemy = false;
			yield return new WaitForFixedUpdate();
		}
		TargetEntity = null;
		if (!InHive) Target = LastTargetPos;
		StopCoroutine(Attention(cd));
		yield break;
	}
	float timer = 0;
	public Vector2 RotatingPoint(Vector2 rotatingPoint, float radius,float speed, float freq)
	{
		Vector2 a = new Vector2();
		if (speed != 0) timer = Time.time;
		else timer = 0;
		var z = timer * speed + freq * radius;
		a = Vector2.right * Mathf.Sin(z)*radius + rotatingPoint;
		if(SelfRB.velocity.x < 0)
		{
			if (((Vector2)transform.position - a).magnitude < 2 + radius)
			{
				foreach (var s in GetComponentsInChildren<SpriteRenderer>())
				{
					s.color = Color.gray;
				}
				transform.position = new Vector3(transform.position.x, transform.position.y, 2);
			}
		}
		else
		{
			foreach (var s in GetComponentsInChildren<SpriteRenderer>())
			{
				s.color = Color.white;
			}
			transform.position = new Vector3(transform.position.x, transform.position.y, 1);

		}
		Misc.DrawCross(a, Time.fixedDeltaTime, Color.magenta, 2);
		return a;
	}

	public void Die()
	{
		if(InHive)DisconnectFromHive();
	}
}

public class Wall
{
	public Vector2 Direction;
	public float Distance;
	public Vector2 Normal;
	/// <summary>
	/// Wall class
	/// </summary>
	/// <param name="normal">Normal of the wall</param>
	/// <param name="distance">Distance to the wall</param>
	public Wall(Vector2 direction, float distance, Vector2 normal)
	{
		Normal = normal;
		Distance = distance;
		Direction = direction;
	}
	public Wall()
	{
		Normal = Vector2.zero;
		Distance = 0;
		Direction = Vector2.zero;
	}

}
