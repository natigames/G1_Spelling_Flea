using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;


public class Authentication : MonoBehaviour
{
    private string email, password, username, mobile;
    private Text feedback;
    WWWForm form;
    public user user;

    public void DoLogin(){StartCoroutine(RequestLogin());}
    public void DoRegister(){StartCoroutine(RequestRegister());}
    public void RecoverPass(){StartCoroutine(RequestPassword());}

    public IEnumerator RequestLogin()
    {
        email = GameObject.Find("EmailTextLogin").GetComponent<Text>().text;
        password = GameObject.Find("PasswordFieldLogin").GetComponent<InputField>().text;
        feedback = FindObjectOfType<UIManager>().feedback;

        form = new WWWForm();
        form.AddField("email", email);
        form.AddField("password", password);

        using (var w = UnityWebRequest.Post("http://nati.games/apis/_account.cfc?method=login", form))
        {
            yield return w.SendWebRequest();
            if (w.isNetworkError || w.isHttpError)
                {
                feedback.text = "Unable to Connect";
                yield return new WaitForSeconds(2);
                feedback.text = "";
                }
            else
            {
                var myString = w.downloadHandler.text;

                user = JsonUtility.FromJson<user>(myString);
                feedback.text = user.error;

                if (user.userid > 0)
                {
                    PlayerPrefs.SetInt("userid", user.userid);

                    yield return new WaitForSeconds(2);
                    SceneManager.LoadScene("game");
                }
                else
                {
                    yield return new WaitForSeconds(2);
                    feedback.text = "";
                }

            }
        }

    }

    public IEnumerator RequestRegister()
    {
        email = GameObject.Find("EmailTextRegister").GetComponent<Text>().text;
        username = GameObject.Find("NameTextRegister").GetComponent<Text>().text;
        mobile = GameObject.Find("MobileTextRegister").GetComponent<Text>().text;
        password = GameObject.Find("PasswordTextRegister").GetComponent<Text>().text;
        feedback = FindObjectOfType<UIManager>().feedback;

        form = new WWWForm();
        form.AddField("email", email);
        form.AddField("password", password);
        form.AddField("mobile", mobile);
        form.AddField("username", username);

        using (var w = UnityWebRequest.Post("http://nati.games/apis/_account.cfc?method=register", form))
        {
            yield return w.SendWebRequest();
            if (w.isNetworkError || w.isHttpError)
            {
                feedback.text = "Unable to Connect";
                yield return new WaitForSeconds(2);
                feedback.text = "";
            }
            else
            {
                var myString = w.downloadHandler.text;

                user = JsonUtility.FromJson<user>(myString);
                feedback.text = user.error;

                if (user.userid > 0)
                {
                    PlayerPrefs.SetInt("userid", user.userid);
                    yield return new WaitForSeconds(2);
                    SceneManager.LoadScene("game");
                }
                else
                {
                    yield return new WaitForSeconds(2);
                    feedback.text = "";
                }

            }
        }

    }

    public IEnumerator RequestPassword()
    {
        email = GameObject.Find("EmailTextLogin").GetComponent<Text>().text;
        feedback = FindObjectOfType<UIManager>().feedback;

        form = new WWWForm();
        form.AddField("email", email);

        using (var w = UnityWebRequest.Post("http://nati.games/apis/_account.cfc?method=recover", form))
        {
            yield return w.SendWebRequest();
            if (w.isNetworkError || w.isHttpError)
            {
                feedback.text = "Unable to Connect";
                yield return new WaitForSeconds(2);
                feedback.text = "";
            }
            else
            {
                var myString = w.downloadHandler.text;
                user = JsonUtility.FromJson<user>(myString);
                feedback.text = user.error;
                yield return new WaitForSeconds(2);
                feedback.text = "";
            }
        }

    }


    public void Logout()
    {
        PlayerPrefs.DeleteKey("userid");
        SceneManager.LoadScene("UI");
    }

}
