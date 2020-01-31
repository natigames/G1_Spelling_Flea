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

    public GameObject menu;
    public GameObject game;
    public GameObject shop;
    public GameObject help;


    public TMP_InputField answer;
    public string[] wordlist;
    public GCTextToSpeech MC;
    VoiceConfig myvoice = new VoiceConfig();
    public AudioSource audioSource;
    public AudioClip correctsound;
    public AudioClip wrongsound;
    public AudioClip openmenu;

    private help book;
    public Text definition;
    public Text examples;
    public Text synonyms;
    public string usedwords;

    public Button easybtn;
    public Button normalbtn;
    public Button hardbtn;

    // Start is called before the first frame update
    public void init()
    {
        StartCoroutine(CheckIAP());
        shop.SetActive(true);
        help.SetActive(false);
        game.SetActive(false);
        menu.SetActive(true);

        MC = GCTextToSpeech.Instance;
        MC.SynthesizeSuccessEvent += _gcTextToSpeech_SynthesizeSuccessEvent;
        MC.SynthesizeFailedEvent += _gcTextToSpeech_SynthesizeFailedEvent;

        myvoice.languageCode = "en_AU";
        myvoice.gender = Enumerators.SsmlVoiceGender.FEMALE;
        myvoice.name = "en-GB-Wavenet-A";

        UIController.instance.updateScore(-1);
        PlayerPrefs.SetInt("currentword", 0);
        answer.text = "";
        usedwords = "";
        focusoninput();
    }

    void Start()
    {
        init();
        StartCoroutine(API_getWords());
        Menu();
    }

    // Catch user inputs;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Submit();
        }
    }

    // this happens when enter word is submitted
    public void Submit()
    {

        if (answer.text.Length > 0)
        {
            // Repeat Full Word
            StartCoroutine(WaitforFeedback());
            speakWord();
            UIController.instance.showFeedback(wordlist[PlayerPrefs.GetInt("currentword")]);

            // Grade Word
            Grade();

            // Clear text
            answer.text = "";
            focusoninput();

            //go to next
            nextword();
        }
    }

    // update index and speak next word according to profile
    public void nextword()
    {
        PlayerPrefs.SetInt("currentword", PlayerPrefs.GetInt("currentword") + 1); 

        if (PlayerPrefs.GetInt("premium") == 1)
        {
            if (PlayerPrefs.GetInt("currentword") >= wordlist.Length)
            {
                PlayerPrefs.SetInt("currentword", 0);
                StartCoroutine(API_getWords());
            }
        }
        else
        {
            if (PlayerPrefs.GetInt("currentword") >= 5)
            {
                PlayerPrefs.SetInt("currentword", 0);
                PlayerPrefs.SetString("words", "");
                answer.text = "Subscribe/Reload to continue";
                reloadwords();
            }
        }
    }

    //speak the word at current index (reloads if nulls)
    public void speakWord()
    {
        reloadwords();
        if (PlayerPrefs.GetInt("premium") == 1 || wordlist.Length > 1)
        {
            string thisWord = wordlist[PlayerPrefs.GetInt("currentword")];
            MC.Synthesize(thisWord, myvoice, false, 1, 1, 16000);
        }
    }

    //speak word that is passed into
    public void speakLetter(string thisletter)
    {
        MC.Synthesize(thisletter, myvoice, false, 1, 1, 16000);
    }

    //set focus on input box
    public void focusoninput()
    {
        EventSystem.current.SetSelectedGameObject(answer.gameObject, null);
        answer.OnPointerClick(new PointerEventData(EventSystem.current));
    }

    public void Replay()  
    {
        audioSource.Stop();
       focusoninput();
       menu.SetActive(false);
        help.SetActive(false);
       game.SetActive(true);
       speakWord();
    }

    public void Restart()
    {
        StartCoroutine(CheckIAP());
        init();
        Replay();
    }

    public void Menu()
    {
        audioSource.PlayOneShot(openmenu);
        menu.SetActive(true);
        game.SetActive(false);
        shop.SetActive(true);
        if (PlayerPrefs.GetInt("premium") == 1)
        {
            shop.SetActive(false);
        }
        levelGUI();
    }

    public void Help()
    {
        StartCoroutine(getHelp());
    }

    public void closeIAP()
    {
        //reload words
        reloadwords();
        shop.SetActive(false);
    }

    public void closeHelp()
    {
        help.SetActive(false);

        //reload words
        reloadwords();

        game.SetActive(true);
    }

    // Gets Words from Playerprefs andupdates game.array
    public void reloadwords()
    {
        string myString = PlayerPrefs.GetString("words");
        wordlist = myString.Split(","[0]);
        if (wordlist.Length <= 1)
        {
            answer.text = "Subscribe/Reload to continue";
        }
    }

    // Speaks typed letters
    void OnGUI()
    {
        Event e = Event.current;
        if (e.isKey)
        {
            if (e.type == EventType.KeyUp)
            {
                if (e.keyCode != KeyCode.Return && e.keyCode != KeyCode.Backspace && e.keyCode != KeyCode.Escape)
                {
                    if (wordlist.Length > 1)
                    {
                        speakLetter(e.keyCode.ToString());
                    }
                }
            }
        }
    }

    // Calls Log, Grades input and Displays Result
    public void Grade()
    {
        StartCoroutine(LogAnswer(wordlist[PlayerPrefs.GetInt("currentword")]));

        if (wordlist[PlayerPrefs.GetInt("currentword")].ToLower() == answer.text.ToLower())
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

    // Play audio & video for correct word
    private IEnumerator playCorrect()
    {
        BGScroller.instance.moveRight();
        Player.instance.doAnim("correct");
        audioSource.PlayOneShot(correctsound);
        yield return new WaitForSeconds(2f);
        Player.instance.doAnim("normal");
    }

    // Play audio & video for wrong word
    private IEnumerator playWrong()
    {
        Player.instance.doAnim("wrong");
        audioSource.PlayOneShot(wrongsound);
        yield return new WaitForSeconds(2f);
        Player.instance.doAnim("normal");
    }

    // Speaks back Original Word as Feedback
    private IEnumerator WaitforFeedback()
    {
        yield return new WaitForSeconds(1.5f);
        UIController.instance.showFeedback("");
        Replay();
    }

    // Fetch words and store them
    public IEnumerator API_getWords()
    {
        wordlist = new string[0];
        WWWForm form = new WWWForm();
        form.AddField("method", "getWords");
        form.AddField("maxrows", PlayerPrefs.GetInt("maxrows"));
        form.AddField("level", PlayerPrefs.GetString("level"));
        form.AddField("pack", PlayerPrefs.GetInt("pack"));
        form.AddField("except", usedwords);
        using (var w = UnityWebRequest.Post("http://nati.games/apis/spellingflea.cfc", form))
        {
            yield return w.SendWebRequest();
            if (w.isNetworkError || w.isHttpError)
            { print(w.error); }
            else
            {
                var myString = w.downloadHandler.text;
                PlayerPrefs.SetString("words", myString);
                usedwords = usedwords + "," + myString;
                reloadwords();

            }
        }
    }

    // Logs Answer in DB
    private IEnumerator LogAnswer(string myword)
    {

        WWWForm form = new WWWForm();
        form.AddField("method", "logAnswer");
        form.AddField("input", myword);
        form.AddField("word", wordlist[PlayerPrefs.GetInt("currentword")]);
        form.AddField("level", PlayerPrefs.GetInt("level"));
        form.AddField("pack", PlayerPrefs.GetInt("pack"));
        form.AddField("userid", PlayerPrefs.GetInt("userid"));
        using (var w = UnityWebRequest.Post("http://nati.games/apis/spellingflea.cfc", form))
        {
            yield return w.SendWebRequest();
        }
    }

    // Updates PlayerPrefs.Premium(int)
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
                if (w.downloadHandler.text == "0")
                {
                    PlayerPrefs.SetInt("premium", 0);
                    PlayerPrefs.SetInt("maxrows", 5);
                    PlayerPrefs.SetInt("pack", 0);
                    PlayerPrefs.SetInt("currentword", 0);
                    PlayerPrefs.SetString("words", "");
                    PlayerPrefs.SetString("level", "E");
                    shop.SetActive(true);
                }
                else if (w.downloadHandler.text == "1")
                {
                    PlayerPrefs.SetInt("premium", 1);
                    PlayerPrefs.SetString("words", "");
                    PlayerPrefs.SetInt("currentword", 0);
                    shop.SetActive(false);
                }

                if (wordlist.Length <= 1 && PlayerPrefs.GetInt("premium") == 1)
                {
                    StartCoroutine(API_getWords());
                }

            }
        }
    }

    // Gets info for word and populates
    public IEnumerator getHelp()
    {
        reloadwords();
        string myword = wordlist[PlayerPrefs.GetInt("currentword")];

        wordlist = new string[0];
        WWWForm form = new WWWForm();
        form.AddField("method", "getHelp");
        form.AddField("word", myword);
        using (var w = UnityWebRequest.Post("http://nati.games/apis/spellingflea.cfc", form))
        {
            yield return w.SendWebRequest();

            if (w.isNetworkError || w.isHttpError)
            { print(w.error); }
            else
            {
                var myString = w.downloadHandler.text;
                book = JsonUtility.FromJson<help>(myString);
                definition.text = book.definition;
                examples.text = book.examples;
                synonyms.text = book.synonyms;
                help.SetActive(true);
                game.SetActive(false);

            }
        }
    }

    // Dump Player preferences
    public void DumpPP()
    {
        Debug.Log("maxrows: " + PlayerPrefs.GetInt("maxrows"));
        Debug.Log("currentword: " + PlayerPrefs.GetInt("currentword"));
        Debug.Log("premium: " + PlayerPrefs.GetInt("premium"));
        Debug.Log("pack: " + PlayerPrefs.GetInt("pack"));
        Debug.Log("userid: " + PlayerPrefs.GetInt("userid"));
        Debug.Log("level: " + PlayerPrefs.GetString("level"));
        Debug.Log("words: " + PlayerPrefs.GetString("words"));
    }

    public void levelGUI()
    {
        switch (PlayerPrefs.GetString("level"))
        {
            case "E":
                easybtn.gameObject.SetActive(false);
                normalbtn.gameObject.SetActive(true);
                hardbtn.gameObject.SetActive(true);
                break;
            case "M":
                easybtn.gameObject.SetActive(true);
                normalbtn.gameObject.SetActive(false);
                hardbtn.gameObject.SetActive(true);
                break;
            case "H":
                easybtn.gameObject.SetActive(true);
                normalbtn.gameObject.SetActive(true);
                hardbtn.gameObject.SetActive(false);
                break;
            default:
                break;
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
