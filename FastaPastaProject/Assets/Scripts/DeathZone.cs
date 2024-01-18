using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private CanvasButtons canvasButtons;

    private void Start()
    {
        canvasButtons = GameObject.FindGameObjectWithTag("UI").GetComponent<CanvasButtons>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            canvasButtons.ReloadCurrentScene();
        }
    }
}
