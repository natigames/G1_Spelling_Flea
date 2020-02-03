using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class settings : MonoBehaviour
{
    public VehiclesList VehicleList = new VehiclesList();
    public Dropdown PackDropDown;
    public Dropdown LevelDropDown;
    public Dropdown VoiceDropDown;
    public Dropdown LangDropDown;
    public Text SelectedPackName;
    public InputField AllWords;
    public InputField InputPackName;
    private List<string> options = new List<string>();
    private string[] wordlist;

    // Start is called before the first frame update
    void Start()
    {
        wordlist = new string[0];
        AllWords.text = "";
        PlayerPrefs.SetString("packname", "Columbia 1st Grade - English");
        PlayerPrefs.SetInt("pack",1);
        PlayerPrefs.SetString("voice", "English");
        PlayerPrefs.SetString("level", "E");

        switch (PlayerPrefs.GetString("Level"))
        {
            case "E":
                LevelDropDown.value = 0;
                break;
            case "M":
                LevelDropDown.value = 1;
                break;
            case "H":
                LevelDropDown.value = 2;
                break;
            default:
                LevelDropDown.value = 0;
                break;
        };


        for (var i=0;i<VoiceDropDown.options.Count;i++)
        {
            if (VoiceDropDown.options[i].text == PlayerPrefs.GetString("voice"))
            {
                VoiceDropDown.value = i;
            }
        }


        //get pack name
        for (var i = 0; i < PackDropDown.options.Count; i++)
        {
            if (PackDropDown.options[i].text == PlayerPrefs.GetString("packname"))
            {
                PackDropDown.value = i;
            }
        }


        PackDropDown.onValueChanged.AddListener(delegate {
            Listen2Select();
        });
        LevelDropDown.onValueChanged.AddListener(delegate {
            Listen2Select();
        });
        VoiceDropDown.onValueChanged.AddListener(delegate {
            PlayerPrefs.SetString("voice", VoiceDropDown.options[VoiceDropDown.value].text);
            Debug.Log("Voice: " + PlayerPrefs.GetString("voice"));
        });

        StartCoroutine(getPacks());
        StartCoroutine(getWords());

    }

    public void Listen2Select()
    {
        foreach (Vehicles vehicle in VehicleList.Vehicles)
        {
            if (PackDropDown.options[PackDropDown.value].text == vehicle.Model)
            {
                PlayerPrefs.SetInt("pack", vehicle.Make);
                SelectedPackName.text = vehicle.Model;
                PlayerPrefs.SetString("packname", vehicle.Model);
            }
        }

        PlayerPrefs.SetString("level", LevelDropDown.options[LevelDropDown.value].text.Substring(0, 1));
        StartCoroutine(getWords());
    }

    public void NewPack()
    {
        VehicleList = new VehiclesList();
        PackDropDown.ClearOptions();
        Debug.Log("total options: " + PackDropDown.options.Count);
        StartCoroutine(createPack());
    }

    public void DelPack()
    {
        options.Clear();
        StartCoroutine(delPackCR());
    }

    public void addWords()
    {
        StartCoroutine(updateWords());
    }


    public void exit()
    {
        SceneManager.LoadScene("menu");   
    }

    // Fetch words and store them
    public IEnumerator delPackCR()
    {
        WWWForm form = new WWWForm();
        form.AddField("method", "delPack");
        form.AddField("userid", PlayerPrefs.GetInt("userid"));
        form.AddField("pack", PlayerPrefs.GetInt("pack"));
        using (var w = UnityWebRequest.Post("http://nati.games/apis/spellingflea.cfc", form))
        {
            yield return w.SendWebRequest();
            PlayerPrefs.SetInt("pack", 0);
            StartCoroutine(getPacks());
            PackDropDown.value = 0;
        }

    }


    // Fetch words and store them
    public IEnumerator getPacks()
    {
        options.Clear();
        PackDropDown.options.Clear();
        PackDropDown.ClearOptions();
        VehicleList = new VehiclesList();

        WWWForm form = new WWWForm();
        form.AddField("method", "getPacks");
        form.AddField("userid", PlayerPrefs.GetInt("userid"));
        using (var w = UnityWebRequest.Post("http://nati.games/apis/spellingflea.cfc", form))
        {
            yield return w.SendWebRequest();
            if (w.isNetworkError || w.isHttpError)
            { print(w.error); }
            else
            {
                string  myString = w.downloadHandler.text;
                VehicleList = JsonUtility.FromJson<VehiclesList>(myString);

                foreach (Vehicles vehicle in VehicleList.Vehicles)
                {
                    options.Add(vehicle.Model);
                }

                PackDropDown.AddOptions(options);

                StartCoroutine(getWords());
            }
        }
    }


    // Fetch words and store them
    public IEnumerator getWords()
    {
        wordlist = new string[0];
        AllWords.text = "";
        WWWForm form = new WWWForm();
        form.AddField("method", "getWords");
        form.AddField("level", PlayerPrefs.GetString("level"));
        form.AddField("pack", PlayerPrefs.GetInt("pack"));
        form.AddField("order","insdate");
        using (var w = UnityWebRequest.Post("http://nati.games/apis/spellingflea.cfc", form))
        {
            yield return w.SendWebRequest();
            if (w.isNetworkError || w.isHttpError)
            { print(w.error); }
            else
            {
                var myString = w.downloadHandler.text;
                wordlist = myString.Split(","[0]);

                AllWords.text = myString;

            }
        }
    }




    // Fetch words and store them
    public IEnumerator createPack()
    {
        wordlist = new string[0];
        AllWords.text = "";
        WWWForm form = new WWWForm();
        form.AddField("method", "newPack");
        form.AddField("packname", InputPackName.text);
        form.AddField("userid", PlayerPrefs.GetInt("userid"));
        using (var w = UnityWebRequest.Post("http://nati.games/apis/spellingflea.cfc", form))
        {
            yield return w.SendWebRequest();
            if (w.isNetworkError || w.isHttpError)
            { print(w.error); }
            else
            {
                var myString = w.downloadHandler.text;
                InputPackName.text = "";
                StartCoroutine(getPacks());
            }
        }
    }


    public IEnumerator updateWords()
    {

        Debug.Log(PlayerPrefs.GetInt("pack"));
        Debug.Log(PlayerPrefs.GetString("level"));
        Debug.Log(PlayerPrefs.GetInt("userid"));
        Debug.Log(AllWords.text);

        WWWForm form = new WWWForm();
        form.AddField("method", "insertWords");
        form.AddField("pack", PlayerPrefs.GetInt("pack"));
        form.AddField("level", PlayerPrefs.GetString("level"));
        form.AddField("userid", PlayerPrefs.GetInt("userid"));
        form.AddField("wordlist", AllWords.text);
   
        using (var w = UnityWebRequest.Post("http://nati.games/apis/spellingflea.cfc", form))
        {
            yield return w.SendWebRequest();
            StartCoroutine(getWords());
        }
    }



}
