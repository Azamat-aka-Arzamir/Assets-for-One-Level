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
		for (int i = 0; i < InspectedEntity.HealthPoints; i++)
		{
			InitializeHealthPoint(i);
		}
		InspectedEntity.DamageEvent.AddListener(GetDamage);
		InitializeStamina();

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
		var point = Instantiate(StaminaHolder, transform);
		var rect = point.GetComponent<RectTransform>();
		rect.anchorMax = StartPosInLocalSpace;
		rect.anchorMin = StartPosInLocalSpace;
		rect.pivot=Vector2.up*0.5f;
		rect.anchoredPosition = new Vector2(-50, -100);
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
		Destroy(HealthPoints[HealthPoints.Count - 1]);
		HealthPoints.RemoveAt(HealthPoints.Count - 1);
	}
}
