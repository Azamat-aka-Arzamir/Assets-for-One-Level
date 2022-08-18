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
			a.transform.localRotation=(Quaternion.Euler(0, 0, frame.rotation));
			Handles.color = Color.blue;
			//if(frame.PhysicsShape.Length>2) frame.PhysicsShape = a.GetComponent<PolygonCollider2D>().points;
			for (int i = 0; i<frame.PhysicsShape.Length;i++)
			{
				var currP = frame.PhysicsShape[i];
				var nextP = frame.PhysicsShape[0];
				if(i< frame.PhysicsShape.Length-1)
				{
					nextP= frame.PhysicsShape[i+1];
				}
				frame.PhysicsShape[i] = Handles.PositionHandle(frame.PhysicsShape[i], Quaternion.identity);
				sv.Repaint();
				Handles.DrawLine(currP, nextP);

			}
			Handles.color = Color.red;
			frame.point=Handles.PositionHandle(frame.point, Quaternion.identity);
			Handles.DrawWireDisc(frame.point, Vector3.forward, 0.05f);
		}

	}
#endif
	public Sprite sprite;
	public Vector3 position= new Vector3(0,0,0);
	public Vector3 point;
	public float rotation;
	public Vector2[] PhysicsShape;
	public CustomFrame(Sprite _sprite)
	{
		sprite = _sprite;
		name = _sprite.name;
	}
	public static implicit operator Sprite(CustomFrame frame)=>frame.sprite;
	public static explicit operator CustomFrame(Sprite sprite) => new CustomFrame(sprite);
}

