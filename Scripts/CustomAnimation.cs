using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Custom Animation", menuName = "Custom Animations", order = 51)]
public class CustomAnimation : ScriptableObject
{
	public float speed;//same as framerate or FPS
	public List<Sprite> frames;
	public string animName;
	public int priority;
	public bool repeatable;
}