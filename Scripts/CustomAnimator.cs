using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Events;

public class CustomAnimator : MonoBehaviour
{
	public string DefaultAnim;
	private CustomAnimation defaultAnim;
	public bool Reset;
	public float timeFromFrameStart;
	private int currentFrameIndex;
	private SpriteRenderer selfRender;
	[HideInInspector]public CustomAnimation CurrentAnim;
	public List<CustomAnimation> AllAnims = new List<CustomAnimation>();
	public List<Sprite> SpritesInOT;
	public int[][] SliceScheme;
	public List<int> FireAnimScheme;
	public List<int> FireUpAnimScheme;
	public List<int> FireDownAnimScheme;
	public List<int> IdleAnimScheme;
	public List<int> DefAnimScheme;
	public List<int> AttackAnimScheme;
	public List<int> JumpAnimScheme;
	public List<int> FlyUpAnimScheme;
	public List<int> FlyDownAnimScheme;
	public List<int> LandAnimScheme;
	public List<int> RunAnimScheme;
	public List<int> RollAnimScheme;
	public List<int> Attack2AnimScheme;
	public List<int> Attack3AnimScheme;
	public List<int> GunIdleAnimScheme;

#if UNITY_EDITOR
	[CustomEditor(typeof(CustomAnimator))]
	public class AnimatorEditor : Editor
	{
		CustomAnimator animator;
		private void OnEnable()
		{
			animator = (CustomAnimator)target;
			animator.SerializeAnimations();
			animator.AssignPriority();
		}
		public override void OnInspectorGUI()
		{
			if (animator.Reset)
			{
				animator.Reset = false;
				animator.SerializeAnimations();
				animator.AssignPriority();
			}
			if(animator.CurrentAnim!=null)EditorGUILayout.LabelField("CurrentAnimName",animator.CurrentAnim.name);
			else EditorGUILayout.LabelField("CurrentAnimName", "Null");
			EditorGUILayout.LabelField("CurrentFrameIndex", animator.currentFrameIndex.ToString());
			base.OnInspectorGUI();
		}
	}
#endif

