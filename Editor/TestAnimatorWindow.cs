
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
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
	Box statesSpace;
	public List<StateBox> states = new List<StateBox>();

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
		Button btn2 = new Button(Save);
		btn2.text = "Save";
		Button btn3 = new Button(Load);
		btn3.text = "Load";
		controlPanel.Add(btn);
		controlPanel.Add(btn2);
		controlPanel.Add(btn3);
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
				CreateState(x.mousePosition, DragAndDrop.objectReferences[0].name);
			}
		});
	}
	void Save()
	{

		var scheme = new AnimatorScheme();
		scheme.Initialize(states);
		if (!AssetDatabase.IsValidFolder("Assets/Animators"))
		{
			System.IO.Directory.CreateDirectory("Assets/Animators");
		}
		AssetDatabase.Refresh();
		//AssetDatabase.CreateAsset(scheme, "Assets/Animators/cum.asset");
		//AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(scheme));


	}
	void Load()
	{
		AnimatorScheme scheme = AnimatorScheme.LoadFromFile("Assets/Animators/semen");
		states.Clear();
		statesSpace.Clear();
		Debug.Log(scheme.states.Count);
		foreach (var state in scheme.states)
		{
			var s = new StateBox(state.name);
			states.Add(s);
			s.transform.position = new Vector3(state.position[0], state.position[1]);
			statesSpace.Add(s);
		}
		foreach (var state in states)
		{
			var corState = scheme.states.ElementAt(states.IndexOf(state));
			List<Line> m_trans = new List<Line>();
			foreach (var tran in corState.transitons)
			{
				var a = new Line(state, states.ElementAt(scheme.states.IndexOf(tran.endState)), tran.conditions);
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


	void CreateState(Vector3 pos, string name)
	{
		var s = new StateBox(name);
		states.Add(s);
		s.transform.position = pos;
		statesSpace.Add(s);
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
			if (!(focusOn as Line).set) return;
			rightpane.Add(new Label((focusOn as Line).startState.stateName + " --> " + (focusOn as Line).endState.stateName));
			rightpane.Add(new Label("Conditions"));
			var list = DrawCondList(focusOn as Line);
			var but = new Button(() =>
			{
				(focusOn as Line).conditions.Add(new Condition());
				rightpane.RemoveAt(4);
				list = DrawCondList(focusOn as Line);
				rightpane.Add(list);

			});
			but.text = "Add Conditon";

			var but2 = new Button(() =>
			{
				list.Refresh();
				if (list.selectedIndex >= 0) (focusOn as Line).conditions.RemoveAt(list.selectedIndex);
				rightpane.RemoveAt(4);
				list = DrawCondList(focusOn as Line);
				rightpane.Add(list);


			});
			but2.text = "Remove condition";



			rightpane.Add(but);
			rightpane.Add(but2);
			rightpane.Add(list);
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
