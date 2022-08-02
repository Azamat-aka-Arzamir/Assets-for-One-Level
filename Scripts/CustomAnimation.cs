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
	public string[] transitionsTo;
	public DelegateHolder dholder = new DelegateHolder();
	public string conditionName;
	public Misc.condition m_condition
	{
		get
		{
			return dholder.condition;

		}
		set
		{
			dholder.condition = value;
		}
	}
	private void OnEnable()
	{
		name = animName;
	}
	private void OnValidate()
	{
		name = animName;
	}
	public void InitializeCondition()
	{
		m_condition = (Misc.condition)typeof(CustomAnimator).GetMethod(conditionName).CreateDelegate(typeof(Misc.condition));
	}
}
public class DelegateHolder
{
	public Misc.condition condition;
	public DelegateHolder()
	{
		condition = null;
	}
}