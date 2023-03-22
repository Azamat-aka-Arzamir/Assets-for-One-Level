using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class IDCard : MonoBehaviour
{
    public string ID;
    private void Awake()
    {
        if (ID == null || ID == "") ID = Guid.NewGuid().ToString();
        //print(ID);
    }
    public static GameObject FindByID(string objectID)
    {
        List<UnityEngine.Object> list = new List<UnityEngine.Object>();
        list.AddRange(UnityEngine.Object.FindObjectsOfType(typeof(IDCard)));
        var obj = list.Find((x) => (x as IDCard).ID == objectID);
        if(obj ==null) return null;
        return (obj as IDCard).gameObject;
    }
}
