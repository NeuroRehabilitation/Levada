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

    public static bool submitButtonPressed = false;

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
            Manager.SAM_answers[currentToggle] = selected.name;
            Manager.SAM.StreamData(answers);
            submitButtonPressed = true;
            ResetToggleGroup();
        }
    }

    void ResetToggleGroup()
    {
        SAM_Items[currentToggle].SetActive(false); //Deactivate Arousal Scale

        currentToggle = 0; //Restart from Valence Scale

        SAM_Items[currentToggle].SetActive(true); //Activate Valence Scale

        foreach (ToggleGroup scale in toggles)
        {
            // Get all toggles in the toggle group
            Toggle[] toggles = scale.GetComponentsInChildren<Toggle>();

            // Loop through each toggle and set isOn to false
            foreach (Toggle toggle in toggles)
            {
                toggle.isOn = false;
            }
        }
        
    }
}
