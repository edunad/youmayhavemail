﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* BASED ON : https://weesals.wordpress.com/2014/03/15/unity-rts-part-2-line-of-sight/ */
public enum RevealStates { Hidden, Fogged, Unfogged, };

public class LOSEntity : MonoBehaviour {

    // Revealers reveal the surrounding area of terrain
    public bool IsRevealer = false;
    public float Range = 10;

    // Height is used for AO and peering over walls
    public float Height = 1;
    public float BaseSize = 0;

    // Our bounds on the terrain
    public Rect Bounds
    {
        get
        {
            var bounds = new Rect(
                transform.position.x - BaseSize / 2,
                transform.position.z - BaseSize / 2,
                BaseSize, BaseSize);
            return bounds;
        }
    }

    // Current state, if we're visible, fogged, or hidden
    public RevealStates RevealState;

    public void SetIsCurrentTeam(bool isCurrent)
    {
        IsRevealer |= isCurrent;
    }

    // Tell the LOS manager that we're here
    public void OnEnable()
    {
        LOSManager.AddEntity(this);
    }
    public void OnDisable()
    {
        LOSManager.RemoveEntity(this);
    }

    // Some cache parameters for FOW animation
    Color _oldfowColor = Color.clear;
    Color _fowColor = Color.clear;
    float _fowInterp = 1;
    // Request a change to the FOW colour
    public void SetFOWColor(Color fowColor, bool interpolate)
    {
        if (fowColor == _fowColor) return;

        if (!interpolate)
        {
            _fowColor = fowColor;
            _fowInterp = 1;

            UpdateFOWColor();
            return;
        }

        _oldfowColor = Color.Lerp(_oldfowColor, _fowColor, _fowInterp);
        _fowColor = fowColor;
        _fowInterp = 0;

        UpdateFOWColor();
    }
    // Does this item require fog of war updates
    public bool RequiresFOWUpdate { get { return _fowInterp < 1; } }
    // Returns true when the item has completed transitioning its fog of war colour
    public bool UpdateFOWColor()
    {
        _fowInterp = Mathf.Clamp01(_fowInterp + Time.deltaTime / 0.4f);
        var fowColor = Color.Lerp(_oldfowColor, _fowColor, _fowInterp);
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            if (fowColor.r > 0 || fowColor.g > 0)
            {
                foreach (var material in renderer.materials)
                {
                    material.SetColor("_FOWColor", fowColor);
                }
            }
        }
        return !RequiresFOWUpdate;
    }
}
