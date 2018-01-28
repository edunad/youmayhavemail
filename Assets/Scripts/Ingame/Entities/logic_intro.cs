using Assets.Scripts.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class logic_intro : MonoBehaviour {

    public string introID = "lvl0";
    public float fadeInSpeed = 0.05f;
    public float fadeOutSpeed = 0.05f;
    public float fadeTime = 2f;

    private SpriteRenderer _introSprite;
    private float _currentAlpha;
    private int _fadeType;

	private void Awake () {
        this._introSprite = GetComponent<SpriteRenderer>();
        this._introSprite.color = new Color(1f, 1f, 1f, 0f);
        this._introSprite.enabled = true;

        if (Global.lastShownIntro == introID || introID == "")
        {
            Destroy(this);
            return;
        }

        Global.lastShownIntro = introID;
        this._currentAlpha = 0f;
        this._fadeType = 0;
	}
	
	// Update is called once per frame
	private void Update () {
        if (this._currentAlpha <= 1f && this._fadeType == 0)
        {
            float alpha = Mathf.Lerp(0, 1f, this._currentAlpha);
            this._introSprite.color = new Color(1f, 1f, 1f, alpha);
            this._currentAlpha += fadeInSpeed;

            if (this._currentAlpha >= 0.9f)
            {
                this._fadeType = 1;

                Timer.Simple(this.fadeTime, () =>
                {
                    this._fadeType = 2;
                    this._currentAlpha = 1f;
                });
            }
        }
        else if (this._currentAlpha >= 0f && this._fadeType == 2)
        {
            float alpha = Mathf.Lerp(0, 1f, this._currentAlpha);
            this._introSprite.color = new Color(1f, 1f, 1f, alpha);
            this._currentAlpha -= fadeInSpeed;

            if (this._currentAlpha <= 0f)
            {
                this._fadeType = 4;
                this._introSprite.enabled = false;
                Destroy(this);
            }
        }
	}
}
