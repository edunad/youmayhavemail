using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class logic_death_collision : MonoBehaviour {
    private bool _killEnabled = false;
    private SpriteRenderer _renderer;
    private BoxCollider _collider;

    public void Start()
    {
        this._renderer = GetComponent<SpriteRenderer>();
        this._collider = GetComponent<BoxCollider>();
    }

    public void canKill(bool isActive)
    {
        this._killEnabled = isActive;

        if (this._renderer == null || this._collider == null) return;
        if (_killEnabled)
        {
            this._renderer.enabled = true;
            this._collider.enabled = true;
        }
        else
        {
            this._renderer.enabled = false;
            this._collider.enabled = false;
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (CoreController.ArePlayersDead) return;
        if (collision.collider.name != "Player" || !this._killEnabled) return;
        CoreController.TriggerPlayerDeath(new DeathMsg { type = DeathType.DEATH_GENERIC, deathLocation = this.transform });
    }
}
