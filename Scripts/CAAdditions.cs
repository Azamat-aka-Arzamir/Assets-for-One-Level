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
	void IsTrue()
	{

	}
}
#if UNITY_EDITOR
public class ConditionDrawer
{
	Condition inspectedCondition;
	VisualElement parentVE = new VisualElement();
	public ConditionDrawer(Condition cond)
	{
		inspectedCondition = cond;
	}

	public VisualElement GetContentDrawer()
	{
		
		ObjectField objectRef = new ObjectField("Component");
		objectRef.objectType = typeof(Component);
		parentVE.Add(objectRef);
		objectRef.value = inspectedCondition.objectRef;
		objectRef.RegisterValueChangedCallback(x =>
		{
			if (x.newValue !=null&& x.newValue.GetType().GetFields().Length > 0) inspectedCondition.objectRef = x.newValue as Component;
		});
		if(objectRef.value!=inspectedCondition.objectRef) objectRef.value = inspectedCondition.objectRef;
		if (objectRef.value != null)
		{
			DrawFieldsPopUp(inspectedCondition, "Field", parentVE);
		}
		#region switch case for types
		System.Type type = null;
		if (inspectedCondition.property != null) type = inspectedCondition.property.FieldType;

		if (type == typeof(int))
		{
			var value = new IntegerField("Value");
			if (inspectedCondition.value != null && inspectedCondition.value.GetType() == type) value.value = (int)inspectedCondition.value;
			parentVE.Add(value);
			value.RegisterValueChangedCallback(x => inspectedCondition.value = x.newValue);

		}
		else if (type == typeof(string))
		{
			var value = new TextField("Value");
			if (inspectedCondition.value != null && inspectedCondition.value.GetType() == type) value.value = (string)inspectedCondition.value;
			parentVE.Add(value);
			value.RegisterValueChangedCallback(x => inspectedCondition.value = x.newValue);

		}
		else if (type == typeof(float))
		{
			var value = new FloatField("Value");
			if (inspectedCondition.value != null && inspectedCondition.value.GetType() == type) value.value = (float)inspectedCondition.value;
			parentVE.Add(value);
			value.RegisterValueChangedCallback(x => inspectedCondition.value = x.newValue);

		}
		else if (type == typeof(bool))
		{
			var value = new Toggle("Value");
			if (inspectedCondition.value != null && inspectedCondition.value.GetType() == type) value.value = (bool)inspectedCondition.value;
			parentVE.Add(value);
			value.RegisterValueChangedCallback(x => inspectedCondition.value = x.newValue);

		}
		#endregion

		return parentVE;

	}

	//This method is used only once, the only reason of this decision is "not a null check",
	//where this method is used as delegate in case if waiting is needed
	private void DrawFieldsPopUp(Condition inspectedCondition, string label, VisualElement addTo)
	{
		List<FieldInfo> varsList = new List<FieldInfo>();
		varsList.AddRange(inspectedCondition.objectRef.GetType().GetFields());
		PopupField<FieldInfo> variables = new PopupField<FieldInfo>(label, varsList, varsList[0]);
		variables.RegisterValueChangedCallback(x => inspectedCondition.property = x.newValue);
		if (inspectedCondition.property != null) variables.value = inspectedCondition.property;
		addTo.Add(variables);
	}
	List<VisualElement> FieldsWaitingForNewValue = new List<VisualElement>();
	/*private void CheckForUpdates()
	{
		Action<object, VisualElement> answer = (x, y) =>
		{
			Debug.Log($"{x.GetType().Name} came");
			
			CheckForUpdates();

			FieldsWaitingForNewValue.Remove(y);
		};

		foreach (var field in parentVE.Children())
		{
			if (!FieldsWaitingForNewValue.Contains(field))
			{
				if (field as Toggle != null) (field as Toggle).RegisterValueChangedCallback(x => answer(x.newValue, field));
				if (field as TextField != null) (field as TextField).RegisterValueChangedCallback(x => answer(x.newValue, field));
				if (field as IntegerField != null) (field as IntegerField).RegisterValueChangedCallback(x => answer(x.newValue, field));
				if (field as FloatField != null) (field as FloatField).RegisterValueChangedCallback(x => answer(x.newValue, field));
				if (field as ObjectField != null) (field as ObjectField).RegisterValueChangedCallback(x => answer(x.newValue, field));
				if (field as PopupField<FieldInfo> != null) (field as PopupField<FieldInfo>).RegisterValueChangedCallback(x => answer(x.newValue, field));
				FieldsWaitingForNewValue.Add(field);
				Debug.Log(FieldsWaitingForNewValue.Count);
			}

		}
	}*/
#endif
}



