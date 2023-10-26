using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{

    private bool menuAlreadyLoaded = false;

    void Start()
    {
        if (menuAlreadyLoaded == false)
        {
            SceneManager.LoadScene("Menu");
            menuAlreadyLoaded = true;
        }

    }
}
