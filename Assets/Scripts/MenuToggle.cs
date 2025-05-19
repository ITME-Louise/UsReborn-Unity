using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuToggle : MonoBehaviour
{
    // 하위 메뉴 아이콘들을 묶어놓은 GameObject
    public GameObject subMenuGroup;

    // 현재 펼쳐진 상태인지
    private bool isOpen = false;

    public void ToggleMenu()
    {
        isOpen = !isOpen;
        subMenuGroup.SetActive(isOpen);
    }
}