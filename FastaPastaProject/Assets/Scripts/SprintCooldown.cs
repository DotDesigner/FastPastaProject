using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

public class SprintCooldown : MonoBehaviour
{
    [Header ("UI")]
    public FirstPersonController firstPersonController;
    public Slider rightCooldownSlider; // Reference to the UI Slider
    public Slider leftCooldownSlider; // Reference to the UI Slider

    [Header("Settings")]
    [SerializeField] private float sprintDuration = 2.0f;
    [SerializeField] private float sprintCooldown = 4.0f;
    [SerializeField] private float sprintTimer = 0.0f;
    [SerializeField] private float cooldownDelayTimer = 0.0f; // Timer for the cooldown delay

    [Header ("NOT CHANGABLE")]
    public bool isSprinting = false;
    private bool isCooldown = false;

    private void Start()
    {
        if (rightCooldownSlider != null)
        {
            rightCooldownSlider.maxValue = sprintDuration;
            rightCooldownSlider.value = 0;
        }
        if (leftCooldownSlider != null)
        {
            leftCooldownSlider.maxValue = sprintDuration;
            leftCooldownSlider.value = 0;
        }
    }
    private void Update()
    {
        UpdateSlider();
        HandleSprint();
    }

    private void HandleSprint()
    {
        if (isCooldown)
        {
            // Cooldown delay logic
            cooldownDelayTimer += Time.deltaTime;
            if (cooldownDelayTimer >= sprintCooldown)
            {
                isCooldown = false;
                sprintTimer = 0;
                cooldownDelayTimer = 0.0f;
            }
        }
        else if (firstPersonController._input.sprint && sprintTimer < sprintDuration)
        {
            // Start or continue sprinting
            isSprinting = true;
            sprintTimer += Time.deltaTime;

        }
        else
        {
            // Not sprinting
            isSprinting = false;

            // Cooldown timer decreases after delay
            if (sprintTimer >= sprintDuration)
            {
                isCooldown = true;
                sprintTimer = sprintDuration; // Keep sprintTimer at max during cooldown delay
            }
            else if (sprintTimer > 0)
            {
                sprintTimer -= Time.deltaTime;
            }
        }

        // Clamp sprintTimer to 0
        if (sprintTimer < 0)
        {
            sprintTimer = 0;
        }

        // Disable sprinting during cooldown delay
        if (isCooldown)
        {
            firstPersonController._input.sprint = false;
        }
    }
    private void UpdateSlider()
    {
        if (rightCooldownSlider != null)
        {
            rightCooldownSlider.value = sprintTimer;
        }
        if (leftCooldownSlider != null)
        {
            leftCooldownSlider.value = sprintTimer;
        }
    }
}
