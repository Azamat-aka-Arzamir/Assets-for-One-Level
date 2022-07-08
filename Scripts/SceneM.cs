using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneM
{
	// Start is called before the first frame update
	public static void ReloadActiveScene()
	{
		Scene activeScene = SceneManager.GetActiveScene();
		SceneManager.LoadScene(activeScene.buildIndex);
	}
}
