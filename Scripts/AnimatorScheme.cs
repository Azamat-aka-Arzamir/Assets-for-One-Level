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
}
[System.Serializable]
public class Condition
{
    public FieldInfo property;
    public System.Reflection.TypeInfo objectRef;
    public int objectHash;
    public object value;

    public enum CondType { E, G, L, LOE, GOE, NE }
    public CondType type;
    public bool IsTrue()
    {

        var a = property.GetValue(GetObject());
        if (property.GetType() == typeof(string) || property.GetType() == typeof(bool))
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
                    throw new Exception("Wrong operation in some condition (FIND IT BY YOURSELF, BITCH!)\n" + "ok, property holder on " + GetObject() + " and its name is  " + property.Name);
            }
        }
        else
        {
            float b = 0;
            float c = 0;
            if (a.GetType() == typeof(int))
            {
                b = (int)a;
                c= (int)value;
            }
            else if(a.GetType() == typeof(float))
            {
                b = (float)a;
                c = (float)value;
            }
            else if(a.GetType() == typeof(double))
            {
                b = (float)(double)a ;
                c= (float)(double)value;
            }

            switch (type)
            {
                case CondType.E:
                    if (a == value) return true;
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
    public UnityEngine.Object GetObject()
    {
        if (objectRef == null || objectHash == 0) return null;
        List<UnityEngine.Object> list = new List<UnityEngine.Object>();
        list.AddRange(UnityEngine.Object.FindObjectsOfType(objectRef));
        return list.Find((x) => x.GetHashCode() == objectHash);
    }
}




