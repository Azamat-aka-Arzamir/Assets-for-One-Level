using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class AnimatorScheme
{
	public List<StateInfo> states { get; } = new List<StateInfo>();
	[SerializeField] int num = 0;
	public void Initialize(List<StateBox> boxes)
	{
	num = 0;
		foreach (var box in boxes)
		{
			num++;
			states.Add(new StateInfo(box.stateName, box.transform.position));
		}
		foreach (var state in states)
		{
			var corBox = boxes.ElementAt(states.IndexOf(state));
			List<Transition> m_trans = new List<Transition>();
			foreach (var tran in corBox.trans)
			{
				m_trans.Add(new Transition(state, states.ElementAt(boxes.IndexOf(tran.endState)), tran.conditions));
			}
			state.transitons.Clear();
			state.transitons.AddRange(m_trans);
		}

		Save();
	}
	void Save()
	{
		var formatter = new BinaryFormatter();
		var fileStream = new FileStream("Assets/Animators/semen", FileMode.Create);
		formatter.Serialize(fileStream, this);
		fileStream.Close();
	}
	public static AnimatorScheme LoadFromFile(string path)
	{
		var formatter = new BinaryFormatter();
		var fileStream = new FileStream("Assets/Animators/semen", FileMode.Open);
		var aS = (AnimatorScheme)formatter.Deserialize(fileStream);
		fileStream.Close();
		return aS;
	}
	[System.Serializable]
	public struct StateInfo
	{
		public string name { get; }
		public float[] position { get; }
		public List<Transition> transitons { get; internal set; }

		//animation, frames etc.
		public StateInfo(string _text, Vector2 _pos)
		{
			name = _text;
			position = new float[] {_pos.x,_pos.y};
			transitons = new List<Transition>();
		}

	}
	[System.Serializable]
	public struct Transition
	{
		public StateInfo startState { get; }
		public StateInfo endState { get; }
		public List<Condition> conditions { get; }
		public Transition(StateInfo ss, StateInfo es, List<Condition> _conditions)
		{
			startState = ss;
			endState = es;
			conditions = _conditions;
		}
	}
}



