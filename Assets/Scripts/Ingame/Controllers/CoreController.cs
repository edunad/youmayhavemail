using Assets.Scripts.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoreController : MonoBehaviour {

    public static DamageController damageController;
    public static bool ArePlayersDead;

    private static SpriteRenderer _gameOverRenderer;
    private static SpriteRenderer _gameOverRenderer_paper;
    private static bool _canRetry;

    private static GameObject _player;
    private static GameObject _cloud;

    private void Awake()
    {
        CoreController.damageController = GetComponent<DamageController>();
        CoreController.ArePlayersDead = false;

        CoreController._gameOverRenderer = GameObject.Find("YouDied_Sign").GetComponent<SpriteRenderer>();
        CoreController._gameOverRenderer.enabled = false;

        CoreController._gameOverRenderer_paper = GameObject.Find("YouDied_SignRetry").GetComponent<SpriteRenderer>();
        CoreController._gameOverRenderer_paper.enabled = false;
            
        CoreController._canRetry = false;

        CoreController._player = GameObject.Find("Player");
        CoreController._cloud = GameObject.Find("Cloud");
	}

    private void Update()
    {
        Timer.Update();

        if (!CoreController.ArePlayersDead || !CoreController._canRetry) return;
        if (Input.GetButtonDown("Reset"))
        {
            // Reset the scene
            CoreController._canRetry = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public static void TriggerPlayerDeath(DeathMsg msg)
    {
        if (CoreController._gameOverRenderer == null || CoreController._gameOverRenderer_paper == null) return;

        CoreController.ArePlayersDead = true;
        CoreController._gameOverRenderer.enabled = true;

        CoreController._player.SendMessage("OnDeath", msg, SendMessageOptions.DontRequireReceiver);
        CoreController._cloud.SendMessage("OnDeath", msg, SendMessageOptions.DontRequireReceiver);

        Global.deathCounter += 1;
        CoreController._canRetry = true;
        CoreController._gameOverRenderer_paper.enabled = true;
    }
}
