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
    [SerializeField]
    string animatorPath;
    AnimatorScheme animatorScheme;
    [SerializeField] CustomAnimation[] animations;
    [SerializeField] StateInfo[] states;
    //Dictionary<StateInfo, CustomAnimation> animDependencies = new Dictionary<StateInfo, CustomAnimation>();
    [Tooltip("If animaions between mother and child are equal, child get frame index of mother's animation")]
    public CustomAnimator MotherAnimator;
    public string CurrentAnimName;
    public string DefaultAnim;
    private StateInfo defaultAnim;
    public bool AddNewAnim;
    public bool AssignNewPosToEveryFrame;
    public float timeFromFrameStart;
    private int currentFrameIndex;
    [Tooltip("If zero, doesn't override")]
    public int OverrideLayer = 0;
    [Tooltip("If true, override layer regardless of current side, else sends to back on turn")]
    public bool AlwaysOnTop = false;
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
    public List<Sprite> SpritesInOTSecondSide;

#if UNITY_EDITOR
    public DefaultAsset schemeAsset;
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
            if (OnEnableEvent != null) OnEnableEvent.Invoke(animator);
            animator.animatorPath = AssetDatabase.GetAssetPath(animator.schemeAsset);
            animator.SetDependencies();
            //print(animator.animations.Length);
        }


        private void OnDisable()
        {

        }
        Vector3 value = new Vector3();
        public override void OnInspectorGUI()
        {
            animator.animatorPath = AssetDatabase.GetAssetPath(animator.schemeAsset);
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
    void SetDependencies()
    {
        print("Set dependencies");
        animatorScheme = LoadScheme(animatorPath);
        if (animatorScheme == null) return;
        states = new StateInfo[animatorScheme.states.Count];
        animations = new CustomAnimation[animatorScheme.states.Count];
        int i = 0;
        foreach (var state in animatorScheme.states)
        {
            states[i] = state;
            animations[i] = AssetDatabase.LoadAssetAtPath(state.animationPath, typeof(CustomAnimation)) as CustomAnimation;
            i++;
        }
        print(animatorScheme.name + "  " + states.Length);
    }
#endif
    AnimatorScheme LoadScheme(string path)
    {
        if (path == "") return null;
        path = path.Remove(0, 7);

        TextAsset asset = Resources.Load(path) as TextAsset;
        Stream s = new MemoryStream(asset.bytes);
        var formatter = new BinaryFormatter();
        //var fileStream = new FileStream(Application.dataPath+path, FileMode.Open);
        var scheme = (AnimatorScheme)formatter.Deserialize(s);
        s.Close();
        //fileStream.Close();
        AnimatorScheme.AssignObjectsForScheme(scheme);
        return scheme;
    }
    //I can store only data about path of animation in stateInfo,
    //so i need to get CustomAnimation for every state from path,
    //but i don't want to do it neither in runtime nor in other specific class like "runtimeStateInfo"
    //This dictionary contains anims for every state and should be filled only at start


    bool ui = false;
    // Start is called before the first frame update
    void Start()
    {
        Log += (string a) => { };
        animatorScheme = LoadScheme(animatorPath);
        blank = Resources.Load<Sprite>("Defaults/Blank");
        //SerializeAnimations();
        if (animatorScheme != null) defaultAnim = animatorScheme.states.Find(x => x.name == DefaultAnim);
        if (animatorScheme != null) CurrentAnim = defaultAnim;

        TryGetComponent<SpriteRenderer>(out selfRender);
        if (selfRender == null) { selfImage = GetComponent<UnityEngine.UI.Image>(); ui = true; }
        var curanim = GetCurrentAnimation();
        if (animatorScheme == null) return;
        if (!ui) selfRender.sprite = curanim.frames[0];
        else selfImage.sprite = curanim.frames[0];
        if (transform.parent != null && !ui) transform.localPosition = curanim.frames[0].position;
        transform.localRotation = Quaternion.Euler(0, 0, curanim.frames[0].rotation);
        AnimatorService.AddAnimator(this);
    }
    //Rewrite all scripts to erase these two
    public CustomAnimation GetCurrentAnimation()
    {
        if (CurrentAnim.name.Contains("_L"))
        {

        }
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].name == CurrentAnim.name)
            {

                return animations[i];
            }
        }
        print(animatorScheme.name + "  " + animatorPath + "   " + gameObject.name);
        throw new Exception("Animation for state " + CurrentAnim.name + " not found");
    }
    public CustomAnimation GetAnimation(StateInfo st)
    {
        string output = "";
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].name == st.name)
            {
                output += states[i].name + "\n";
                return animations[i];
            }
        }
        print(animatorScheme.name + "  " + animatorPath + "   " + gameObject.name);
        throw new Exception("Animation for state " + st.name + " not found");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        AnimatorService.Update();
    }
    public void AnimatorUpdate()
    {
        if (animatorScheme != null && animatorScheme.states.Count > 0) PlayCurrentAnim();
        ALog("-----" + gameObject.name + " says: everything's done", "globalLog",this);
    }
    //is called if animation state changed
    public AnimContextEvent animChangedEvent = new AnimContextEvent();

    void OnStateChanged(StateInfo newAnim, Transition trans)
    {
        CurrentAnim = newAnim;
        CurrentAnimName = newAnim.name;

        if (MotherAnimator != null && MotherAnimator.CurrentAnim.name.Contains(CurrentAnim.name))
        {
            ChangeFrame(newAnim, MotherAnimator.CurrentFrameIndex, MotherAnimator.timeFromFrameStart);
            ALog(gameObject.name + " has started playing " + CurrentAnim.name + " with frame " + MotherAnimator.CurrentFrameIndex + " and time " + MotherAnimator.timeFromFrameStart, "globalLog", this);
            return;
        }
        ALog(gameObject.name + " has started playing " + CurrentAnim.name, "globalLog", this);
        ChangeFrame(newAnim, 0);
    }

    public UnityEvent newFrameEvent = new UnityEvent();
    void PlayCurrentAnim()
    {
        timeFromFrameStart += Time.fixedDeltaTime;

        if (timeFromFrameStart >= 1f / GetCurrentAnimation().speed)
        {
            NewFrame();

        }
    }
    void SynchronizeWithMother()
    {
        timeFromFrameStart = MotherAnimator.timeFromFrameStart;
        ChangeFrame(CurrentAnim, MotherAnimator.currentFrameIndex);
        MotherAnimator.newFrameEvent.RemoveListener(SynchronizeWithMother);
        print("listening");
    }

    void NewFrame()
    {
        if (animatorScheme == null) return;
        bool finished = false;
        if (currentFrameIndex < GetCurrentAnimation().frames.Count - 1)
        {
            //there were smth, but don't need it anymore
        }
        else
        {
            finished = true;
        }
        bool stateChanged = false;
        var nextState = defaultAnim;

        var anyState = animatorScheme.states.Find(x => x.name == "Any State" || x.name == "Any state" || x.name == "any state" || x.name == "AnyState");
        List<Transition> transToCheck = new List<Transition>();
        bool result = true;
        int counter = 0;
        //while (result == true&&counter<10)
        {
            counter++;
            transToCheck.AddRange(anyState.transitons);
            transToCheck.RemoveAll(x => x.endState.name == CurrentAnim.name);//prevent from looping on same anim through anystate
            transToCheck.AddRange(CurrentAnim.transitons);
            foreach (var trans in transToCheck)
            {
                ALog("   *OwO* Trying to transit from " + CurrentAnim.name + " to " + trans.endState.name);
                result = true;
                if (trans.hasExitTime && !finished) result = false;
                int ctr = 0;
                if (result)//if there is sense in it after first check
                {
                    foreach (var cond in trans.conditions)
                    {
                        ctr++;
                        ALog("   Checking cond " + ctr + "/" + trans.conditions.Count);
                        if (!cond.IsTrue(gameObject))
                        {
                            result = false;
                            ALog("      Cond check failed at " + CurrentAnim.name + ", failed trans: " + trans.endState.name);
                            break;
                        }
                    }
                }
                else
                {
                    ALog("  Transition denied: not finished yet");
                }
                if (result)
                {
                    nextState = trans.endState;
                    ALog("Successful trans from " + CurrentAnim.name + " to " + nextState.name);
                    animChangedEvent.Invoke(nextState, trans);
                    OnStateChanged(nextState, trans);


                    //setting all eventinvoked var to false, so if event was invoked before new state start, it doesn't invoke next anim
                    foreach (var tr in nextState.transitons)
                    {
                        foreach (var c in tr.conditions)
                        {
                            c.eventinvoked = false;
                        }
                    }
                    foreach (var tr in anyState.transitons)
                    {
                        foreach (var c in tr.conditions)
                        {
                            c.eventinvoked = false;
                        }
                    }
                    stateChanged = true;
                    break;
                }
            }
        }
        if (stateChanged)
        {

        }
        else
        {
            ALog("  Continue playing " + CurrentAnim.name);
            ChangeFrame(CurrentAnim);
        }
    }

    void ChangeFrame(StateInfo animation)
    {
        currentFrameIndex++;
        ChangeFrame(animation, currentFrameIndex);
    }
    public delegate void LogEvent(string message);
    public static event LogEvent Log;
    void ChangeFrame(StateInfo animation, int frame)
    {
        ChangeFrame(animation, frame, 0);
    }
    void ChangeFrame(StateInfo animation, int frame, float _timeFromFrameStart)
    {
        timeFromFrameStart = _timeFromFrameStart;
        currentFrameIndex = frame;

        CustomAnimation curAnim = GetAnimation(animation);
        var _frame = new CustomFrame(blank);
        try
        {
            _frame = curAnim.frames[currentFrameIndex];
        }
        catch (Exception e)
        {
            Debug.LogError("Frame index is more than number of frames in current animation " + e);
        }
        if (curAnim.relativeFrames)
        {
            if (!curAnim.TakeFrameFromSecondList)
            {
                if (curAnim.frames[frame].numberInSheet < SpritesInOT.Count) _frame.sprite = SpritesInOT[curAnim.frames[frame].numberInSheet];
                else
                {
                    _frame.sprite = blank;
                    Log.Invoke(gameObject.name + " don't have enough frames for " + animation.name + " animation. Using blank instead.");
                }
            }
            else
            {
                if (curAnim.frames[frame].numberInSheet < SpritesInOTSecondSide.Count) _frame.sprite = SpritesInOTSecondSide[curAnim.frames[frame].numberInSheet];
                else
                {
                    _frame.sprite = blank;
                    Log.Invoke(gameObject.name + " don't have enough frames for " + animation.name + " animation. Using blank instead.");
                }
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
        int i = -1;
        if (curAnim.flip) i = 1;
        if (curAnim.TakeFrameFromSecondList && !AlwaysOnTop) i = 1;
        if (transform.parent != null && !ui)
        {
            transform.localPosition = new Vector3((_frame.position.x) * -i, _frame.position.y, (_frame.position.z + OverrideLayer) * i);
        }
        transform.localRotation = Quaternion.Euler(0, 0, _frame.rotation * i);
        if (!ui) selfRender.flipX = curAnim.flip;

        newFrameEvent.Invoke();
    }
    public void HardDestroy()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }
    public bool TOLOG;

    int attemptNum = -1;
    void PrepareFolder()
    {
        if (!Directory.Exists("C:/OneLevelLogs/")) Directory.CreateDirectory("C:/OneLevelLogs/");
        do
        {
            attemptNum++;
        }
        while (File.Exists("C:/OneLevelLogs/Attempt_" + attemptNum + "/" + gameObject.name + ".txt"));
        Directory.CreateDirectory("C:/OneLevelLogs/Attempt_" + attemptNum);
    }
    public void ALog(string msg)
    {
        if (!TOLOG) return;
        if (attemptNum == -1) PrepareFolder();
        ALog(msg, "Attempt_"+attemptNum+" / "+gameObject.name, this);
    }
    public static void ALog(string msg, string file, CustomAnimator animator)
    {
        StreamWriter writer = File.AppendText("C:/OneLevelLogs/" + file + ".txt");
        writer.WriteLine(Time.frameCount + " " + msg + animator.GetInfo());
        writer.Close();
    }
    public static void ALog(string msg, string file)
    {
        StreamWriter writer = File.AppendText("C:/OneLevelLogs/" + file + ".txt");
        writer.WriteLine(Time.frameCount + " " + msg);
        writer.Close();
    }
    public static void DeleteFile(string file)
    {
        File.Delete("C:/OneLevelLogs/"+file+".txt");
    }
    string GetInfo()
    {
        string o = " (" + CurrentAnim.name + " " + currentFrameIndex + " " + timeFromFrameStart + ")";
        return o;
    }
}

public class AnimContextEvent : UnityEngine.Events.UnityEvent<StateInfo, Transition> { }
