using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FrostweepGames.Plugins.GoogleCloud.TextToSpeech;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Purchasing;

public class Game : MonoBehaviour
{
    public static Game instance;
    public bool play = false;
    public bool isPremium = false;

    public GameObject menu;
    public GameObject game;
    public GameObject shop;

    public TMP_InputField answer;
    public int currentlevel;
    public int currentWord;
    public string[] wordlist;
    public GCTextToSpeech MC = new GCTextToSpeech();
    VoiceConfig myvoice = new VoiceConfig();
    public AudioSource audioSource;
    public AudioClip correctsound;
    public AudioClip wrongsound;
    public AudioClip openmenu;

    // Start is called before the first frame update
    void Start()
    {
        MC = GCTextToSpeech.Instance; 
        MC.SynthesizeSuccessEvent += _gcTextToSpeech_SynthesizeSuccessEvent;
        MC.SynthesizeFailedEvent += _gcTextToSpeech_SynthesizeFailedEvent;

        myvoice.languageCode = "en_AU";
        myvoice.gender = Enumerators.SsmlVoiceGender.FEMALE;
        myvoice.name = "en-GB-Wavenet-A";

        currentlevel = 1;
        currentWord = 0;

        StartCoroutine(API_getWords());
        focusoninput();



        if (PlayerPrefs.GetInt("premium") == 1){isPremium = true;}

        if (!play)
        {
            Menu();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(play)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Submit();
            }
        }
    }

    public void speakWord()
    {
        if (wordlist.Length >= currentWord && wordlist.Length > 0 && currentWord >= 0)
        {
            string thisWord = wordlist[currentWord];
            MC.Synthesize(thisWord, myvoice, false, 1, 1, 16000);
        }
    }

    public void speakLetter(string thisletter)
    {
        MC.Synthesize(thisletter, myvoice, false, 1, 1, 16000);
    }

    public void focusoninput()
    {
        EventSystem.current.SetSelectedGameObject(answer.gameObject, null);
        answer.OnPointerClick(new PointerEventData(EventSystem.current));
    }

    public IEnumerator API_getWords()
    {
        string myLevel = PlayerPrefs.GetString("level");
        if(myLevel != "E" && myLevel != "M" && myLevel != "H")
        {
            myLevel = "E";
            PlayerPrefs.SetString("level", myLevel);
            PlayerPrefs.SetInt("pack", 0);
            PlayerPrefs.SetInt("maxrows", 5);
        }
        int myPack = PlayerPrefs.GetInt("pack");
        int myRows = PlayerPrefs.GetInt("maxrows");


        wordlist = new string[0];
        WWWForm form = new WWWForm();
        form.AddField("method", "getWords");
        form.AddField("maxrows", myRows);
        form.AddField("level", myLevel);
        form.AddField("pack", myPack);
        using (var w = UnityWebRequest.Post("http://nati.games/apis/spellingflea.cfc", form))
        {
            yield return w.SendWebRequest();

            if (w.isNetworkError || w.isHttpError)
            { print(w.error); }
            else
            {
                var myString = w.downloadHandler.text;
                wordlist = myString.Split(","[0]);
                if (wordlist.Length > 0)
                {
                    if (play)
                    {
                        speakWord();
                    }
                }
            }
        }

    }

    public void Submit()
    {

        if (answer.text.Length > 0)
        {
            // Repeat Full Word
            StartCoroutine(WaitforFeedback());
            speakWord();
            UIController.instance.showFeedback(wordlist[currentWord]);

            // Grade Word
            Grade(answer.text, currentWord);

            // Clear text
            answer.text = "";
            focusoninput();

            //go to next
            currentWord++;
            if (currentWord >= wordlist.Length)
            {
                StartCoroutine(CheckIAP());
            }

        }

    }

    public void Replay()  
    {
        audioSource.Stop();
        menu.gameObject.SetActive(false);
        game.gameObject.SetActive(true);
        play = true;
        focusoninput();
        speakWord();
    }

    public void Restart()
    {
        UIController.instance.updateScore(0);
        audioSource.Stop();
        menu.gameObject.SetActive(false);
        game.gameObject.SetActive(true);
        play = true;
        focusoninput();
        if (PlayerPrefs.GetInt("premium") == 1)
        {
            StartCoroutine(API_getWords());
            currentlevel = 1;
            currentWord = 0;
        }
        else
        {
            currentWord = 0;
        }

    }

    public void Menu()
    {
        audioSource.PlayOneShot(openmenu);
        menu.gameObject.SetActive(true);
        game.gameObject.SetActive(false);

        if (isPremium)
        {
            shop.gameObject.SetActive(false);
        }
    }


    public void closeIAP()
    {
        shop.gameObject.SetActive(false);
    }


    void OnGUI()
    {
        Event e = Event.current;
        if (e.isKey)
        {
            if (e.type == EventType.KeyUp)
            {
                if (e.keyCode != KeyCode.Return && e.keyCode != KeyCode.Backspace && e.keyCode != KeyCode.Escape)
                {
                    if (PlayerPrefs.GetInt("premium") == 1 || currentlevel == 1)
                    {
                        speakLetter(e.keyCode.ToString());
                    }

                }
            }
        }
    }


    public void Grade(string answer, int index)
    {
        StartCoroutine(LogAnswer(wordlist[index], answer));

        if (wordlist[index].ToLower() == answer.ToLower())
        {
            int points = 1;
            UIController.instance.updateScore(points);
            StartCoroutine(playCorrect());
        }
        else 
        {
            StartCoroutine(playWrong());
        }

    }

    private IEnumerator playCorrect()
    {
        BGScroller.instance.moveRight();
        Player.instance.doAnim("correct");
        audioSource.PlayOneShot(correctsound);
        yield return new WaitForSeconds(2f);
        Player.instance.doAnim("normal");
    }

    private IEnumerator playWrong()
    {
        Player.instance.doAnim("wrong");
        audioSource.PlayOneShot(wrongsound);
        yield return new WaitForSeconds(2f);
        Player.instance.doAnim("normal");
    }


    private IEnumerator WaitforFeedback()
    {
        yield return new WaitForSeconds(2f);
        UIController.instance.showFeedback("");
        Replay();
    }

    private IEnumerator LogAnswer(string original, string capture)
    {
        WWWForm form = new WWWForm();
        form.AddField("method", "logAnswer");
        form.AddField("word", original);
        form.AddField("input", capture);
        form.AddField("pack", PlayerPrefs.GetInt("pack"));
        form.AddField("userid", PlayerPrefs.GetInt("userid"));
        using (var w = UnityWebRequest.Post("http://nati.games/apis/spellingflea.cfc", form))
        {
            yield return w.SendWebRequest();
        }
    }

    private IEnumerator CheckIAP()
    {
        WWWForm form = new WWWForm();
        form.AddField("method", "checkIAP");
        form.AddField("userid", PlayerPrefs.GetInt("userid"));
        using (var w = UnityWebRequest.Post("http://nati.games/apis/spellingflea.cfc", form))
        {
            yield return w.SendWebRequest();
            if (w.isNetworkError || w.isHttpError)
            { print(w.error); }
            else
            {
                var myStatus = w.downloadHandler.text;
                if (myStatus == "0")
                {
                    shop.SetActive(true);
                    PlayerPrefs.SetInt("premium",0);
                }
                else if (myStatus == "1")
                {
                    shop.SetActive(false);
                    PlayerPrefs.SetInt("premium",1);
                }
            }


            // change level, fetch new words and reset index
            if (PlayerPrefs.GetInt("premium") == 1)
            {
                currentlevel++;
                answer.text = "";
                StartCoroutine(API_getWords());
                currentWord = 0;
                UIController.instance.updateLevel(currentlevel);
            }
            else
            {
                currentlevel++;
                answer.text = "Please purchase to continue";
                shop.SetActive(true);
                Menu();
            }

        }
    }




    #region failed handlers
    private void _gcTextToSpeech_SynthesizeFailedEvent(string error)
    {
        Debug.Log(error);
    }
    #endregion failed handlers

    #region sucess handlers
    private void _gcTextToSpeech_SynthesizeSuccessEvent(PostSynthesizeResponse response)
    {
        audioSource.clip = MC.GetAudioClipFromBase64(response.audioContent, Constants.DEFAULT_AUDIO_ENCODING);
        audioSource.Play();
    }
    #endregion sucess handlers




}
