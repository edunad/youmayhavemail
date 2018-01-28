using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class logic_goal : MonoBehaviour {

    public string nextScene = "Main_Menu";

    private void OnCollisionEnter(Collision collision)
    {
        if (CoreController.ArePlayersDead) return;

        if(collision.gameObject.name == "Player")
            SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
    }
}
