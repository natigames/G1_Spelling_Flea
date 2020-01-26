using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int currentlevel = 1;
    public int maxrows = 5;
    public string[] wordlist;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(API_getWords());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        // notice the plural for SINGLETON Pattern (only 1)
        int gameStatusCount = FindObjectsOfType<GameManager>().Length;
        if (gameStatusCount > 1)
        {
            //make inactive first (best practice)
            gameObject.SetActive(false);

            //  if one already exists and I'm number two, destroy self...
            Destroy(gameObject);
        }
        else
        {
            // Don't destroy me, I need to prevail (I'm the first)
            DontDestroyOnLoad(gameObject);
        }
    }

    public void startGame()
    {
        SceneManager.LoadScene("Game");
    }


    public IEnumerator API_getWords()
    {
        WWWForm form = new WWWForm();
        form.AddField("method", "getWords");
        form.AddField("level", currentlevel);
        form.AddField("maxrows", maxrows);
        using (var w = UnityWebRequest.Post("http://nati.games/apis/1_spelling_flea.cfc", form))
        {
            yield return w.SendWebRequest();
            if (w.isNetworkError || w.isHttpError)
            { print(w.error); }
            else
            { 
                var myString = w.downloadHandler.text;
                wordlist = myString.Split(","[0]);
            }
        }

    }




}
