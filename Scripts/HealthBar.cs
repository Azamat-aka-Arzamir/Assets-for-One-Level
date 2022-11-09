using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HealthBar : MonoBehaviour
{
	[SerializeField] Entity InspectedEntity;
	[SerializeField] GameObject HealthPoint;
	[SerializeField] GameObject StaminaHolder;
	RectTransform StaminaObject;
	Vector3 StaminaDefaultScale;
	[SerializeField] GameObject ReloadTimer;
	[SerializeField] Vector2 StartPos;
	[SerializeField] Vector2 StartPosInLocalSpace;
	[SerializeField] float PointDistance;
	List<GameObject> HealthPoints = new List<GameObject>();
	Canvas selfCanvas;

#if UNITY_EDITOR
	[CustomEditor(typeof(HealthBar))]
	public class HealthHolderEditor : Editor
	{
		HealthBar _target;
		private void OnEnable()
		{
			_target = (HealthBar)target;
		}
		private void OnSceneGUI()
		{
			_target.StartPos = Handles.PositionHandle(_target.StartPos, Quaternion.identity);
			_target.StartPosInLocalSpace = _target.StartPos - (Vector2)_target.GetComponent<RectTransform>().position;
			_target.StartPosInLocalSpace = _target.StartPosInLocalSpace / _target.GetComponent<RectTransform>().sizeDelta + Vector2.one / 2;
		}
	}
#endif
	// Start is called before the first frame update
	void Start()
	{
		cum();
		for (int i = 0; i < InspectedEntity.HealthPoints; i+=2)
		{
			InitializeHealthPoint(i);
		}
		InspectedEntity.DamageEvent.AddListener(GetDamage);
		InitializeStamina();

	}
	void cum()
	{
		string[,] n = new string[16, 16];
		for(int i = 0;i<15; i++)
		{
			for (int j = 0; j < 15; j++)
			{
				string _n = "";
				int toCalc = i*j;
				while (toCalc>15)
				{
					var k = toCalc % 16;
					if (k > 9)
					{
						switch (k)
						{
							case 10:
								_n += "A";
								break;
							case 11:
								_n += "B";
								break;
							case 12:
								_n += "C";
								break;
							case 13:
								_n += "D";
								break;
							case 14:
								_n += "E";
								break;
							case 15:
								_n += "F";
								break;
						}
					}
					else
					{
						_n += k;
					}
					_n += "_";
					toCalc = toCalc / 16;
				}
				n[i, j] = _n + toCalc;
				string[] h = new string[n[i, j].Split('_').Length];
				int g = h.Length -1;
				foreach(var a in n[i, j].Split('_'))
				{
					h[g] = a;
					g--;
				}
				n[i, j] = "";
				foreach(var a in h)
				{
					n[i, j] += a;
				}
			}
		
		}

		for(int i = 0; i < 15; i++)
		{
			print(n[0, i] + "|" + n[1, i] + "|" + n[2, i] + "|" + n[3, i] + "|" + n[4, i] + "|" + n[5, i] + "|" + n[6, i] + "|" + n[7, i] + "|" + n[8, i] + "|" + n[9, i] + "|" + n[10, i] + "|" + n[11, i] + "|" + n[12, i] + "|" + n[13, i] + "|" + n[14, i] + "|" + n[15, i]);
		}
	}
	void InitializeHealthPoint(int number)
	{
		var point = Instantiate(HealthPoint, transform);
		var rect = point.GetComponent<RectTransform>();
		rect.anchorMax = StartPosInLocalSpace;
		rect.anchorMin = StartPosInLocalSpace;
		rect.anchoredPosition = new Vector2(PointDistance * number, 0);
		HealthPoints.Add(point);
	}
	void InitializeStamina()
	{
		var point = StaminaHolder;
		var rect = point.GetComponent<RectTransform>();
		//rect.anchorMax = StartPosInLocalSpace;
		//rect.anchorMin = StartPosInLocalSpace;
		//rect.pivot=Vector2.up*0.2f;
		//rect.anchoredPosition = new Vector2(-50, -100);
		StaminaDefaultScale = rect.sizeDelta;
		StaminaObject = point.GetComponent<RectTransform>();
	}
	void UpdateStamina()
	{
		StaminaObject.sizeDelta = new Vector3(StaminaDefaultScale.x*InspectedEntity.StaminaRemains / InspectedEntity.Stamina, StaminaDefaultScale.y);
	}
	// Update is called once per frame
	void Update()
	{
		UpdateStamina();
	}
	private void GetDamage()
	{
		//Destroy(HealthPoints[HealthPoints.Count - 1]);
		int curHeart = InspectedEntity.HealthPoints / 2;
		if(InspectedEntity.HealthPoints%2==1)HealthPoints[curHeart].GetComponent<CustomAnimator>().PlayAnim("HalfHP");
		else HealthPoints[curHeart].GetComponent<CustomAnimator>().PlayAnim("EmptyHP");
		//HealthPoints.RemoveAt(HealthPoints.Count - 1);
	}
}
