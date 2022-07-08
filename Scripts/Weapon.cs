using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Weapon : MonoBehaviour
{
	public enum type { sword, shield, gun };
	public type weaponType;
	Animator SelfAnim;
	PolygonCollider2D selfColl;
	[HideInInspector]
	public bool Activate;
	public int Damage;
	[HideInInspector] public Vector3 BarrelPoint;
	[HideInInspector] public int Bullets;
	[HideInInspector] public Bullet BulletType;
	[SerializeField] int StaminaCost;
	[HideInInspector]
	public float StartZ;
	Animator parentAnim;
	Sprite[] sprites;
	SpriteRenderer selfRender;
	[HideInInspector] public int attackFrames;
	[HideInInspector] public int beforeAttackFrames;
	[HideInInspector] public int pushingForce;
	[HideInInspector] public bool AttackSameEntity;
	[HideInInspector] public bool DynamicAttackFrames;


#if UNITY_EDITOR
	[CustomEditor(typeof(Weapon)), CanEditMultipleObjects]
	public class WeaponEditor : Editor
	{
		Weapon selfW;
		private SerializedProperty _activate;
		private SerializedProperty _framesOA;
		private SerializedProperty _psoa;
		private SerializedProperty _barrelPoint;
		private SerializedProperty _bulletsCount;
		private SerializedProperty _bulletType;
		private SerializedProperty _ASE;
		private SerializedProperty _daf;
		private SerializedProperty _baf;
		Vector3 a;
		Vector3 b;
		bool c;
		private void OnEnable()
		{
			selfW = (Weapon)target;
			_activate = serializedObject.FindProperty("Activate");
			_framesOA = serializedObject.FindProperty("attackFrames");
			_psoa = serializedObject.FindProperty("pushingForce");
			_barrelPoint = serializedObject.FindProperty("BarrelPoint");
			_bulletsCount = serializedObject.FindProperty("Bullets");
			_bulletType = serializedObject.FindProperty("BulletType");
			_ASE = serializedObject.FindProperty("AttackSameEntity");
			_daf = serializedObject.FindProperty("DynamicAttackFrames");
			_baf = serializedObject.FindProperty("beforeAttackFrames");
			c = true;

		}
		private void OnDisable()
		{
			c = false;
		}
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			base.OnInspectorGUI();
			Weapon weapon = selfW;
			switch (weapon.weaponType)
			{
				case type.sword:
					EditorGUILayout.PropertyField(_activate);

					EditorGUILayout.PropertyField(_framesOA);

					EditorGUILayout.PropertyField(_psoa);

					EditorGUILayout.PropertyField(_ASE);
					EditorGUILayout.PropertyField(_daf);
					EditorGUILayout.PropertyField(_baf);
					//weapon.AnimLength = EditorGUILayout.FloatField("Animation Length", weapon.AnimLength);
					break;
				case type.shield:
					weapon.Activate = EditorGUILayout.Toggle("Activate", weapon.Activate);
					break;
				case type.gun:
					weapon.BarrelPoint = EditorGUILayout.Vector3Field("Barrel point", weapon.BarrelPoint);
					weapon.Bullets = EditorGUILayout.IntField("Bullets count", weapon.Bullets);
					weapon.BulletType = EditorGUILayout.ObjectField("Bullet type", weapon.BulletType, typeof(Bullet), false) as Bullet;
					break;

			}
			serializedObject.ApplyModifiedProperties();
		}
		private void OnSceneGUI()
		{
			Weapon weapon = (Weapon)target;
			Handles.matrix = weapon.transform.localToWorldMatrix;
			if (c && weapon.weaponType == type.gun)
			{
				weapon.BarrelPoint = Handles.PositionHandle(weapon.BarrelPoint, Quaternion.identity);
				Handles.DrawWireDisc(weapon.BarrelPoint, Vector3.forward, 0.05f);
				Handles.DrawLine(weapon.BarrelPoint + Vector3.up * 0.05f, weapon.BarrelPoint - Vector3.up * 0.05f);
				Handles.DrawLine(weapon.BarrelPoint + Vector3.right * 0.05f, weapon.BarrelPoint - Vector3.right * 0.05f);

			}
		}


	}
