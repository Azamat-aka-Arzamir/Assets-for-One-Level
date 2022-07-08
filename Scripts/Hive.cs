using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Misc;
using UnityEditor;

public class Hive : MonoBehaviour
{
	[SerializeField] float MovementSpeed = 18;
	Vector2 GTarget;
	Vector2 Target;
	public List<ImpController> Imps = new List<ImpController>();
	float HivingHeight = 3;
	float DefaultHivingHeight = 3;
	[SerializeField] float RoomSize;
	List<Vector2> Walls = new List<Vector2>();
	List<Vector2> Space = new List<Vector2>();
	float WallCheckingRayLength = 30;
	[SerializeField] float MinRequiredSpace = 5;
	[SerializeField] float RequiredSpace = 0;
	bool OpenSpace;
	int simpleRayNumber;
	Vector2 LastEscapingDirection;
	public bool Initializing;
	public bool Hurricane;
	public bool SeeEnemy;
	bool stop;
	[SerializeField] bool DestinationAchivied;
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
			}
		}
	}


	// Start is called before the first frame update
	void Start()
	{
		InitializeHive();
		StartCoroutine(WaitToStartHurricane());
		StartCoroutine(HurricaneAttack());
	}

	// Update is called once per frame
	void Update()
	{
		CheckWall();
		RoomEstimation();
		RoomSize = RoomSizeCalculation();
		if (RoomSize > RequiredSpace)
		{
			OpenSpace = true;
		}
		else OpenSpace = false;
		SetCurrentTarget();
		if (!Hurricane)
		{
			if (!DestinationAchivied)
			{
				SetTargetForImps(Imps,transform.position);
			}
			else
			{
				SetTwoPointsForImps(GTarget);
			}
		}
		else ToHurricane();
		if (OpenSpace)
		{
			if(!stop)MoveTowards(Target);
		}
		else
		{
			TryToEscape();
		}
		if (Imps.Count == 1 && !Initializing)
		{
			Imps[0].MyHive = null;
			Imps[0].InHive = false;
			Destroy(gameObject);
		}
		if (Imps.Count == 0 && !Initializing)
		{
			Destroy(gameObject);
		}
		RequiredSpace = MinRequiredSpace + Imps.Count;


		//ConnectImps();
		//VizualizeRoom();
	}

	void InitializeHive()
	{
		foreach (var imp in Imps)
		{
			imp.MyHive = this;
			imp.InHive = true;
		}
	}

	float addHivingHeight = 1;
	void SetCurrentTarget()
	{
		List<ImpController> activeImps = new List<ImpController>();
		foreach (var imp in Imps)
		{
			if (imp.SeeEnemy)
			{
				activeImps.Add(imp);
			}
		}
		if (activeImps.Count > 0)
		{
			var randomImp = activeImps[Random.Range(0, activeImps.Count)];
			Target = randomImp.LastTargetPos + Vector2.up * Imps.Count / 4;
			GTarget = randomImp.LastTargetPos;
			SeeEnemy = true;
		}
		else SeeEnemy = false;
	}
	void SetTargetForImps(List<ImpController> Imps, Vector2 target)
	{
		foreach (var imp in Imps)
		{
			imp.Target = target;
		}
	}
	void SetTwoPointsForImps(Vector2 target)
	{
		List<ImpController> firstGroup = new List<ImpController>();
		firstGroup.AddRange(Imps.GetRange(0, Imps.Count / 2));
		List<ImpController> secondGroup = new List<ImpController>();
		secondGroup.AddRange(Imps.GetRange(Imps.Count / 2, Imps.Count / 2));
		SetTargetForImps(firstGroup, target + new Vector2(-10, 10));
		SetTargetForImps(secondGroup, target + new Vector2(10, 10));


	}
	void MoveTowards(Vector2 target)
	{
		DrawCross(transform.position, 0.02f, Color.yellow, 5);
		DrawCross(target, 0.02f, Color.white, 5);
		if (((Vector2)transform.position - target).magnitude > 1)
		{
			var a = (target - (Vector2)transform.position).normalized;
			LastEscapingDirection = a;
			transform.Translate(a * MovementSpeed * Time.deltaTime);
			DestinationAchivied = false;
		}
		else DestinationAchivied = true;
	}
	void CheckWall()
	{
		var ray = Physics2D.Raycast(transform.position, Vector3.down, transform.position.y - GTarget.y, LayerMask.GetMask("Ground"));
		if (ray.collider != null && ray.collider.tag == "Ground")
		{
			HivingHeight += 0.1f * Mathf.Sign(GTarget.y - transform.position.y);
		}
		else if (HivingHeight < DefaultHivingHeight)
		{
			HivingHeight += 0.1f;
		}
	}
	void RoomEstimation()
	{
		Vector2 ray = new Vector2(Mathf.Cos((SimpleRayNumber * 20) * Mathf.PI / 180), Mathf.Sin((SimpleRayNumber * 20) * Mathf.PI / 180));
		ray *= WallCheckingRayLength;
		RaycastHit2D hit = Physics2D.Raycast(transform.position, ray, WallCheckingRayLength, LayerMask.GetMask("Ground"));
		if (hit && hit.collider.tag == "Ground")
		{
			DrawCross(hit.point, 1, Color.green, 1);
			foreach (var _ray in Walls)
			{
				if (_ray.normalized == ray.normalized)
				{
					Walls.Remove(_ray);
					break;
				}
			}
			if (hit.distance > 0)
			{
				Walls.Add(ray.normalized * hit.distance);
			}
			else
			{
				Walls.Add(ray.normalized * 0.1f);

			}
			if (Space.Contains(ray)) Space.Remove(ray);
		}
		else
		{
			if (!Space.Contains(ray)) Space.Add(ray);
			foreach (var a in Walls)
			{
				if (a.normalized == ray.normalized)
				{
					Walls.Remove(a);
					break;
				}
			}

		}
		SimpleRayNumber++;
	}
	void VizualizeRoom()
	{
		foreach (var ray in Walls)
		{
			Debug.DrawRay(transform.position, ray, Color.green);
		}
		foreach (var ray in Space)
		{
			Debug.DrawRay(transform.position, ray, Color.cyan);
		}
	}
	float RoomSizeCalculation()
	{
		float allLengths = 0;
		foreach (var ray in Walls)
		{
			allLengths += ray.magnitude;
		}
		allLengths += Space.Count * WallCheckingRayLength * 2;
		return allLengths / 18;
	}
	private void OnDrawGizmos()
	{

	}
	void TryToEscape()
	{
		Target = transform.position;
		Vector2 escapingDirection = new Vector2();

		//Just one direction without obstacles
		if (Space.Count == 1)
		{
			escapingDirection = Space[0];
		}
		//Few free directions
		else if (Space.Count > 1)
		{
			List<List<Vector2>> listOfSequences = new List<List<Vector2>>();
			List<Vector2> freeSpaces = new List<Vector2>();
			freeSpaces.AddRange(Space);
			int emergencyStop = 0;
			int numberOfSeq = 0;
			while (freeSpaces.Count > 0)
			{
				emergencyStop++;
				if (emergencyStop > 36) break;
				listOfSequences.Add(new List<Vector2>());
				listOfSequences[numberOfSeq].Add(freeSpaces[0]);
				freeSpaces.Remove(freeSpaces[0]);

				foreach (var ray in freeSpaces)
				{
					foreach (var _ray in listOfSequences[numberOfSeq])
					{
						var angle = Vector2.Angle(_ray, ray);
						if (angle < 21)
						{
							listOfSequences[numberOfSeq].Add(ray);
							break;
						}
					}
				}
				foreach (var ray in listOfSequences[numberOfSeq])
				{
					if (freeSpaces.Contains(ray))
					{
						freeSpaces.Remove(ray);
					}
				}
				numberOfSeq++;
			}
			int maxLength = 0;
			List<Vector2> longestSeq = new List<Vector2>();
			foreach (var seq in listOfSequences)
			{
				if (seq.Count > maxLength)
				{
					maxLength = seq.Count;
					longestSeq = seq;
				}
			}
			Vector2 centerRay = new Vector2();
			foreach (var ray in longestSeq)
			{
				centerRay += ray;
			}
			centerRay = centerRay.normalized * longestSeq[0].magnitude;
			escapingDirection = FindNearestRay(longestSeq.ToArray());

		}
		//Only obstacles
		else if (Space.Count == 0)
		{
			//Choosing 5 longest rays where obstacle is further 
			var arrayOfLongestWallRays = FindLongestWallRays();
			//Choosing nearest to last direction ray. Illusion of innertia
			Vector2 nearestRay = FindNearestRay(arrayOfLongestWallRays);
			escapingDirection = nearestRay;
		}
		var trueEscape = escapingDirection;
		Debug.DrawRay(transform.position, trueEscape, Color.green);
		Debug.DrawRay(transform.position, LastEscapingDirection, MyColors.darkGreen);
		if (trueEscape == Vector2.zero)
		{
			trueEscape = -LastEscapingDirection;
		}
		else
		{
			LastEscapingDirection = trueEscape;
		}
		transform.Translate(trueEscape.normalized * MovementSpeed * Time.deltaTime);
	}
	Vector2 FindNearestRay(Vector2[] raysArray)
	{
		var minAngle = 360f;
		Vector2 nearestRay = new Vector2();
		foreach (var ray in raysArray)
		{
			var cock = Vector2.Angle(ray, LastEscapingDirection);
			if (cock < minAngle)
			{
				minAngle = cock;
				nearestRay = ray;
			}
		}
		return nearestRay;
	}
	Vector2[] FindLongestWallRays()
	{
		List<Vector2> wallRays = new List<Vector2>();
		wallRays.AddRange(Walls);
		Vector2[] rays = new Vector2[5];
		for (int i = 0; i < 5; i++)
		{
			float maxLength = 0;
			foreach (var ray in wallRays)
			{
				if (ray.magnitude > maxLength)
				{
					maxLength = ray.magnitude;
					rays[i] = ray;
				}
			}
			wallRays.Remove(rays[i]);
		}
		return rays;
	}
	bool CanHurricane()
	{
		return OpenSpace && Imps.Count >= 10;
	}
	IEnumerator WaitToStartHurricane()
	{
		while (true)
		{
			yield return new WaitUntil(CanHurricane);
			//PrepareToHurricane();
			Hurricane = true;
			yield return new WaitWhile(CanHurricane);
			//UnprepareToHurricane();
			Hurricane = false;
		}
	}
	void PrepareToHurricane()
	{
		foreach (var imp in Imps)
		{
			print("unpush");
			imp.GetComponent<Entity>().Push = false;
		}
	}
	void UnprepareToHurricane()
	{
		foreach (var imp in Imps)
		{
			print("unpush");
			imp.GetComponent<Entity>().Push = true;
		}
	}
	public float hurricaneSpeed = 1.5f;
	public float hurricaneStartWidth = 1;
	public float hurricaneWidth = 0.3f;
	void ToHurricane()
	{
		foreach (var imp in Imps)
		{
			imp.Target = imp.RotatingPoint(transform.position + Vector3.up * 0.5f * Imps.IndexOf(imp)*addHivingHeight/2+Vector3.up*addHivingHeight, hurricaneStartWidth + (float)Imps.IndexOf(imp)* hurricaneWidth, hurricaneSpeed, 2);
		}
	}
	IEnumerator HurricaneAttack()
	{
		while (true)
		{
			System.Func<bool> can = () => SeeEnemy && Hurricane;
			yield return new WaitUntil(can);
			{
				stop = true;
				hurricaneStartWidth = 2;
				hurricaneSpeed = 4;
				addHivingHeight = 4;
			}
			yield return new WaitForSeconds(2);
			foreach(var imp in Imps)
			{
				if (SeeEnemy) imp.SpearAttack();
				else imp.ParabAttack();
			}
			hurricaneStartWidth = 1;
			stop = false;
			hurricaneSpeed = 1.5f;
			addHivingHeight = 1;
			yield return new WaitForSeconds(8);
		}
	}

}
