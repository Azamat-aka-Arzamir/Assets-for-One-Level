using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using Codice.Client.BaseCommands;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditorInternal;
using System.Runtime.Remoting.Messaging;


#if UNITY_EDITOR
public class ConditionDrawer
{
	Condition inspectedCondition;
	VisualElement parentVE = new VisualElement();
	ObjectField objField = new ObjectField();
	PopupField<FieldInfo> propField = new PopupField<FieldInfo>("Field");
	VisualElement valueField = new Label("Value");
	EnumField operField = new EnumField("Operation");
	public ConditionDrawer(Condition cond)
	{
		inspectedCondition = cond;
	}
	const int ObjPlace = 0;
	const int PropPlace = 1;
	const int ValuePlace = 2;

	public VisualElement GetContentDrawer()
	{

		objField = new ObjectField("Component");
		objField.objectType = typeof(Component);
		operField.Init(Condition.CondType.E);
		operField.value = inspectedCondition.type;
		operField.RegisterValueChangedCallback(x => inspectedCondition.type = (Condition.CondType)x.newValue);
		parentVE.Add(objField);
		parentVE.Add(propField);
		parentVE.Add(valueField);
		parentVE.Add(operField);

		objField.value = inspectedCondition.GetObject();
		objField.RegisterValueChangedCallback(x =>
		{
			if (x.newValue != null && x.newValue.GetType().GetFields().Length > 0)
			{
				inspectedCondition.objectRef = x.newValue.GetType().GetTypeInfo();
				inspectedCondition.objectHash = x.newValue.GetHashCode();
				//parentVE.Remove(propField);
				DrawFieldsPopUp();
			}
			if (objField.value != inspectedCondition.GetObject()) objField.value = inspectedCondition.GetObject();
		});

		if (inspectedCondition.objectRef != null) DrawFieldsPopUp();
		if (inspectedCondition.property != null) DrawValueField();

		return parentVE;

	}

	//This method is used only once, the only reason of this decision is "not a null check",
	//where this method is used as delegate in case if waiting is needed
	private void DrawFieldsPopUp()
	{
		List<FieldInfo> varsList = new List<FieldInfo>();
		varsList.AddRange(inspectedCondition.objectRef.GetFields());
		propField = new PopupField<FieldInfo>("Field", varsList, varsList[0]);
        DrawValueField();
        propField.RegisterValueChangedCallback(x => { inspectedCondition.property = x.newValue; DrawValueField(); });
		if (inspectedCondition.property != null && varsList.Contains(inspectedCondition.property)) propField.value = inspectedCondition.property;
		else inspectedCondition.property = null;
		parentVE.RemoveAt(PropPlace);
		parentVE.Insert(PropPlace, propField);
	}

	private void DrawValueField()
	{
		#region switch case for types
		System.Type type = null;
		if (inspectedCondition.property != null) type = inspectedCondition.property.FieldType;

		if (type == typeof(int))
		{
			var value = new IntegerField("Value");
			if (inspectedCondition.value != null && inspectedCondition.value.GetType() == type) value.value = (int)inspectedCondition.value;
			value.RegisterValueChangedCallback(x => inspectedCondition.value = x.newValue);
			valueField = value;

		}
		else if (type == typeof(string))
		{
			var value = new TextField("Value");
			if (inspectedCondition.value != null && inspectedCondition.value.GetType() == type) value.value = (string)inspectedCondition.value;
			value.RegisterValueChangedCallback(x => inspectedCondition.value = x.newValue);
			valueField = value;

		}
		else if (type == typeof(float))
		{
			var value = new FloatField("Value");
			if (inspectedCondition.value != null && inspectedCondition.value.GetType() == type) value.value = (float)inspectedCondition.value;
			value.RegisterValueChangedCallback(x => inspectedCondition.value = x.newValue);
			valueField = value;

		}
		else if (type == typeof(bool))
		{
			var value = new Toggle("Value");
			if (inspectedCondition.value != null && inspectedCondition.value.GetType() == type) value.value = (bool)inspectedCondition.value;
			value.RegisterValueChangedCallback(x => inspectedCondition.value = x.newValue);
			valueField = value;

		}
		parentVE.RemoveAt(ValuePlace);
		parentVE.Insert(ValuePlace, valueField);
		#endregion
	}

}

