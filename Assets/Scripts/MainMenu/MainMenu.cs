using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    private bool isStarting = false;
    public void Awake()
    {
        isStarting = false;
    }

    public void Update()
    {
        if (Input.GetButtonDown("Start") && !isStarting)
        {
            this.isStarting = true;
            SceneManager.LoadScene("Intro", LoadSceneMode.Single);
        }
    }
}
