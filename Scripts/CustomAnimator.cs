using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Events;

public class CustomAnimator : MonoBehaviour
{
	[Tooltip("If animaions between mother and child are equal, child get frame index of mother's animation")]
	public CustomAnimator MotherAnimator;
	public string DefaultAnim;
	private CustomAnimation defaultAnim;
	public bool Reset;
	public bool AddNewAnim;
	public float timeFromFrameStart;
	private int currentFrameIndex;
	public int CurrentFrameIndex
	{
		get
		{
			return currentFrameIndex;
		}
	}
	private SpriteRenderer selfRender;
	[HideInInspector] public CustomAnimation CurrentAnim;
	public List<CustomAnimation> AllAnims = new List<CustomAnimation>();
	public List<string> SynchronizeWithMA;
	public List<Sprite> SpritesInOT;
	[SerializeField] List<string> Impulses = new List<string>();
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
			//animator.SerializeAnimations();
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
			if (animator.AddNewAnim)
			{
				animator.AddNewAnim = false;
				animator.SerializeAnimation(null, "New Anim", 0, false, null);
			}
			if (animator.CurrentAnim != null) EditorGUILayout.LabelField("CurrentAnimName", animator.CurrentAnim.name);
			else EditorGUILayout.LabelField("CurrentAnimName", "Null");
			EditorGUILayout.LabelField("CurrentFrameIndex", animator.currentFrameIndex.ToString());
			base.OnInspectorGUI();
		}
	}
