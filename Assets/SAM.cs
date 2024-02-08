using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SAM : MonoBehaviour
{
    [Header("SAM Toggle Scales")]
    public ToggleGroup[] toggles;

    [Header("SAM Gameobjects")]
    public GameObject[] SAM_Items;

    [Header("Next Button")]
    public Button NextButton;

    public string[] answers = new string[2];

    private int currentToggle = 0;

    Toggle selected = null;

    private Manager Manager;

    void Start()
    {
        Manager = FindObjectOfType<Manager>();

        for (int i = 0; i < toggles.Length; i++)
        {
            toggles[i].allowSwitchOff = true;

            foreach (Toggle toggle in toggles[i].GetComponentsInChildren<Toggle>())
            {
                toggle.onValueChanged.AddListener(OnToggleChanged);
            }
        }

        NextButton.interactable = false;
    }

    void OnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            selected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Toggle>();
            NextButton.interactable = true;
        }
        else
            NextButton.interactable = false;
    }

    public void Submit()
    {
        if(currentToggle == 0) 
        {
            //answers[currentToggle] = float.Parse(selected.name);
            answers[0] = "Valence";
            answers[1] = selected.name;
            Manager.SAM_answers[currentToggle] = selected.name;
            Manager.SAM.StreamData(answers);
            SAM_Items[currentToggle].SetActive(false);
            currentToggle++;
            SAM_Items[currentToggle].SetActive(true);
            NextButton.interactable = false;
        }
        else
        {
            answers[0] = "Arousal";
            answers[1] = selected.name;
            //answers[currentToggle] = float.Parse(selected.name);
            Manager.SAM_answers[currentToggle] = selected.name;
            Manager.SAM.StreamData(answers);
            SceneManager.LoadScene("VAS");
        }
    }
}
