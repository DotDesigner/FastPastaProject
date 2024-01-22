using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class PauseGame : MonoBehaviour
{
    private bool isPause = false;
    private StarterAssetsInputs _input;

    private void Start()
    {
        _input = GameObject.FindWithTag("Player").GetComponent<StarterAssetsInputs>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }


    }

    private void Pause()
    {
        if (!isPause)
        {
            Time.timeScale = 0;
            isPause = true;
            Cursor.lockState = CursorLockMode.None;
            _input.cursorInputForLook = false;
        }
        else
        {
            Time.timeScale = 1;
            isPause = false;
            Cursor.lockState = CursorLockMode.Locked;
            _input.cursorInputForLook = true;
        }
    }
}
