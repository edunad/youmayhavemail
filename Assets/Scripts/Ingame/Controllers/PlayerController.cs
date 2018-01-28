using Assets.Scripts.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;

public class PlayerController : MonoBehaviour {

    [Header("Ray Settings")]
    public LayerMask CollisionMask;

    [Header("Player Settings")]
    public float player_jumpPower = 4;
    public float player_Acceleration = 2.0f;
    public float player_MaximumSpeed = 28f;

    // Controllers
    private Rigidbody _body;
    private Animator _animator;
    private SpriteRenderer _renderer;

	public void Awake () {
        this._body = GetComponent<Rigidbody>();
        this._body.isKinematic = false;

        this._renderer = GetComponentInChildren<SpriteRenderer>();
        this._renderer.enabled = true;

        this._animator = GetComponentInChildren<Animator>();
	}

    public void FixedUpdate()
    {
        if (this._body == null) return;
        this._animator.SetFloat("Speed", Mathf.Abs(this._body.velocity.x));
    }

	public void Update () {
        if (this._body == null || CoreController.ArePlayersDead) return;

        #region Keyboard
        float vertical = Mathf.Round(Input.GetAxis("VerticalPlayer"));
        float verticalKey = Mathf.Round(Input.GetAxis("VerticalPlayerKey"));

        if ((vertical >= 0.1f || verticalKey >= 0.1f) && this._body.velocity.x < player_MaximumSpeed)
            this._body.AddForce(new Vector3(player_Acceleration * this._body.mass, 0f, 0f));

        if ((vertical <= -0.1f || verticalKey <= -0.1f) && this._body.velocity.x > -player_MaximumSpeed)
            this._body.AddForce(new Vector3(-player_Acceleration * this._body.mass, 0f, 0f));

        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
            this.characterJump();
        #endregion
       
    }

    private void characterJump()
    {
        if (!isOnGround()) return;
        this._body.AddForce(new Vector3(0f, 0f, player_jumpPower * this._body.mass));
        
    }

    private bool isOnGround()
    {
        return Physics.Raycast(this.transform.position, -Vector3.forward, 0.8f, CollisionMask);
    }

    public void OnDeath(DeathMsg deathMsg)
    {
        this._body.isKinematic = true;
        this._renderer.enabled = false;
        // TODO : POOF ANIMATION
    }
}
