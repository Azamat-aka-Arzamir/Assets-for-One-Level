using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAnimHolder : MonoBehaviour
{
	public List<CustomAnimator> Animators;
	public Movement Invoker;
	MovementContext context;
	public string aadf;
	string[] cum = new string[] { "FlyUp","FlyDown","Jump","Land","Run","Idle" };
	public void PlayAnim(string Name)
	{
		GetContext();

		if (Name == "Idle" && context.ShieldUp)
		{
			Name = "Def";
		}
		if (Name == "Fire")
		{
			switch (context.DirY)
			{
				case 1:
					Name = "FireUp";
					break;
				case -1:
					Name = "FireDown";
					break;
				case 0:
					Name = "Fire";
					break;
			}
		}

		foreach (var a in Animators)
		{
			a.PlayAnim(Name);
		}
	}
	void GetContext()
	{
		context = new MovementContext(Invoker);
	}
}
