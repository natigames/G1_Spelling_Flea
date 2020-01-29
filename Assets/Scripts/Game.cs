using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FrostweepGames.Plugins.GoogleCloud.TextToSpeech;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Game : MonoBehaviour
{
    public static Game instance;

    public TMP_InputField answer;
    private List<string> wrong = new List<string>();
    private List<string> right = new List<string>();
    public int currentlevel;
    public int currentWord;
    public string[] wordlist;
    public GCTextToSpeech MC = new GCTextToSpeech();
    VoiceConfig myvoice = new VoiceConfig();
    public AudioSource audioSource;

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

    public void speakWord(string thisWord)
    {
        MC.Synthesize(thisWord, myvoice, false, 1, 1, 16000);
        Debug.Log(thisWord); //replace this with actual text to voice
    }

    public void focusoninput()
    {
        EventSystem.current.SetSelectedGameObject(answer.gameObject, null);
        answer.OnPointerClick(new PointerEventData(EventSystem.current));
    }

    public IEnumerator API_getWords()
    {
        wordlist = new string[0];
        WWWForm form = new WWWForm();
        form.AddField("method", "getWords");
        form.AddField("maxrows", 5);
        form.AddField("level", currentlevel);
        using (var w = UnityWebRequest.Post("http://nati.games/apis/1_spelling_flea.cfc", form))
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
                    speakWord(wordlist[0]);
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
            speakWord(wordlist[currentWord]);
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
                // change level, fetch new words and reset index
                currentlevel++;
                StartCoroutine(API_getWords());
                currentWord = 0;
                UIController.instance.updateLevel(currentlevel);
            }

        }

    }

    public void Replay()  
    {
        focusoninput();
        speakWord(wordlist[currentWord]);
    }


    void OnGUI()
    {
        Event e = Event.current;
        if (e.isKey)
        {
            if (e.type == EventType.KeyUp)
            {
                Debug.Log(e.keyCode); // PLAY AUDIO FOR INDIVIDUAL LETTERS
                if (e.keyCode != KeyCode.Return && e.keyCode != KeyCode.Backspace && e.keyCode != KeyCode.Escape)
                {
                    speakWord(e.keyCode.ToString());
                }
            }
        }
    }


    public void Grade(string answer, int index)
    {
        if (wordlist[index] == answer)
        {
            Debug.Log("correct!"); // speak correct
            right.Add(answer);
            UIController.instance.updateScore(currentlevel);
            //Player.instance.animCorrect(); 
        }
        else 
        {
            Debug.Log("error!"); // speak oops
            wrong.Add(answer);
        }

    }


    private IEnumerator WaitforFeedback()
    {
        yield return new WaitForSeconds(2f);
        UIController.instance.showFeedback("");
        Replay();
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
