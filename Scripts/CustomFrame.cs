using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CustomFrame : ScriptableObject
{
#if UNITY_EDITOR

	[CustomEditor(typeof(CustomFrame))]
	public class FrameEditor : Editor
	{
		CustomFrame frame;
		GameObject a;
		private void OnEnable()
		{
			frame = (CustomFrame)target;
			a = new GameObject();
			var b = a.AddComponent<SpriteRenderer>();
			b.sprite = frame;
			a.name = frame.sprite.name;
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
			frame.point=Handles.PositionHandle(frame.point, Quaternion.identity);
			Handles.DrawWireDisc(frame.point, Vector3.forward, 0.05f);
		}
	}
#endif
	public Sprite sprite;
	public Vector3 position= new Vector3(0,0,0);
	public Vector3 point;
	public CustomFrame(Sprite _sprite)
	{
		sprite = _sprite;
		name = _sprite.name;
	}
	public static implicit operator Sprite(CustomFrame frame)=>frame.sprite;
	public static explicit operator CustomFrame(Sprite sprite) => new CustomFrame(sprite);
}

