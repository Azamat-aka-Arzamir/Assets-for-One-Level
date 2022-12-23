using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Entity : MonoBehaviour
{
	public enum entityType { Demon, Human, FuckingSlave }
	public entityType Type;
	public int MaxHealthPoints;
	public int HealthPoints;
	public int Stamina;
	public int StaminaRemains;
	public int StaminaRegenerationSpeed;
	[Header("Misc")]
	public  List<GameObject> PushingExceptions = new List<GameObject>();
	public bool Push = true;
	[SerializeField] float PushingForce = 150;
	[SerializeField] float PushingRadius = 1;
	[SerializeField] bool DyamicForce;
	[Space]
	float maxDistance;
	Collider2D SelfColl;
	Rigidbody2D SelfRb;

	[HideInInspector] public bool GettingDamage;
	SpriteRenderer selfRender;
	List<GameObject> LastDamages = new List<GameObject>();
	public int a;
	public bool IsDead = false;
	public bool ImmuneToDamage;
	public Vector2 ImmuneToDamageV2;



	// Start is called before the first frame update
	void Start()
	{
		HealthPoints = MaxHealthPoints;
		SelfColl = GetComponent<Collider2D>();
		TryGetComponent(out SelfRb);
		InitializeEntityTrigger();
		TryGetComponent(out selfRender);
	}
	void InitializeEntityTrigger()
	{
		var VitunPaska = new GameObject();
		VitunPaska.transform.parent = transform;
		VitunPaska.AddComponent<CircleCollider2D>();
		VitunPaska.GetComponent<CircleCollider2D>().radius = PushingRadius;
		VitunPaska.name = "EntityTrigger " + gameObject.name;
		VitunPaska.GetComponent<CircleCollider2D>().isTrigger = true;
		VitunPaska.transform.localPosition = SelfColl.offset;
		//Instantiate(VitunPaska, transform);
	}
	public bool kill;
	// Update is called once per frame
	void Update()
	{
		RegenerateStamina();
		a = LastDamages.Count;
	}
	void RegenerateStamina()
	{
		if (StaminaRemains < Stamina)
		{
			StaminaRemains += StaminaRegenerationSpeed;
		}
	}
	private void OnTriggerStay2D(Collider2D collision)
	{

		if (collision.gameObject.layer == gameObject.layer)
		{
			if (!Push) return;
			if (PushingExceptions.Contains(collision.gameObject)) return ;
			var pushingForce = PushingForce;
			if (DyamicForce && SelfRb != null) pushingForce = SelfRb.velocity.magnitude / 2 / Time.fixedDeltaTime * SelfRb.mass;
			var dist = (transform.position - collision.transform.position).magnitude;
			if (maxDistance == 0 && dist != 0) maxDistance = dist;
			if (maxDistance == 0 && dist == 0) maxDistance = 1;

			var k = 1 - dist / maxDistance;
			if (k < 0) k = 0;
			if (k > 1) k = 1;
			var dir = -(transform.position - collision.transform.position).normalized;
			if (SelfRb != null)
			{
				if (Mathf.Abs(SelfRb.velocity.x) > 1f)
				{
					// dir = dir * Mathf.Sign(dir.x) * SelfRb.velocity.x;
				}
			}
			Rigidbody2D colRb;
			collision.TryGetComponent(out colRb);
			if (dist < 0.1f) dir = Vector2.up; k = 0.5f;
			if (colRb != null) colRb.AddForce(dir * pushingForce * k);
		}
	}
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.layer == gameObject.layer)
		{
			maxDistance = (transform.position - collision.transform.position).magnitude;
		}
	}
	public void GetDamage(int damage, int dir, GameObject origin, int force, Weapon weapon)
	{
		if (dir == ImmuneToDamageV2.x)
		{
			return;
		}
		if (!LastDamages.Contains(origin)&&!ImmuneToDamage)
		{
			HealthPoints -= damage;
			if (HealthPoints <= 0)
			{

				if(!IsDead)Die(origin.name, weapon);

			}
			if (SelfRb != null)
			{
				SelfRb.velocity = Vector2.zero;
				SelfRb.AddForce(force * SelfRb.mass * dir * Vector2.right);
			}
			if(damage>0&&!IsDead)StartCoroutine(IeGetDamage(origin));
		}
	}
	public UnityEngine.Events.UnityEvent DamageEvent = new UnityEngine.Events.UnityEvent();
	IEnumerator IeGetDamage(GameObject origin)
	{
		var blink = StartCoroutine(Blink());
		GettingDamage = true;
		DamageEvent.Invoke();
		ImmuneToDamage = true;
		LastDamages.Add(origin);
		for (int i = 0; i < 20; i++)
		{
			yield return new WaitForFixedUpdate();
		}

		GettingDamage = false;
		LastDamages.Remove(origin);
		yield return new WaitForSeconds(2);
		ImmuneToDamage = false;
		StopCoroutine(blink);
		var v = GetComponentsInChildren<SpriteRenderer>();
		foreach (var a in v)
		{
			a.color =  new Color(1, 1, 1, 1);
		}
	}
	IEnumerator Blink()
	{
		var v = GetComponentsInChildren<SpriteRenderer>();
		while (true)
		{
			foreach(var a in v)
			{
				a.color = new Color(1, 0, 0, 0.5f);
			}
			yield return new WaitForSeconds(0.2f);
			foreach (var a in v)
			{
				a.color = new Color(1, 1, 1, 0.5f);
			}
			yield return new WaitForSeconds(0.2f);
		}
	}
	public UnityEngine.Events.UnityEvent DeathEvent = new UnityEngine.Events.UnityEvent();
	[SerializeField]
	bool DestroyOnDeath;
	[SerializeField]
	float DestroyTimer;
	void Die(string killerName, Weapon weapon)
	{
		IsDead = true;
		DeathEvent.Invoke();
		Push = false;
		Component[] components = GetComponents<Component>();
		for(int i = 0; i < components.Length; i++)
		{
			var type = components[i].GetType();
			if (type != typeof(Transform) && !Misc.IsCollider(type) && type != typeof(Animator) && type != typeof(SpriteRenderer) && type != typeof(Rigidbody2D)&&type!=typeof(Entity)&&type!=typeof(CustomAnimator))
			{
				Misc.KillComponent(components[i]);
				if(type!=typeof(Controller)&&type!=typeof(UnityEngine.InputSystem.PlayerInput))Destroy(components[i]);
			}
		}
		PushingForce *= 0.5f;
		SelfColl.sharedMaterial = new PhysicsMaterial2D();
		SelfColl.sharedMaterial.friction = 10;
		print(gameObject.name + " was killed by " + weapon.name + " in " + killerName + "'s hands");
		if(DestroyOnDeath)Destroy(gameObject, DestroyTimer);
	}

}
public static class Misc
{
    //public delegate bool condition(CustomAnimatorContextInfo animatorContextInfo);
    public static AnimatorScheme LoadScheme(string path)
    {
		if (!Directory.Exists("Assets/Animators/" + path)) return null;
        var formatter = new BinaryFormatter();
        var fileStream = new FileStream("Assets/Animators/" + path, FileMode.Open);
        var scheme = (AnimatorScheme)formatter.Deserialize(fileStream);
        fileStream.Close();
        return scheme;
    }
    public enum Side { L, R };
	public struct MyColors
	{
		public static UnityEngine.Color pink
		{
			get
			{
				return new UnityEngine.Color(1, 0.4f, 0.7f);
			}
		}
		public static UnityEngine.Color darkGreen
		{
			get
			{
				return new UnityEngine.Color(0, 0.5f, 0);
			}
		}
	}
	public struct MyVector2
	{
		/// <summary>
		/// Vector2 (0,1)
		/// </summary>
		public static Vector2 u
		{
			get
			{
				return new Vector2(0, 1);
			}
		}
		/// <summary>
		/// Vector2 (1,1)
		/// </summary>
		public static Vector2 ur
		{
			get
			{
				return new Vector2(1, 1);
			}
		}
		/// <summary>
		/// Vector2 (1,0)
		/// </summary>
		public static Vector2 r
		{
			get
			{
				return new Vector2(1, 0);
			}
		}
		/// <summary>
		/// Vector2 (1,-1)
		/// </summary>
		public static Vector2 dr
		{
			get
			{
				return new Vector2(1, -1);
			}
		}
		/// <summary>
		/// Vector2 (0,-1)
		/// </summary>
		public static Vector2 d
		{
			get
			{
				return new Vector2(0, -1);
			}
		}
		/// <summary>
		/// Vector2 (-1,-1)
		/// </summary>
		public static Vector2 dl
		{
			get
			{
				return new Vector2(-1, -1);
			}
		}
		/// <summary>
		/// Vector2 (-1,0)
		/// </summary>
		public static Vector2 l
		{
			get
			{
				return new Vector2(-1,0);
			}
		}
		/// <summary>
		/// Vector2 (-1,1)
		/// </summary>
		public static Vector2 ul
		{
			get
			{
				return new Vector2(-1, 1);
			}
		}
	}
	public static void DrawCross(Vector2 pos, float duration, UnityEngine.Color color, float size)
	{
		Debug.DrawRay(pos, new Vector2(1, 1).normalized * size / 2, color, duration);
		Debug.DrawRay(pos, new Vector2(-1, 1).normalized * size / 2, color, duration);
		Debug.DrawRay(pos, new Vector2(-1, -1).normalized * size / 2, color, duration);
		Debug.DrawRay(pos, new Vector2(1, -1).normalized * size / 2, color, duration);
	}
	public static bool IsCollider(System.Type type)
	{
		bool collider = type.FullName.Contains("Collider");
		if (collider)
		{
			return true;
		}
		else return false;
	}
	public static bool IsCollider(Component component)
	{
		var type = component.GetType();
		bool collider = type.FullName.Contains("Collider");
		if (collider)
		{
			return true;
		}
		else return false;
	}
	public static void KillComponent(Component component)
	{
		var type = component.GetType();
		if (type == typeof(ImpController))
		{
			component.gameObject.GetComponent<ImpController>().Die();
		}
		if (type == typeof(Controller))
		{
			component.gameObject.GetComponent<Controller>().Die();
		}
	}
	public static AnimationClip GetAnimation(string name, Animator anim)
	{
		List<AnimationClip> clips = new List<AnimationClip>();
		clips.AddRange(anim.runtimeAnimatorController.animationClips);
		if (anim.HasState(0, Animator.StringToHash(name))) return clips.Find(x => x.name == name);
		else return null;
	}
}
public class IntContextEvent : UnityEngine.Events.UnityEvent<int> { }
