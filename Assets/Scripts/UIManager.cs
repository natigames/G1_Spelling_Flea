using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
     public Text feedback;

    public void setFeedback(string status)
    {
        feedback.text = status;
    }
}
