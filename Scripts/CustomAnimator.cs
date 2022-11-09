using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Events;

public class CustomAnimator : MonoBehaviour
{
	public int refInt;
	[SerializeField] Misc.Side m_side = Misc.Side.R;
	[HideInInspector]
	public Misc.Side side
	{
		get
		{
			return m_side;
		}
	}
	[Tooltip("If animaions between mother and child are equal, child get frame index of mother's animation")]
	public CustomAnimator MotherAnimator;
	public string DefaultAnim;
	private CustomAnimation defaultAnim;
	public bool Reset;
	public bool AddNewAnim;
	public bool AssignNewPosToEveryFrame;
	public float timeFromFrameStart;
	private int currentFrameIndex;
	private Sprite blank;
	private CustomFrame m_currentFrame;
	public CustomFrame currentFrame
	{
		get
		{
			return m_currentFrame;
		}
	}
	public int CurrentFrameIndex
	{
		get
		{
			return currentFrameIndex;
		}
	}
	private SpriteRenderer selfRender;
	private UnityEngine.UI.Image selfImage;
	[HideInInspector] public CustomAnimation CurrentAnim;
	[SerializeField] public List<CustomAnimation> AllAnims = new List<CustomAnimation>();
	[SerializeField] public List<CustomAnimation> AllAnimsL = new List<CustomAnimation>();
	public List<string> SynchronizeWithMA;
	public List<Sprite> SpritesInOT;
	public List<Sprite> SpritesInOTForOtherSide;
	[SerializeField] List<CustomAnimation> PlayingQueue = new List<CustomAnimation>();
	List<int> WaitingTimer = new List<int>();
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
		CustomFrame frame;
		bool invert = true;
		private void OnEnable()
		{
			animator = (CustomAnimator)target;
			//animator.SerializeAnimations();
			animator.AssignPriority();
			value = new Vector3();
		}
		private void OnDisable()
		{
			
		}
		Vector3 value = new Vector3();
		public override void OnInspectorGUI()
		{
			if (animator.Reset)
			{
				animator.Reset = false;
				animator.SerializeAnimations();
				animator.AssignPriority();
				animator.AssignPriorityForOtherSide();
			}
			if (animator.AssignNewPosToEveryFrame)
			{

				bool g = false;
				value = EditorGUILayout.Vector3Field("New Position", value);
				invert = EditorGUILayout.Toggle("Invert for left side", invert);
				g = EditorGUILayout.Toggle("Assign", g);
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				if (g)
				{

					animator.AssignNewPosToEveryFrame = false;
					foreach (var anim in animator.AllAnims)
					{
						foreach (var frame in anim.frames)
						{
							var x = value.x;
							var y = value.y;
							var z=value.z;
							print(value);
							if(x == 0)
							{
								x = frame.position.x;
							}
							if (y == 0)
							{
								y = frame.position.y;
							}
							if (z == 0)
							{
								z = frame.position.z;
							}
							frame.position = new Vector3(x,y,z);
						}
					}
					foreach (var anim in animator.AllAnimsL)
					{
						foreach (var frame in anim.frames)
						{
							print(value);
							if (invert) frame.position = -value;
							else frame.position = value;
						}
					}
					g = false;
				}
			}
			if (animator.AddNewAnim)
			{
				animator.AddNewAnim = false;
				animator.SerializeAnimation(Misc.Side.R, null, "New Anim", 0, false, null);
			}
			if (animator.CurrentAnim != null) EditorGUILayout.LabelField("CurrentAnimName", animator.CurrentAnim.animName);
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
		else
		{
			a.animator.PlayingQueue.RemoveAll(x => x.animName == "DefReverse");
			return false;
		}

	}
	public static bool DefCond(CustomAnimatorContextInfo a)
	{
		a.animator.PlayingQueue.RemoveAll(x => x.animName == "DefReverse");
		return true;
	}
	public static bool OnGroundCond(CustomAnimatorContextInfo a)
	{
		if (!a.animator.GetComponentInParent<Movement>().IsOnGround) return true;
		else return false;
	}
	bool ui = false;
	// Start is called before the first frame update
	void Start()
	{
		blank = Resources.Load<Sprite>("Defaults/Blank");
		//SerializeAnimations();
		defaultAnim = AllAnims.Find(x => x.animName == DefaultAnim);
		CurrentAnim = defaultAnim;

		TryGetComponent<SpriteRenderer>(out selfRender);
		if (selfRender == null) { selfImage = GetComponent<UnityEngine.UI.Image>(); ui = true; }
		if (!ui) selfRender.sprite = CurrentAnim.frames[0]; else selfImage.sprite = CurrentAnim.frames[0];
		if (transform.parent != null && !ui) transform.localPosition = CurrentAnim.frames[0].position;
		transform.localRotation = Quaternion.Euler(0, 0, CurrentAnim.frames[0].rotation);
		animChanged.AddListener(OnStateChanged);
		//AnimEnd.AddListener(OnAnimFinished);
		AssignPriority();
		AssignPriorityForOtherSide();
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
		SerializeAnimation(Misc.Side.R, FireAnimScheme, "Fire", 8, false, new string[] { "GunIdle" });
		SerializeAnimation(Misc.Side.R, FireUpAnimScheme, "FireUp", 8, false, new string[] { "GunIdle", "AimUp" });
		SerializeAnimation(Misc.Side.R, FireDownAnimScheme, "FireDown", 8, false, new string[] { "GunIdle", "AimDown" });
		SerializeAnimation(Misc.Side.R, IdleAnimScheme, "Idle", 8, true);
		SerializeAnimation(Misc.Side.R, DefAnimScheme, "Def", 8, false, new string[0], DefCond, false);
		if (DefAnimScheme.Count > 0) SerializeAnimation(Misc.Side.R, new List<int>() { DefAnimScheme[1] }, "DefStatic", 8, true);
		SerializeAnimation(Misc.Side.R, DefAnimScheme, "DefReverse", 6, false, new string[0], Cond, true);
		SerializeAnimation(Misc.Side.R, AttackAnimScheme, "Attack", 12, false, new string[0], "Attack");
		SerializeAnimation(Misc.Side.R, JumpAnimScheme, "Jump");
		SerializeAnimation(Misc.Side.R, FlyUpAnimScheme, "FlyUp", 8, true);
		SerializeAnimation(Misc.Side.R, FlyDownAnimScheme, "FlyDown", 8, true, new string[] { "Land" }, OnGroundCond, false);
		SerializeAnimation(Misc.Side.R, LandAnimScheme, "Land", 8, false, new string[] { "Idle", "Def", "GunIdle" });
		SerializeAnimation(Misc.Side.R, RunAnimScheme, "Run", 12, true, new string[] { "Idle", "Def", "GunIdle" }, "Run");
		SerializeAnimation(Misc.Side.R, RollAnimScheme, "Roll", 12, false, null, "Roll");
		SerializeAnimation(Misc.Side.R, Attack2AnimScheme, "Attack2", 12, false, new string[0], "Attack");
		SerializeAnimation(Misc.Side.R, Attack3AnimScheme, "Attack3", 12, false, new string[] { "Attack" }, "Attack");
		SerializeAnimation(Misc.Side.R, GunIdleAnimScheme, "GunIdle", 8, true);
		SerializeAnimation(Misc.Side.R, FireUpAnimScheme, "AimUp", 8, true, new string[] { "GunIdle", "AimDown" });
		SerializeAnimation(Misc.Side.R, FireDownAnimScheme, "AimDown", 8, true, new string[] { "GunIdle", "AimUp" });


		SerializeAnimation(Misc.Side.L, FireAnimScheme, "Fire", 8, false, new string[] { "GunIdle" });
		SerializeAnimation(Misc.Side.L, FireUpAnimScheme, "FireUp", 8, false, new string[] { "GunIdle", "AimUp" });
		SerializeAnimation(Misc.Side.L, FireDownAnimScheme, "FireDown", 8, false, new string[] { "GunIdle", "AimDown" });
		SerializeAnimation(Misc.Side.L, IdleAnimScheme, "Idle", 8, true);
		SerializeAnimation(Misc.Side.L, DefAnimScheme, "Def", 8, false, new string[0], DefCond, false);
		if (DefAnimScheme.Count > 0) SerializeAnimation(Misc.Side.L, new List<int>() { DefAnimScheme[1] }, "DefStatic", 8, true);
		SerializeAnimation(Misc.Side.L, DefAnimScheme, "DefReverse", 6, false, new string[0], Cond, true);
		SerializeAnimation(Misc.Side.L, AttackAnimScheme, "Attack", 12, false, new string[0], "Attack");
		SerializeAnimation(Misc.Side.L, JumpAnimScheme, "Jump");
		SerializeAnimation(Misc.Side.L, FlyUpAnimScheme, "FlyUp", 8, true);
		SerializeAnimation(Misc.Side.L, FlyDownAnimScheme, "FlyDown", 8, true, new string[] { "Land" }, OnGroundCond, false);
		SerializeAnimation(Misc.Side.L, LandAnimScheme, "Land", 8, false, new string[] { "Idle", "Def", "GunIdle" });
		SerializeAnimation(Misc.Side.L, RunAnimScheme, "Run", 12, true, new string[] { "Idle", "Def", "GunIdle" }, "Lun");
		SerializeAnimation(Misc.Side.L, RollAnimScheme, "Roll", 12, false, null, "Roll");
		SerializeAnimation(Misc.Side.L, Attack2AnimScheme, "Attack2", 12, false, new string[0], "Attack");
		SerializeAnimation(Misc.Side.L, Attack3AnimScheme, "Attack3", 12, false, new string[] { "Attack" }, "Attack");
		SerializeAnimation(Misc.Side.L, GunIdleAnimScheme, "GunIdle", 8, true);
		SerializeAnimation(Misc.Side.L, FireUpAnimScheme, "AimUp", 8, true, new string[] { "GunIdle", "AimDown" });
		SerializeAnimation(Misc.Side.L, FireDownAnimScheme, "AimDown", 8, true, new string[] { "GunIdle", "AimUp" });

	}

