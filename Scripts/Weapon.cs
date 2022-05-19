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
	public int Damage;
	Vector3 BarrelPoint;
	int Bullets;
	Bullet BulletType;
	[SerializeField] int StaminaCost;
	[HideInInspector]
	public float StartZ;
	Animator parentAnim;
	Sprite[] sprites;
	SpriteRenderer selfRender;


#if UNITY_EDITOR
	[CustomEditor(typeof(Weapon))]
	public class WeaponEditor : Editor
	{
		Vector3 a;
		Vector3 b;
		bool c;
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			Weapon weapon = (Weapon)target;
			switch (weapon.weaponType)
			{
				case type.sword:
					//weapon.AnimLength = EditorGUILayout.FloatField("Animation Length", weapon.AnimLength);
					break;
				case type.shield:
					weapon.Activate = EditorGUILayout.Toggle("Activate",weapon.Activate);
					break;
				case type.gun:
					weapon.BarrelPoint = EditorGUILayout.Vector3Field("Barrel point", weapon.BarrelPoint);
					weapon.Bullets = EditorGUILayout.IntField("Bullets count", weapon.Bullets);
					weapon.BulletType = EditorGUILayout.ObjectField("Bullet type", weapon.BulletType, typeof(Bullet), false) as Bullet;
					break;
			}
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
		private void OnEnable()
		{
			c = true;
		}
		private void OnDisable()
		{
			c = false;
		}

	}
#endif

	// Start is called before the first frame update
	void Start()
	{
		sprites = Resources.LoadAll<Sprite>("Weapons Spritesheets");
		selfRender = GetComponent<SpriteRenderer>();
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
				break;
			case type.shield:
				ShieldAttack(phase);
				break;
			case type.gun:
				break;
		}
	}

	void CheckForActivation()
	{
		var currentStateTag = SelfAnim.GetCurrentAnimatorStateInfo(0);
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
		if(weaponType == type.sword)CheckForActivation();
	}
	private void Update()
	{
		ControlAnims();

	}
	private void OnTriggerEnter2D(Collider2D collision)
	{
		Entity entity;
		collision.TryGetComponent(out entity);
		if (weaponType == type.sword && Activate)
		{
			entity.HealthPoints -= Damage;
		}
	}

	void ControlAnims()
	{
		var a = parentAnim.GetCurrentAnimatorStateInfo(0);
		SelfAnim.Play(a.shortNameHash, 0, a.normalizedTime);
	}

	private void LateUpdate()
	{
		var structure = selfRender.sprite.name.Split('_');
		selfRender.sprite = System.Array.Find<Sprite>(sprites, x => x.name == (structure[0]+'_'+gameObject.name+'_'+structure[2]));
	}
}
