using ICSharpCode.NRefactory.Ast;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;
using static GluonGui.WorkspaceWindow.Views.WorkspaceExplorer.Configuration.ConfigurationTreeNodeCheck;
using static TestAnimatorWindow;
using static UnityEditor.Rendering.FilterWindow;
using static UnityEngine.GraphicsBuffer;


public class TestAnimatorWindow : EditorWindow
{
	StateBox activeState;
	[MenuItem("Window/UI Toolkit/TestAnimatorWindow")]
	public static void ShowExample()
	{
		TestAnimatorWindow wnd = GetWindow<TestAnimatorWindow>();
		wnd.titleContent = new GUIContent("TestAnimatorWindow");
		wnd.maximized = true;
	}
	TwoPaneSplitView panes;
	VisualElement leftpane;
	VisualElement rightpane;
	Label stateName;
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
		Button btn = new Button(CreateState);
		root.Add(btn);

		stateName = new Label();
		rightpane.Add(stateName);

		AnimEventHandler.AddListenerToTransitionEvent<StateBox>(StartMakingTransition, AnimEventHandler.eventType.start);
		AnimEventHandler.AddListenerToTransitionEvent<StateBox>(StopMakingTransition, AnimEventHandler.eventType.stop);
		AnimEventHandler.SetActiveTransition.AddListener(DrawTransitionWindow);
		//AnimEventHandler.AddListenerToTransitionEvent<Line>(RemoveActiveLine, AnimEventHandler.eventType.removeline);
	}
	private void Update()
	{

	}
	Line activeLine;
	void StartMakingTransition(StateBox box)
	{
		activeLine = new Line(box);
		leftpane.Add(activeLine);
		rootVisualElement.RegisterCallback<PointerMoveEvent>(activeLine.OnMouseMove);
		rootVisualElement.RegisterCallback<PointerDownEvent>(activeLine.OnPointerDown);
	}
	void StopMakingTransition(StateBox box)
	{
		activeLine.Set(box);
		activeLine = null;
	}

	void DrawTransitionWindow(Line activeState)
	{
		rightpane.Clear();
		rightpane.Add(new ConditionDrawer(activeState.condition).GetContentDrawer());
	}


	List<StateBox> stateBoxes = new List<StateBox>();
	void CreateState()
	{
		var s = new StateBox("New State", this);
		s.transform.position = new Vector3(UnityEngine.Random.Range(0f, 50f), UnityEngine.Random.Range(0f, 50f));
		leftpane.Add(s);
		stateBoxes.Add(s);

	}
	public class Line : Image
	{
		//delete
		public Condition condition = new Condition();

		private Vector3 start;
		private Vector3 end;
		static Texture texture;
		bool set = false;

		public Line(StateBox startBox)
		{
			start = startBox.center;
			startBox.AddOnMoveListener(OnStartMoved);
			if (texture == null)
			{
				GetTexture();
			}
			image = texture;
			style.width = 10;
			style.height = 10;
			style.position = Position.Absolute;
			transform.position = Vector3.zero;
			RegisterCallback<PointerDownEvent>(OnPointerDownLocal);
		}
		public void OnPointerDown(PointerDownEvent evt)
		{
			if (!set && evt.button == 1)
			{
				parent.Remove(this);
			}

		}
		void OnPointerDownLocal(PointerDownEvent evt)
		{
			if (set && evt.button == 1 && evt.shiftKey)
			{
				parent.Remove(this);
			}
			if (set && evt.button == 0)
			{
				AnimEventHandler.SetActiveTransition.Invoke(this);
			}
		}


		public void OnMouseMove(PointerMoveEvent evt)
		{
			if (!set) UpdatePos(start, evt.position);
		}
		public void OnEndMoved(StateBox endBox)
		{
			UpdatePos(start, endBox.center);
		}
		public void OnStartMoved(StateBox startBox)
		{
			UpdatePos(startBox.center, end);
		}
		public void UpdatePos(Vector3 spos, Vector3 epos)
		{
			start = spos;
			end = epos;
			style.width = (epos - spos).magnitude;
			transform.position = spos;
			transform.rotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.right, (epos - spos), Vector3.forward));

		}
		public void Set(StateBox box)
		{
			box.AddOnMoveListener(OnEndMoved);
			set = true;
		}
		public static void GetTexture()
		{
			texture = (AssetDatabase.LoadAssetAtPath("Assets/Defaults/Editor/SpriteBox.png", typeof(Sprite)) as Sprite).texture;
		}


	}
	public class StateBox : Image
	{


		public static Texture stateBox;
		//Only for inner use to have int values;
		private Vector2Int size;
		Box textbox;
		public Vector3 center
		{
			get
			{
				return worldBound.center;
			}
		}
		public StateBox(string text, TestAnimatorWindow parentWindow)
		{
			parentWindow.rootVisualElement.RegisterCallback<PointerMoveEvent>(Drag);
			InitializeStateBox(text);
		}
		public StateBox(string text)
		{
			InitializeStateBox(text);
		}
		void InitializeStateBox(string text)
		{
			GetTexture();
			image = stateBox;
			size.x = stateBox.width / 5;
			size.y = stateBox.height / 5;
			style.width = size.x;
			style.height = size.y;
			var _text = new Label(text);
			textbox = new Box();
			textbox.style.position = Position.Absolute;
			style.position = Position.Absolute;
			textbox.Add(new Label(text));
			textbox.style.backgroundColor = Color.clear;
			Add(textbox);

			RegisterCallback<GeometryChangedEvent>((evt) => SetCenter());
			RegisterCallback<PointerDownEvent>(StartDrag);

			RegisterCallback<PointerUpEvent>(StopDrag);
			RegisterCallback<PointerDownEvent>(InvokeTransitionEndEvent);
			RegisterCallback<PointerOutEvent>(StopDrag);

			ContextualMenuManipulator m = new ContextualMenuManipulator(ContextMenuActions);
			m.target = this;

		}
		void InvokeTransitionEndEvent(PointerDownEvent evt)
		{
			AnimEventHandler.Stop(this);
		}

		void ContextMenuActions(ContextualMenuPopulateEvent _event)
		{
			_event.menu.AppendAction("Make transition", MakeTransition, DropdownMenuAction.AlwaysEnabled);
		}
		bool makingTrans = false;
		void MakeTransition(DropdownMenuAction action)
		{
			Debug.Log("Im fucking CUMMING!!!!");
			makingTrans = true;
			if (makingTrans)
			{
				AnimEventHandler.Invoke(this);
			}
		}

		Vector2 targetStartPosition;
		Vector3 pointerStartPosition;
		bool isDragged;
		void StartDrag(PointerDownEvent evt)
		{
			if (evt.button != 0) return;
			AnimEventHandler.SetActiveStateBox.Invoke(this);
			isDragged = true;
			targetStartPosition = transform.position;
			pointerStartPosition = evt.position;
			//DragAndDrop.StartDrag("Dragging title");
		}
		List<Action<StateBox>> onMoveListeners = new List<Action<StateBox>>();
		void Drag(PointerMoveEvent evt)
		{
			if (!isDragged) return;
			Vector3 pointerDelta = evt.position - pointerStartPosition;

			transform.position = new Vector2(
				Mathf.Clamp(targetStartPosition.x + pointerDelta.x, 0, panel.visualTree.worldBound.width),
				Mathf.Clamp(targetStartPosition.y + pointerDelta.y, 0, panel.visualTree.worldBound.height));

			foreach (var listener in onMoveListeners)
			{
				listener(this);
			}
		}
		void StopDrag(PointerOutEvent evt)
		{
			//isDragged = false;
		}
		public void AddOnMoveListener(Action<StateBox> action)
		{
			onMoveListeners.Add(action);
		}
		void StopDrag(PointerUpEvent evt)
		{
			isDragged = false;
		}
		public static void GetTexture()
		{
			stateBox = (AssetDatabase.LoadAssetAtPath("Assets/Defaults/Editor/SpriteBox.png", typeof(Sprite)) as Sprite).texture;
		}

		public void SetCenter()
		{
			textbox.transform.position = new Vector3(size.x / 2 - textbox.layout.width / 2, size.y / 2 - textbox.layout.height / 2);
			style.width = size.x;
			style.height = size.y;
		}
	}
	public class AnimEvent<N>
	{
		List<Action<N>> listeners = new List<Action<N>>();
		public void Invoke(N returnObject)
		{
			foreach (var listener in listeners)
			{
				listener.Invoke(returnObject);
			}
		}
		public void AddListener(Action<N> action)
		{
			listeners.Add(action);
		}
		public void RemoveListener(Action<N> action)
		{
			listeners.Remove(action);
		}
	}
	public struct AnimEventHandler
	{
		static List<Action<StateBox>> Listeners = new List<Action<StateBox>>();
		static List<Action<StateBox>> ListenersToStop = new List<Action<StateBox>>();
		static List<Action<Line>> RemoveLine = new List<Action<Line>>();
		public static AnimEvent<StateBox> SetActiveStateBox = new AnimEvent<StateBox>();


		//must be Transition type
		public static AnimEvent<Line> SetActiveTransition = new AnimEvent<Line>();
		public enum eventType { start, stop, removeline };
		public static void AddListenerToTransitionEvent<T>(Action<T> action, eventType type)
		{
			if (action.GetType() == typeof(Action<StateBox>))
			{
				if (type == eventType.start) Listeners.Add(action as Action<StateBox>);
				else if (type == eventType.stop) ListenersToStop.Add(action as Action<StateBox>);

			}
			else if (action.GetType() == typeof(Action<Line>))
			{
				if (type == eventType.removeline) RemoveLine.Add(action as Action<Line>);
			}
		}
		public static bool Invoked = false;
		public static void Invoke(StateBox invoker)
		{
			Invoked = true;
			foreach (var a in Listeners)
			{
				a(invoker);
			}
		}
		public static void Invoke(Line invoker, eventType evt)
		{
			Invoked = true;
			foreach (var a in RemoveLine)
			{
				a(invoker);
			}
		}
		public static void Stop(StateBox invoker)
		{
			if (!Invoked) return;
			Invoked = false;
			foreach (var a in ListenersToStop)
			{
				a(invoker);
			}
		}

	}
}
