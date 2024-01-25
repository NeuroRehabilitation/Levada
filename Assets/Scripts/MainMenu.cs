using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public  Toggle vrToggle;
    public static bool isnonVR;
    public InputField UserIdInputField;
    public Button StartButton;
    public Button RecordButton;
    public GameObject KinectGameObject;
    public Text[] KinectValues = new Text[8];
    public GameObject[] waitPanels;
    public GameObject calPanel;
    public Sprite[] bgPanels;// by me
    public GameObject panel_image;

    // Use this for initialization
    int i = 0;
    IEnumerator changePanelImages()
    {
        yield return new WaitForSeconds(3f);
        panel_image.GetComponent<Image>().sprite = bgPanels[i++];
        if (i >= bgPanels.Length)
            i = 0;
        StartCoroutine(changePanelImages());
    }
    void Start()
    {
        TestDetails.TestDesc.UserId = "1";
        Cursor.visible = true;
        TestDetails.TestDesc.UserId = UserIdInputField.text;// by me
        Debug.Log("input field value>>>>>" + TestDetails.TestDesc.UserId);

        //Screen.SetResolution(3520, 1200, false);
        //OnCalibrateButton();
        StartCoroutine(changePanelImages());
        StartButton.interactable = false;
        RecordButton.interactable = false;
        if (!string.IsNullOrEmpty(TestDetails.TestDesc.UserId))
            UserIdInputField.text = TestDetails.TestDesc.UserId;
        TestDetails.TestDesc.Test = TestDetails.TestType._8FtUpGo;
        TestDetails.TestDesc.Feedback = TestDetails.FeedbackType.Mirror;

        TestDetails.KinectDesc.Position = new Vector3(0, PlayerPrefs.GetFloat("KinectHeight", 0f), 0);
        TestDetails.KinectDesc.Rotation = new Quaternion(PlayerPrefs.GetFloat("KinectRotX", 0f),
            PlayerPrefs.GetFloat("KinectRotY", 0f), PlayerPrefs.GetFloat("KinectRotZ", 0f),
            PlayerPrefs.GetFloat("KinectRotW", 0f));

        KinectValues[4].text = TestDetails.KinectDesc.Position.y.ToString(CultureInfo.InvariantCulture);
        KinectValues[5].text = TestDetails.KinectDesc.Rotation.eulerAngles.x.ToString(CultureInfo.InvariantCulture);
        KinectValues[6].text = TestDetails.KinectDesc.Rotation.eulerAngles.y.ToString(CultureInfo.InvariantCulture);
        KinectValues[7].text = TestDetails.KinectDesc.Rotation.eulerAngles.z.ToString(CultureInfo.InvariantCulture);
    }

    // Update is called once per frame
    void Update()
    {
        //if ( Input.GetKeyUp(KeyCode.S)) //Input.GetButtonUp ("Fire2")||
        //      {
        //	waitPanels[1].SetActive (true);
        //	Debug.Log ("fire2 called ****");
        //	TestDetails.TestDesc.Test = TestDetails.TestType._2MStep;
        //          SceneManager.LoadScene(2);
        //      }


        //      if (Input.GetKeyUp(KeyCode.A)) // Input.GetButtonUp ("Fire1")||
        //      {
        //	waitPanels[0].SetActive (true);
        //	TestDetails.TestDesc.Test = TestDetails.TestType._30SChair;

        //	Debug.Log ("fire1 called ***");
        //	SceneManager.LoadScene (1);// sit and stand test

        //}

        //else if (Input.GetKeyUp(KeyCode.D))// Input.GetButtonUp("Fire3")||
        //{
        //    waitPanels[3].SetActive(true);
        //    TestDetails.TestDesc.Test = TestDetails.TestType._2MStep;
        //    SceneManager.LoadScene("2m Step Test Trial");


        //}
        
        if (Input.GetKeyUp(KeyCode.F)) // Input.GetButtonUp("Fire4")||
        {
            PlayerPrefs.Save();
            //PlayerPrefs.SetString("environment_selection", "levada_canal");
            PlayerPrefs.SetString("environment_selection", "25 Fontes");
            waitPanels[2].SetActive(true);
            Debug.Log("fire4 called ****");
            TestDetails.TestDesc.Test = TestDetails.TestType._2MStep;
           // SceneManager.LoadScene("levada_canal");
            SceneManager.LoadScene("complexityLevel");
            isnonVR = vrToggle.isOn;
            Debug.Log(isnonVR+"nonVR");

        }
        if (Input.GetKeyUp(KeyCode.E)) // Input.GetButtonUp("Fire4")||
        {
            PlayerPrefs.Save();
            //PlayerPrefs.SetString("environment_selection", "levada_canal");
            PlayerPrefs.SetString("environment_selection", "Pico Areeiro - Ruivo");
            waitPanels[2].SetActive(true);
            Debug.Log("fire4 called ****");
            TestDetails.TestDesc.Test = TestDetails.TestType._2MStep;
            // SceneManager.LoadScene("levada_canal");
            SceneManager.LoadScene("complexityLevel");
            isnonVR = vrToggle.isOn;
            Debug.Log(isnonVR + "nonVR");

        }
        else if (Input.GetKeyUp(KeyCode.A)) // Input.GetButtonUp("Fire4")||
        {
            PlayerPrefs.Save();
            //PlayerPrefs.SetString("environment_selection", "levada_canal");
            PlayerPrefs.SetString("environment_selection","S Lourenço");
            waitPanels[2].SetActive(true);
            Debug.Log("fire4 called ****");
            TestDetails.TestDesc.Test = TestDetails.TestType._2MStep;
            // SceneManager.LoadScene("levada_canal");
            SceneManager.LoadScene("complexityLevel");
            isnonVR = vrToggle.isOn;
            Debug.Log(isnonVR + "nonVR");

        }
        else if (Input.GetKeyUp(KeyCode.C)) // Input.GetButtonUp("Fire4")||
        {
            PlayerPrefs.Save();
            //PlayerPrefs.SetString("environment_selection", "levada_canal");
            PlayerPrefs.SetString("environment_selection", "Caldeirao Verde");
            waitPanels[2].SetActive(true);
            Debug.Log("fire4 called ****");
            TestDetails.TestDesc.Test = TestDetails.TestType._2MStep;
            // SceneManager.LoadScene("levada_canal");
            SceneManager.LoadScene("complexityLevel");
            isnonVR = vrToggle.isOn;
            Debug.Log(isnonVR + "nonVR");

        }
        else if (Input.GetKeyUp(KeyCode.G))
        {

            calPanel.SetActive(true);
            OnCalibrateButton();
            Debug.Log("fire3 called ***");


        }


        StartButton.interactable = !string.IsNullOrEmpty(TestDetails.TestDesc.UserId);
        RecordButton.interactable = !string.IsNullOrEmpty(TestDetails.TestDesc.UserId);
        KinectValues[0].text = KinectGameObject.transform.position.y.ToString(CultureInfo.InvariantCulture);
        KinectValues[1].text = KinectGameObject.transform.rotation.eulerAngles.x.ToString(CultureInfo.InvariantCulture);
        KinectValues[2].text = KinectGameObject.transform.rotation.eulerAngles.y.ToString(CultureInfo.InvariantCulture);
        KinectValues[3].text = KinectGameObject.transform.rotation.eulerAngles.z.ToString(CultureInfo.InvariantCulture);
    }

    public void OnStartButton()
    {
        if (string.IsNullOrEmpty(TestDetails.TestDesc.UserId)) return;

        switch (TestDetails.TestDesc.Test)
        {
            case TestDetails.TestType._8FtUpGo:
                SceneManager.LoadScene("8ft Up & Go");
                break;
            case TestDetails.TestType._30SChair:
                SceneManager.LoadScene("30s Chair Stand Test");
                break;
            case TestDetails.TestType._2MStep:
                SceneManager.LoadScene("2m Step Test");
                break;
            default:
                Debug.Log("Invalid Scene to load");
                break;
        }
    }

    public void OnRecordButton()
    {
        if (string.IsNullOrEmpty(TestDetails.TestDesc.UserId)) return;
        TestDetails.TestDesc.Feedback = TestDetails.FeedbackType.Control;
        SceneManager.LoadScene("Record");
    }

    public void OnCalibrateButton()
    {
        PlayerPrefs.SetFloat("KinectHeight", KinectGameObject.transform.position.y);
        PlayerPrefs.SetFloat("KinectRotX", KinectGameObject.transform.rotation.x);
        PlayerPrefs.SetFloat("KinectRotY", KinectGameObject.transform.rotation.y);
        PlayerPrefs.SetFloat("KinectRotZ", KinectGameObject.transform.rotation.z);
        PlayerPrefs.SetFloat("KinectRotW", KinectGameObject.transform.rotation.w);
        PlayerPrefs.Save();

        TestDetails.KinectDesc.Position = new Vector3(0, PlayerPrefs.GetFloat("KinectHeight"), 0);
        TestDetails.KinectDesc.Rotation = new Quaternion(PlayerPrefs.GetFloat("KinectRotX"), PlayerPrefs.GetFloat("KinectRotY"), PlayerPrefs.GetFloat("KinectRotZ"), PlayerPrefs.GetFloat("KinectRotW"));

        KinectValues[4].text = TestDetails.KinectDesc.Position.y.ToString(CultureInfo.InvariantCulture);
        KinectValues[5].text = TestDetails.KinectDesc.Rotation.eulerAngles.x.ToString(CultureInfo.InvariantCulture);
        KinectValues[6].text = TestDetails.KinectDesc.Rotation.eulerAngles.y.ToString(CultureInfo.InvariantCulture);
        KinectValues[7].text = TestDetails.KinectDesc.Rotation.eulerAngles.z.ToString(CultureInfo.InvariantCulture);
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }

    public void OnUserIdValueChanged(string value)
    {
        TestDetails.TestDesc.UserId = value;
    }

    public void On8FtUpGoToggle(bool value)
    {
        if (value)
            TestDetails.TestDesc.Test = TestDetails.TestType._8FtUpGo;
    }

    public void On30SChairToggle(bool value)
    {
        if (value)
            TestDetails.TestDesc.Test = TestDetails.TestType._30SChair;
    }

    public void On2MStepToggle(bool value)
    {
        if (value)
            TestDetails.TestDesc.Test = TestDetails.TestType._2MStep;
    }

    public void OnMirrorToggle(bool value)
    {
        if (value)
            TestDetails.TestDesc.Feedback = TestDetails.FeedbackType.Mirror;
    }

    public void OnAbstractToggle(bool value)
    {
        if (value)
            TestDetails.TestDesc.Feedback = TestDetails.FeedbackType.Abstract;
    }

    public void OnGameToggle(bool value)
    {
        if (value)
            TestDetails.TestDesc.Feedback = TestDetails.FeedbackType.Game;
    }

    public void OnControlToggle(bool value)
    {
        if (value)
            TestDetails.TestDesc.Feedback = TestDetails.FeedbackType.Control;
    }
}
