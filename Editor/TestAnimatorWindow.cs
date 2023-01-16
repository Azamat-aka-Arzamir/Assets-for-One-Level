
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using static AnimatorScheme;
using static UnityEditor.PlayerSettings;

public class vittu { }
public class TestAnimatorWindow : EditorWindow
{

    [MenuItem("Window/2D/AnimatorWindow")]
    public static void ShowWindow()
    {
        TestAnimatorWindow wnd = GetWindow<TestAnimatorWindow>();

        wnd.maximized = true;
        wnd.minSize = new Vector2(450, 200);
        wnd.maxSize = new Vector2(1920, 720);
    }
    [MenuItem("Window/COLLAPSE ANAL")]
    public static void TOTAL_COLLAPSE()
    {
        TestAnimatorWindow wnd = GetWindow<TestAnimatorWindow>();
        wnd.Close();
    }
    TwoPaneSplitView panes;
    VisualElement leftpane;
    VisualElement rightpane;
    TextField commandLine;
    FileField ASField;
    public Box statesSpace { get; private set; }   
    public List<StateBox> states = new List<StateBox>();
    [SerializeField] string inspectedSchemePath = "";
    public CustomAnimator inspectedAnimator { get; private set; }
    [SerializeField] string inspectedAnimID;

    public VisualElement activeElement { get; private set; }
    VisualElement ActiveElement;
    Vector2 lastLeftPaneSize;
    public void CreateGUI()
    {

        VisualElement root = rootVisualElement;
        panes = new TwoPaneSplitView(0, 700, TwoPaneSplitViewOrientation.Horizontal);
        leftpane = new VisualElement();
        rightpane = new VisualElement();
        rightpane.style.backgroundColor = new Color(75 / 255f, 75 / 255f, 75 / 255f);
        panes.Add(leftpane);
        leftpane.RegisterCallback<GeometryChangedEvent>(x => lastLeftPaneSize = leftpane.layout.size);

        panes.Add(rightpane);
        root.Add(panes);
        var controlPanel = new Box();
        controlPanel.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1);
        Button btn = new Button(() => CreateState(new Vector3(UnityEngine.Random.Range(0f, 50f), UnityEngine.Random.Range(0f, 50f)), "NewState"));
        btn.text = "New state";
        ASField = new FileField();
        Button btn2 = new Button(() => Save(ASField.fileName));
        btn2.text = "Save";

        //Button btn3 = new Button(() => Load("semen"));
        //btn3.text = "Load";
        //ASField.RegisterCallback<ChangeEvent<string>>(x => Load(ASField.path));
        ASField.OnFileChange += (sender, args) => { Load(ASField.file); };
        //ASField.file = AssetDatabase.LoadAssetAtPath(path, typeof(DefaultAsset)) as DefaultAsset;
        controlPanel.Add(ASField);
        controlPanel.Add(btn);
        controlPanel.Add(btn2);
        //controlPanel.Add(btn3);


        commandLine = new TextField();//Command line init
        controlPanel.Add(commandLine);
        commandLine.RegisterCallback<KeyDownEvent>(CommandLineParsing);



        controlPanel.style.width = 100;
        leftpane.Add(controlPanel);
        statesSpace = new Box();
        

        statesSpace.name = "StateSpace";
        statesSpace.style.width = 5000;
        statesSpace.style.height = 5000;
        statesSpace.style.backgroundImage = (AssetDatabase.LoadAssetAtPath("Assets/Defaults/Editor/AnimatorBG.png", typeof(Sprite)) as Sprite).texture;
        statesSpace.style.position = Position.Absolute;
        leftpane.RegisterCallback<WheelEvent>(x =>
        {
            statesSpace.transform.scale +=Vector3.one* x.delta.y / 100;
            if (statesSpace.transform.scale.x <= 0.1) { statesSpace.transform.scale = Vector3.one / 10; return; }
            statesSpace.transform.position += (statesSpace.transform.position - (Vector3)lastLeftPaneSize/2) / statesSpace.transform.scale.x * x.delta.y / 100;
        });
        leftpane.RegisterCallback<MouseDownEvent>(x =>
        {
            if(x.button == 0&&x.shiftKey)
            {
                leftpane.RegisterCallback<MouseMoveEvent>(Scroll);
            }
        });
        leftpane.RegisterCallback<MouseUpEvent>(x =>
        {
            if (x.button == 0)
            {
                leftpane.UnregisterCallback<MouseMoveEvent>(Scroll);
            }
        });
        leftpane.RegisterCallback<MouseLeaveEvent>(x =>
        {
            leftpane.UnregisterCallback<MouseMoveEvent>(Scroll);
        });
        leftpane.Add(statesSpace);
        statesSpace.SendToBack();

