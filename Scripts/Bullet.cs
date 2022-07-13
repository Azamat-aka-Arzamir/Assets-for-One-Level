using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer),typeof(Rigidbody2D),typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
	[Tooltip("Collisions count before destructing")]
	public int PossibleCollisions;
	public delegate void Action();
    Action actionOnDestruct;
    public float Speed;
	[HideInInspector]public Weapon weapon;
    [HideInInspector]public Vector2 Dir;
    public enum type { Bullet,Bomb}
    public type _type;
    Rigidbody2D srb;
    float timest;
    // Start is called before the first frame update
    void Start()
    {
        timest = Time.time;
        srb = GetComponent<Rigidbody2D>();
        srb.AddForce(Dir * Speed);
		if (_type == type.Bomb)
		{
			actionOnDestruct = Detonate;
		}
        if(_type == type.Bullet)
		{
            actionOnDestruct = Die;
		}
    }

    // Update is called once per frame
    void Update()
    {
		Entity ent;
		var ray = Physics2D.Raycast((Vector2)transform.position+Dir*0.2f, srb.velocity*Time.deltaTime, srb.velocity.magnitude,LayerMask.GetMask("Entity"));
		Debug.DrawRay(transform.position, srb.velocity * Time.deltaTime, Color.cyan);
		if (ray)
		{
			print(ray.collider);
			ray.collider.TryGetComponent(out ent);
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
		}
    }


	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (Time.time - timest < 0.2)
		{
			return;
		}
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
	private void OnCollisionStay2D(Collision2D collision)
	{
		if (Time.time - timest < 0.2)
		{
			return;
		}
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


	void Detonate()
	{

	}
    void Die()
	{
        Destroy(gameObject);
    }
}
