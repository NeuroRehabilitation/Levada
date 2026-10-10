using UnityEngine;
using System;
using System.Collections.Generic;

public class MenuController : MonoBehaviour
{
    public List<GameObject> menus;
    public static List<GameObject> menusGlobal;

    public List<string> shortcuts;
    public static List<string> shortcutsGlobal;
    private string KeycodeB;

    public void Awake()
    {
        menusGlobal = menus;
        shortcutsGlobal = shortcuts;
    }

    void Update()
    {
        int i = 0;

        if (shortcuts == null || menus == null)
            return;

        foreach (string shortcut in shortcuts)
        {
            if (Enum.TryParse(shortcut, true, out KeyCode key))
            {
                if (Input.GetKeyDown(key))
                {
                    menus[i].SetActive(true);
                }
            }

            i++;
        }
    }

    public void ActivateOption(GameObject Option)
    {
        Option.SetActive(true);
    }

    public void DeactivateOption(GameObject Option)
    {
        Option.SetActive(false);
    }

    public void AlternateOptions(GameObject Option)
    {
        bool isOn = Option.activeSelf;
        Option.SetActive(!isOn);
    }

    public static void ActivateOptionGlobal(int i)
    {
        Debug.Log($"[ActivateOptionGlobal] Called with index: {i}");

        if (menusGlobal == null)
        {
            Debug.LogError("[ActivateOptionGlobal] menusGlobal is NULL");
            return;
        }

        if (i < 0 || i >= menusGlobal.Count)
        {
            Debug.LogError($"[ActivateOptionGlobal] Index out of range: {i} | Valid range: 0-{menusGlobal.Count - 1}");
            return;
        }
        menusGlobal[i].SetActive(true);
    }

    public static void DeactivateOptionGlobal(int i)
    {
        Debug.Log($"[ActivateOptionGlobal] Called with index: {i}");

        if (menusGlobal == null)
        {
            Debug.LogError("[ActivateOptionGlobal] menusGlobal is NULL");
            return;
        }

        if (i < 0 || i >= menusGlobal.Count)
        {
            Debug.LogError($"[ActivateOptionGlobal] Index out of range: {i} | Valid range: 0-{menusGlobal.Count - 1}");
            return;
        }
        menusGlobal[i].SetActive(false);
    }

    public void closeAllMenu(){
        foreach (GameObject  menu in menus)
        {
           menu.SetActive(false);   
        }
    }

    public int getMenuOpen(){
        int value = 0;
        foreach (GameObject menu in menus)
        {
           if(menu.activeSelf) {
                break;
           }
           value++;
        }
        return value;
    }
}
