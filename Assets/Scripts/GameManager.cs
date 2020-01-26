using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int currentlevel;
    public int currentWord;
    public int maxrows = 5;
    public string[] wordlist;
    public int score = 0;
    public TMP_InputField answer;
    private List<string> wrong = new List<string>();
    private List<string> right = new List<string>();


    // Start is called before the first frame update
    void Start()
    {
        currentlevel = 1;
        currentWord = 0;
        StartCoroutine(API_getWords());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Return key was pressed. WORD: " + answer.text);
            Submit();
        }

    }

    public void Submit()
    {

        if (answer.text.Length > 0)
        {
            // Repeat Full Word

            // Grade Word
            Grade(answer.text, currentWord);

            // Clear text
            answer.text = "";

            //go to next
            speakWord();

        }

    }


    void OnGUI()
    {
        Event e = Event.current;
        if (e.isKey)
        {
            if (e.type == EventType.KeyUp)
            {
                Debug.Log(e.keyCode); // PLAY AUDIO FOR INDIVIDUAL LETTRS
            }
        }
    }


    public void Grade(string answer, int index)
    {
        if (wordlist[index] == answer)
            {
            right.Add(answer);
            Debug.Log("correct!"); // speak correct
            }
        else
            {
            wrong.Add(answer);
            Debug.Log("incorrect"); // speak incorrect
            }

        currentWord++;
        if (currentWord >= wordlist.Length)
        {
            // change level, fetch new words and reset index
            currentlevel++;
            StartCoroutine(API_getWords());
            currentWord = 0;
        }
    }


    // SINGLETON TO MAINTAIN VARIABLES
    private void Awake()
    {
        int gameStatusCount = FindObjectsOfType<GameManager>().Length;
        if (gameStatusCount > 1)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }


    public void startGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void speakWord()
    {
        if (wordlist.Length > 0)
        {
            var thisWord = wordlist[currentWord];
            Debug.Log(thisWord); //replace this with actual text to voice
        }

    }


    public IEnumerator API_getWords()
    {
        wordlist = new string[0];
    
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
                speakWord();
            }
        }

    }




}
