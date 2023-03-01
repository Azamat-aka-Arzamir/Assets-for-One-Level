using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using log4net.Util;
using UnityEngine.Events;

#if UNITY_EDITOR
public class ConditionDrawer
{
    Condition inspectedCondition;
    VisualElement parentVE = new VisualElement();
    ObjectField objField = new ObjectField();
    PopupField<FieldInfo> propField = new PopupField<FieldInfo>("Field");
    VisualElement valueField = new Label("Value");
    EnumField operField = new EnumField("Operation");
    public ConditionDrawer(Condition cond)
    {
        inspectedCondition = cond;
    }
    const int ObjPlace = 1;
    const int PropPlace = 2;
    const int ValuePlace = 3;

    public VisualElement GetContentDrawer()
    {
        var localCompField = new Toggle("Local condition");
        localCompField.value = inspectedCondition.localComponent;
        localCompField.RegisterValueChangedCallback(x =>
        {
            inspectedCondition.localComponent = x.newValue;
        });
        objField = new ObjectField("Component");
        objField.objectType = typeof(Component);
        operField.Init(Condition.CondType.E);
        operField.value = inspectedCondition.type;
        operField.RegisterValueChangedCallback(x => inspectedCondition.type = (Condition.CondType)x.newValue);
        parentVE.Add(localCompField);
        parentVE.Add(objField);
        parentVE.Add(propField);
        parentVE.Add(valueField);
        parentVE.Add(operField);

        objField.value = inspectedCondition.objectRef;
        objField.RegisterValueChangedCallback(x =>
        {
            if (x.newValue != null && x.newValue.GetType().GetFields().Length > 0)
            {
                IDCard newObjectID;
                if (!(x.newValue as Component).gameObject.TryGetComponent<IDCard>(out newObjectID))
                {
                    newObjectID = (x.newValue as Component).gameObject.AddComponent<IDCard>();
                }
                inspectedCondition.typeRef = x.newValue.GetType().GetTypeInfo();
                inspectedCondition.objectID = newObjectID.ID;
                inspectedCondition.FindObject();
                //parentVE.Remove(propField);
                DrawFieldsPopUp();
            }
            if (objField.value != inspectedCondition.objectRef) objField.value = inspectedCondition.objectRef;
        });

        if (inspectedCondition.typeRef != null) DrawFieldsPopUp();
        if (inspectedCondition.property != null) DrawValueField();

        return parentVE;

    }

    //This method is used only once, the only reason of this decision is "not a null check",
    //where this method is used as delegate in case if waiting is needed
    private void DrawFieldsPopUp()
    {
        List<FieldInfo> varsList = new List<FieldInfo>();
        varsList.AddRange(inspectedCondition.typeRef.GetFields());
        propField = new PopupField<FieldInfo>("Field", varsList, varsList[0]);
        if (inspectedCondition.property == null) inspectedCondition.property = varsList[0];
        DrawValueField();
        propField.RegisterValueChangedCallback(x => 
        { 
            inspectedCondition.property = x.newValue; DrawValueField(); 
        });
        if (inspectedCondition.property != null && varsList.Contains(inspectedCondition.property)) propField.value = inspectedCondition.property;
        else inspectedCondition.property = null;
        parentVE.RemoveAt(PropPlace);
        parentVE.Insert(PropPlace, propField);
    }

    private void DrawValueField()
    {
        #region switch case for types
        System.Type type = null;
        if (inspectedCondition.property != null) type = inspectedCondition.property.FieldType;

        if (type == typeof(int))
        {
            var value = new IntegerField("Value");
            if (inspectedCondition.value != null && inspectedCondition.value.GetType() == type) value.value = (int)inspectedCondition.value;
            value.RegisterValueChangedCallback(x => inspectedCondition.value = x.newValue);
            valueField = value;

        }
        else if (type == typeof(string))
        {
            var value = new TextField("Value");
            if (inspectedCondition.value != null && inspectedCondition.value.GetType() == type) value.value = (string)inspectedCondition.value;
            value.RegisterValueChangedCallback(x => inspectedCondition.value = x.newValue);
            valueField = value;

        }
        else if (type == typeof(float))
        {
            var value = new FloatField("Value");
            if (inspectedCondition.value != null && inspectedCondition.value.GetType() == type) value.value = (float)inspectedCondition.value;
            value.RegisterValueChangedCallback(x => inspectedCondition.value = x.newValue);
            valueField = value;

        }
        else if (type == typeof(bool))
        {
            var value = new Toggle("Value");
            if (inspectedCondition.value != null && inspectedCondition.value.GetType() == type) value.value = (bool)inspectedCondition.value;
            value.RegisterValueChangedCallback(x => inspectedCondition.value = x.newValue);
            valueField = value;

        }
        else if(type == typeof(UnityEvent))
        {
            valueField = new VisualElement();//empty
        }
        parentVE.RemoveAt(ValuePlace);
        parentVE.Insert(ValuePlace, valueField);
        #endregion
    }

}

