
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public static AnimationController instance;
    private Animator anim;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        anim.SetBool("IsIdle", true);
    }

    public void Running()
    {
        anim.SetBool("IsRunning", true);
        anim.SetBool("IsIdle", true);
    }
    public void StopRunning()
    {
        anim.SetBool("IsRunning", false);
        anim.SetBool("IsIdle", true);
    }
    public void StartRunFast()
    {
        anim.SetBool("IsRunFast", true);
    }
    public void StopRunFast()
    {
        anim.SetBool("IsRunFast", false);
    }

    public void TurnOnAnimator()
    {
        anim.enabled = true;
    }
    public void TurnOffAnimator()
    {
        anim.enabled = false;
    }
}