#endif
	public static Misc.condition alwaysTrue = (CustomAnimatorContextInfo a) => true;
	public static bool cond(CustomAnimatorContextInfo a)
	{
		if (a.currentStateName == "DefStatic" || a.currentStateName == "Def") return true;
		else return false;
	}
	public static bool DefCond(CustomAnimatorContextInfo a)
	{
		a.animator.Impulses.RemoveAll(x => x == "DefReverse");
		return true;
	}
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
		InitializeConditions();
		//print(AllAnims.Find(x => x.animName == "Def").m_condition.Method);
	}
	void InitializeConditions()
	{
		foreach (var anim in AllAnims)
		{
			if (anim.conditionName != "")
			{
				anim.InitializeCondition();
			}
		}
	}
	void SerializeAnimations()
	{
		SerializeAnimation(FireAnimScheme, "Fire");
		SerializeAnimation(FireUpAnimScheme, "FireUp");
		SerializeAnimation(FireDownAnimScheme, "FireDown");
		SerializeAnimation(IdleAnimScheme, "Idle", 8, true);
		SerializeAnimation(DefAnimScheme, "Def", 8, false, new string[0], false, DefCond, false);
		SerializeAnimation(new List<int>() { DefAnimScheme[1] }, "DefStatic", 8, true);
		SerializeAnimation(DefAnimScheme, "DefReverse", 6, false, new string[0], true, cond, true);
		SerializeAnimation(AttackAnimScheme, "Attack", 12, false, new string[0], true,"Attack");
		SerializeAnimation(JumpAnimScheme, "Jump");
		SerializeAnimation(FlyUpAnimScheme, "FlyUp", 8, true);
		SerializeAnimation(FlyDownAnimScheme, "FlyDown", 8, true, new string[] { "Land" });
		SerializeAnimation(LandAnimScheme, "Land", 8, false, new string[] { "Idle", "Def" });
		SerializeAnimation(RunAnimScheme, "Run", 12, true, new string[] { "Idle", "Def" },false,"Run");
		SerializeAnimation(RollAnimScheme, "Roll", 12,false,null,false,"Roll");
		SerializeAnimation(Attack2AnimScheme, "Attack2", 12, false, new string[0], true,"Attack");
		SerializeAnimation(Attack3AnimScheme, "Attack3", 12, false, new string[] { "Attack" }, true,"Attack");
		SerializeAnimation(GunIdleAnimScheme, "GunIdle", 8, true);
	}

	void AssignPriority()
	{
		int prior = 0;
		foreach (var anim in AllAnims)
		{
			anim.priority = prior;
			prior++;
		}
	}
	void AddAnimation(CustomAnimation pred,CustomAnimation newAnim)
	{
		if (pred == null)
		{
			AllAnims.Add(newAnim);
		}
		else
		{
			var ind = AllAnims.IndexOf(pred);
			for (int i = 0; i < newAnim.frames.Count - 1; i++)
			{
				if (i > pred.frames.Count - 1) break;
				newAnim.frames[i].point = pred.frames[i].point;
				newAnim.frames[i].position = pred.frames[i].position;
				newAnim.frames[i].PhysicsShape = pred.frames[i].PhysicsShape;
			}
			if (pred.tag != "") newAnim.tag = pred.tag;
			AllAnims.Remove(pred);
			AllAnims.Insert(ind, newAnim);
		}
	}
	void SerializeAnimation(List<int> framesInOT, string name)
	{
		List<CustomFrame> frames = new List<CustomFrame>();
		if (framesInOT != null)
		{
			foreach (var num in framesInOT)
			{
				CustomFrame a = ScriptableObject.CreateInstance<CustomFrame>();
				a.sprite = SpritesInOT[num];
				frames.Add(a);
			}
		}
		var newAnim = ScriptableObject.CreateInstance<CustomAnimation>();
		newAnim.frames = frames;
		newAnim.animName = name;
		newAnim.speed = 8;
		newAnim.name = name;
		newAnim.repeatable = false;
		newAnim.m_condition = alwaysTrue;
		var pred = AllAnims.Find(x => x.name == name);
		AddAnimation(pred, newAnim);
	}
	void SerializeAnimation(List<int> framesInOT, string name, int fps)
	{
		List<CustomFrame> frames = new List<CustomFrame>();
		if (framesInOT != null)
		{
			foreach (var num in framesInOT)
			{
				CustomFrame a = ScriptableObject.CreateInstance<CustomFrame>();
				a.sprite = SpritesInOT[num];
				frames.Add(a);
			}
		}
		var newAnim = ScriptableObject.CreateInstance(typeof(CustomAnimation));
		newAnim.name = name;
		var an = (CustomAnimation)newAnim;
		an.frames = frames;
		an.animName = name;
		an.speed = fps;
		an.repeatable = false;
		an.m_condition = alwaysTrue;
		var pred = AllAnims.Find(x => x.name == name);
		AddAnimation(pred, an);
	}
	void SerializeAnimation(List<int> framesInOT, string name, int fps, bool repeatable)
	{
		List<CustomFrame> frames = new List<CustomFrame>();
		if (framesInOT != null)
		{
			foreach (var num in framesInOT)
			{
				CustomFrame a = ScriptableObject.CreateInstance<CustomFrame>();
				a.sprite = SpritesInOT[num];
				frames.Add(a);
			}
		}
		var newAnim = ScriptableObject.CreateInstance<CustomAnimation>();
		newAnim.frames = frames;
		newAnim.animName = name;
		newAnim.speed = fps;
		newAnim.name = name;
		newAnim.repeatable = repeatable;
		newAnim.m_condition = alwaysTrue;
		var pred = AllAnims.Find(x => x.name == name);
		AddAnimation(pred, newAnim);
	}
	void SerializeAnimation(List<int> framesInOT, string name, int fps, bool repeatable, string[] transitionsTo)
	{
		List<CustomFrame> frames = new List<CustomFrame>();
		if (framesInOT != null)
		{
			foreach (var num in framesInOT)
			{
				CustomFrame a = ScriptableObject.CreateInstance<CustomFrame>();
				a.sprite = SpritesInOT[num];
				frames.Add(a);
			}
		}

		var newAnim = ScriptableObject.CreateInstance<CustomAnimation>();
		newAnim.frames = frames;
		newAnim.animName = name;
		newAnim.speed = fps;
		newAnim.name = name;
		newAnim.repeatable = repeatable;
		newAnim.transitionsTo = transitionsTo;
		newAnim.m_condition = alwaysTrue;
		var pred = AllAnims.Find(x => x.name == name);
		AddAnimation(pred, newAnim);
	}
	void SerializeAnimation(List<int> framesInOT, string name, int fps, bool repeatable, string[] transitionsTo, bool saveImp)
	{
		List<CustomFrame> frames = new List<CustomFrame>();
		if (framesInOT != null)
		{
			foreach (var num in framesInOT)
			{
				CustomFrame a = ScriptableObject.CreateInstance<CustomFrame>();
				a.sprite = SpritesInOT[num];
				frames.Add(a);
			}
		}
		var newAnim = ScriptableObject.CreateInstance<CustomAnimation>();
		newAnim.frames = frames;
		newAnim.animName = name;
		newAnim.speed = fps;
		newAnim.name = name;
		newAnim.repeatable = repeatable;
		newAnim.transitionsTo = transitionsTo;
		newAnim.saveImpulse = saveImp;
		newAnim.m_condition = alwaysTrue;
		var pred = AllAnims.Find(x => x.name == name);
		AddAnimation(pred, newAnim);
	}
	void SerializeAnimation(List<int> framesInOT, string name, int fps, bool repeatable, string[] transitionsTo, bool saveImp,string tag)
	{
		List<CustomFrame> frames = new List<CustomFrame>();
		if (framesInOT != null)
		{
			foreach (var num in framesInOT)
			{
				CustomFrame a = ScriptableObject.CreateInstance<CustomFrame>();
				a.sprite = SpritesInOT[num];
				frames.Add(a);
			}
		}
		var newAnim = ScriptableObject.CreateInstance<CustomAnimation>();
		newAnim.frames = frames;
		newAnim.animName = name;
		newAnim.speed = fps;
		newAnim.name = name;
		newAnim.repeatable = repeatable;
		newAnim.transitionsTo = transitionsTo;
		newAnim.saveImpulse = saveImp;
		newAnim.m_condition = alwaysTrue;
		newAnim.tag = tag;
		var pred = AllAnims.Find(x => x.name == name);
		AddAnimation(pred, newAnim);
	}
	void SerializeAnimation(List<int> framesInOT, string name, int fps, bool repeatable, string[] transitionsTo, bool saveImp, Misc.condition condition, bool reversed)
	{
		List<CustomFrame> frames = new List<CustomFrame>();
		if (framesInOT != null)
		{
			foreach (var num in framesInOT)
			{
				CustomFrame a = ScriptableObject.CreateInstance<CustomFrame>();
				a.sprite = SpritesInOT[num];
				frames.Add(a);
			}
		}
		var newAnim = ScriptableObject.CreateInstance<CustomAnimation>();
		newAnim.frames = frames;
		newAnim.animName = name;
		newAnim.speed = fps;
		newAnim.name = name;
		newAnim.repeatable = repeatable;
		newAnim.transitionsTo = transitionsTo;
		newAnim.saveImpulse = saveImp;
		newAnim.m_condition = condition;
		newAnim.conditionName = condition.Method.Name;
		var pred = AllAnims.Find(x => x.name == name);
		AddAnimation(pred, newAnim);
	}

	// Update is called once per frame
	void Update()
	{

		if (AllAnims.Count > 0) PlayCurrentAnim();
		//if (CurrentAnim.animName == "Idle") Impulses.Clear();
	}
	AnimContextEvent animChanged = new AnimContextEvent();
	public void PlayAnim(CustomAnimation anim)
	{
		if (anim.saveImpulse) Impulses.Add(anim.animName);
		if (anim.name == CurrentAnim.animName)
		{
			return;
		}
		if (anim.m_condition != null && !anim.m_condition(new CustomAnimatorContextInfo(this))) return;
		//MostCurrentAnim = anim;

		if (anim.priority < CurrentAnim.priority)
		{
			if (CurrentAnim.transitionsTo == null) return;
			if (!System.Array.Exists(CurrentAnim.transitionsTo, x => x == anim.name))
			{
				return;
			}
			if (!CurrentAnim.repeatable)
			{
				if (currentFrameIndex < CurrentAnim.frames.Count - 1)
				{
					return;
				}
			}

		}
		StopAllCoroutines();
		StartCoroutine(WaitforEndOfAnimFrame(animChanged, anim, false));
	}
	public void PlayAnim(string animName)
	{
		var anim = AllAnims.Find(x => x.animName == animName);
		PlayAnim(anim);
	}
	public void PlayAnim(int animNum)
	{
		var anim = AllAnims[animNum];
		PlayAnim(anim);
	}
	void OnStateChanged(CustomAnimation newAnim)
	{
		if (Impulses.Exists(x => x == newAnim.animName))
		{
			Impulses.RemoveAll(x => x == newAnim.animName);
		}
		currentFrameIndex = 0;
		//print(CurrentAnim.name + newAnim.name);
		selfRender.sprite = newAnim.frames[0];
		newFrame.Invoke();
		if (MotherAnimator.name == "SteelCuirass" && newAnim.animName.Contains("Attack"))
		{

		}
		if (MotherAnimator != null &&SynchronizeWithMA.Exists(x=>x==newAnim.animName) &&MotherAnimator.CurrentAnim.tag == newAnim.tag)
		{
			CurrentAnim = AllAnims.Find(x=>x.animName==MotherAnimator.CurrentAnim.name);
			if (MotherAnimator.CurrentAnim.frames.Count == CurrentAnim.frames.Count)
			{
				currentFrameIndex = MotherAnimator.currentFrameIndex;
				timeFromFrameStart = MotherAnimator.timeFromFrameStart;
				selfRender.sprite = CurrentAnim.frames[MotherAnimator.currentFrameIndex];
				newFrame.Invoke();
			}
		}


		//print("StateChanged  "+name);
	}
	public void ReturnToDefaultAnim()
	{
		//Impulses.Clear();
		if (CurrentAnim == defaultAnim)
		{
			return;
		}
		if (!CurrentAnim.repeatable && currentFrameIndex < CurrentAnim.frames.Count - 1 && timeFromFrameStart < 1 / CurrentAnim.speed)
		{
			return;
		}
		StartCoroutine(WaitforEndOfAnimFrame(animChanged, defaultAnim, true));
	}
	bool IfTimeZero()
	{
		return timeFromFrameStart == 0;
	}
	IEnumerator WaitforEndOfAnimFrame(AnimContextEvent todo, CustomAnimation startAnim, bool waitNextFrame)
	{
		//if (waitNextFrame) yield return new WaitWhile(IfTimeNotZero);
		if (timeFromFrameStart != 0) yield return new WaitUntil(IfTimeZero);
		todo.Invoke(startAnim);
		CurrentAnim = startAnim;
	}
	AnimContextEvent AnimEnd = new AnimContextEvent();
	string[] NotReturnToDefault = new string[] { "Attack", "Attack2", "Attack3", "Def" };

	void OnAnimFinished(CustomAnimation finishedAnim)
	{
		//var a = System.Array.Exists(NotReturnToDefault, x => x == finishedAnim.animName);
		if (!finishedAnim.repeatable && !System.Array.Exists(NotReturnToDefault, x => x == finishedAnim.animName))
		{
			ReturnToDefaultAnim();
		}
		if (finishedAnim.repeatable)
		{
			currentFrameIndex = 0;
		}
		if (finishedAnim.name == "Jump")
		{
			PlayAnim("FlyUp");
		}
		if (finishedAnim.animName == "Def")
		{
			if (!Impulses.Exists(x => x == "DefReverse"))
			{
				PlayAnim("DefStatic");
			}
			else
			{
				PlayAnim("DefReverse");
			}
		}
		if (finishedAnim.name == "Roll")
		{
			PlayAnim("Idle");
		}
		if (finishedAnim.animName == "Attack")
		{
			if (Impulses.Exists(x => x == "Attack"))
			{
				PlayAnim("Attack2");
				Impulses.RemoveAll(z => z == "Attack");
			}
			else
			{
				ReturnToDefaultAnim();
			}
		}
		if (finishedAnim.animName == "Attack2")
		{
			if (Impulses.Exists(x => x == "Attack"))
			{
				PlayAnim("Attack3");
				Impulses.RemoveAll(z => z == "Attack");
			}
			else
			{
				ReturnToDefaultAnim();
			}
		}
		if (finishedAnim.animName == "Attack3")
		{
			if (Impulses.Exists(x => x == "Attack"))
			{
				PlayAnim("Attack");
				Impulses.RemoveAll(z => z == "Attack");
			}
			else
			{
				ReturnToDefaultAnim();
			}
		}

	}
	public UnityEvent newFrame = new UnityEvent();
	void PlayCurrentAnim()
	{
		/*if (MotherAnimator != null && MotherAnimator.CurrentAnim.animName == CurrentAnim.animName && MotherAnimator.CurrentAnim.frames.Count == CurrentAnim.frames.Count && MotherAnimator.CurrentAnim.priority > CurrentAnim.priority)
		{


			currentFrameIndex = MotherAnimator.currentFrameIndex;
			timeFromFrameStart = MotherAnimator.timeFromFrameStart;
			selfRender.sprite = CurrentAnim.frames[MotherAnimator.currentFrameIndex];

		}*/
		//else
		{
			timeFromFrameStart += Time.deltaTime;
		}
		if (timeFromFrameStart >= 1 / CurrentAnim.speed)
		{
			currentFrameIndex++;
			timeFromFrameStart = 0;
			if (currentFrameIndex >= CurrentAnim.frames.Count)
			{
				AnimEnd.Invoke(CurrentAnim);
			}
			var frame = CurrentAnim.frames[currentFrameIndex];
			selfRender.sprite = frame;
			newFrame.Invoke();
			if (frame.position != Vector3.zero)
			{
				transform.position = frame.position;
			}
		}


	}

}

public class AnimContextEvent : UnityEngine.Events.UnityEvent<CustomAnimation> { }
public class CustomAnimatorContextInfo
{
	public CustomAnimator animator;
	int cfi;
	public int currentFrameIndex
	{
		get
		{
			return cfi;
		}
	}
	string csn;
	public string currentStateName
	{
		get
		{
			return csn;
		}
	}
	float tffs;
	public float timeFromFrameStart
	{
		get
		{
			return tffs;
		}
	}
	string[] attributes;
	public string[] currentAnimationAttributes
	{
		get
		{
			return attributes;
		}
	}
	public CustomAnimatorContextInfo(CustomAnimator customAnimator)
	{
		animator = customAnimator;
		cfi = customAnimator.CurrentFrameIndex;
		csn = customAnimator.CurrentAnim.animName;
		tffs = customAnimator.timeFromFrameStart;
		//attributes = customAnimator.CurrentAnim.attributes;
	}
}
