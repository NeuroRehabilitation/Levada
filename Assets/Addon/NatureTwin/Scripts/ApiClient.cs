using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using TMPro;

public class ApiClient : MonoBehaviour
{
    [SerializeField] 
    private string base_url = "http://127.0.0.1:5000/levada/Levada das 25 Fontes";
    private List<string> names = new List<string>();
    public TextMeshProUGUI timerText;

    void Start()
    {
        StartCoroutine(GetData(base_url));
    }

    IEnumerator GetData(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("X-API-Key", "my-api-key-123");  
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {request.error}");
                yield break;
            }

            string json = request.downloadHandler.text;
            NatureTwinApi post = JsonUtility.FromJson<NatureTwinApi>(json);
            Current cur =  post.current;
            timerText.text = JsonUtility.ToJson(cur);
        }
    }

    public void GetAPIData(string extra_url = null) {
        StartCoroutine(GetData(base_url));
    }
}
