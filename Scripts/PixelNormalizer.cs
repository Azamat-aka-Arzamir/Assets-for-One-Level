using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelNormalizer : MonoBehaviour
{
	public Vector2 posDifference;
	public float PPU;
	 Vector2 globalPos;
	float onePixelCost;
	// Start is called before the first frame update
	void Start()
	{
		onePixelCost = 1 / PPU;
	}

	// Update is called once per frame
	void Update()
	{
		if (Mathf.Abs(transform.localPosition.x) > onePixelCost/2)
		{
			transform.localPosition = new Vector3(0, transform.localPosition.y, transform.localPosition.z);
		}
		if (Mathf.Abs(transform.localPosition.y) > onePixelCost/2)
		{
			transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
		}
		globalPos = transform.position;
		var sx = Mathf.Sign(globalPos.x);
		var sy = Mathf.Sign(globalPos.y);
		var intDX = Mathf.FloorToInt(sx * globalPos.x / onePixelCost);
		var intDY = Mathf.FloorToInt(sy * globalPos.y / onePixelCost);
		var diffX = sx * globalPos.x -  intDX * onePixelCost;
		var diffY = sy * globalPos.y -  intDY * onePixelCost;
		if (Mathf.Abs(diffX) < onePixelCost / 2)
		{
			diffX = -diffX*sx;
		}
		else
		{
			diffX = (onePixelCost - diffX)*sx;
		}

		if (Mathf.Abs(diffY) < onePixelCost / 2)
		{
			diffY = -diffY*sy;
		}
		else
		{
			diffY = (onePixelCost - diffY)*sy;
		}

		posDifference = new Vector2(diffX, diffY);
		transform.localPosition += (Vector3)posDifference;
	}
}
