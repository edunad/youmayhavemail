using Assets.Scripts.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour {

    private GameObject _postIt;
    private SpriteRenderer _postItRenderer;
    private Animator _postItAnimator;

    private GameObject _cloudAnim;
    private SpriteRenderer _cloudAnimRenderer;
    private Animator _cloudAnimAnimator;

    private bool allowFade;
    private float fadeTimer;
    private bool endLevel;
    private bool ALLDONE;

	public void Awake () {

        this._postIt = GameObject.Find("PostitIntro");
        if (this._postIt == null)
            Debug.LogError("'PostitIntro' not found, is it renamed?");

        this._cloudAnim = GameObject.Find("CloudAnim");
        if (this._cloudAnim == null)
            Debug.LogError("'CloudAnim' not found, is it renamed?");

        this._cloudAnimRenderer = this._cloudAnim.GetComponent<SpriteRenderer>();
        this._cloudAnimAnimator = this._cloudAnim.GetComponent<Animator>();

        this._postItRenderer = this._postIt.GetComponent<SpriteRenderer>();
        this._postItAnimator = this._postIt.GetComponent<Animator>();

        // Reset variables
        this._postItAnimator.SetBool("startIntro", false);
        this._postItRenderer.color = new Color(1f, 1f, 1f, 0f);
        this._cloudAnimAnimator.SetBool("startIntro", false);
        this._cloudAnimRenderer.color = new Color(1f, 1f, 1f, 0f);

        this.fadeTimer = 0f;
        this.ALLDONE = false;
        this.endLevel = false;

        Timer.Simple(2f, () =>
        {
            StartIntro();
        });
	}

    private void StartIntro()
    {
        this.allowFade = true;
    }

    private void Update () {
        Timer.Update();

        if (this.ALLDONE) return;

        if (this.allowFade && this.fadeTimer < 1 && !this.endLevel)
        {
            float alpha = Mathf.Lerp(0f, 1f, this.fadeTimer);
            this._postItRenderer.color = new Color(1f, 1f, 1f, alpha);
            this._cloudAnimRenderer.color = new Color(1f, 1f, 1f, alpha);

            this.fadeTimer += 0.03f;
            if (this.fadeTimer >= 1f)
            {
                Timer.Simple(0.5f, () =>
                {
                    this._postItAnimator.SetBool("startIntro", true);
                    this._cloudAnimAnimator.SetBool("startIntro", true);

                    Timer.Simple(7.5f, () =>
                    {
                        this.endLevel = true;
                        this.fadeTimer = 1f;
                        return;
                    });
                });
            }
        }

        if (this.endLevel && this.fadeTimer > 0f)
        {
            float alpha = Mathf.Lerp(0f, 1f, this.fadeTimer);
            this._postItRenderer.color = new Color(1f, 1f, 1f, alpha);
            this._cloudAnimRenderer.color = new Color(1f, 1f, 1f, alpha);

            this.fadeTimer -= 0.03f;

            if (this.fadeTimer <= 0f)
            {
                this.ALLDONE = true;
                SceneManager.LoadScene("1_Belfast", LoadSceneMode.Single);
                return;
            }
        }
	}
}
