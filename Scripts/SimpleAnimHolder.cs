using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAnimHolder : MonoBehaviour
{
	Misc.Side side = Misc.Side.R;
	public List<CustomAnimator> Animators;
	public Movement Invoker;
	MovementContext context;
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
	public void Turn()
	{
		side = side is Misc.Side.R ? Misc.Side.L : Misc.Side.R;
		foreach(var a in Animators)
		{
			a.ChangeSide();
		}
	}
	void GetContext()
	{
		context = new MovementContext(Invoker);
	}
}
