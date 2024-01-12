
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
    }

    public void Running()
    {
        anim.SetBool("IsRunning", true);
    }
    public void StopRunning()
    {
        anim.SetBool("IsRunning", false);
    }
}
