using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuToggle : MonoBehaviour
{
    // ���� �޴� �����ܵ��� ������� GameObject
    public GameObject subMenuGroup;

    // ���� ������ ��������
    private bool isOpen = false;

    public void ToggleMenu()
    {
        isOpen = !isOpen;
        subMenuGroup.SetActive(isOpen);
    }
}