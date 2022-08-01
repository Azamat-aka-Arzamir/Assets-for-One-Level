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
	Collider2D selfColl;
	[HideInInspector]
	public bool Activate;
	bool Reloaded = true;
	[HideInInspector] public float GunCD;
	Movement parentMove;
	public int Damage;
	[HideInInspector] public Vector3 BarrelPoint;
	[HideInInspector] public int Bullets;
	[HideInInspector] public GameObject Bullet;
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
	[HideInInspector] public Entity parentEnt;
	 [HideInInspector]public Vector2 Dir;


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
		private SerializedProperty _GCD;
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
			_GCD = serializedObject.FindProperty("GunCD");
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
					EditorGUILayout.PropertyField(_psoa);
					break;
				case type.gun:
					weapon.BarrelPoint = EditorGUILayout.Vector3Field("Barrel point", weapon.BarrelPoint);
					weapon.Bullets = EditorGUILayout.IntField("Bullets count", weapon.Bullets);
					weapon.Bullet = EditorGUILayout.ObjectField("Bullet type", weapon.Bullet, typeof(GameObject), false) as GameObject;
					EditorGUILayout.PropertyField(_GCD);
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
		Dir = Vector2.right;
		sprites = Resources.LoadAll<Sprite>("Weapons Spritesheets/" + name);
		TryGetComponent(out selfRender);
		StartZ = transform.localPosition.z;
		TryGetComponent(out selfColl);
		parentEnt = GetComponentInParent<Entity>();
		SelfAnim = GetComponent<Animator>();
		parentAnim = transform.parent.GetComponent<Animator>();
		if (transform.parent != null)
		{
			transform.parent.TryGetComponent(out parentMove);
		}
		if (parentMove == null)
		{
			if (transform.parent.parent != null)
			{
				transform.parent.parent.TryGetComponent(out parentMove);
			}
		}
	}
	public void OnTurned()
	{
		//ChangeLayer
		transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z * -1);
		if (weaponType == type.gun)
		{
			transform.localScale = new Vector3(transform.localScale.x * -1, 1, 1);
			transform.localEulerAngles = new Vector3(0, 0, 90 * Dir.y * Dir.x);
		}
		Dir = new Vector2(Dir.x * -1, Dir.y);
	}
	public void Fire()
	{
		parentMove.IsAttack = true;
		switch (weaponType)
		{
			case type.sword:
				SwordAttack();
				break;
			case type.shield:
				ShieldAttack();
				break;
			case type.gun:
				if (Reloaded) GunAttack();
				break;
		}
	}
	void UpdatePhysicsOutline()
	{
		//selfColl.points = selfRender.sprite.vertices;
		//if (selfRender.sprite.name.ToString().StartsWith("L"))
		{
			selfColl.offset = new Vector2(-Mathf.Abs(selfColl.offset.x), selfColl.offset.y);
		}
		//if (selfRender.sprite.name.ToString().StartsWith("R"))
		{
			//selfColl.offset = new Vector2(Mathf.Abs(selfColl.offset.x), selfColl.offset.y);
		}
	}

	void SwordAttack()
	{
		StartCoroutine(IeSwordAttack());
	}
	public void OnLookUp(int context)
	{
		Dir = new Vector2(Dir.x, context);
		transform.localEulerAngles = new Vector3(0, 0, 90 * Dir.y * Dir.x);
	}
	IEnumerator IeSwordAttack()
	{
		//var currentState = parentAnim.GetCurrentAnimatorStateInfo(0);
		//var a = currentState.length;
		//if (a == 0 || !DynamicAttackFrames)
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
				Misc.DrawCross(new Vector2(transform.position.x - 2 * transform.localPosition.z, transform.position.y), Time.fixedDeltaTime, Color.red, 3);
				yield return new WaitForFixedUpdate();
			}
		}
		//else
		{
		//	Activate = true;
			//yield return new WaitForSeconds(a);
		}
		Activate = false;
		parentMove.IsAttack = false;
		StopCoroutine(IeSwordAttack());
		yield break;
	}
	void ShieldAttack()
	{
		Activate = !Activate;
		parentMove.IsAttack = false;
	}
	void GunAttack()
	{
		transform.localEulerAngles = new Vector3(0, 0, 90 * Dir.y * Dir.x);
		selfRender.enabled = true;
		var bullet = Instantiate(Bullet, (Vector3)(transform.localToWorldMatrix*BarrelPoint)+ transform.position, Quaternion.identity);
		bullet.GetComponent<Bullet>().weapon = this;
		Bullets--;
		parentMove.IsAttack = false;
		StartCoroutine(CoolDown());
	}
	IEnumerator CoolDown()
	{
		Reloaded = false;
		yield return new WaitForSeconds(GunCD);
		Reloaded = true;
		StopCoroutine(CoolDown());
		selfRender.enabled = false;
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
		if (weaponType == type.shield)
		{
			UpdateImmunity();
		}
	}
	void UpdateImmunity()
	{
		if (parentEnt != null && Activate)
		{
			if(transform.parent.localScale.x ==-1)
			//if (selfRender.sprite.name.ToString().StartsWith("L"))
			{
				parentEnt.ImmuneToDamageV2 = new Vector2(-1, 0);
			}
			if (transform.parent.localScale.x == 1)
			//if (selfRender.sprite.name.ToString().StartsWith("R"))
			{
				parentEnt.ImmuneToDamageV2 = new Vector2(1, 0);
			}
		}
		if (!Activate)
		{
			parentEnt.ImmuneToDamageV2 = new Vector2(0, 0);
		}
	}
	private void Update()
	{

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
				entity.GetDamage(Damage, (int)Mathf.Sign((entity.transform.position - transform.position).normalized.x), transform.parent.gameObject, pushingForce, this);
			}
			else
			{
				if (entity.Type != transform.GetComponentInParent<Entity>().Type)
				{
					entity.GetDamage(Damage, (int)Mathf.Sign((entity.transform.position - transform.position).normalized.x), transform.parent.gameObject, pushingForce, this);
				}
			}
		}
		if (weaponType == type.shield && Activate && (int)Mathf.Sign((entity.transform.position - transform.position).normalized.x) == parentEnt.ImmuneToDamageV2.x && entity != null)
		{
			entity.GetDamage(Damage, (int)Mathf.Sign((entity.transform.position - transform.position).normalized.x), transform.parent.gameObject, pushingForce, this);
		}
	}

	[System.Obsolete("����� �����")]
	void ControlAnims()
	{
		var a = parentAnim.GetCurrentAnimatorStateInfo(0);
		SelfAnim.Play(a.shortNameHash, 0, a.normalizedTime + Time.fixedDeltaTime / a.length);
	}

	private void LateUpdate()
	{
		if (selfRender != null && weaponType != type.gun) ChangeSprite();
	}
	void ChangeSprite()
	{
		var structure = transform.parent.GetComponent<SpriteRenderer>();
		if (structure.sprite == null) return;
		var b = structure.sprite.name;
		if (selfRender != null) selfRender.sprite = System.Array.Find(sprites, x => x.name == b);
	}
}
