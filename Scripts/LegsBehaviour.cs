using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegsBehaviour : MonoBehaviour
{
    CustomAnimator selfAnim;
    Vector2 bodyPoint;
    public List<GameObject> BodyParts = new List<GameObject>();
    List<Vector2> bodyPartsStartPos = new List<Vector2>();
    // Start is called before the first frame update
    void Start()
    {
        selfAnim=GetComponent<CustomAnimator>();
        AssignStartPos();
        selfAnim.newFrameEvent.AddListener(ReplaceBody);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void ReplaceBody()
	{
        var addPos = selfAnim.GetCurrentAnimation().frames[selfAnim.CurrentFrameIndex].point;
        //print(addPos+"  "+ selfAnim.CurrentAnim.animName);
        foreach(var part in BodyParts)
		{
            var newVector2 = bodyPartsStartPos[BodyParts.IndexOf(part)] + (Vector2)addPos;
            part.transform.localPosition = new Vector3(newVector2.x, newVector2.y, part.transform.localPosition.z);
		}
	}
    void AssignStartPos()
	{
        foreach(var part in BodyParts)
		{
            bodyPartsStartPos.Add(new Vector2(part.transform.localPosition.x, part.transform.localPosition.y));
		}
	}
}
