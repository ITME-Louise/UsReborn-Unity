using UnityEngine;
using UnityEngine.InputSystem;
public class FoxAnicontroller : MonoBehaviour
{
    public Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        var keyboard = Keyboard.current;

        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            anim.SetTrigger("Clapping");
        }
        if (keyboard.digit2Key.wasPressedThisFrame)
        {
            anim.SetTrigger("happy");
        }
    }
}
