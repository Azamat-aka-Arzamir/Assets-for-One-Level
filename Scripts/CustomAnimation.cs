using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New Custom Animation", menuName = "Custom Animations", order = 51)]
public class CustomAnimation : ScriptableObject
{
	//public bool playTrigger;
	//public string[] attributes=new string[0];
	public string tag;
	public bool saveImpulse = false;
	public float speed;//same as framerate or FPS
	public List<CustomFrame> frames;
	public string animName;
	public int priority;
	public bool repeatable;
	public bool interruptable;
	public string[] transitionsTo;
	public string[] doNotTransitTo;
	public string conditionName = "";
	public Misc.condition m_condition;
	private void OnEnable()
	{
		name = animName;
	}
	private void OnValidate()
	{
		name = animName;
	}
	static Misc.condition alwaysTrue = (CustomAnimatorContextInfo a) => true;
	public void InitializeCondition()
	{
		Debug.Log(conditionName);
		m_condition = conditionName is""? alwaysTrue:FindCondition();

	}
	Misc.condition FindCondition()
	{
		var a = (Misc.condition)typeof(CustomAnimator).GetMethod(conditionName).CreateDelegate(typeof(Misc.condition));
		return a;
	}
}
