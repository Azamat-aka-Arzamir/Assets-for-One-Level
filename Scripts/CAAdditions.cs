using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
public class Condition
{
	public FieldInfo property;
	public Component objectRef;
	private object _value;
	public object value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}
	public enum CondType { E, G, L, LOE, GOE, NE }
	public CondType type;
	bool IsTrue()
	{

		var a = property.GetValue(objectRef);
		if (value.GetType() == typeof(string) || value.GetType() == typeof(bool))
		{
			switch (type)
			{
				case CondType.E:
					if (a == value) return true;
					else return false;
				case CondType.NE:
					if (a != value) return true;
					else return false;
				default:
					throw new Exception("Wrong operation in some condition (FIND IT BY YOURSELF, BITCH!)\n"+"ok, property holder on "+objectRef+" and its name is  "+property.Name);
			}
		}
		else
		{
			var b = (float)a;
			var c = (float)value;
			switch (type)
			{
				case CondType.E:
					if (b == c) return true;
					break;
				case CondType.G:
					if (b > c) return true;
					break;
				case CondType.L:
					if (b < c) return true;
					break;
				case CondType.GOE:
					if (b >= c) return true;
					break;
				case CondType.LOE:
					if (b <= c) return true;
					break;
				case CondType.NE:
					if (b != c) return true;
					break;
			}
			return false;
		}
	}
}
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

		objField.value = inspectedCondition.objectRef;
		objField.RegisterValueChangedCallback(x =>
		{
			if (x.newValue != null && x.newValue.GetType().GetFields().Length > 0)
			{
				inspectedCondition.objectRef = x.newValue as Component;
				//parentVE.Remove(propField);
				DrawFieldsPopUp();
			}
			if (objField.value != inspectedCondition.objectRef) objField.value = inspectedCondition.objectRef;
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
		varsList.AddRange(inspectedCondition.objectRef.GetType().GetFields());
		propField = new PopupField<FieldInfo>("Field", varsList, varsList[0]);
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
#endif
}
public struct State
{
	public string name;
}
public struct Transition
{
	public State start;
	public State end;
	public List<Condition> conditions;
}



