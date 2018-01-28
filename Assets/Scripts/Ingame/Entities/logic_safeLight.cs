using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(LOSEntity))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class logic_safeLight : MonoBehaviour {

    private LOSEntity _losEntity;
    private SpriteRenderer _renderer;
    private Animator _animator;

	void Awake () {
        this._renderer = GetComponent<SpriteRenderer>();
        this._losEntity = GetComponent<LOSEntity>();
        this._animator = GetComponent<Animator>();

        this._losEntity.IsRevealer = true;
	}
}