public class StateBox : VisualElement
{
    TestAnimatorWindow parentWindow;
    bool isRenaming;
    static Texture stateBox;
    static Texture CogTexture;
    public VisualElement cogPivot { get; private set; }
    public Image cogImage { get; private set; }
    Image boxImage;
    //Only for inner use to have int values;
    public Vector2 size { get; private set; }
    Box textbox;
    public string stateName;
    public CustomAnimation animation;
    VisualElement textLabel = new Label();
    public List<Line> trans { get; protected set; } = new List<Line>();
    public List<Line> endOnMe { get; protected set; } = new List<Line>();
    void MainBoxSerialization()
    {
        boxImage = new Image();
        boxImage.name = "Box";
        boxImage.image = stateBox;
        boxImage.style.position = Position.Absolute;
        boxImage.style.width = size.x;
        boxImage.style.height = size.y;


        Add(boxImage);
    }
    void CogSerialization()
    {
        cogPivot = new VisualElement();
        cogImage = new Image();

        cogImage.name = "Cog";
        cogImage.image = CogTexture;
        cogImage.style.position = Position.Absolute;

        cogPivot.name = "Cog Pivot";
        cogPivot.style.position = Position.Absolute;

        cogImage.style.width = 50;
        cogImage.style.height = 50;
        cogImage.transform.position = new Vector3(-25, -25);
        cogPivot.transform.position = new Vector3(size.x - 5, 5);
        Add(cogPivot);
        cogPivot.Add(cogImage);
    }
    public void Resize(float currentScale)
    {
        if (currentScale > 0.5)
        {
            textLabel.style.fontSize = 12;
            textLabel.style.backgroundColor = new Color(0f, 0f, 0f, 0f);
            textLabel.style.borderBottomLeftRadius = 0;
            textLabel.style.borderBottomRightRadius = 0;
            textLabel.style.borderTopRightRadius = 0;
            textLabel.style.paddingBottom = 0;
            textLabel.style.paddingTop = 0;
            textLabel.style.paddingLeft = 0;
            textLabel.style.paddingRight = 0;
            textLabel.style.color = new Color(0, 0, 0, 1);
        }
        else
        {
            if (20 / currentScale < 2000) { textLabel.style.fontSize = 20 / currentScale; }
            else textLabel.style.fontSize = 2000;

            textLabel.style.backgroundColor = new Color(0f, 0f, 0f, 0.2f);
            textLabel.style.borderBottomLeftRadius = 20;
            textLabel.style.borderBottomRightRadius = 20;
            textLabel.style.borderTopRightRadius = 20;
            textLabel.style.paddingBottom = 10;
            textLabel.style.paddingTop = 10;
            textLabel.style.paddingLeft= 10;
            textLabel.style.paddingRight = 10;
            textLabel.style.color = new Color(1, 1, 1, 0.8f);
            textbox.style.width = (textLabel as Label).text.Length*15 / currentScale;

        }
    }
    public StateBox(string text)
    {
        parentWindow = EditorWindow.GetWindow<TestAnimatorWindow>();
        GetTexture();

        size = new Vector2(stateBox.width / 4, stateBox.height / 4);
        style.width = size.x;
        style.height = size.y;

        CogSerialization();
        MainBoxSerialization();


        textbox = new Box();
        textbox.style.position = Position.Absolute;
        style.position = Position.Absolute;
        stateName = text;
        (textLabel as Label).text = text;
        (textLabel as Label).style.color = Color.black;
        textbox.Add(textLabel);
        textbox.style.backgroundColor = Color.clear;
        Add(textbox);

        RegisterCallback<GeometryChangedEvent>((evt) => SetCenter());
        RegisterCallback<PointerDownEvent>(OnPointerDown);

        RegisterCallback<PointerUpEvent>(StopDrag);
        parentWindow.rootVisualElement.RegisterCallback<PointerMoveEvent>(Drag);

        ContextualMenuManipulator m = new ContextualMenuManipulator(ContextMenuActions);
        m.target = this;

        if (stateName == "Any State" || stateName == "Any state" || stateName == "any state")
        {
            boxImage.tintColor = Color.green;
        }

    }

