using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using UnityEngine.Events;
using System.Threading;

[Serializable]
public class AnimatorScheme
{
    public string name;
	public List<StateInfo> states { get; } = new List<StateInfo>();
#if UNITY_EDITOR
	
#endif
	[System.Serializable]
	public struct StateInfo
	{
		public string name { get; }
		public float[] position { get; }
		public List<Transition> transitons { get; internal set; }

		public string animationPath;
		//animation, frames etc.
		public StateInfo(string _text, Vector2 _pos, string animPath)
		{
			name = _text;
			position = new float[] {_pos.x,_pos.y};
			transitons = new List<Transition>();
			animationPath = animPath;
		}

	}
	[System.Serializable]
	public struct Transition
	{
        public bool hasExitTime;
		public StateInfo startState { get; }
		public StateInfo endState { get; }
		public List<Condition> conditions { get; }
		public Transition(StateInfo ss, StateInfo es, List<Condition> _conditions, bool het)
		{
			startState = ss;
			endState = es;
			conditions = _conditions;
            hasExitTime = het;
		}
	}
    public static void AssignObjectsForScheme(AnimatorScheme s)
    {
        foreach(var st in s.states)
        {
            foreach(var tr in st.transitons)
            {
                foreach(var c in tr.conditions)
                {
                    c.FindObject();
                }
            }
        }
    }
}
[System.Serializable]
public class Condition
{
    /// <summary>
    /// Reference to field
    /// </summary>
    public FieldInfo property;
    /// <summary>
    /// Type info
    /// </summary>
    public System.Reflection.TypeInfo typeRef;

    public string objectID;
    /// <summary>
    /// Value to compare with
    /// </summary>
    public object value;
    public bool eventinvoked;
    [System.NonSerialized]
    public Component objectRef;
    public bool localComponent;

    public enum CondType { E, G, L, LOE, GOE, NE }
    public CondType type;
    /// <summary>
    /// Checks condition and needs invoker in case if component is local
    /// </summary>
    /// <param name="invoker">object that called this function. It's supposed to be an animator</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public bool IsTrue(GameObject invoker)
    {
        if (localComponent) 
        {
            if(!invoker.TryGetComponent(typeRef, out objectRef))
            {
                Debug.LogError("There is no " + typeRef + " component on " + invoker.name + " object. Animator fail FATAL");
            }
            else
            {
                objectRef = invoker.GetComponent(typeRef);
            }
        }
        var a = property.GetValue(objectRef);
        if (property.FieldType == typeof(string) || property.FieldType == typeof(bool))
        {
            if(value==null&& property.FieldType == typeof(string))
            {
                value = string.Empty; 
            }
            else if (value == null && (property.FieldType == typeof(bool)))
            {
                value = false;
            }
            switch (type)
            {
                case CondType.E:
                    if (a.Equals(value)) return true;
                    else return false;
                case CondType.NE:
                    if (!a.Equals(value)) return true;
                    else return false;
                default:
                    throw new Exception("Wrong operation in some condition (FIND IT BY YOURSELF, BITCH!)\n" + "ok, property holder on " + objectRef + " and its name is  " + property.Name);
            }
        }
        else if (property.FieldType==typeof(UnityEvent))
        {
            (a as UnityEvent).RemoveListener(ResetInvokeBool);
            (a as UnityEvent).AddListener(ResetInvokeBool);
            if (eventinvoked)
            {
                (a as UnityEvent).RemoveListener(ResetInvokeBool);
                return true;
            }
            else return false;
        }
        else
        {
            if (value == null) value = 0;
            float b = 0;
            float c = 0;
            if (a.GetType() == typeof(int))
            {
                b = (int)a;
                c= Convert.ToInt32(value);
            }
            else if(a.GetType() == typeof(float))
            {
                b = (float)a;
                c = Convert.ToSingle(value);
            }
            else if(a.GetType() == typeof(double))
            {
                b = (float)(double)a ;
                c= Convert.ToSingle(value);
            }

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
    //Call at load
    public void FindObject()
    {
        if (typeRef == null || objectID == null||objectID=="") return;
        List<UnityEngine.Object> list = new List<UnityEngine.Object>();
        list.AddRange(UnityEngine.Object.FindObjectsOfType(typeof(IDCard)));
        var obj = list.Find((x) => (x as IDCard).ID == objectID);
        if(obj!=null)objectRef = (obj as IDCard).gameObject.GetComponent(typeRef);
    }
    void ResetInvokeBool()
    {
        eventinvoked = true;
        Debug.Log("inv");
    }
}






