using Assets.Scripts.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageController : MonoBehaviour {

    private GameObject _player;
    private SpriteRenderer _healthRenderer;

    private LOSEntity _playerLOS;
    private RevealStates _lastState;

    private bool _canRegainHealth;
    private float _oldPower;

    public float damagePerSecond = 0.5f;
    public float maxShakePower = 0.3f;

    [HideInInspector]
    public float playerHealth;

	public void Awake () {
        this._player = GameObject.Find("Player");
        if (this._player == null)
            Debug.LogError("'Player' not found, did you change the name?");

        this._playerLOS = this._player.GetComponent<LOSEntity>();

        GameObject healthDng = GameObject.Find("HealthDanger");
        if (healthDng == null)
            Debug.LogError("'HealthDanger' not found, did you change the name?");

        this._healthRenderer = healthDng.GetComponent<SpriteRenderer>();
        this._healthRenderer.color = new Color(1f, 1f, 1f, 0f);
        this.playerHealth = 100;
	}
	
	// Update is called once per frame
	public void Update () {
        this.checkPlayerOnStatic();
	}

    private void checkPlayerOnStatic()
    {
        if (CoreController.ArePlayersDead) return;
        
        #region LOSChecker
        RevealStates stat = this._playerLOS.RevealState;
        if (_lastState != stat)
        {
            if (stat != RevealStates.Hidden && !this._canRegainHealth)
            {
                this._canRegainHealth = true;
            }

            this._lastState = stat;
        }

        if (stat == RevealStates.Hidden)
        {
            this.playerHealth = Mathf.Clamp(this.playerHealth - this.damagePerSecond, 0, 100);
            this._canRegainHealth = false;

            if (this.playerHealth <= 0)
            {
                CoreController.TriggerPlayerDeath(new DeathMsg { type = DeathType.DEATH_STATIC, deathLocation = _player.transform });
                return;
            }

            #region SHAKE BABY SHAKE
            float power = (100 - this.playerHealth) * maxShakePower / 100;
            if (this._oldPower != power)
            {
                this._oldPower = power;
                Camera2D.ShakeCamera(power);
            }
            #endregion
            
        }
        else if(_canRegainHealth && this.playerHealth < 100)
        {
            this.playerHealth = Mathf.Clamp(this.playerHealth + 0.3f, 0, 100);
        }
        #endregion

        if (_healthRenderer != null)
        {
            float alpha = Mathf.InverseLerp(60f, 0f, this.playerHealth);
            _healthRenderer.color = new Color(255, 255, 255, alpha);
        }
    }
}