    void ContextMenuActions(ContextualMenuPopulateEvent _event)
    {
        _event.menu.AppendAction("Make transition", MakeTransition, DropdownMenuAction.AlwaysEnabled);
        _event.menu.AppendAction("Rename", Rename, DropdownMenuAction.AlwaysEnabled);
        _event.menu.AppendAction("Delete", Delete, DropdownMenuAction.AlwaysEnabled);
        _event.menu.AppendSeparator();
        _event.menu.AppendAction("DebugInfo", (x) => Debug.Log(trans.Count + "  " + endOnMe.Count));
    }
    public void Delete(DropdownMenuAction action)
    {
        for (int i = endOnMe.Count; i > 0; i--)
        {
            var _trans = endOnMe[i - 1];
            _trans.startState.trans.Remove(_trans);
            endOnMe.Remove(_trans);
            _trans.parent.Remove(_trans);
        }
        foreach (var tr in trans)
        {
            tr.parent.Remove(tr);
        }
        parentWindow.states.Remove(this);
        parent.Remove(this);
    }
    void Rename(DropdownMenuAction action)
    {
        isRenaming = true;
        textLabel = new TextField();
        textbox.RemoveAt(0);
        textbox.Insert(0, textLabel);
        (textLabel as TextField).Focus();
        textbox.RegisterCallback<KeyDownEvent>((x) =>
        {
            if (x.keyCode == KeyCode.Return)
            {
                Label l = new Label((textLabel as TextField).text);
                if (l.text == "") l.text = "U fORGOT TO \n WRITE NAME";
                l.style.color = Color.black;
                textLabel = l;
                isRenaming = false;
                stateName = l.text;
                textbox.RemoveAt(0);
                textbox.Insert(0, textLabel);
                if(stateName=="Any State"|| stateName == "Any state"||stateName == "any state")
                {
                    boxImage.tintColor = Color.green;
                }
                else
                {
                    boxImage.tintColor = Color.white;
                }
            }
        });

    }
    void MakeTransition(DropdownMenuAction action)
    {
        Debug.Log("Im fucking CUMMING!!!!");
        var newTrans = new Line(this);
        parent.Add(newTrans);
        trans.Add(newTrans);
    }

    Vector2 targetStartPosition;
    Vector3 pointerStartPosition;
    Vector2 startDif;
    bool isDragged;
    void OnPointerDown(PointerDownEvent evt)
    {
        //Trying to set unset active transition
        var aLine = (parentWindow as TestAnimatorWindow).activeElement;
        if (aLine != null && aLine.GetType() == typeof(Line))
        {
            //if it is already set, it will just ignore set request
            if ((aLine as Line).Set(this)) endOnMe.Add(aLine as Line);
            (parentWindow as TestAnimatorWindow).FocusOn(aLine);
        }
        (parentWindow as TestAnimatorWindow).FocusOn(this);


        //preparing for dragging

        if (isRenaming) return;
        if (evt.button != 0||evt.shiftKey) return;
        isDragged = true;
        targetStartPosition = transform.position;
        pointerStartPosition = (evt.position - parentWindow.statesSpace.transform.position) / parentWindow.statesSpace.transform.scale.x ;

    }
    List<Action<StateBox>> onMoveListeners = new List<Action<StateBox>>();
    void Drag(PointerMoveEvent evt)
    {
        Vector2 glPos = parentWindow.statesSpace.transform.position;
        float _scale = parentWindow.statesSpace.transform.scale.x;
        if (!isDragged) return;
        Vector3 pointerDelta = ((evt.position- parentWindow.statesSpace.transform.position) / _scale - pointerStartPosition);

        transform.position = new Vector2(
            Mathf.Clamp(targetStartPosition.x + pointerDelta.x, 0, parentWindow.statesSpace.worldBound.width / _scale -size.x),
            Mathf.Clamp(targetStartPosition.y + pointerDelta.y, 0, parentWindow.statesSpace.worldBound.height / _scale - size.y));

        foreach (var listener in onMoveListeners)
        {
            listener(this);
        }
    }
    public void AddOnMoveListener(Action<StateBox> action)
    {
        onMoveListeners.Add(action);
    }
    void StopDrag(PointerUpEvent evt)
    {
        isDragged = false;
    }
    public static void GetTexture()
    {
        stateBox = (AssetDatabase.LoadAssetAtPath("Assets/Defaults/Editor/SpriteBox.png", typeof(Sprite)) as Sprite).texture;
        CogTexture = (AssetDatabase.LoadAssetAtPath("Assets/Defaults/Editor/Cog.png", typeof(Sprite)) as Sprite).texture;
    }