#endif

	// Start is called before the first frame update
	void Start()
	{
		sprites = Resources.LoadAll<Sprite>("Weapons Spritesheets/" + name);
		TryGetComponent(out selfRender);
		StartZ = transform.localPosition.z;
		TryGetComponent(out selfColl);
		SelfAnim = GetComponent<Animator>();
		parentAnim = transform.parent.GetComponent<Animator>();
	}
	public void Fire(bool phase)
	{
		switch (weaponType)
		{
			case type.sword:
				SwordAttack();
				break;
			case type.shield:
				ShieldAttack(phase);
				break;
			case type.gun:
				break;
		}
	}
	void UpdatePhysicsOutline()
	{
		selfColl.points = selfRender.sprite.vertices;
	}

	[System.Obsolete("Удали потом")]
	void CheckForActivation()
	{
		var currentStateTag = parentAnim.GetCurrentAnimatorStateInfo(0);
		if (currentStateTag.IsTag("Attack"))
		{
			Activate = true;
		}
		else
		{
			Activate = false;
		}
	}
	void SwordAttack()
	{
		StartCoroutine(IeSwordAttack());
	}
	IEnumerator IeSwordAttack()
	{
		var currentState = parentAnim.GetCurrentAnimatorStateInfo(0);
		var a = currentState.length;
		if (a == 0||!DynamicAttackFrames)
		{
			if (beforeAttackFrames != 0)
			{
				for (int i = 0; i < beforeAttackFrames; i++)
				{
					yield return new WaitForFixedUpdate();
				}
			}

			Activate = true;
			for (int i = 0; i < attackFrames; i++)
			{
				Misc.DrawCross(new Vector2(transform.position.x-2*transform.localPosition.z, transform.position.y), Time.fixedDeltaTime, Color.red, 3);
				yield return new WaitForFixedUpdate();
			}
		}
		else
		{
			Activate = true;
			yield return new WaitForSeconds(a);
		}
		Activate = false;
		StopCoroutine(IeGunAttack());
		yield break;
	}
	void ShieldAttack(bool phase)
	{
		Activate = phase;
		print(phase);
	}
	void GunAttack()
	{

	}
	IEnumerator IeGunAttack()
	{

		yield break;
	}
	// Update is called once per frame
	void FixedUpdate()
	{
		//if (weaponType != type.gun) selfColl.bounds.SetMinMax(GetComponent<SpriteRenderer>().bounds.min, GetComponent<SpriteRenderer>().bounds.max);
		if (weaponType == type.sword)
		{
			//CheckForActivation();
			if (selfRender != null) UpdatePhysicsOutline();
		}
	}
	private void Update()
	{
		//ControlAnims();
		if (Activate)
		{
			//print("suction");
		}
	}
	private void OnTriggerStay2D(Collider2D collision)
	{
		Entity entity;
		collision.TryGetComponent(out entity);
		if (collision.tag == "HitBox")
		{
			collision.transform.parent.TryGetComponent(out entity);
		}
		else
		{
			return;
		}
		if (weaponType == type.sword && Activate && entity != null)
		{
			if (AttackSameEntity)
			{
				entity.GetDamage(Damage, (int)Mathf.Sign((entity.transform.position - transform.position).normalized.x), transform.parent.gameObject, pushingForce,this);
			}
			else
			{
				if(entity.Type != transform.GetComponentInParent<Entity>().Type)
				{
					entity.GetDamage(Damage, (int)Mathf.Sign((entity.transform.position - transform.position).normalized.x), transform.parent.gameObject, pushingForce,this);
				}
			}
		}
	}

	[System.Obsolete("Удали потом")]
	void ControlAnims()
	{
		var a = parentAnim.GetCurrentAnimatorStateInfo(0);
		SelfAnim.Play(a.shortNameHash, 0, a.normalizedTime + Time.fixedDeltaTime / a.length);
	}

	private void LateUpdate()
	{
		var structure = transform.parent.GetComponent<SpriteRenderer>();
		var b = structure.sprite.name;
		if (selfRender != null) selfRender.sprite = System.Array.Find(sprites, x => x.name == b);
		//print(b[0] + '_' + gameObject.name + '_' + b[1]+"  "+ b[0]+'_'+b[1]);
	}
}
