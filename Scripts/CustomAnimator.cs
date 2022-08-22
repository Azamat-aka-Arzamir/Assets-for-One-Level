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
	public bool AssignNewPosToEveryFrame;
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
	[SerializeField] List<CustomAnimation> PlayingQueue = new List<CustomAnimation>();
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
			value = new Vector3();
		}
		Vector3 value = new Vector3();
		public override void OnInspectorGUI()
		{
			if (animator.Reset)
			{
				animator.Reset = false;
				animator.SerializeAnimations();
				animator.AssignPriority();
			}
			if (animator.AssignNewPosToEveryFrame)
			{
				
				bool g = false;
				value = EditorGUILayout.Vector3Field("New Position", value);
				g = EditorGUILayout.Toggle("Assign", g);
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				if (g)
				{
					
					animator.AssignNewPosToEveryFrame = false;
					foreach(var anim in animator.AllAnims)
					{
						foreach(var frame in anim.frames)
						{
							print(value);
							frame.position = value;
						}
					}
					g = false;
				}
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
	static Misc.condition alwaysTrue = (CustomAnimatorContextInfo a) => true;
	public static bool Cond(CustomAnimatorContextInfo a)
	{
		if (a.currentStateName == "DefStatic" || a.currentStateName == "Def") return true;
		else return false;
	}
	public static bool DefCond(CustomAnimatorContextInfo a)
	{
		a.animator.PlayingQueue.RemoveAll(x => x.animName == "DefReverse");
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
	#region Initialization
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
		SerializeAnimation(FireAnimScheme, "Fire", 8, false, new string[] { "GunIdle" });
		SerializeAnimation(FireUpAnimScheme, "FireUp", 8, false, new string[] { "GunIdle", "AimUp" });
		SerializeAnimation(FireDownAnimScheme, "FireDown", 8, false, new string[] { "GunIdle", "AimDown" });
		SerializeAnimation(IdleAnimScheme, "Idle", 8, true);
		SerializeAnimation(DefAnimScheme, "Def", 8, false, new string[0], false, DefCond, false);
		SerializeAnimation(new List<int>() { DefAnimScheme[1] }, "DefStatic", 8, true);
		SerializeAnimation(DefAnimScheme, "DefReverse", 6, false, new string[0], true, Cond, true);
		SerializeAnimation(AttackAnimScheme, "Attack", 12, false, new string[0], true, "Attack");
		SerializeAnimation(JumpAnimScheme, "Jump");
		SerializeAnimation(FlyUpAnimScheme, "FlyUp", 8, true);
		SerializeAnimation(FlyDownAnimScheme, "FlyDown", 8, true, new string[] { "Land" });
		SerializeAnimation(LandAnimScheme, "Land", 8, false, new string[] { "Idle", "Def", "GunIdle" });
		SerializeAnimation(RunAnimScheme, "Run", 12, true, new string[] { "Idle", "Def", "GunIdle" }, false, "Run");
		SerializeAnimation(RollAnimScheme, "Roll", 12, false, null, false, "Roll");
		SerializeAnimation(Attack2AnimScheme, "Attack2", 12, false, new string[0], true, "Attack");
		SerializeAnimation(Attack3AnimScheme, "Attack3", 12, false, new string[] { "Attack" }, true, "Attack");
		SerializeAnimation(GunIdleAnimScheme, "GunIdle", 8, true);
		SerializeAnimation(FireUpAnimScheme, "AimUp", 8, true, new string[] { "GunIdle", "AimDown" });
		SerializeAnimation(FireDownAnimScheme, "AimDown", 8, true, new string[] { "GunIdle", "AimUp" });

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
	void AddAnimation(CustomAnimation pred, CustomAnimation newAnim)
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
			if (pred.transitionsTo != null && (newAnim.transitionsTo == null || pred.transitionsTo.Length > newAnim.transitionsTo.Length))
			{
				newAnim.transitionsTo = pred.transitionsTo;
			}
			if (pred.doNotTransitTo != null && (newAnim.doNotTransitTo == null || pred.doNotTransitTo.Length > newAnim.doNotTransitTo.Length))
			{
				newAnim.doNotTransitTo = pred.doNotTransitTo;
			}
			if (pred.tag != "") newAnim.tag = pred.tag;
			newAnim.interruptable = pred.interruptable;
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
	void SerializeAnimation(List<int> framesInOT, string name, int fps, bool repeatable, string[] transitionsTo, bool saveImp, string tag)
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
	#endregion
	// Update is called once per frame
	void Update()
	{
		if (AllAnims.Count > 0) PlayCurrentAnim();
	}
	AnimContextEvent animChanged = new AnimContextEvent();
	public void PlayAnim(CustomAnimation anim)
	{
		if (!PlayingQueue.Exists(x => x == anim)) PlayingQueue.Add(anim);
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
		currentFrameIndex = 0;
		selfRender.sprite = newAnim.frames[0];
		transform.localPosition= newAnim.frames[0].position;
		transform.localRotation=Quaternion.Euler(0,0,newAnim.frames[0].rotation);
		CurrentAnim = newAnim;
		if (MotherAnimator != null && SynchronizeWithMA.Exists(x => x == newAnim.animName) && MotherAnimator.CurrentAnim.tag == newAnim.tag)
		{
			CurrentAnim = AllAnims.Find(x => x.animName == MotherAnimator.CurrentAnim.name);
			if (MotherAnimator.CurrentAnim.frames.Count == CurrentAnim.frames.Count)
			{
				currentFrameIndex = MotherAnimator.currentFrameIndex;
				timeFromFrameStart = MotherAnimator.timeFromFrameStart;
				var _frame = CurrentAnim.frames[MotherAnimator.currentFrameIndex];
				selfRender.sprite = _frame;
				transform.localPosition =_frame.position;
				transform.localRotation=Quaternion.Euler(0, 0, _frame.rotation);
			}
		}
	}
	AnimContextEvent AnimEnd = new AnimContextEvent();
	string[] NotReturnToDefault = new string[] { "Attack", "Attack2", "Attack3", "Def", "Fire", "FireDown", "FireUp", "AimUp", "AimDown" };

	void OnAnimFinished(CustomAnimation finishedAnim)
	{
		if (finishedAnim.repeatable && !System.Array.Exists(NotReturnToDefault, x => x == finishedAnim.animName))
		{
			PlayAnim(finishedAnim);
			return;
		}
		if (finishedAnim.name == "Jump")
		{
			PlayAnim("FlyUp");
			return;
		}
		if (finishedAnim.name == "Fire")
		{
			PlayAnim("GunIdle");
			return;
		}
		if (finishedAnim.name == "FireUp")
		{
			PlayAnim("AimUp");
			return;
		}
		if (finishedAnim.name == "FireDown")
		{
			PlayAnim("AimDown");
			return;
		}
		if (finishedAnim.name == "AimDown")
		{
			PlayAnim("GunIdle");
			return;
		}
		if (finishedAnim.name == "AimUp")
		{
			print("Cock");
			PlayAnim("GunIdle");
			return;
		}
		if (finishedAnim.animName == "Def")
		{
			if (!PlayingQueue.Exists(x => x.animName == "DefReverse"))
			{
				PlayAnim("DefStatic");
				return;
			}
			else
			{
				PlayAnim("DefReverse");
				return;
			}
		}
		if (finishedAnim.name == "Roll")
		{
			PlayAnim("Idle");
			return;
		}
		if (finishedAnim.animName == "Attack")
		{
			if (PlayingQueue.Exists(x => x.animName == "Attack"))
			{
				PlayAnim("Attack2");
				return;
			}
		}
		if (finishedAnim.animName == "Attack2")
		{
			if (PlayingQueue.Exists(x => x.animName == "Attack"))
			{
				PlayAnim("Attack3");
				return;
			}
		}
		if (finishedAnim.animName == "Attack3")
		{
			if (PlayingQueue.Exists(x => x.animName == "Attack"))
			{
				PlayAnim("Attack");
				return;
			}
		}

	}
	public UnityEvent newFrame = new UnityEvent();
	void PlayCurrentAnim()
	{
		timeFromFrameStart += Time.deltaTime;
		if (timeFromFrameStart >= 1 / CurrentAnim.speed)
		{
			timeFromFrameStart = 0;
			NewFrame();
		}
	}
	void NewFrame()
	{
		bool finished = false;
		if (currentFrameIndex < CurrentAnim.frames.Count - 1)
		{
			if (!CurrentAnim.repeatable&&!CurrentAnim.interruptable)
			{
				//Animation is not finished, cannot be interrupted and must be continued
				ContinueAnimation();
				return;
			}
		}
		else
		{
			AnimEnd.Invoke(CurrentAnim);
			finished = true;
			if(!CurrentAnim.repeatable)PlayingQueue.RemoveAll(x => x == CurrentAnim);
			//Animation finished
		}
		//Animation can be interrupted or has already finished
		if (name == "SteelGreaves" && CurrentAnim.animName == "FlyDown" && FindMostPrioritizedAnim(!finished).animName == "Idle")
		{

		}
		var nextState = defaultAnim;
		nextState = FindMostPrioritizedAnim(!finished);

		if (nextState!=CurrentAnim||(CurrentAnim.repeatable&&finished))
		{
			animChanged.Invoke(nextState);
		}
		else if (nextState == CurrentAnim &&!CurrentAnim.repeatable&& finished)
		{
			print("Oops... There's someone's shit in ur pants. Clean it and I'll clean playing queue");
			animChanged.Invoke(defaultAnim);
		}
		else
		{
			ContinueAnimation();
		}
		PlayingQueue.Clear();
		newFrame.Invoke();
	}
	void ContinueAnimation()
	{
		currentFrameIndex++;
		var _frame = CurrentAnim.frames[currentFrameIndex];
		selfRender.sprite = _frame;
		transform.localPosition = _frame.position;
		transform.localRotation = Quaternion.Euler(0, 0, _frame.rotation);
	}
	CustomAnimation FindMostPrioritizedAnim(bool CountSelf)
	{
		if (PlayingQueue.Count == 0)
		{
			return CountSelf is false ? defaultAnim : CurrentAnim;
		}
		var mostPrioritizedAnim = defaultAnim;
		if (CountSelf) mostPrioritizedAnim = CurrentAnim;
		foreach(var anim in PlayingQueue)
		{
			if (!System.Array.Exists(CurrentAnim.doNotTransitTo, x => x == anim.animName))
			{
				if (anim.priority > mostPrioritizedAnim.priority || (System.Array.Exists(CurrentAnim.transitionsTo, x => x == anim.animName) && mostPrioritizedAnim == CurrentAnim && CountSelf))
				{
					if (anim.m_condition == null || anim.m_condition(new CustomAnimatorContextInfo(this)))
					{
						mostPrioritizedAnim = anim;
					}
				}
			}
		}
		return mostPrioritizedAnim;
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
	}
}