    public void SetCenter()
    {
        textbox.transform.position = new Vector3(size.x / 2 - textbox.layout.width / 2, size.y / 2 - textbox.layout.height / 2);
        style.width = size.x;
        style.height = size.y;
    }
}
public class StateBoxDrawer
{
    StateBox inspectedState;
    ObjectField animBox;

    public StateBoxDrawer(StateBox _inspectedState)
    {
        inspectedState = _inspectedState;
    }
    public VisualElement GetVisualElement()
    {
        SyncValuesWithInspectedState();
        VisualElement output = new VisualElement();
        animBox.RegisterValueChangedCallback(x => inspectedState.animation = x.newValue as CustomAnimation);
        output.Add(animBox);
        var listView = new ListView(inspectedState.trans, 16, () => new Label(), (e, i) => { (e as Label).text = inspectedState.trans[i].endState.stateName; });
        listView.reorderable = true;
        listView.style.height = inspectedState.trans.Count * 16;
        listView.style.width = 100;
        listView.style.flexGrow = 1f;
        listView.onItemsChosen +=(x)=> {TestAnimatorWindow.GetWindow<TestAnimatorWindow>().FocusOn(inspectedState.trans[listView.selectedIndex]); };
        var h1 = new Label("Transitions");
        h1.transform.position += new Vector3(20, 20);
        listView.transform.position += new Vector3(20, 50);
        output.Add(h1);
        output.Add(listView);

        return output;
    }

    //Sets values in inspector according to inspected StateBox
    void SyncValuesWithInspectedState()
    {
        animBox = new ObjectField();
        animBox.objectType = typeof(CustomAnimation);
        animBox.value = inspectedState.animation;
    }
}
public class Line : Image
{
    TestAnimatorWindow parentWindow;
    //delete
    public List<Condition> conditions = new List<Condition>();
    public bool hasExitTime;
    private Vector3 start;
    private Vector3 end;
    public StateBox startState { get; private set; }
    public StateBox endState { get; private set; }

    static Texture texture;
    static Texture roundTexture;
    static Texture bulbOnTexture;
    static Texture bulbOffTexture;
    public Image bulb { get; private set; }
    public bool set { get; private set; } = false;

    public Line(StateBox startBox)
    {
        startState = startBox;
        parentWindow = EditorWindow.GetWindow<TestAnimatorWindow>();
        parentWindow.rootVisualElement.RegisterCallback<PointerMoveEvent>(OnMouseMove);
        parentWindow.rootVisualElement.RegisterCallback<PointerDownEvent>(OnPointerDown);
        parentWindow.FocusOn(this);
        start = startBox.localBound.center;
        startBox.AddOnMoveListener(OnStartMoved);
        if (texture == null)
        {
            GetTexture();
        }
        image = texture;
        bulb = new Image();
        bulb.image = bulbOffTexture;
        bulb.style.width = bulbOffTexture.width / 3;
        bulb.style.height = bulbOffTexture.height / 3;
        bulb.style.position = Position.Absolute;
        bulb.transform.position = new Vector3(texture.width / 8, -texture.height / 2);
        Add(bulb);
        style.position = Position.Absolute;
        SendToBack();
        RegisterCallback<PointerDownEvent>(OnPointerDownLocal);
    }

