﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using TMPro;
//using FrostweepGames.Plugins.GoogleCloud.TextToSpeech;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using UnityEngine.Purchasing;

namespace FrostweepGames.Plugins.GoogleCloud.TextToSpeech
{
    public class Game : MonoBehaviour
    {
        public static Game instance;

        public GameObject game;
        public GameObject help;

        public InputField answer;
        public string[] wordlist;
        private GCTextToSpeech MC;
        VoiceConfig myvoice = new VoiceConfig();
        public AudioClip correctsound;
        public AudioClip wrongsound;

        private help book;
        private info info;
        public Text definition;
        public Text examples;
        public Text synonyms;

        public Text remain;
        public Text score;
        public Text pack;
        public Text level;
        public Text feedback;

        public List<string> right;
        public List<string> wrong;

        public float myscore = 0;

        private TouchScreenKeyboard mobileKeys;

            /*
        public void OnInputEvent()
        {
            mobileKeys = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false);
        }
        */

        [System.Obsolete]
        void Start()
        {
            StartCoroutine(API_getWords());
            StartCoroutine(PlayerInfo());

            mobileKeys = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false);


            MC = GCTextToSpeech.Instance;
            MC.SynthesizeSuccessEvent += _gcTextToSpeech_SynthesizeSuccessEvent;
            MC.SynthesizeFailedEvent += _gcTextToSpeech_SynthesizeFailedEvent;

            /*
            myvoice.gender = Enumerators.SsmlVoiceGender.FEMALE;
            myvoice.languageCode = "en_AU";
            myvoice.name = "en-AU-Wavenet-A";
            */