public class StateBox : Image
{
	TestAnimatorWindow parentWindow;
	bool isRenaming;
	static Texture stateBox;
	//Only for inner use to have int values;
	private Vector2Int size;
	Box textbox;
	public string stateName;
	public CustomAnimation animation;
	VisualElement textLabel = new Label();
	public  List<Line> trans { get; protected set; } = new List<Line>();
	public List<Line> endOnMe { get; protected set; } = new List<Line>();
	public StateBox(string text)
	{
		parentWindow = EditorWindow.GetWindow<TestAnimatorWindow>();
		GetTexture();
		image = stateBox;
		size.x = stateBox.width /2;
		size.y = stateBox.height /2;
		style.width = size.x;
		style.height = size.y;
		textbox = new Box();
		textbox.style.position = Position.Absolute;
		style.position = Position.Absolute;
		stateName = text;
		(textLabel as Label).text = text;
		textbox.Add(textLabel);
		textbox.style.backgroundColor = Color.clear;
		Add(textbox);

		RegisterCallback<GeometryChangedEvent>((evt) => SetCenter());
		RegisterCallback<PointerDownEvent>(OnPointerDown);

		RegisterCallback<PointerUpEvent>(StopDrag);
		parentWindow.rootVisualElement.RegisterCallback<PointerMoveEvent>(Drag);

		ContextualMenuManipulator m = new ContextualMenuManipulator(ContextMenuActions);
		m.target = this;

	}

	void ContextMenuActions(ContextualMenuPopulateEvent _event)
	{
		_event.menu.AppendAction("Make transition", MakeTransition, DropdownMenuAction.AlwaysEnabled);
		_event.menu.AppendAction("Rename", Rename, DropdownMenuAction.AlwaysEnabled);
		_event.menu.AppendAction("Delete", Delete, DropdownMenuAction.AlwaysEnabled);
		_event.menu.AppendSeparator();
		_event.menu.AppendAction("DebugInfo", (x) => Debug.Log(trans.Count + "  " + endOnMe.Count));
	}
	public void Delete(DropdownMenuAction action)
	{
		 for(int i = endOnMe.Count; i > 0; i--)
		{
			var _trans = endOnMe[i-1];
			_trans.startState.trans.Remove(_trans);
			endOnMe.Remove(_trans);
			_trans.parent.Remove(_trans);
		}
		 foreach(var tr in trans)
		{
			tr.parent.Remove(tr);
		}
		parentWindow.states.Remove(this);
		parent.Remove(this);
	}
	void Rename(DropdownMenuAction action)
	{
		isRenaming = true;
		textLabel = new TextField();
		textbox.RemoveAt(0);
		textbox.Insert(0, textLabel);
		(textLabel as TextField).Focus();
		textbox.RegisterCallback<KeyDownEvent>((x) =>
		{
			if (x.keyCode == KeyCode.Return)
			{
				Label l = new Label((textLabel as TextField).text);
				if (l.text == "") l.text = "U fORGOT TO \n WRITE NAME";
				textLabel = l;
				isRenaming = false;
				stateName = l.text;
				textbox.RemoveAt(0);
				textbox.Insert(0, textLabel);
			}
		});

	}
	void MakeTransition(DropdownMenuAction action)
	{
		Debug.Log("Im fucking CUMMING!!!!");
		var newTrans = new Line(this);
		parent.Add(newTrans);
		trans.Add(newTrans);
	}

