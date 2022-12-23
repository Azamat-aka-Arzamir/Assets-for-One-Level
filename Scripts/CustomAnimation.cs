using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditorInternal;

[CreateAssetMenu(fileName = "New Custom Animation", menuName = "Custom Animations", order = 51)]
public class CustomAnimation : ScriptableObject
{
#if UNITY_EDITOR
	[CustomEditor(typeof(CustomAnimation))]
	public class AnimEditor : Editor
	{
		int activeFrame = 0;
		CustomFrame frame;
		CustomAnimation animation;
		int[] numbers;
		string[] names;
		GameObject a;
		GameObject parent;
		SpriteRenderer b;
		PixelNormalizer pn;
		public override void OnInspectorGUI()
		{
			Debug.Log("Cock");
			timer = 0;
			activeFrame = EditorGUILayout.IntPopup("Inspected Frame", activeFrame, names, numbers);
			if (animation.frames.Count != 0) frame = animation.frames[activeFrame];

			a.transform.parent = parent.transform;
			if(frame!=null&&frame.sprite!=null)pn.PPU = frame.sprite.pixelsPerUnit;
			else pn.PPU = 16;
			if (animation.frames.Count != 0 && frame.sprite != null) b.sprite = frame;
			if (animation.frames.Count != 0 && frame.sprite != null) a.name = frame.sprite.name;
			if (a != null && frame != null) if(Mathf.Abs(a.transform.localRotation.z-frame.rotation)>0.01)a.transform.localRotation = (Quaternion.Euler(0, 0, frame.rotation));
			if (a != null && frame != null) { parent.transform.position = frame.position; a.transform.localPosition = Vector3.zero; }
			base.OnInspectorGUI();
		}
		private void OnEnable()
		{
			animation = (CustomAnimation)target;
			numbers = new int[animation.frames.Count];
			names = new string[animation.frames.Count];
			if (animation.frames.Count != 0)
			{
				for (int i = 0; i < numbers.Length; i++)
				{
					numbers[i] = i;
					names[i] = i.ToString();
				}
			}
			a = new GameObject();
			parent = new GameObject();

			pn = a.AddComponent<PixelNormalizer>();

			b = a.AddComponent<SpriteRenderer>();

			Handles.matrix = a.transform.localToWorldMatrix;
			SceneView.duringSceneGui += OnSceneGUI;
		}
		private void OnDisable()
		{
			DestroyImmediate(a);
			DestroyImmediate(parent);
			SceneView.duringSceneGui -= OnSceneGUI;
		}
		float timer;
		private void OnSceneGUI(SceneView sv)
		{
			timer += Time.deltaTime;
			if (a != null && frame != null && timer > 1) frame.rotation = a.transform.localRotation.eulerAngles.z;
			if (a != null && frame != null && timer > 1) frame.position = a.transform.localPosition + parent.transform.position;
			Handles.color = Color.blue;
			//if(frame.PhysicsShape.Length>2) frame.PhysicsShape = a.GetComponent<PolygonCollider2D>().points;
			if (frame != null && frame.PhysicsShape.Length != 0)
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
			if (frame != null) frame.point = Handles.PositionHandle(frame.point, Quaternion.identity);
			if (frame != null) Handles.DrawWireDisc(frame.point, Vector3.forward, 0.05f);
		}
	}
#endif
	public string animName;
	public string tag;
	public bool flip = false;
	public float speed;//same as framerate or FPS
	public List<CustomFrame> frames = new List<CustomFrame>();
	private void OnEnable()
	{
		name = animName;
	}
	private void OnValidate()
	{
		name = animName;
	}
}
