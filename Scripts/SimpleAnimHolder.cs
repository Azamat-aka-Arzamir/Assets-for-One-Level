using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAnimHolder : MonoBehaviour
{
    public List<CustomAnimator> Animators;
    public void PlayAnim(string Name)
	{
		foreach(var a in Animators)
		{
			a.PlayAnim(Name);
		}
	}
}
