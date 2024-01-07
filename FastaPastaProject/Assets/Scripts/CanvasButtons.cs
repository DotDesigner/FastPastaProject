using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasButtons : MonoBehaviour
{
    private StarterAssetsInputs starterAssets;
    private void Start()
    {
        starterAssets = GameObject.FindGameObjectWithTag("Player").GetComponent<StarterAssetsInputs>();
    }
    private void Update()
    {
        if (starterAssets.UI)
        {
            ReloadCurrentScene();
        }
    }
    public void ReloadCurrentScene()
    {
        // Get the current scene and reload it
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}