    public Line(StateBox startBox, StateBox endBox, List<Condition> conds, bool _het)
    {
        hasExitTime = _het;
        conditions = conds;
        set = true;
        startState = startBox;
        endState = endBox;
        parentWindow = EditorWindow.GetWindow<TestAnimatorWindow>();
        parentWindow.FocusOn(this);
        if (startBox == endBox)
        {
            end += Vector3.down * 20;
        }
        RegisterCallback<GeometryChangedEvent>((x) => UpdatePos(startState.worldBound.center, endState.worldBound.center));
        startBox.AddOnMoveListener(OnStartMoved);
        endBox.AddOnMoveListener(OnEndMoved);
        if (texture == null)
        {
            GetTexture();
        }

        image = texture;
        bulb = new Image();
        bulb.image = bulbOffTexture;
        bulb.style.width = bulbOffTexture.width / 3;
        bulb.style.height = bulbOffTexture.height / 3;
        bulb.transform.position = new Vector3(texture.width / 8, -texture.height / 2);
        bulb.style.position = Position.Absolute;
        Add(bulb);
        style.position = Position.Absolute;
        SendToBack();
        RegisterCallback<PointerDownEvent>(OnPointerDownLocal);
        endBox.endOnMe.Add(this);
        RegisterCallback<GeometryChangedEvent>(x =>
        {
            OnStartMoved(startBox);
            OnEndMoved(endBox);
        });

    }
    public void OnPointerDown(PointerDownEvent evt)
    {
        if (!set && evt.button == 1)
        {
            parent.Remove(this);
        }

    }
    void OnPointerDownLocal(PointerDownEvent evt)
    {
        if (set && evt.button == 1 && evt.shiftKey)
        {
            parent.Remove(this);
            startState.trans.Remove(this);
            endState.trans.Remove(this);
        }
        if (set && evt.button == 0)
        {
            parentWindow.FocusOn(this);
            SendToBack();
        }
    }


    public void OnMouseMove(PointerMoveEvent evt)
    {

        if (!set)
        {
            UpdatePos(start, (evt.localPosition - parentWindow.statesSpace.transform.position) / parentWindow.statesSpace.transform.scale.x);
        }
        else parentWindow.rootVisualElement.UnregisterCallback<PointerMoveEvent>(OnMouseMove);
    }
    public void OnEndMoved(StateBox endBox)
    {
        UpdatePos(start, (endBox.transform.position+(Vector3)endBox.size/2));
    }
    public void OnStartMoved(StateBox startBox)
    {
        UpdatePos((startBox.transform.position + (Vector3)startBox.size / 2), end);
    }
    public void TurnOnTheLight()
    {
        bulb.image = bulbOnTexture;
    }
    public void TurnOffTheLight()
    {
        bulb.image = bulbOffTexture;
    }
    public void UpdatePos(Vector3 spos, Vector3 epos)
    {
        if (startState == endState)
        {
            start = spos;
            style.width = 60;
            style.height = 100;
            transform.scale=new Vector3(1, 1, 1);
            bulb.transform.scale = new Vector3(1, 1, 1);
            transform.position = spos + new Vector3(30, 100);
            bulb.transform.position = new Vector3(18, -26);
            transform.rotation = Quaternion.Euler(0, 0, 180);
            image = roundTexture;
            SendToBack();
            return;
        }
        start = spos;
        end = epos;
        style.width = 600 / 4;
        style.height = 50 / 4;
        transform.scale = new Vector3((epos - spos).magnitude / (600 / 4), 1);
        transform.position = spos;
        bulb.transform.scale = new Vector3(1 / transform.scale.x, 1 / transform.scale.y);
        transform.rotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.right, (epos - spos), Vector3.forward));
        SendToBack();
    }
    public bool Set(StateBox box)
    {
        if (set) return false;
        box.AddOnMoveListener(OnEndMoved);
        set = true;
        endState = box;
        UpdatePos(startState.transform.position + (Vector3)startState.size / 2, (endState.transform.position + (Vector3)endState.size / 2));

        return true;
    }
    public static void GetTexture()
    {
        texture = (AssetDatabase.LoadAssetAtPath("Assets/Defaults/Editor/Line.png", typeof(Sprite)) as Sprite).texture;
        roundTexture = (AssetDatabase.LoadAssetAtPath("Assets/Defaults/Editor/RoundLine.png", typeof(Sprite)) as Sprite).texture;
        bulbOnTexture = (AssetDatabase.LoadAssetAtPath("Assets/Defaults/Editor/LampEnabled.png", typeof(Sprite)) as Sprite).texture;
        bulbOffTexture = (AssetDatabase.LoadAssetAtPath("Assets/Defaults/Editor/LampDisabled.png", typeof(Sprite)) as Sprite).texture;
    }


}
#endif
public class AnimEvent<N>
{
    List<Action<N>> listeners = new List<Action<N>>();
    public void Invoke(N returnObject)
    {
        foreach (var listener in listeners)
        {
            listener.Invoke(returnObject);
        }
    }
    public void AddListener(Action<N> action)
    {
        listeners.Add(action);
    }
    public void RemoveListener(Action<N> action)
    {
        listeners.Remove(action);
    }
}
public struct AnimEventHandler
{

}




