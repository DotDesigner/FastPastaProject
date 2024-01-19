using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText; // Current timer
    public TextMeshProUGUI bestTimeText; // Best time
    public GameObject startTrigger; // Reference to the StartTrigger GameObject
    public GameObject finishTrigger; // Reference to the StartTrigger GameObject
    private GameObject player;

    private bool timerRunning = false;
    private float startTime;
    private float elapsedTime;
    private bool collisionDetected = false;


    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Trigger");
        UpdateBestTimeDisplay(PlayerPrefs.GetFloat("BestTime", 0));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ResetAllPlayerPrefs();
        }

        if (timerRunning)
        {
            elapsedTime = Time.time - startTime;
            UpdateTimerDisplay();
        }

        if (!collisionDetected && player.GetComponent<Collider>().bounds.Intersects(startTrigger.GetComponent<Collider>().bounds))
        {
            Debug.Log("sssi");
            collisionDetected = true;
            StartTimer();
            DestroyStartTrigger();
        }
        if (collisionDetected && player.GetComponent<Collider>().bounds.Intersects(finishTrigger.GetComponent<Collider>().bounds))
        {
            Debug.Log("sssdi");
            collisionDetected = false;
            StopTimer();
            CheckForBestTime();
            DestroyFinishTrigger();
        }


    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trigger"))
        {
            Debug.Log("si");
            if (startTrigger && other.gameObject == startTrigger)
            {
                Debug.Log("Start Trigger activated");
 // Destroy the StartTrigger GameObject
            }
            else if (finishTrigger && other.gameObject == finishTrigger)
            {

            }
        }
    }

    private void StartTimer()
    {
        startTime = Time.time;
        timerRunning = true;
    }

    private void StopTimer()
    {
        timerRunning = false;
        UpdateTimerDisplay(); // Update display one last time to show final time
    }

    private void CheckForBestTime()
    {
        float bestTime = PlayerPrefs.GetFloat("BestTime", float.MaxValue);
        if (elapsedTime < bestTime)
        {
            SaveTime();
            UpdateBestTimeDisplay(elapsedTime);
        }
    }

    private void SaveTime()
    {
        PlayerPrefs.SetFloat("BestTime", elapsedTime);
        PlayerPrefs.Save();
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = FormatTime(elapsedTime);
        }
    }

    private void UpdateBestTimeDisplay(float time)
    {
        if (bestTimeText != null)
        {
            bestTimeText.text = "Best Time: " + FormatTime(time);
        }
    }

    private void ResetAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        UpdateBestTimeDisplay(0); // Reset the display of best time
        // Optionally recreate or reactivate the StartTrigger GameObject here
    }

    private void DestroyStartTrigger()
    {
        if (startTrigger != null)
        {
            Destroy(startTrigger);
        }
    }
    private void DestroyFinishTrigger()
    {
        if (finishTrigger != null)
        {
            Destroy(finishTrigger);
            this.enabled = false;
        }
    }

    private string FormatTime(float time)
    {
        int minutes = (int)time / 60;
        int seconds = (int)time % 60;
        int milliseconds = (int)(time * 100) % 100;
        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }
}