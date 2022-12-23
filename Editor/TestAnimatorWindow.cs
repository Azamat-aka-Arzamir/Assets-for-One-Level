
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
	public static void ShowExample()
	{
		TestAnimatorWindow wnd = GetWindow<TestAnimatorWindow>();
		wnd.titleContent = new GUIContent("TestAnimatorWindow");
		wnd.maximized = true;
	}
	TwoPaneSplitView panes;
	VisualElement leftpane;
	VisualElement rightpane;
	TextField commandLine;
	FileField ASField;
	Box statesSpace;
	public List<StateBox> states = new List<StateBox>();
	AnimatorScheme inspectedScheme;

	public VisualElement activeElement { get; private set; }
	VisualElement ActiveElement;
	public void CreateGUI()
	{
		VisualElement root = rootVisualElement;
		panes = new TwoPaneSplitView(0, 700, TwoPaneSplitViewOrientation.Horizontal);
		leftpane = new VisualElement();
		rightpane = new VisualElement();
		rightpane.style.backgroundColor = new Color(75 / 255f, 75 / 255f, 75 / 255f);
		panes.Add(leftpane);
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
		ASField.RegisterCallback<ChangeEvent<string>>(x => Load(ASField.path));

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
		statesSpace.style.position = Position.Absolute;

		leftpane.Add(statesSpace);
		leftpane.RegisterCallback<DragUpdatedEvent>((x) => {

			if (DragAndDrop.objectReferences[0].GetType() == typeof(CustomAnimation))
			{
				DragAndDrop.visualMode = DragAndDropVisualMode.Link;
			}
		});
		leftpane.RegisterCallback<DragPerformEvent>((x) =>
		{
			if (DragAndDrop.objectReferences[0].GetType() == typeof(CustomAnimation))
			{
				CreateState(x.mousePosition, DragAndDrop.objectReferences[0].name).animation = DragAndDrop.objectReferences[0] as CustomAnimation;
			}
		});
	}
	class FileField : VisualElement
	{
		Box box = new Box();
		VisualElement label = new Label("null");
		public DefaultAsset file;
		public string path;
		public string fileName = null;
		public FileField()
		{
            ContextualMenuManipulator m = new ContextualMenuManipulator(ContextMenuActions);
            m.target = this;
            box.style.width = 100;
			box.style.height = 20;
			box.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
			box.Add(label);
			Add(box);
            box.RegisterCallback<DragUpdatedEvent>((x) => {

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
					path = AssetDatabase.GetAssetPath(file);
					fileName = file.name;
					(label as Label).text = DragAndDrop.objectReferences[0].name;
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
		"ld x - Load x-named scheme from defaults !without saving current scheme!\n";
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
				if(commandLine.value.Split(' ').Length == 1)
				{
					throw new System.Exception("Incorrect Argument");
				}
				arg = commandLine.value.Split(' ')[1];
				Load("Defaults/" + arg);
				break;
			case "help":
				Debug.Log(helpData);
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
			for(int i = 0;i< splittedPath.Length-1;i++)
			{
				string p = splittedPath[i];
				if (!AssetDatabase.IsValidFolder("Assets/Animators"+pa+p))
				{
					System.IO.Directory.CreateDirectory("Assets/Animators"+pa+p);
				}
				pa += p + "/";
			}
		}

		var formatter = new BinaryFormatter();
		FileStream fileStream = new FileStream("Assets/Animators/"+path,FileMode.Create);
		formatter.Serialize(fileStream, scheme);
        
        fileStream.Close();
        AssetDatabase.Refresh();
    }

    void Load(string path)
	{

		var p=path.Split('/');
		//p[p.Length - 1] = "";
		string folderOnly="";
		for(int i = 0; i < p.Length-1; i++)
		{
			if (p[i] != "Animators" && p[i] != "Assets")
			{
				folderOnly += p[i];
				if (i < p.Length - 2) folderOnly += "/";
			}
		}
        if (!Directory.Exists("Assets/Animators/" +folderOnly))
		{
			throw new System.Exception("Loading failed - Incorrect Directory: Assets/Animators/" + folderOnly+"/");
		}

		var formatter = new BinaryFormatter();
		var fileStream = new FileStream("Assets/Animators/" + folderOnly+"/"+ p[p.Length - 1], FileMode.Open);
		var scheme= (AnimatorScheme)formatter.Deserialize(fileStream);

		fileStream.Close();
		states.Clear();
		statesSpace.Clear();
		Debug.Log(scheme.states.Count);
		foreach (var state in scheme.states)
		{
			var s = new StateBox(state.name);
			states.Add(s);
			s.transform.position = new Vector3(state.position[0], state.position[1]);
			s.animation = AssetDatabase.LoadAssetAtPath(state.animationPath,typeof(CustomAnimation)) as CustomAnimation;
			statesSpace.Add(s);
		}
		foreach (var state in states)
		{
			var corState = scheme.states.ElementAt(states.IndexOf(state));
			List<Line> m_trans = new List<Line>();
			foreach (var tran in corState.transitons)
			{
				var a = new Line(state, states.ElementAt(scheme.states.IndexOf(tran.endState)), tran.conditions,tran.hasExitTime);
				m_trans.Add(a);
				statesSpace.Add(a);
			}
			state.trans.Clear();
			state.trans.AddRange(m_trans);
		}
	}
	private void Update()
	{

	}


	StateBox CreateState(Vector3 pos, string name)
	{
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
		Debug.Log(focusOn.GetType().ToString());
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
		else if(focusOn.GetType() == typeof(StateBox))
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
		var listView = new ListView(items, 100, () => new Box(), (e, i) => { e.Clear(); e.Add(items[i]); });
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

