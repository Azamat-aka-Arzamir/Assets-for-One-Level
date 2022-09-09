using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SimpleAnimHolder : MonoBehaviour
{
#if UNITY_EDITOR
	[CustomEditor(typeof(SimpleAnimHolder))]
	public class SAHEditor : Editor
	{
		SimpleAnimHolder self;
		int[] numbers;
		string[] names;
		int activeAnim;
		int activeFrame;
		public override void OnInspectorGUI()
		{
			activeAnim = EditorGUILayout.IntPopup("Inspected Animation",activeAnim, names, numbers);
			activeFrame = EditorGUILayout.IntSlider(activeFrame, 0, self.Animators[0].AllAnims[activeAnim].frames.Count-1);
			if(!Application.IsPlaying(self))self.ChangeSprite(self.Animators[0].AllAnims[activeAnim].animName, activeFrame,self.side);
			base.OnInspectorGUI();

		}
		private void OnEnable()
		{
			self = (SimpleAnimHolder)target;
			numbers = new int[self.Animators[0].AllAnims.Count];
			names = new string[self.Animators[0].AllAnims.Count];
			if (self.Animators[0].AllAnims.Count != 0)
			{
				for (int i = 0; i < numbers.Length; i++)
				{
					numbers[i] = i;
					names[i] = self.Animators[0].AllAnims[i].animName;
				}
			}
		}
	}

#endif
	public Misc.Side side = Misc.Side.R;
	public List<CustomAnimator> Animators;
	public Movement Invoker;
	public string anim;
	public int frame;
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
	public void ChangeSprite(string animName,int frame,Misc.Side _side)
	{
		foreach(var animator in Animators)
		{
			if(_side==Misc.Side.R)animator.GetComponent<SpriteRenderer>().sprite = animator.AllAnims.Find(x => x.animName == animName).frames[frame];
			else animator.GetComponent<SpriteRenderer>().sprite = animator.AllAnimsL.Find(x => x.animName == animName).frames[frame];
		}
	}
}
