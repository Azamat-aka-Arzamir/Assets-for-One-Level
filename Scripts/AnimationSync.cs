using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSync : MonoBehaviour
{
    CustomAnimator[] animators;
    // Start is called before the first frame update
    void Start()
    {
        AssignNewPieces();
    }
    public void AssignNewPieces()
    {
        animators = GetComponentsInChildren<CustomAnimator>();
        foreach (var a in animators)
        {
            a.animChangedEvent.AddListener(SyncAnimStarts);
        }
    }
    int counter = 0;

    public void RemoveOldPieces()
    {
        foreach (var a in animators)
        {
            a.animChangedEvent.RemoveListener(SyncAnimStarts);
        }
        animators = new CustomAnimator[0];
    }
    bool busy = false;
    //Makes every children element (armour piece) try to find new transition and then animation
    void SyncAnimStarts(AnimatorScheme.StateInfo sti, AnimatorScheme.Transition tr)
    {
        int f=0;
        float t=0;
        if (tr.endState.name.Contains("Attack"))
        {
            foreach (var a in animators)
            {
                if (!a.CurrentAnim.name.Contains("Attack"))
                {
                   // a.ForceNewFrame();
                }
                else
                {
                    if (a.CurrentFrameIndex > f)
                    {
                        f = a.CurrentFrameIndex;
                        t = a.timeFromFrameStart;
                    }
                }
            }
        }
        foreach (var a in animators)
        {
            if (a.CurrentAnim.name.Contains("Attack"))
            {
               // a.currentFrameIndex = f;
                a.timeFromFrameStart = t;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
