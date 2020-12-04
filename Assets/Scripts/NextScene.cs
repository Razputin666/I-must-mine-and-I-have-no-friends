using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NextScene : MonoBehaviour
{
    private int nextSceneToLoad;
    
        private void Start()
    {
        nextSceneToLoad = SceneManager.GetActiveScene().buildIndex + 1;
    }

    
        public void changescene()
        {
        SceneManager.LoadScene(nextSceneToLoad);
        }
}