	Vector2 targetStartPosition;
	Vector3 pointerStartPosition;
	bool isDragged;
	void OnPointerDown(PointerDownEvent evt)
	{
		//Trying to set unset active transition
		var aLine = (parentWindow as TestAnimatorWindow).activeElement;
		if (aLine != null && aLine.GetType() == typeof(Line))
		{
			//if it is already set, it will just ignore set request
			if((aLine as Line).Set(this)) endOnMe.Add(aLine as Line);
            (parentWindow as TestAnimatorWindow).FocusOn(aLine);
        }
        (parentWindow as TestAnimatorWindow).FocusOn(this);


		//preparing for dragging

		if (isRenaming) return;
		if (evt.button != 0) return;
		isDragged = true;
		targetStartPosition = transform.position;
		pointerStartPosition = evt.position;
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
public class StateBoxDrawer
{
	StateBox inspectedState;
	ObjectField animBox;

    public StateBoxDrawer(StateBox _inspectedState)
	{
		inspectedState = _inspectedState;
	}
	public VisualElement GetVisualElement()
	{
		SyncValuesWithInspectedState();
		VisualElement output = new VisualElement();
		animBox.RegisterValueChangedCallback(x=> inspectedState.animation = x.newValue as CustomAnimation);
		output.Add(animBox);
		var listView = new ListView(inspectedState.trans, 16, () =>  new Label(), (e, i) => { (e as Label).text = inspectedState.trans[i].endState.stateName; });
		listView.reorderable = true;
		listView.style.height = inspectedState.trans.Count*16;
		listView.style.width = 100;
        listView.style.flexGrow = 1f;
		var h1 = new Label("Transitions");
		h1.transform.position+=new Vector3(20,20);
		listView.transform.position += new Vector3(20, 50);
        output.Add(h1);

        output.Add(listView);
		
		return output;
	}

	//Sets values in inspector according to inspected StateBox
	void SyncValuesWithInspectedState()
	{
        animBox = new ObjectField();
		animBox.objectType = typeof(CustomAnimation);
		animBox.value = inspectedState.animation;
    }
}
public class Line : Image
{
	TestAnimatorWindow parentWindow;
	//delete
	public List<Condition> conditions = new List<Condition>();
	public bool hasExitTime;
	private Vector3 start;
	private Vector3 end;
	public StateBox startState { get; private set; }
	public StateBox endState { get; private set; }

	static Texture texture;
	public bool set { get; private set; } = false;

	public Line(StateBox startBox)
	{
		startState = startBox;
		parentWindow = EditorWindow.GetWindow<TestAnimatorWindow>();
		parentWindow.rootVisualElement.RegisterCallback<PointerMoveEvent>(OnMouseMove);
		parentWindow.rootVisualElement.RegisterCallback<PointerDownEvent>(OnPointerDown);
		parentWindow.FocusOn(this);
		start = startBox.localBound.center;
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

	public Line(StateBox startBox, StateBox endBox, List<Condition> conds,bool _het)
	{
		hasExitTime = _het;
		conditions = conds;
		set = true;
		startState = startBox;
		endState = endBox;
		parentWindow = EditorWindow.GetWindow<TestAnimatorWindow>();
		parentWindow.FocusOn(this);
		start = startBox.transform.position + Vector3.down*startBox.style.height.value.value;
		end = endBox.transform.position + Vector3.down * startBox.style.height.value.value;
		if (startBox == endBox)
		{
			end += Vector3.down * 20;
		}
		RegisterCallback<GeometryChangedEvent>((x) => UpdatePos(startState.worldBound.center,endState.worldBound.center));
		startBox.AddOnMoveListener(OnStartMoved);
		endBox.AddOnMoveListener(OnEndMoved);
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
		endBox.endOnMe.Add(this);
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
			startState.trans.Remove(this);
			endState.trans.Remove(this);
        }
		if (set && evt.button == 0)
		{
			parentWindow.FocusOn(this);
		}
	}


	public void OnMouseMove(PointerMoveEvent evt)
	{

		if (!set)
		{
			UpdatePos(start, evt.localPosition);
		}
		else parentWindow.rootVisualElement.UnregisterCallback<PointerMoveEvent>(OnMouseMove);
	}
	public void OnEndMoved(StateBox endBox)
	{
		UpdatePos(start, endBox.worldBound.center);
	}
	public void OnStartMoved(StateBox startBox)
	{
		UpdatePos(startBox.worldBound.center, end);
	}
	public void UpdatePos(Vector3 spos, Vector3 epos)
	{
		if (startState == endState)
		{
            start = spos;
			end= epos+Vector3.down*20;
			style.width = 20;
            transform.position = spos;
            transform.rotation = Quaternion.Euler(0, 0, 90);
            return;
		}
		start = spos;
		end = epos;
		style.width = (epos - spos).magnitude;
		transform.position = spos;
		transform.rotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.right, (epos - spos), Vector3.forward));
	}
	public bool Set(StateBox box)
	{
		if (set) return false;
		box.AddOnMoveListener(OnEndMoved);
		set = true;
		endState = box;
		return true;
	}
	public static void GetTexture()
	{
		texture = (AssetDatabase.LoadAssetAtPath("Assets/Defaults/Editor/Line.png", typeof(Sprite)) as Sprite).texture;
	}


}
#endif
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

}




