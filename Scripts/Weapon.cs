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
	Collider2D selfColl;
	float AnimLength;
	public bool Activate;
	public int Damage;
	Vector3 BarrelPoint;
	int Bullets;
	Bullet BulletType;
	[SerializeField] int StaminaCost;


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
					weapon.AnimLength = EditorGUILayout.FloatField("Animation Length", weapon.AnimLength);
					break;
				case type.shield:
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
			if (c&&weapon.weaponType==type.gun)
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
		TryGetComponent(out selfColl);
	}
	void Fire()
	{
		switch (weaponType)
		{
			case type.sword:
				StartCoroutine(SwordAttack());
				break;
			case type.shield:
				break;
			case type.gun:
				break;
		}
	}
	IEnumerator SwordAttack()
	{
		Activate = true;
		yield return new WaitForSeconds(AnimLength);
		Activate = false;
		yield break;
	}
	void ShieldAttack()
	{

	}
	void GunAttack()
	{

	}
	IEnumerator IeGunAttack()
	{

		yield break;
	}
	// Update is called once per frame
	void Update()
	{
		if (weaponType != type.gun) selfColl.bounds.SetMinMax(GetComponent<SpriteRenderer>().bounds.min, GetComponent<SpriteRenderer>().bounds.max);
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
}
