using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BearAnicontroller : MonoBehaviour
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
            anim.SetTrigger("laying");
        }
        if (keyboard.digit2Key.wasPressedThisFrame)
        {
            anim.SetTrigger("standing");
        }
    }
}
