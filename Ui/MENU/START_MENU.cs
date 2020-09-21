using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class START_MENU : MonoBehaviour
{
    public GameObject Loading_Canvas;
    public Image Title_Text;
    Color set_color = new Color(1, 1, 1, 1);
    float d_color = 0.01f;
    bool colorflag = true;

    public void Update()
    {
        Debug.Log(set_color);

        if (colorflag)
        {
            Title_Text.color = set_color;
            set_color.a = d_color;
            d_color -= 0.005f;

            if (set_color.a <= 0)
            {
                colorflag = false;
            }
        }
        else if (!colorflag)
        {
            Title_Text.color = set_color;
            set_color.a = d_color;
            d_color += 0.005f;

            if (set_color.a >= 1)
            {
                colorflag = true;
            }

        }
    }

    public void Start_Loading()
    {
        Debug.Log("start");
        StartCoroutine(Load_Play());
    }

    public void GOTO_PLAY()
    {
        SceneManager.LoadScene("PLAY");
    }

    IEnumerator Load_Play()
    {
        Loading_Canvas.SetActive(true);
        yield return null;
        GOTO_PLAY();
    }
}