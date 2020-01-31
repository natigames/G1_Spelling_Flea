using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;

public class menu : MonoBehaviour
{

    public Button easybtn;
    public Button normalbtn;
    public Button hardbtn;
    public Text subscribe;

    public GameObject shop;


    private void Start()
    {
        StartCoroutine(CheckIAP());
        if (PlayerPrefs.GetInt("premium") == 1)
        {
            shop.SetActive(false);
        }
        levelGUI();
    }

    public void Restart()
    {
        //vars iniciales playerprfs
        PlayerPrefs.SetInt("currentword",0);
        PlayerPrefs.SetString("words", "");
    }

    public void Logout()
    {
        PlayerPrefs.SetInt("userid", 0);
        SceneManager.LoadScene("UI");
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

    public void setLevel(string newlevel)
    {
        if (PlayerPrefs.GetInt("premium") == 0)
        {
            StartCoroutine(subscribeText());
        }
        else
        {
            PlayerPrefs.SetString("level", newlevel);
            levelGUI();
        }
    }


    public IEnumerator subscribeText()
    {
        subscribe.CrossFadeAlpha(1, 3, false);
        yield return new WaitForSeconds(3);
        subscribe.CrossFadeAlpha(0, 3, false);
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

            }
        }
    }


}
