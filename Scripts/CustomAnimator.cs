using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Events;
using static AnimatorScheme;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public class CustomAnimator : MonoBehaviour
{
    public DefaultAsset schemeAsset;
    AnimatorScheme animatorScheme;
    Dictionary<StateInfo, CustomAnimation> animDependencies = new Dictionary<StateInfo, CustomAnimation>();
    [Tooltip("If animaions between mother and child are equal, child get frame index of mother's animation")]
    public CustomAnimator MotherAnimator;
    public string DefaultAnim;
    private StateInfo defaultAnim;
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
    [HideInInspector] public StateInfo CurrentAnim;
    public List<string> SynchronizeWithMA;
    public List<Sprite> SpritesInOT;

#if UNITY_EDITOR
    [CustomEditor(typeof(CustomAnimator))]
    public class AnimatorEditor : Editor
    {
        CustomAnimator animator;
        CustomFrame frame;
        bool invert = true;
        public delegate void AnimatorDelegate(CustomAnimator sender);
        public static event AnimatorDelegate OnEnableEvent;
        private void OnEnable()
        {

            animator = (CustomAnimator)target;
            value = new Vector3();
            if (OnEnableEvent!=null)OnEnableEvent.Invoke(animator);

        }


        private void OnDisable()
        {

        }
        Vector3 value = new Vector3();
        public override void OnInspectorGUI()
        {
            if (animator.AssignNewPosToEveryFrame)
            {

                bool g = false;
                value = EditorGUILayout.Vector3Field("New Position", value);
                invert = EditorGUILayout.Toggle("Invert for left side", invert);
                g = EditorGUILayout.Toggle("Assign", g);
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                { /* if (g)
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
				}*/
                }
            }
            if (animator.AddNewAnim)
            {
                animator.AddNewAnim = false;
                //animator.SerializeAnimation(Misc.Side.R, null, "New Anim", 0, false, null);
            }
            EditorGUILayout.LabelField("CurrentAnimName", animator.CurrentAnim.name);
            EditorGUILayout.LabelField("CurrentFrameIndex", animator.currentFrameIndex.ToString());
            base.OnInspectorGUI();
        }


    }
#endif
    AnimatorScheme LoadScheme(DefaultAsset asset)
    {
        string path = AssetDatabase.GetAssetPath(asset);
        var formatter = new BinaryFormatter();
        var fileStream = new FileStream(path, FileMode.Open);
        var scheme = (AnimatorScheme)formatter.Deserialize(fileStream);

        fileStream.Close();
        AnimatorScheme.AssignObjectsForScheme(scheme);
        return scheme;
    }
    //I can store only data about path of animation in stateInfo,
    //so i need to get CustomAnimation for every state from path,
    //but i don't want to do it neither in runtime nor in other specific class like "runtimeStateInfo"
    //This dictionary contains anims for every state and should be filled only at start
    void SetDependencies()
    {
        print(animatorScheme.name);
        foreach (var state in animatorScheme.states)
        {
            animDependencies.Add(state, AssetDatabase.LoadAssetAtPath(state.animationPath, typeof(CustomAnimation)) as CustomAnimation);
        }
    }
    
    bool ui = false;
    // Start is called before the first frame update
    void Start()
    {
        animatorScheme = LoadScheme(schemeAsset);
        SetDependencies();
        blank = Resources.Load<Sprite>("Defaults/Blank");
        //SerializeAnimations();
        defaultAnim = animatorScheme.states.Find(x => x.name == DefaultAnim);
        CurrentAnim = defaultAnim;

        TryGetComponent<SpriteRenderer>(out selfRender);
        if (selfRender == null) { selfImage = GetComponent<UnityEngine.UI.Image>(); ui = true; }
        if (!ui) selfRender.sprite = animDependencies[CurrentAnim].frames[0];
        else selfImage.sprite = animDependencies[CurrentAnim].frames[0];
        if (transform.parent != null && !ui) transform.localPosition = animDependencies[CurrentAnim].frames[0].position;
        transform.localRotation = Quaternion.Euler(0, 0, animDependencies[CurrentAnim].frames[0].rotation);
        animChanged.AddListener(OnStateChanged);
    }
    //Rewrite all scripts to erase these two
    public CustomAnimation GetCurrentAnimation()
    {
        return animDependencies[CurrentAnim];
    }
    public CustomAnimation GetAnimation(StateInfo st)
    {
        return animDependencies[st];
    }
        /*SerializeAnimation(Misc.Side.R, FireAnimScheme, "Fire", 8, false, new string[] { "GunIdle" });
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
		*/
    // Update is called once per frame
    void Update()
    {
        if (animatorScheme != null && animatorScheme.states.Count > 0) PlayCurrentAnim();
    }

    public AnimContextEvent animChanged = new AnimContextEvent();

    void OnStateChanged(StateInfo newAnim, Transition trans)
    {

        CurrentAnim = newAnim;
        if (MotherAnimator != null && SynchronizeWithMA.Exists(x => x == newAnim.name))
        {
            //CurrentAnim = AllAnims.Find(x => x.animName == MotherAnimator.CurrentAnim.animName);
            if (MotherAnimator.animDependencies[CurrentAnim].frames.Count == animDependencies[CurrentAnim].frames.Count)
            {
                MotherAnimator.newFrame.AddListener(SynchronizeWithMother);
                return;
            }
        }
        if (MotherAnimator != null) MotherAnimator.newFrame.RemoveListener(SynchronizeWithMother);
        ChangeFrame(newAnim, 0);
    }
    //for external purposes
    /// <summary>
    /// Is called after the last frame of every animation
    /// </summary>
    public UnityEvent<CustomAnimation> NewAnim = new UnityEvent<CustomAnimation>();
    string[] NotReturnToDefault = new string[] { "Attack", "Attack2", "Attack3", "Def", "Fire", "FireDown", "FireUp", "AimUp", "AimDown" };

    /*void OnAnimFinished(StateInfo finishedAnim)
    {

        if (animDependencies[finishedAnim].animName == "Die")
        {
            HardDestroy();
        }

    }*/
    public UnityEvent newFrame = new UnityEvent();
    void PlayCurrentAnim()
    {
        timeFromFrameStart += Time.deltaTime;
        if (timeFromFrameStart >= 1 / animDependencies[CurrentAnim].speed)
        {
            timeFromFrameStart = 0;
            NewFrame();
        }
    }
    void SynchronizeWithMother()
    {
        timeFromFrameStart = MotherAnimator.timeFromFrameStart;
        ChangeFrame(CurrentAnim, MotherAnimator.currentFrameIndex);
        MotherAnimator.newFrame.RemoveListener(SynchronizeWithMother);
    }
    void NewFrame()
    {
        bool finished = false;
        if (currentFrameIndex < animDependencies[CurrentAnim].frames.Count - 1)
        {
            /*//if (!CurrentAnim.repeatable && !CurrentAnim.interruptable)
            {
                //Animation is not finished, cannot be interrupted and must be continued
                ChangeFrame(CurrentAnim);
                return;
            }*/
        }
        else
        {
            //OnAnimFinished(CurrentAnim);
            NewAnim.Invoke(animDependencies[CurrentAnim]);
            finished = true;
            //if (!CurrentAnim.repeatable) PlayingQueue.RemoveAll(x => x == CurrentAnim);
            //Animation finished
        }
        //Animation can be interrupted or has already finished
        bool stateChanged= false;
        var nextState = defaultAnim;
        foreach (var trans in CurrentAnim.transitons)
        {
            bool result = true;
            if (trans.hasExitTime && !finished) result = false;
            foreach (var cond in trans.conditions)
            {
                if (!cond.IsTrue(gameObject)) { result = false; break; } 
            }
            if (result)
            {
                nextState = trans.endState;
                animChanged.Invoke(nextState, trans);
                stateChanged = true;
                break;
            }
        }

        if (stateChanged)
        {
           
        }
        else
        {
            ChangeFrame(CurrentAnim);
        }
        /*List<StateInfo> unclearableAnims = PlayingQueue.FindAll(x => System.Array.Exists(saveImpulse, z => z == x.name));
        PlayingQueue.Clear();
        PlayingQueue.AddRange(unclearableAnims);*/
    }
    void ChangeFrame(StateInfo animation)
    {
        currentFrameIndex++;
        ChangeFrame(animation, currentFrameIndex);
    }
    void ChangeFrame(StateInfo animation, int frame)
    {
        currentFrameIndex = frame;

        CustomAnimation curAnim = GetAnimation(animation);
        var _frame = curAnim.frames[currentFrameIndex];
        if (curAnim.relativeFrames)
        {
            if(curAnim.frames[frame].numberInSheet <SpritesInOT.Count)_frame.sprite = SpritesInOT[curAnim.frames[frame].numberInSheet];
            else
            {
                _frame.sprite = blank;
                Debug.Log(gameObject.name + " don't have enough frames for " + animation.name + " animation. Using blank instead.");
            }
        }

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
        if (curAnim.flip) i = -1;
        if (transform.parent != null && !ui)
        {
            transform.localPosition = new Vector3(_frame.position.x * i, _frame.position.y, _frame.position.z);
        }
        transform.localRotation = Quaternion.Euler(0, 0, _frame.rotation * i);
        if (!ui) selfRender.flipX = curAnim.flip;

        newFrame.Invoke();
    }
    public void HardDestroy()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }
}

public class AnimContextEvent : UnityEngine.Events.UnityEvent<StateInfo,Transition> { }
