using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "New Custom Animation", menuName = "Custom Animations", order = 51)]
public class CustomAnimation : ScriptableObject
{
#if UNITY_EDITOR
	[CustomEditor(typeof(CustomAnimation))]
	public class AnimEditor : Editor
	{
		int activeFrame=0;
		CustomFrame frame;
		CustomAnimation animation;
		int[] numbers;
		string[] names;
		GameObject a;
		SpriteRenderer b;
		public override void OnInspectorGUI()
		{
			activeFrame = EditorGUILayout.IntPopup("Inspected Frame",activeFrame, names,numbers);
			frame = animation.frames[activeFrame];

			b.sprite = frame;
			a.name = frame.sprite.name;
			base.OnInspectorGUI();
		}
		private void OnEnable()
		{
			animation = (CustomAnimation)target;
			numbers = new int[animation.frames.Count];
			names = new string[animation.frames.Count];
			for (int i = 0; i < numbers.Length; i++)
			{
				numbers[i] = i;
				names[i] = i.ToString();
			}

			a = new GameObject();
			b=a.AddComponent<SpriteRenderer>();

			Handles.matrix = a.transform.localToWorldMatrix;
			SceneView.duringSceneGui += OnSceneGUI;
		}
		private void OnDisable()
		{
			DestroyImmediate(a);
			SceneView.duringSceneGui -= OnSceneGUI;
		}
		private void OnSceneGUI(SceneView sv)
		{
			if(a!=null)a.transform.localRotation = (Quaternion.Euler(0, 0, frame.rotation));
			Handles.color = Color.blue;
			//if(frame.PhysicsShape.Length>2) frame.PhysicsShape = a.GetComponent<PolygonCollider2D>().points;
			if (frame.PhysicsShape.Length != 0)
			{
				for (int i = 0; i < frame.PhysicsShape.Length; i++)
				{
					var currP = frame.PhysicsShape[i];
					var nextP = frame.PhysicsShape[0];
					if (i < frame.PhysicsShape.Length - 1)
					{
						nextP = frame.PhysicsShape[i + 1];
					}
					frame.PhysicsShape[i] = Handles.PositionHandle(frame.PhysicsShape[i], Quaternion.identity);
					sv.Repaint();
					Handles.DrawLine(currP, nextP);

				}
			}
			Handles.color = Color.red;
			frame.point = Handles.PositionHandle(frame.point, Quaternion.identity);
			Handles.DrawWireDisc(frame.point, Vector3.forward, 0.05f);
		}
	}
#endif
	public string animName;
	//public bool playTrigger;
	//public string[] attributes=new string[0];
	public string tag;
	public bool saveImpulse = false;
	public float speed;//same as framerate or FPS
	public List<CustomFrame> frames;

	public int priority;
	public bool repeatable;
	public bool interruptable;
	public string[] transitionsTo;
	public string[] doNotTransitTo;
	public string conditionName = "";
	public Misc.condition m_condition;
	private void OnEnable()
	{
		name = animName;
	}
	private void OnValidate()
	{
		name = animName;
	}
	static Misc.condition alwaysTrue = (CustomAnimatorContextInfo a) => true;
	public void InitializeCondition()
	{
		Debug.Log(conditionName);
		m_condition = conditionName is""? alwaysTrue:FindCondition();

	}
	Misc.condition FindCondition()
	{
		var a = (Misc.condition)typeof(CustomAnimator).GetMethod(conditionName).CreateDelegate(typeof(Misc.condition));
		return a;
	}
}