	void AssignPriority()
	{
		int prior = 0;
		foreach (var anim in AllAnims)
		{
			anim.priority = prior;
			var lCopy = AllAnimsL.Find(x => x.animName == anim.animName);
			if (lCopy != null) lCopy.priority = prior; else throw new System.Exception("Left copy of Animation named " + anim.animName + " has not found");
			prior++;
		}
	}
	void AssignPriorityForOtherSide()
	{
		foreach (var anim in AllAnims)
		{
			var lCopy = AllAnimsL.Find(z => z.animName == anim.animName);
			if (lCopy != null)
			{
				AllAnimsL.Remove(lCopy);
				AllAnimsL.Insert(AllAnims.IndexOf(anim), lCopy);
			}

		}
		AssignPriority();
	}
	void AddFrames(Misc.Side mm_side, List<int> framesInOT, ref List<CustomFrame> frames)
	{
		foreach (var num in framesInOT)
		{
			Sprite sprite;
			if (mm_side == Misc.Side.R)
			{
				sprite = SpritesInOT[num];
			}
			else
			{
				sprite = SpritesInOTForOtherSide[num];
			}

			CustomFrame a = new CustomFrame(sprite);

			frames.Add(a);
		}
	}
	void AddAnimation(List<CustomAnimation> AddTo, CustomAnimation pred, CustomAnimation newAnim, string side)
	{
		if (pred == null)
		{
			AddTo.Add(newAnim);
		}
		else
		{
			var ind = AddTo.IndexOf(pred);
			for (int i = 0; i < newAnim.frames.Count - 1; i++)
			{
				if (i > pred.frames.Count - 1) break;
				newAnim.frames[i].point = pred.frames[i].point;
				newAnim.frames[i].position = pred.frames[i].position;
				newAnim.frames[i].PhysicsShape = pred.frames[i].PhysicsShape;
				newAnim.frames[i].rotation = pred.frames[i].rotation;
				newAnim.frames[i].invisible = pred.frames[i].invisible;
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
			AddTo.Remove(pred);
			AddTo.Insert(ind, newAnim);
			if (!AssetDatabase.IsValidFolder("Assets/Animations/" + gameObject.name))
			{
				Directory.CreateDirectory("Assets/Animations/" + gameObject.name);
			}
			AssetDatabase.Refresh();
			if (!AssetDatabase.IsValidFolder("Assets/Animations/" + gameObject.name + "/" + side))
			{
				Directory.CreateDirectory("Assets/Animations/" + gameObject.name + "/" + side);
			}
			AssetDatabase.Refresh();
			AssetDatabase.CreateAsset(newAnim, "Assets/Animations/" + gameObject.name + "/" + side + "/" + newAnim.animName + ".asset");
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newAnim));
		}
	}
	void SerializeAnimation(Misc.Side mm_side, List<int> framesInOT, string name)
	{
		List<CustomFrame> frames = new List<CustomFrame>();
		if (framesInOT != null && framesInOT.Count > 0)
		{
			AddFrames(mm_side, framesInOT, ref frames);
		}
		else
		{
			return;
		}
		var newAnim = (CustomAnimation)ScriptableObject.CreateInstance("CustomAnimation");
		newAnim.frames = frames;
		newAnim.animName = name;
		newAnim.speed = 8;
		newAnim.animName = name;
		newAnim.repeatable = false;
		newAnim.m_condition = alwaysTrue;
		var pred = AllAnims.Find(x => x.animName == name);
		if (mm_side == Misc.Side.L) pred = AllAnimsL.Find(x => x.animName == name);
		if (mm_side == Misc.Side.R) AddAnimation(AllAnims, pred, newAnim, "R");
		if (mm_side == Misc.Side.L) AddAnimation(AllAnimsL, pred, newAnim, "L");
	}
	void SerializeAnimation(Misc.Side mm_side, List<int> framesInOT, string name, int fps)
	{
		List<CustomFrame> frames = new List<CustomFrame>();
		if (framesInOT != null && framesInOT.Count > 0)
		{
			AddFrames(mm_side, framesInOT, ref frames);
		}
		else
		{
			return;
		}
		var newAnim = (CustomAnimation)ScriptableObject.CreateInstance("CustomAnimation");
		newAnim.animName = name;
		var an = (CustomAnimation)newAnim;
		an.frames = frames;
		an.animName = name;
		an.speed = fps;
		an.repeatable = false;
		an.m_condition = alwaysTrue;
		var pred = AllAnims.Find(x => x.animName == name);
		if (mm_side == Misc.Side.L) pred = AllAnimsL.Find(x => x.animName == name);
		if (mm_side == Misc.Side.R) AddAnimation(AllAnims, pred, newAnim, "R");
		if (mm_side == Misc.Side.L) AddAnimation(AllAnimsL, pred, newAnim, "L");
	}
	void SerializeAnimation(Misc.Side mm_side, List<int> framesInOT, string name, int fps, bool repeatable)
	{
		List<CustomFrame> frames = new List<CustomFrame>();
		if (framesInOT != null && framesInOT.Count > 0)
		{
			AddFrames(mm_side, framesInOT, ref frames);
		}
		else
		{
			return;
		}
		var newAnim = (CustomAnimation)ScriptableObject.CreateInstance("CustomAnimation");
		newAnim.frames = frames;
		newAnim.animName = name;
		newAnim.speed = fps;
		newAnim.animName = name;
		newAnim.repeatable = repeatable;
		newAnim.m_condition = alwaysTrue;
		var pred = AllAnims.Find(x => x.animName == name);
		if (mm_side == Misc.Side.L) pred = AllAnimsL.Find(x => x.animName == name);
		if (mm_side == Misc.Side.R) AddAnimation(AllAnims, pred, newAnim, "R");
		if (mm_side == Misc.Side.L) AddAnimation(AllAnimsL, pred, newAnim, "L");
	}
	void SerializeAnimation(Misc.Side mm_side, List<int> framesInOT, string name, int fps, bool repeatable, string[] transitionsTo)
	{
		List<CustomFrame> frames = new List<CustomFrame>();
		if (framesInOT != null && framesInOT.Count > 0)
		{
			AddFrames(mm_side, framesInOT, ref frames);
		}
		else
		{
			return;
		}
		var newAnim = (CustomAnimation)ScriptableObject.CreateInstance("CustomAnimation");
		newAnim.frames = frames;
		newAnim.animName = name;
		newAnim.speed = fps;
		newAnim.animName = name;
		newAnim.repeatable = repeatable;
		newAnim.transitionsTo = transitionsTo;
		newAnim.m_condition = alwaysTrue;
		var pred = AllAnims.Find(x => x.animName == name);
		if (mm_side == Misc.Side.L) pred = AllAnimsL.Find(x => x.animName == name);
		if (mm_side == Misc.Side.R) AddAnimation(AllAnims, pred, newAnim, "R");
		if (mm_side == Misc.Side.L) AddAnimation(AllAnimsL, pred, newAnim, "L");
	}
	/*void SerializeAnimation(List<int> framesInOT, string name, int fps, bool repeatable, string[] transitionsTo)
	{
		List<CustomFrame> frames = new List<CustomFrame>();
		if (framesInOT != null && framesInOT.Count > 0)
		{
			AddFrames(framesInOT, ref frames);
		}
		else
		{
			return;
		}
		var newAnim = (CustomAnimation)ScriptableObject.CreateInstance("CustomAnimation");
		newAnim.frames = frames;
		newAnim.animName = name;
		newAnim.speed = fps;
		newAnim.animName = name;
		newAnim.repeatable = repeatable;
		newAnim.transitionsTo = transitionsTo;
		newAnim.m_condition = alwaysTrue;
		var pred = AllAnims.Find(x => x.animName == name);
		AddAnimation(pred, newAnim);
	}*/
	void SerializeAnimation(Misc.Side mm_side, List<int> framesInOT, string name, int fps, bool repeatable, string[] transitionsTo, string tag)
	{
		List<CustomFrame> frames = new List<CustomFrame>();
		if (framesInOT != null && framesInOT.Count > 0)
		{
			AddFrames(mm_side, framesInOT, ref frames);
		}
		else
		{
			return;
		}
		var newAnim = (CustomAnimation)ScriptableObject.CreateInstance("CustomAnimation");
		newAnim.frames = frames;
		newAnim.animName = name;
		newAnim.speed = fps;
		newAnim.animName = name;
		newAnim.repeatable = repeatable;
		newAnim.transitionsTo = transitionsTo;
		newAnim.m_condition = alwaysTrue;
		newAnim.tag = tag;
		var pred = AllAnims.Find(x => x.animName == name);
		if (mm_side == Misc.Side.L) pred = AllAnimsL.Find(x => x.animName == name);
		if (mm_side == Misc.Side.R) AddAnimation(AllAnims, pred, newAnim, "R");
		if (mm_side == Misc.Side.L) AddAnimation(AllAnimsL, pred, newAnim, "L");
	}
	void SerializeAnimation(Misc.Side mm_side, List<int> framesInOT, string name, int fps, bool repeatable, string[] transitionsTo, Misc.condition condition, bool reversed)
	{
		List<CustomFrame> frames = new List<CustomFrame>();
		if (framesInOT != null && framesInOT.Count > 0)
		{
			AddFrames(mm_side, framesInOT, ref frames);
		}
		else
		{
			return;
		}
		var newAnim = (CustomAnimation)ScriptableObject.CreateInstance("CustomAnimation");
		newAnim.frames = frames;
		newAnim.animName = name;
		newAnim.speed = fps;
		newAnim.animName = name;
		newAnim.repeatable = repeatable;
		newAnim.transitionsTo = transitionsTo;
		newAnim.m_condition = condition;
		newAnim.conditionName = condition.Method.Name;
		var pred = AllAnims.Find(x => x.animName == name);
		if (mm_side == Misc.Side.L) pred = AllAnimsL.Find(x => x.animName == name);
		if (mm_side == Misc.Side.R) AddAnimation(AllAnims, pred, newAnim, "R");
		if (mm_side == Misc.Side.L) AddAnimation(AllAnimsL, pred, newAnim, "L");
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
		if (anim == null) return;
		if (!PlayingQueue.Exists(x => x == anim) && AllAnims.Exists(x => x.animName == anim.animName)) PlayingQueue.Add(anim);
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

		CurrentAnim = newAnim;
		if (MotherAnimator != null && SynchronizeWithMA.Exists(x => x == newAnim.animName))
		{
			//CurrentAnim = AllAnims.Find(x => x.animName == MotherAnimator.CurrentAnim.animName);
			if (MotherAnimator.CurrentAnim.frames.Count == CurrentAnim.frames.Count)
			{
				MotherAnimator.newFrame.AddListener(SynchronizeWithMother);
				return;
			}
		}
		if (MotherAnimator != null) MotherAnimator.newFrame.RemoveListener(SynchronizeWithMother);
		ChangeFrame(newAnim, 0);
	}
	//for external purposes
	public UnityEvent NewAnim = new UnityEvent();
	string[] NotReturnToDefault = new string[] { "Attack", "Attack2", "Attack3", "Def", "Fire", "FireDown", "FireUp", "AimUp", "AimDown" };

	void OnAnimFinished(CustomAnimation finishedAnim)
	{
		if (finishedAnim.repeatable && !System.Array.Exists(NotReturnToDefault, x => x == finishedAnim.animName))
		{
			PlayAnim(finishedAnim);
			return;
		}
		if (finishedAnim.animName == "Jump")
		{
			PlayAnim("FlyUp");
			return;
		}
		if (finishedAnim.animName == "Fire")
		{
			PlayAnim("GunIdle");
			return;
		}
		if (finishedAnim.animName == "FireUp")
		{
			PlayAnim("AimUp");
			return;
		}
		if (finishedAnim.animName == "FireDown")
		{
			PlayAnim("AimDown");
			return;
		}
		if (finishedAnim.animName == "AimDown")
		{
			PlayAnim("GunIdle");
			return;
		}
		if (finishedAnim.animName == "AimUp")
		{
			PlayAnim("GunIdle");
			return;
		}
		if (finishedAnim.animName == "Def")
		{
			//if (!PlayingQueue.Exists(x => x.animName == "DefReverse"))
			{
				PlayAnim("DefStatic");
				return;
			}
			//else
			{
				//PlayAnim("DefReverse");
				//return;
			}
		}
		if (finishedAnim.animName == "DefReverse")
		{
			PlayingQueue.RemoveAll(z => z.animName == "Def");
		}
		if (finishedAnim.animName == "Roll")
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
		if (finishedAnim.animName == "Die")
		{
			HardDestroy();
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
	void SynchronizeWithMother()
	{
		timeFromFrameStart = MotherAnimator.timeFromFrameStart;
		m_side = MotherAnimator.side;
		ChangeFrame(CurrentAnim, MotherAnimator.currentFrameIndex);
		MotherAnimator.newFrame.RemoveListener(SynchronizeWithMother);
	}
	[SerializeField] string[] saveImpulse = new string[] { "DefReverse" };
	void NewFrame()
	{
		bool finished = false;
		if (currentFrameIndex < CurrentAnim.frames.Count - 1)
		{
			if (!CurrentAnim.repeatable && !CurrentAnim.interruptable)
			{
				//Animation is not finished, cannot be interrupted and must be continued
				ChangeFrame(CurrentAnim);
				return;
			}
		}
		else
		{
			OnAnimFinished(CurrentAnim);
			NewAnim.Invoke();
			finished = true;
			if (!CurrentAnim.repeatable) PlayingQueue.RemoveAll(x => x == CurrentAnim);
			//Animation finished
		}
		//Animation can be interrupted or has already finished
		var nextState = defaultAnim;
		nextState = FindMostPrioritizedAnim(!finished);

		if (nextState != CurrentAnim || (CurrentAnim.repeatable && finished))
		{
			animChanged.Invoke(nextState);
		}
		else if (nextState == CurrentAnim && !CurrentAnim.repeatable && finished)
		{
			print("Oops... There's someone's sh*t in ur pants. Clean it and I'll clean playing queue");
			animChanged.Invoke(defaultAnim);
		}
		else
		{
			ChangeFrame(CurrentAnim);
		}
		List<CustomAnimation> unclearableAnims = PlayingQueue.FindAll(x => System.Array.Exists(saveImpulse, z => z == x.animName));
		PlayingQueue.Clear();
		PlayingQueue.AddRange(unclearableAnims);
	}
	void ChangeFrame(CustomAnimation animation)
	{
		if (side == Misc.Side.L)
		{
			animation = AllAnimsL[animation.priority];
		}
		currentFrameIndex++;
		var _frame = animation.frames[currentFrameIndex];
		if (!ui)
		{
			if (!_frame.invisible) selfRender.sprite = _frame;
			else selfRender.sprite = blank;
		}
		else
		{
			if (!_frame.invisible) selfImage.sprite = _frame;
			else selfImage.sprite = blank;
		}

		m_currentFrame = _frame;
		int i = 1;
		if (animation.flip) i = -1;
		if (transform.parent != null && !ui) 
		{
			transform.localPosition = new Vector3(_frame.position.x * i, _frame.position.y, _frame.position.z);
		}


		transform.localRotation = Quaternion.Euler(0, 0, _frame.rotation*i);

		if(!ui)selfRender.flipX = animation.flip;
		newFrame.Invoke();
	}
	void ChangeFrame(CustomAnimation animation, int frame)
	{
		if (side == Misc.Side.L)
		{
			animation = AllAnimsL[animation.priority];
		}
		currentFrameIndex = frame;
		var _frame = animation.frames[currentFrameIndex];
		if (!ui)
		{
			if (!_frame.invisible) selfRender.sprite = _frame;
			else selfRender.sprite = blank;
		}
		else
		{
			if (!_frame.invisible) selfImage.sprite = _frame;
			else selfImage.sprite = blank;
		}

		m_currentFrame = _frame;
		int i = 1;
		if (animation.flip) i = -1;
		if (transform.parent != null&&!ui)
		{
			transform.localPosition = new Vector3(_frame.position.x * i, _frame.position.y, _frame.position.z);
		}
		transform.localRotation = Quaternion.Euler(0, 0, _frame.rotation*i);
		if(!ui)selfRender.flipX = animation.flip;

		newFrame.Invoke();
	}


	CustomAnimation FindMostPrioritizedAnim(bool CountSelf)
	{

		if (PlayingQueue.Count == 0)
		{
			return CountSelf is false ? defaultAnim : CurrentAnim;
		}
		var mostPrioritizedAnim = defaultAnim;
		if (CountSelf && (CurrentAnim.m_condition == null || CurrentAnim.m_condition(new CustomAnimatorContextInfo(this)))) mostPrioritizedAnim = CurrentAnim;
		for (int i = 0; i < PlayingQueue.Count; i++)
		{
			var anim = PlayingQueue[i];
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
	public void HardDestroy()
	{
		StopAllCoroutines();
		Destroy(gameObject);
	}
	public void ChangeSide()
	{
		m_side = m_side is Misc.Side.R ? Misc.Side.L : Misc.Side.R;
	}
	public void ChangeSide(string _side)
	{
		if (_side == "L") m_side = Misc.Side.L;
		if (_side == "R") m_side = Misc.Side.R;
	}
	public void ChangeSide(Misc.Side ___side)
	{
		m_side = ___side;
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
public class AnimTransition
{
	
}
public class AnimState
{

}


