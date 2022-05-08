using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Weapon : MonoBehaviour
{
    public enum type {sword,shield,gun };
    public type weaponType;
    Collider2D selfColl;
    float AnimLength;
    bool Activate;
    public int Damage;


#if UNITY_EDITOR
    [CustomEditor(typeof(Weapon))]
    public class WeaponEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
            Weapon weapon = (Weapon)target;
            switch(weapon.weaponType)
			{
                case type.sword:
                    weapon.AnimLength = EditorGUILayout.FloatField(weapon.AnimLength);
                    break;
                case type.shield:
                    break;
                case type.gun:
                    break;
			}
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
    // Update is called once per frame
    void Update()
    {
        if(weaponType!=type.gun)selfColl.bounds.SetMinMax(GetComponent<SpriteRenderer>().bounds.min, GetComponent<SpriteRenderer>().bounds.max);
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
