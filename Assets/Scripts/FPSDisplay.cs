using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FPSDisplay : MonoBehaviour
{
    public int avgFrameRate;
    public Text display_Text;
    private float deltaTime;
    public void Awake()
    {
        //StartCoroutine(Start());
    }

    IEnumerator Start()
    {
        while (true)
        {
            if (Time.timeScale == 1)
            {
                //yield return new WaitForSeconds(0.1f);
                deltaTime = (1 / Time.deltaTime);
                display_Text.text = "FPS :" + (Mathf.Round(deltaTime));
            }
            else
            {
                display_Text.text = "Pause";
            }
            yield return new WaitForSeconds(1.0f);
        }
    }
}