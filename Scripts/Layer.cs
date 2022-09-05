using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Misc;

public class Layer : MonoBehaviour
{
	public Camera SelfCamera;
	public int LayerNumber;
	public float HorizontalSpeed;
	public float VerticalSpeed;
	public bool RepeatVertically;
	public Vector2 gridPlace;
	Vector3 StartPos;
	SpriteRenderer SelfRender;
	private void Awake()
	{
		gameObject.layer = LayerMask.NameToLayer("BackGround");
	}
	// Start is called before the first frame update
	void Start()
	{
		ParralaxHolder.grid.Add(this);
		StartPos = transform.position;
		SelfCamera = Camera.main;
		TryGetComponent<SpriteRenderer>(out SelfRender);
		if (SelfRender == null)
		{
			SelfRender = GetComponentInChildren<SpriteRenderer>();
		}
		if (gridPlace == new Vector2(0, 0))
		{
			CopyImage(new Vector2(1, 0));
		}
		if (gridPlace == new Vector2(1,0))
		{
			ParralaxHolder.grid[0].Die();
			CopyImage(new Vector2(1,0));
			CopyImage(new Vector2(2, 0));
			CopyImage(new Vector2(3, 0));
			CopyImage(new Vector2(-1, 0));
			CopyImage(new Vector2(-2, 0));
			CopyImage(new Vector2(-3, 0));
			CopyImage(new Vector2(4, 0));
			CopyImage(new Vector2(5, 0));
			CopyImage(new Vector2(6, 0));
			CopyImage(new Vector2(-4, 0));
			CopyImage(new Vector2(-5, 0));
			CopyImage(new Vector2(-6, 0));

			CopyImage(new Vector2(7, 0));
			CopyImage(new Vector2(8, 0));
			CopyImage(new Vector2(9, 0));
			CopyImage(new Vector2(-7, 0));
			CopyImage(new Vector2(-8, 0));
			CopyImage(new Vector2(-9, 0));
		}

		MoveLayer();
		//if(gridPlace.x>-3&& gridPlace.x<3&&((gridPlace.y > -3 && gridPlace.y<3)||!RepeatVertically)) FillSpace();
	}
	void FillSpace()
	{
		{
			CopyImage(MyVector2.u);
			CopyImage(MyVector2.ur);
			CopyImage(MyVector2.r);
			CopyImage(MyVector2.dr);
			CopyImage(MyVector2.d);
			CopyImage(MyVector2.dl);
			CopyImage(MyVector2.l);
			CopyImage(MyVector2.ul);
		}
	}

	// Update is called once per frame
	void Update()
	{
		MoveLayer();
		if (RepeatVertically && ImageEnds(MyVector2.u)) CopyImage(MyVector2.u);
		if (RepeatVertically && ImageEnds(MyVector2.ur)) CopyImage(MyVector2.ur);
		if (ImageEnds(MyVector2.r)) CopyImage(MyVector2.r);
		if (RepeatVertically && ImageEnds(MyVector2.dr)) CopyImage(MyVector2.dr);
		if (RepeatVertically && ImageEnds(MyVector2.d)) CopyImage(MyVector2.d);
		if (RepeatVertically && ImageEnds(MyVector2.dl)) CopyImage(MyVector2.dl);
		if (ImageEnds(MyVector2.l)) CopyImage(MyVector2.l);
		if (RepeatVertically && ImageEnds(MyVector2.ul)) CopyImage(MyVector2.ul);
		//Die();
	}
	void MoveLayer()
	{
		var pos = (Vector2)SelfCamera.transform.position + ((Vector2)StartPos - (Vector2)SelfCamera.transform.position) / new Vector2(HorizontalSpeed, VerticalSpeed);
		transform.position = new Vector3(pos.x, pos.y, StartPos.z);
	}
	bool ImageEnds(Vector2 dir)
	{
		var sx = Mathf.Sign(dir.x);
		var sy = Mathf.Sign(dir.y);
		if (dir.x == 0) sx = 0;
		if (dir.y == 0) sy = 0;
		Vector2 bound = new Vector2(SelfRender.bounds.extents.x * sx, SelfRender.bounds.extents.y * sy);
		var cameraPoint = SelfCamera.ScreenToWorldPoint(new Vector3(SelfCamera.scaledPixelWidth/2 + SelfCamera.scaledPixelWidth * sx/2, SelfCamera.scaledPixelHeight/2 + SelfCamera.scaledPixelHeight * sy/2, 0));

		Vector2 diff = (Vector2)cameraPoint - ((Vector2)transform.position + bound);
		if ((diff * dir).magnitude > -SelfRender.bounds.extents.magnitude / 2 && (diff * dir).magnitude < SelfRender.bounds.extents.magnitude / 2)
		{
			//var a = Physics2D.RaycastAll(cameraPoint, dir, 5f, LayerMask.GetMask("BackGround"));
			Debug.DrawRay(cameraPoint, dir * 2, Color.cyan);
			return true;
		}
		return false;
	}

	void CopyImage(Vector2 place)
	{
		//place *= 0.999f;
		var spawnplace = StartPos + new Vector3(place.x * SelfRender.size.x * transform.localScale.x * HorizontalSpeed, place.y * SelfRender.size.y * transform.localScale.y * VerticalSpeed, 0f);
		Vector2 bound = new Vector2(SelfRender.bounds.extents.x * place.x, SelfRender.bounds.extents.y * place.y);
		if (ParralaxHolder.grid.Find(x => x.gridPlace == gridPlace + place && x.SelfRender.sprite.name == SelfRender.sprite.name))
		{
			return;
		}
		var a = Instantiate(gameObject, spawnplace, Quaternion.identity);
		a.GetComponent<Layer>().gridPlace = gridPlace + place;
		//MoveLayer();
	}
	void Die()
	{
		ParralaxHolder.grid.Remove(this);
		Destroy(gameObject);
	}

}