	// Start is called before the first frame update
	void Start()
	{
		//SerializeAnimations();
		defaultAnim = AllAnims.Find(x => x.animName == DefaultAnim);
		CurrentAnim = defaultAnim;
		selfRender = GetComponent<SpriteRenderer>();
		animChanged.AddListener(OnStateChanged);
		AnimEnd.AddListener(OnAnimFinished);
		AssignPriority();
	}
	void SerializeAnimations()
	{
		SerializeAnimation(FireAnimScheme, "Fire");
		SerializeAnimation(FireUpAnimScheme, "FireUp");
		SerializeAnimation(FireDownAnimScheme, "FireDown");
		SerializeAnimation(IdleAnimScheme, "Idle", 8,true);
		SerializeAnimation(DefAnimScheme, "Def");
		SerializeAnimation(AttackAnimScheme, "Attack",12);
		SerializeAnimation(JumpAnimScheme, "Jump");
		SerializeAnimation(FlyUpAnimScheme, "FlyUp",8,true);
		SerializeAnimation(FlyDownAnimScheme, "FlyDown",8,true);
		SerializeAnimation(LandAnimScheme, "Land");
		SerializeAnimation(RunAnimScheme, "Run",12,true);
		SerializeAnimation(RollAnimScheme, "Roll", 12);
		SerializeAnimation(Attack2AnimScheme, "Attack2");
		SerializeAnimation(Attack3AnimScheme, "Attack3");
		SerializeAnimation(GunIdleAnimScheme, "GunIdle",8,true);
	}
	void AssignPriority()
	{
		int prior = 0;
		foreach(var anim in AllAnims)
		{
			anim.priority = prior;
			prior++;
		}
	}
	void SerializeAnimation(List<int> framesInOT, string name)
	{
		List<Sprite> frames = new List<Sprite>();
		foreach (var num in framesInOT)
		{
			frames.Add(SpritesInOT[num]);
		}

		var newAnim = ScriptableObject.CreateInstance<CustomAnimation>();
		newAnim.frames = frames;
		newAnim.animName = name;
		newAnim.speed = 16;
		newAnim.name=name;
		newAnim.repeatable = false;
		var pred = AllAnims.Find(x => x.name == name);
		if (pred == null)
		{
			AllAnims.Add(newAnim);
		}
		else
		{
			var ind = AllAnims.IndexOf(pred);
			AllAnims.Remove(pred);
			AllAnims.Insert(ind, newAnim);
		}
	}
	void SerializeAnimation(List<int> framesInOT, string name,int fps)
	{
		List<Sprite> frames = new List<Sprite>();
		foreach (var num in framesInOT)
		{
			frames.Add(SpritesInOT[num]);
		}

		var newAnim = ScriptableObject.CreateInstance(typeof(CustomAnimation));
		newAnim.name = name;
		var an = (CustomAnimation)newAnim;
		an.frames = frames;
		an.animName = name;
		an.speed = fps;
		an.repeatable = false;
		var pred = AllAnims.Find(x => x.name == name);
		if (pred == null)
		{
			AllAnims.Add(an);
		}
		else
		{
			var ind = AllAnims.IndexOf(pred);
			AllAnims.Remove(pred);
			AllAnims.Insert(ind, an);
		}
	}
	void SerializeAnimation(List<int> framesInOT, string name, int fps,bool repeatable)
	{
		List<Sprite> frames = new List<Sprite>();
		foreach (var num in framesInOT)
		{
			frames.Add(SpritesInOT[num]);
		}

		var newAnim = ScriptableObject.CreateInstance<CustomAnimation>();
		newAnim.frames = frames;
		newAnim.animName = name;
		newAnim.speed = fps;
		newAnim.name = name;
		newAnim.repeatable = repeatable;
		var pred = AllAnims.Find(x => x.name == name);
		if (pred == null)
		{
			AllAnims.Add(newAnim);
		}
		else
		{
			var ind = AllAnims.IndexOf(pred);
			AllAnims.Remove(pred);
			AllAnims.Insert(ind,newAnim);
		}
	}
	// Update is called once per frame
	void Update()
	{
		if (AllAnims.Count > 0) PlayCurrentAnim();
		if (runInEditMode)
		{
			//CurrentAnim = AllAnims[DefaultAnim];
		}
	}
	UnityEvent animChanged = new UnityEvent();
	public void PlayAnim(CustomAnimation anim)
	{
		if (anim == CurrentAnim) return;
		if (anim.priority < CurrentAnim.priority) return;
		StartCoroutine(WaitforEndOfAnimFrame(animChanged, anim,false));
	}
	public void PlayAnim(string animName)
	{

		var anim = AllAnims.Find(x => x.name == animName);
		if (anim == CurrentAnim) return;

		if (anim.priority < CurrentAnim.priority) return;
		StartCoroutine(WaitforEndOfAnimFrame(animChanged, anim,false));
	}
	public void PlayAnim(int animNum)
	{
		var anim = AllAnims[animNum];
		if (anim == CurrentAnim) return;

		if (anim.priority < CurrentAnim.priority) return;
		StartCoroutine(WaitforEndOfAnimFrame(animChanged, anim,false));
	}
	void OnStateChanged()
	{
		if (name == "SteelGreaves")
		{

		}
		currentFrameIndex = 0;
		selfRender.sprite = CurrentAnim.frames[0];
		print("StateChanged  "+name);
	}
	public void ReturnToDefaultAnim()
	{
		if (CurrentAnim == defaultAnim||(!CurrentAnim.repeatable&&currentFrameIndex<CurrentAnim.frames.Count-1)) return ;
		StartCoroutine(WaitforEndOfAnimFrame(animChanged, defaultAnim,true));
	}
	bool IfTimeZero()
	{
		return timeFromFrameStart == 0;
	}
	IEnumerator WaitforEndOfAnimFrame(UnityEvent todo,CustomAnimation startAnim,bool waitNextFrame)
	{
		//if (waitNextFrame) yield return new WaitWhile(IfTimeZero);
		if(timeFromFrameStart!=0)yield return new WaitUntil(IfTimeZero);
		todo.Invoke();
		CurrentAnim = startAnim;
	}
	AnimContextEvent AnimEnd = new AnimContextEvent();
	void OnAnimFinished(CustomAnimation finishedAnim)
	{
		if (!finishedAnim.repeatable)
		{
			ReturnToDefaultAnim();
		}
		else
		{
			currentFrameIndex = 0;
		}
		if(finishedAnim.name == "Roll")
		{
			PlayAnim("Idle"); 
		}
	}
	void PlayCurrentAnim()
	{
		timeFromFrameStart += Time.deltaTime;
		if (timeFromFrameStart >= 1 / CurrentAnim.speed)
		{
			currentFrameIndex++;
			timeFromFrameStart = 0;
			if (currentFrameIndex >= CurrentAnim.frames.Count)
			{
				AnimEnd.Invoke(CurrentAnim);
			}
			selfRender.sprite = CurrentAnim.frames[currentFrameIndex];
		}
	}
}

public class AnimContextEvent : UnityEngine.Events.UnityEvent<CustomAnimation> { }
