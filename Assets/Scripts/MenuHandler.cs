using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuHandler : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        Cursor.visible = false;
        SceneManager.LoadScene(sceneName);
    }
}
