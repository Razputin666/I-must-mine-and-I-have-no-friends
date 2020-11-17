using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Blink : MonoBehaviour
{

    Text flashingText;

    void Start()
    {
        //get the Text component
        flashingText = gameObject.GetComponent<Text>();
        //Call coroutine BlinkText on Start
        StartCoroutine(BlinkText());
    }

    //function to blink the text 
    public IEnumerator BlinkText()
    {
        //blink it forever. You can set a terminating condition depending upon your requirement
        while (true)
        {
            flashingText.text = "Blackened Stars";
            int i = 255;
            for (; i > 100; i--)
            {
                float j =(float)i / (float)255;
                flashingText.color = new Color(1, 1, 1, j);
                Debug.Log(j);
                yield return new WaitForSeconds(0.01f);
            }
            for (; i < 249; i++)
            {
                float j = (float)i / (float)255;
                flashingText.color = new Color(1, 1, 1, j);
                Debug.Log(j);
                yield return new WaitForSeconds(0.01f);
            }
          //  for (; i == 254; i++)
            {
                //flashingText.color = new Color(flashingText.color.r, flashingText.color.g, flashingText.color.b, i);
                //yield return new WaitForSeconds(0.1f);
            }

        }   
    }

}
