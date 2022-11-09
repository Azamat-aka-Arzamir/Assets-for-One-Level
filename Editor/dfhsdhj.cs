using UnityEditor;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;
using UnityEngine.UIElements;


public class Dfhsdhj : EditorWindow
{
	[MenuItem("Window/UI Toolkit/dfhsdhj")]
	public static void ShowExample()
	{
		Dfhsdhj wnd = GetWindow<Dfhsdhj>();
		wnd.titleContent = new GUIContent("dfhsdhj");
	}
	public void CreateGUI()
	{
		VisualElement root = rootVisualElement;
		Button btn = new Button(Reload);
		VisualElement label = new Label("Hello World! From C#");
		root.Add(label);
		root.Add(btn);
	}
	public void Reload()
	{
		TestAnimatorWindow tst = UnityEditor.EditorWindow.GetWindow<TestAnimatorWindow>();
		tst.Close();
	}
}