        leftpane.RegisterCallback<DragUpdatedEvent>((x) =>
        {

            if (DragAndDrop.objectReferences[0].GetType() == typeof(CustomAnimation))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            }
        });
        leftpane.RegisterCallback<DragPerformEvent>((x) =>
        {
            int i = 0;
            while (DragAndDrop.objectReferences[i].GetType() == typeof(CustomAnimation))
            {
                CreateState(x.mousePosition+Vector2.one*i*5, DragAndDrop.objectReferences[i].name).animation = DragAndDrop.objectReferences[i] as CustomAnimation;
                if (DragAndDrop.objectReferences.Length - 1 > i) i++;
                else break;
            }
        });

        CustomAnimator.AnimatorEditor.OnEnableEvent += OnAnimatorInspected;
        Load(inspectedSchemePath);
        if (inspectedAnimID != null && inspectedAnimID != "") inspectedAnimator = IDCard.FindByID(inspectedAnimID).GetComponent<CustomAnimator>();
        if (inspectedAnimator != null)
        { 
            inspectedAnimator.newFrame.AddListener(OnFrameUpdate); 
            OnAnimatorInspected(inspectedAnimator);
        }
    }
    void Scroll(MouseMoveEvent context)
    {
        statesSpace.transform.position += (Vector3)context.mouseDelta;
    }
    private void OnDestroy()
    {
        CustomAnimator.AnimatorEditor.OnEnableEvent -= OnAnimatorInspected;
    }

    //called when there is CA component on inspected object in inspector
    void OnAnimatorInspected(CustomAnimator c)
    {
        IDCard newObjectID;
        if (!c.gameObject.TryGetComponent<IDCard>(out newObjectID))
        {
            newObjectID = c.gameObject.AddComponent<IDCard>();
        }
        if(c.schemeAsset!=null)ASField.file = c.schemeAsset;
        if (inspectedAnimator != null) inspectedAnimator.newFrame.RemoveListener(OnFrameUpdate);
        if (inspectedAnimator != null) inspectedAnimator.animChanged.RemoveListener(OnAnimChanged);
        inspectedAnimator = c;
        titleContent = new GUIContent(inspectedAnimator.gameObject.name);

        inspectedAnimID = inspectedAnimator.GetComponent<IDCard>().ID;
        inspectedAnimator = IDCard.FindByID(inspectedAnimID).GetComponent<CustomAnimator>();
        inspectedAnimator.newFrame.AddListener(OnFrameUpdate);
        inspectedAnimator.animChanged.AddListener(OnAnimChanged);
    }

    private void OnAnimChanged(StateInfo st, AnimatorScheme.Transition tr)
    {
        Line lastTrans;
        if(lastState!=null)lastState.cogImage.tintColor = Color.white;
        var nextAnim = states.Find(x => x.stateName == st.name);
        if (nextAnim.endOnMe.Count != 0)
        {
            lastTrans = nextAnim.endOnMe.Find(x => x.startState.stateName == tr.startState.name);
            if (LinesToFade.ContainsKey(lastTrans)) LinesToFade.Remove(lastTrans);
            LinesToFade.Add(lastTrans, Time.time);

        }

        //Debug.Log(lastTrans.name);

    }

    Dictionary<Line, float> LinesToFade = new Dictionary<Line, float>();
    //For animating purposes
    StateBox lastState;
    void TransFade()
    {
        List<Line> ToDelete = new List<Line>();
        foreach (var tr in LinesToFade)
        {
            tr.Key.tintColor = new Color(1f, 5 * (Time.time - tr.Value), 5 * (Time.time - tr.Value));
            tr.Key.TurnOnTheLight();
            if (5 * (Time.time - tr.Value) >= 1)
            {
                ToDelete.Add(tr.Key);
            }
        }
        foreach (var tr in ToDelete)
        {
            LinesToFade.Remove(tr);
            tr.TurnOffTheLight();
        }
        ToDelete.Clear();

    }

    void OnFrameUpdate()
    {
        lastState = states.Find(x => x.stateName == inspectedAnimator.CurrentAnim.name);
        var before = lastState.cogPivot.transform.rotation.eulerAngles;
        lastState.cogPivot.transform.rotation = Quaternion.Euler(0f, 0f, before.z+10);
        lastState.cogImage.tintColor = new Color(0.8f,0.8f,1);
        TransFade();
    }
    class FileField : VisualElement
    {
        public event EventHandler<FileChangeContext> OnFileChange;
        Box box = new Box();
        VisualElement label = new Label("null");
        DefaultAsset _file;
        public DefaultAsset file
        {
            get
            {
                return _file;
            }
            set
            {
                _file = value;
                (label as Label).text = value.name;
                path = AssetDatabase.GetAssetPath(value);
                fileName = value.name;
                OnFileChange.Invoke(this, new FileChangeContext(value, value.name, path));
            }
        }
        public string path;
        public string fileName = null;
        public class FileChangeContext : EventArgs
        {
            public readonly DefaultAsset newFile;
            public readonly string newName;
            public readonly string newPath;
            public FileChangeContext(DefaultAsset n_File, string n_Name, string n_Path)
            {
                newFile = n_File;
                newName = n_Name;
                newPath = n_Path;
            }
        }
        public FileField()
        {
            ContextualMenuManipulator m = new ContextualMenuManipulator(ContextMenuActions);
            m.target = this;
            box.style.width = 100;
            box.style.height = 20;
            box.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            box.Add(label);
            Add(box);
            box.RegisterCallback<DragUpdatedEvent>((x) =>
            {

                if (DragAndDrop.objectReferences[0].GetType() == typeof(DefaultAsset))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                }
            });
            box.RegisterCallback<DragPerformEvent>((x) =>
            {
                if (DragAndDrop.objectReferences[0].GetType() == typeof(DefaultAsset))
                {
                    file = DragAndDrop.objectReferences[0] as DefaultAsset;
                }
            });
        }
        void ContextMenuActions(ContextualMenuPopulateEvent _event)
        {
            _event.menu.AppendAction("Rename", Rename, DropdownMenuAction.AlwaysEnabled);
        }
        void Rename(DropdownMenuAction action)
        {
            box.RemoveAt(0);
            label = new TextField();
            box.Add(label);
            (label as TextField).Focus();
            label.RegisterCallback<KeyDownEvent>((x) =>
            {
                if (x.keyCode == KeyCode.Return)
                {
                    Label l = new Label((label as TextField).text);
                    if (l.text == "") l.text = "U fORGOT TO \n WRITE NAME";
                    label = l;
                    box.RemoveAt(0);
                    box.Add(label);
                    fileName = l.text;
                }
            });

        }
    }
    const string helpData =
        "sad x - Save current scheme as default with x-name \n" +
        "ld x - Load x-named scheme from defaults !without saving current scheme!\n" +
        "condinfo - Show all conditions info\n";
    void CommandLineParsing(KeyDownEvent callback)
    {
        if (callback.keyCode != KeyCode.Return) return;
        string command = commandLine.value.Split(' ')[0];
        string arg;
        switch (command)
        {
            case "sad"://SaveAsDefault
                if (commandLine.value.Split(' ').Length == 1)
                {
                    throw new System.Exception("Incorrect Argument");
                }
                arg = commandLine.value.Split(' ')[1];
                Save("/Defaults/" + arg);
                break;
            case "ld"://Load default scheme
                if (commandLine.value.Split(' ').Length == 1)
                {
                    throw new System.Exception("Incorrect Argument");
                }
                arg = commandLine.value.Split(' ')[1];
                Load("Defaults/" + arg);
                break;
            case "help":
                Debug.Log(helpData);
                break;
            case "condinfo":
                foreach (var st in states)
                {
                    foreach (var l in st.trans)
                    {
                        foreach (var c in l.conditions)
                        {
                            Debug.Log(c.typeRef + " - Type\n" + c.typeRef + " - Hash\n");
                        }
                    }
                }
                break;
        }
        commandLine.value = "";
    }
    void Save(string path)
    {

        var scheme = new AnimatorScheme();
        scheme.name = ASField.fileName;
        scheme.InitializeScheme(states);
        if (!AssetDatabase.IsValidFolder("Assets/Animators"))
        {
            System.IO.Directory.CreateDirectory("Assets/Animators");
        }
        AssetDatabase.Refresh();
        var splittedPath = path.Split('/');
        if (splittedPath.Length > 1)
        {
            string pa = "/";
            for (int i = 0; i < splittedPath.Length - 1; i++)
            {
                string p = splittedPath[i];
                if (!AssetDatabase.IsValidFolder("Assets/Animators" + pa + p))
                {
                    System.IO.Directory.CreateDirectory("Assets/Animators" + pa + p);
                }
                pa += p + "/";
            }
        }

        var formatter = new BinaryFormatter();
        FileStream fileStream = new FileStream("Assets/Animators/" + path, FileMode.Create);
        formatter.Serialize(fileStream, scheme);

        fileStream.Close();
        AssetDatabase.Refresh();
    }
    void Load(DefaultAsset asset)
    {
        var p = AssetDatabase.GetAssetPath(asset);
        Load(p);
    }
    void Load(string path)
    {
        if (path == "" || path == null)
        {
            Debug.Log("Nothing to load");
            return;
        }
        var p = path.Split('/');
        //p[p.Length - 1] = "";
        string folderOnly = "";
        for (int i = 0; i < p.Length - 1; i++)
        {
            if (p[i] != "Animators" && p[i] != "Assets")
            {
                folderOnly += p[i];
                if (i < p.Length - 2) folderOnly += "/";
            }
        }
        if (!Directory.Exists("Assets/Animators/" + folderOnly))
        {
            throw new System.Exception("Loading failed - Incorrect Directory: Assets/Animators/" + folderOnly + "/");
        }

        var formatter = new BinaryFormatter();
        var fileStream = new FileStream("Assets/Animators/" + folderOnly + "/" + p[p.Length - 1], FileMode.Open);
        var scheme = (AnimatorScheme)formatter.Deserialize(fileStream);

        fileStream.Close();

        statesSpace.transform.scale = Vector3.one;
        statesSpace.transform.position = Vector3.zero;
        AnimatorScheme.AssignObjectsForScheme(scheme);

        states.Clear();
        statesSpace.Clear();
       
        Debug.Log(scheme.states.Count);
        foreach (var state in scheme.states)
        {
            var s = new StateBox(state.name);
            states.Add(s);
            s.transform.position = new Vector3(state.position[0], state.position[1]);
            s.animation = AssetDatabase.LoadAssetAtPath(state.animationPath, typeof(CustomAnimation)) as CustomAnimation;
            statesSpace.Add(s);
        }
        foreach (var state in states)
        {
            var corState = scheme.states.ElementAt(states.IndexOf(state));
            List<Line> m_trans = new List<Line>();
            foreach (var tran in corState.transitons)
            {
                var a = new Line(state, states.ElementAt(scheme.states.IndexOf(tran.endState)), tran.conditions, tran.hasExitTime);
                m_trans.Add(a);
                statesSpace.Add(a);
            }
            state.trans.Clear();
            state.trans.AddRange(m_trans);
        }
        inspectedSchemePath = path;
        if (ASField.file == null)
        {
            ASField.file = AssetDatabase.LoadAssetAtPath(path, typeof(DefaultAsset)) as DefaultAsset;
        }
    }
    private void Update()
    {

    }


    StateBox CreateState(Vector3 pos, string name)
    {
        if(states.Find(x => x.stateName == name) != null)
        {
            int i = 1;
            while (states.Find(x => x.stateName == name+"_"+i) != null)
            {
                i++;
            }
            name += "_" + i;
        }

        var s = new StateBox(name);
        states.Add(s);
        s.transform.position = pos;
        statesSpace.Add(s);
        return s;
    }

    /// <summary>
    /// Set active element which can be inspected, or used for other reasons
    /// </summary>
    /// <param name="focusOn"></param>
    public void FocusOn(VisualElement focusOn)
    {
        activeElement = focusOn;

        if (focusOn == null) return;
        rightpane.Clear();
        if (focusOn.GetType() == typeof(Line))
        {
            rightpane.Clear();
            if (!(focusOn as Line).set) return;
            rightpane.Add(new Label((focusOn as Line).startState.stateName + " --> " + (focusOn as Line).endState.stateName));
            var het = new Toggle("Has Exit Time");
            het.value = (activeElement as Line).hasExitTime;
            het.RegisterValueChangedCallback(x => (activeElement as Line).hasExitTime = x.newValue);
            rightpane.Add(het);
            rightpane.Add(new Label("Conditions"));
            var list = DrawCondList(focusOn as Line);
            var but = new Button(() =>
            {
                (focusOn as Line).conditions.Add(new Condition());
                rightpane.RemoveAt(5);//CHANGE THIS EVERY TIME YOU ADD NEW VISUAL ELEMENT TO THis INTERFACE
                list = DrawCondList(focusOn as Line);
                rightpane.Add(list);

            });
            but.text = "Add Conditon";

            var but2 = new Button(() =>
            {
                list.Refresh();
                if (list.selectedIndex >= 0) (focusOn as Line).conditions.RemoveAt(list.selectedIndex);
                rightpane.RemoveAt(5);//CHANGE THIS EVERY TIME YOU ADD NEW VISUAL ELEMENT TO THis INTERFACE
                list = DrawCondList(focusOn as Line);
                rightpane.Add(list);


            });
            but2.text = "Remove condition";



            rightpane.Add(but);
            rightpane.Add(but2);
            rightpane.Add(list);
        }
        else if (focusOn.GetType() == typeof(StateBox))
        {
            rightpane.Clear();
            rightpane.Add(new Label((activeElement as StateBox).stateName));
            rightpane.Add(new StateBoxDrawer(activeElement as StateBox).GetVisualElement());

        }
    }
    ListView DrawCondList(Line trans)
    {
        var conds = trans.conditions;
        var items = new List<VisualElement>();
        foreach (var cond in conds)
        {
            items.Add(new ConditionDrawer(cond).GetContentDrawer());
        }
        var listView = new ListView(items, 120, () => new Box(), (e, i) => { e.Clear(); e.Add(items[i]); });
        listView.style.flexGrow = 1f;
        return listView;


    }

}
public static class AnimSchemeExtensions
{
    public static void InitializeScheme(this AnimatorScheme scheme, List<StateBox> boxes)
    {
        foreach (var box in boxes)
        {
            scheme.states.Add(new StateInfo(box.stateName, box.transform.position, AssetDatabase.GetAssetPath(box.animation)));
        }
        foreach (var state in scheme.states)
        {
            var corBox = boxes.ElementAt(scheme.states.IndexOf(state));
            List<AnimatorScheme.Transition> m_trans = new List<AnimatorScheme.Transition>();
            foreach (var tran in corBox.trans)
            {
                m_trans.Add(new AnimatorScheme.Transition(state, scheme.states.ElementAt(boxes.IndexOf(tran.endState)), tran.conditions, tran.hasExitTime));
            }
            state.transitons.Clear();
            state.transitons.AddRange(m_trans);
        }

    }
}

