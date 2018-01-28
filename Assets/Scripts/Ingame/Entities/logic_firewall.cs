using Assets.Scripts.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class logic_firewall : MonoBehaviour {

    [Header("Enemy Settings")]
    public float Time;
    public bool Toggled = false;

    private Timer fireTimer;
    private logic_death_collision fireComponent;

	public void Awake () {
        this.fireComponent = GetComponentInChildren<logic_death_collision>();

        if (Time > 0)
            fireTimer = Timer.Create(0, Time, ToggleFire);
        this.fireComponent.canKill(Toggled);
	}

    public void ToggleFire()
    {
        Toggled = !Toggled;
        this.fireComponent.canKill(Toggled);
	}
}