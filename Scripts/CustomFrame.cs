using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[System.Serializable]
public class CustomFrame
{
	public string name;
	public Sprite sprite;
	public Vector3 position= new Vector3(0,0,0);
	public Vector3 point = new Vector3(0, 0, 0);
	public float rotation = 0 ;
	public bool invisible = false;
	public Vector2[] PhysicsShape = new Vector2[] {new Vector2(0,0) };
	public CustomFrame(Sprite _sprite)
	{
		sprite = _sprite;
		name = _sprite.name;
	}
	public static implicit operator Sprite(CustomFrame frame)=>frame.sprite;
	public static explicit operator CustomFrame(Sprite sprite) => new CustomFrame(sprite);
}

