using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer),typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
	[Tooltip("Collisions count before destructing")]
	public int PossibleCollisions;
	public delegate void Action();
    Action actionOnDestruct;
    public float Speed;
	Vector2 dir;
	[HideInInspector]public Weapon weapon;
    public enum type { Bullet,Bomb}
    public type _type;
    Rigidbody2D srb;
    float timest;
	Collider2D selfCollider;
    // Start is called before the first frame update
    void Start()
    {
		Misc.DrawCross(transform.position, 1, Color.magenta, 2);
		TryGetComponent(out selfCollider);
		var a = weapon.GetComponent<CustomAnimator>().CurrentAnim.name;
		if (a.Contains("Up"))
		{
			dir = Vector2.up;
		}
		else if (a.Contains("Down"))
		{
			dir = Vector2.down;
		}
		else if(weapon.GetComponent<CustomAnimator>().side==Misc.Side.R)
		{
			dir = Vector2.right;
		}
		else
		{
			dir = Vector2.left;
		}
		if (dir.y != 0) dir = new Vector2(0, dir.y);
        timest = Time.time;
        srb = GetComponent<Rigidbody2D>();
        srb.AddForce(dir * Speed);
		if (_type == type.Bomb)
		{
			actionOnDestruct = Detonate;
		}
        if(_type == type.Bullet)
		{
            actionOnDestruct = Die;
		}
    }
	Vector2 direct;
    // Update is called once per frame
    void Update()
    {
		Entity ent;
		if (srb.velocity.magnitude > 0)
		{
			direct = srb.velocity;
		}
		else if (Time.time-timest>Time.fixedDeltaTime*2&&_type == type.Bullet && (selfCollider==null||selfCollider.sharedMaterial == null))
		{
			actionOnDestruct();
		}
		var ray = Physics2D.RaycastAll(transform.position, direct.normalized, direct.magnitude * Time.deltaTime, LayerMask.GetMask("Entity")|LayerMask.GetMask("Ground") | LayerMask.GetMask("HitBox"));
		Debug.DrawRay(transform.position, direct*Time.deltaTime, Color.cyan);
		foreach (var hit in ray)
		{
			hit.collider.TryGetComponent(out ent);
			if (ent == null) hit.collider.transform.parent.TryGetComponent(out ent);
			if (ent != null&&ent!=weapon.parentEnt)
			{
				ent.GetDamage(weapon.Damage, (int)Mathf.Sign((ent.transform.position - transform.position).normalized.x), weapon.transform.parent.gameObject, weapon.pushingForce, weapon);
				if (PossibleCollisions > 0)
				{
					PossibleCollisions--;
					//Jump?
				}
				else
				{
					actionOnDestruct();
				}
			}
			if(hit.collider.tag == "Ground")
			{
				if (PossibleCollisions > 0)
				{
					PossibleCollisions--;
					//Jump?
				}
				else
				{
					actionOnDestruct();
				}
				break;
			}
		}
    }

	void Detonate()
	{

	}
    void Die()
	{
        Destroy(gameObject);
    }
}
