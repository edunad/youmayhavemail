using Assets.Scripts.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudController : MonoBehaviour {

    [Header("Settings")]
    public float acceleration = 2.0f;
    public float maximumSpeed = 28f;

    // Controllers
    private Rigidbody _body;
    private Animator _animator;
    private SpriteRenderer _renderer;

    private float[] _rndAnimations = new float[]{
        0.1f,
        0.5f,
        1f
    };

    public void Awake()
    {
        this._body = GetComponent<Rigidbody>();
        this._renderer = GetComponentInChildren<SpriteRenderer>();
        this._animator = GetComponentInChildren<Animator>();

        Timer.Simple(Random.Range(4f, 8f), TriggerRandomAnimation);
    }
    public void Update ()
    {
        if (_body == null || CoreController.ArePlayersDead) return;
        float horizontal = Mathf.Round(Input.GetAxis("HorizontalCloud"));
        float vertical = Mathf.Round(Input.GetAxis("VerticalCloud"));

        float keyHor = Input.GetAxis("VerticalCloudKey");
        float keyVert = Input.GetAxis("HorizontalCloudKey");

        if ((horizontal <= -0.1f || keyHor >= 0.1f) && this._body.velocity.z > -maximumSpeed)
            this._body.AddForce(new Vector3(0f, 0f, acceleration * this._body.mass));

        if ((horizontal >= 0.1f || keyHor <= -0.1f) && this._body.velocity.z < maximumSpeed)
            this._body.AddForce(new Vector3(0f, 0f, -acceleration * this._body.mass));

        if ((vertical >= 0.1f || keyVert >= 0.1f) && this._body.velocity.x < maximumSpeed)
            this._body.AddForce(new Vector3(acceleration * this._body.mass, 0f, 0f));

        if ((vertical <= -0.1f || keyVert <= -0.1f) && this._body.velocity.x > -maximumSpeed)
            this._body.AddForce(new Vector3(-acceleration * this._body.mass, 0f, 0f));
    }

    public void TriggerRandomAnimation()
    {
        if (this._animator == null || CoreController.ArePlayersDead) return;

        float rndAnim = this._rndAnimations[Random.Range(0, this._rndAnimations.Length)];
        _animator.SetFloat("RndIdle", rndAnim);

        // Reset
        Timer.Simple(4f, () =>
        {
            _animator.SetFloat("RndIdle", 0f);
            Timer.Simple(Random.Range(4f, 8f), TriggerRandomAnimation);
        });
    }

    public void OnDeath(DeathMsg deathMsg)
    {
        this._body.isKinematic = true;
        this._renderer.enabled = false;
        // TODO : POOF ANIMATION
    }
}