            answer.onEndEdit.AddListener(val =>
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || mobileKeys.status == TouchScreenKeyboard.Status.Done)
                    Submit();
            });

            answer.onValueChange.AddListener(val =>
            {
                if (answer.text.Length > 0)
                {
                    var myletter = answer.text.Substring(answer.text.Length - 1, 1);
                    speakLetter(myletter);
                }
            });

            switch (PlayerPrefs.GetString("voice"))
            {
                case "English":
                    myvoice.languageCode = "en-US";
                    myvoice.gender = Enumerators.SsmlVoiceGender.FEMALE;
                    myvoice.name = "en-US-Wavenet-E";
                break;
                case "Español":
                    myvoice.languageCode = "es-ES";
                    myvoice.gender = Enumerators.SsmlVoiceGender.FEMALE;
                    myvoice.name = "es-ES-Standard-A";
               break;
                case "Français":
                    myvoice.languageCode = "fr-FR";
                    myvoice.gender = Enumerators.SsmlVoiceGender.FEMALE;
                    myvoice.name = "fr-FR-Wavenet-E";
                break;
                case "Portuguese":
                    myvoice.languageCode = "pt-BR";
                    myvoice.gender = Enumerators.SsmlVoiceGender.FEMALE;
                    myvoice.name = "pt-BR-Wavenet-A";
                break;
                case "Italiano":
                    myvoice.languageCode = "it-IT";
                    myvoice.gender = Enumerators.SsmlVoiceGender.FEMALE;
                    myvoice.name = "it-IT-Wavenet-B";
                break;
            }


            //Init GUI
            answer.text = "";
            score.text = right.Count + " of " + (right.Count + wrong.Count);
            focusoninput();

            switch (PlayerPrefs.GetString("level"))
            {
                case "E": level.text = "Level: Easy"; break;
                case "M": level.text = "Level: Normal"; break;
                case "H": level.text = "Level: Hard"; break;
            }
        }

        // Catch user inputs;
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
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
                //speakWord();  

                // Grade Word
                Grade();

                // Clear text
                answer.text = "";
                focusoninput();

                //go to next
                nextword();

                if (PlayerPrefs.GetInt("currentword") <= wordlist.Length)
                {
                    StartCoroutine(WaitforFeedback());
                }
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
                    SceneManager.LoadScene("gameover");
                }

            }
            else
            {
                if (PlayerPrefs.GetInt("currentword") >= 5)
                {
                    SceneManager.LoadScene("menu");
                }
            }
        }


        //speak the word at current index (reloads if nulls)
        public void speakWord()
        {
            string thisWord = wordlist[PlayerPrefs.GetInt("currentword")];
            MC.Synthesize(thisWord, myvoice, false, 1, 1, 16000);
        }

        //speak word that is passed into
        public void speakLetter(string thisletter)
        {
            if (mobileKeys.status == TouchScreenKeyboard.Status.Done)
            {
                Submit();
            }
            else
            {
                MC.Synthesize(thisletter, myvoice, false, 1, 1, 16000);
            }
        }

        //set focus on input box
        public void focusoninput()
        {
            EventSystem.current.SetSelectedGameObject(answer.gameObject, null);
            answer.OnPointerClick(new PointerEventData(EventSystem.current));
        }

        public void Replay()
        {
            focusoninput();
            help.SetActive(false);
            wordlist = PlayerPrefs.GetString("words").Split(","[0]);
            speakWord();
        }


        public void Menu()
        {
            SceneManager.LoadScene("menu");
        }

        public void Help()
        {
            if (PlayerPrefs.GetString("voice") == "English")
            {
                StartCoroutine(getHelp());
            }
        }


        public void closeHelp()
        {
            help.SetActive(false);
            wordlist = PlayerPrefs.GetString("words").Split(","[0]);
        }

        // Speaks typed letters
        void OnGUI()
        {
            Event e = Event.current;
            if (e.isKey)
            {
                if (e.type == EventType.KeyUp)
                {
                    if (e.keyCode != KeyCode.Return)
                    {
                        /*
                        speakLetter(e.keyCode.ToString());
                        */
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
                StartCoroutine(playCorrect());
                right.Add(wordlist[PlayerPrefs.GetInt("currentword")]);
                StartCoroutine(DisplayResult("Yes! " + wordlist[PlayerPrefs.GetInt("currentword")]));
            }
            else
            {
                StartCoroutine(playWrong());
                wrong.Add(wordlist[PlayerPrefs.GetInt("currentword")]);
                StartCoroutine(DisplayResult("No! " + wordlist[PlayerPrefs.GetInt("currentword")]));
            }
            score.text = right.Count + " of " + (right.Count + wrong.Count);
            remain.text = wordlist.Length - (right.Count + wrong.Count) + " to go";
            myscore = (right.Count / (right.Count + wrong.Count)*10);
            PlayerPrefs.SetFloat("score", myscore);
        }

        // Play audio & video for correct word
        private IEnumerator playCorrect()
        {
            Player.instance.doAnim("correct");
            NewAudio.Instance.PlayOneShot(correctsound);
            yield return new WaitForSeconds(2f);
            Player.instance.doAnim("normal");
        }

        // Play audio & video for wrong word
        private IEnumerator playWrong()
        {
            Player.instance.doAnim("wrong");
            NewAudio.Instance.PlayOneShot(wrongsound);
            yield return new WaitForSeconds(2f);
            Player.instance.doAnim("normal");
        }

        // Speaks back Original Word as Feedback
        private IEnumerator WaitforFeedback()
        {
            yield return new WaitForSeconds(2.0f);
            speakWord();
        }

        // Speaks back Original Word as Feedback
        private IEnumerator DisplayResult(string mytext)
        {
            feedback.text = mytext;
            yield return new WaitForSeconds(2.0f);
            feedback.text = "";
        }


        // Fetch words and store them
        public IEnumerator API_getWords()
        {
            wordlist = new string[0];
            WWWForm form = new WWWForm();
            form.AddField("method", "getWords");
            form.AddField("level", PlayerPrefs.GetString("level"));
            form.AddField("pack", PlayerPrefs.GetInt("pack"));
            using (var w = UnityWebRequest.Post("http://nati.games/apis/spellingflea.cfc", form))
            {
                yield return w.SendWebRequest();
                if (w.isNetworkError || w.isHttpError)
                { print(w.error); }
                else
                {
                    var myString = w.downloadHandler.text;
                    PlayerPrefs.SetString("words", myString);
                    wordlist = myString.Split(","[0]);
                    remain.text = wordlist.Length + " to go";

                    speakWord();
                }
            }
        }

        // Fetch words and store them
        public IEnumerator PlayerInfo()
        {
            //wordlist = new string[0];
            WWWForm form = new WWWForm();
            form.AddField("method", "playerinfo");
            form.AddField("userid", PlayerPrefs.GetInt("userid"));
            form.AddField("pack", PlayerPrefs.GetInt("pack"));
            form.AddField("level", PlayerPrefs.GetString("level"));
            using (var w = UnityWebRequest.Post("http://nati.games/apis/spellingflea.cfc", form))
            {
                yield return w.SendWebRequest();
                if (w.isNetworkError || w.isHttpError)
                { print(w.error); }
                else
                {
                    var myString = w.downloadHandler.text;
                    info = JsonUtility.FromJson<info>(myString);
                    pack.text = info.packname;
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

        // Gets info for word and populates
        public IEnumerator getHelp()
        {
            wordlist = PlayerPrefs.GetString("words").Split(","[0]);
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
                }
            }
        }

        // Dump Player preferences
        public void DumpPP()
        {
            /*
            Debug.Log("maxrows: " + PlayerPrefs.GetInt("maxrows"));
            Debug.Log("currentword: " + PlayerPrefs.GetInt("currentword"));
            Debug.Log("premium: " + PlayerPrefs.GetInt("premium"));
            Debug.Log("pack: " + PlayerPrefs.GetInt("pack"));
            Debug.Log("userid: " + PlayerPrefs.GetInt("userid"));
            Debug.Log("level: " + PlayerPrefs.GetString("level"));
            */
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
            /*
            audioSource.clip = MC.GetAudioClipFromBase64(response.audioContent, Constants.DEFAULT_AUDIO_ENCODING);
            audioSource.Play();
            */
            var myclip = MC.GetAudioClipFromBase64(response.audioContent, Constants.DEFAULT_AUDIO_ENCODING);
            NewAudio.Instance.PlayWord(myclip);
        }
        #endregion sucess handlers

    }
}