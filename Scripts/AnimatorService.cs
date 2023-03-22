using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class AnimatorService
{
    static LinkedList<CustomAnimator> animators = new LinkedList<CustomAnimator>();
    private static int lastUpdate;
    public static void Update()
    {
        if (lastUpdate == 0) AssignOrder();
        if (lastUpdate == Time.renderedFrameCount) return;
        lastUpdate = Time.renderedFrameCount;

        foreach (CustomAnimator animator in order)
        {
            animator.AnimatorUpdate();
        }
    }
    public static void AddAnimator(CustomAnimator animator)
    {
        animators.AddLast(animator);
    }
    static CustomAnimator[] order;
    static void AssignOrder()
    {
        GetAnimatorsInfo();
        LinkedList<CustomAnimator> list = new LinkedList<CustomAnimator>(); 
        foreach (var a in animators)
        {
            CustomAnimator b = a;
            int counter = 0;
            LinkedList<CustomAnimator> list2 = new LinkedList<CustomAnimator>();
            list2.AddLast(b);
            while (b.MotherAnimator != null&&!list.Contains(b.MotherAnimator))
            {
                list2.AddLast(b.MotherAnimator);
                b = b.MotherAnimator;
                counter++;
                if (counter > animators.Count) throw new System.Exception("Animator inheritance loop at "+a.name);
            }
            foreach(var c in list2.Reverse()){
                if (!list.Contains(c))
                {
                    list.AddFirst(c);
                }
            }
        }
        order = list.Reverse().ToArray();
    }
    static void GetAnimatorsInfo()
    {
        var formatter = new BinaryFormatter();
        var fileStream = new FileStream("Assets/Animators/torso_head", FileMode.Open);
        var scheme = (AnimatorScheme)formatter.Deserialize(fileStream);

        foreach(var a in scheme.states)
        {
            Debug.Log(a.name);
        }
        fileStream.Close();
    }
}
