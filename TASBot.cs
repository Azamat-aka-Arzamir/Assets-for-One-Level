using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TASBot : MonoBehaviour
{
    int time;
    int startTime;
    public double duration => time - startTime;
    Dictionary<string,InputAction.CallbackContext> actions = new Dictionary<string, InputAction.CallbackContext>();
    string[] strings;
    // Start is called before the first frame update
    void Start()
    {
        print(new InputAction.CallbackContext());
        try
        {
            //LoadTASCmd();
            foreach(var cont in actions)
            {
                print(cont.Key);
            }
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
        strings = File.ReadAllLines("C:/OneLevelLogs/TAASBOT.txt");
    }
    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount >= strings.Length){ Debug.Break(); return; }
        string currentCMD = strings[Time.frameCount];
        //a d space LMB
        var input = currentCMD.Split('\t');
        if (input[0]=="1")
        {
            GetComponent<Controller>().SetInput(Vector2.left);
        }
        if (input[1] == "1")
        {
            GetComponent<Controller>().SetInput(Vector2.right);
        }
        if (input[2] == "1")
        {
            GetComponent<Controller>().GetJump(new InputAction.CallbackContext());
        }
        if (input[3] == "1")
        {
            GetComponent<Controller>().GetAttack(new InputAction.CallbackContext());
        }
        if (input[0] != "1" && input[1] != "1")
        {
            GetComponent<Controller>().SetInput(Vector2.zero);
        }
    }
    public void AddContext(InputAction.CallbackContext context)
    {
        string d = "";
        if (context.valueType == typeof(Vector2)) d += context.ReadValue<Vector2>();
        var name = context.action.name + d + " " + context.phase;
        if (actions.ContainsKey(name)) return;

        var a = GetComponent<PlayerInput>().actionEvents.ToArray();
        var aa = Array.Find(a, x => x.actionName == context.action.name);
        actions.Add(name, context);
        //SaveTASCmd();
        print(name);
    }

    void SaveTASCmd()
    {
        var formatter = new BinaryFormatter();
        var fileStream = new FileStream("C:/OneLevelLogs/Tas", FileMode.Create);
        formatter.Serialize(fileStream, actions);
        fileStream.Close();
    }
    void LoadTASCmd()
    {
        var formatter = new BinaryFormatter();
        var fileStream = new FileStream("C:/OneLevelLogs/Tas", FileMode.Open);
        actions = (Dictionary<string, InputAction.CallbackContext>)formatter.Deserialize(fileStream);
        fileStream.Close();
    }
}
