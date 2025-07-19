using UnityEngine;
public class AnimationStarter : MonoBehaviour
{
    public Animator animator;

    private void OnTriggerEnter(Collider other)
    {
        animator.SetTrigger("PlayAnim");
    }
}

