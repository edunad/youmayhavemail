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
        this._renderer = GetComponentInChildren<SpriteRenderer>();
        this._animator = GetComponentInChildren<Animator>();
	}

    public void FixedUpdate()
    {
        if (this._body == null) return;
        this._animator.SetFloat("Speed", Mathf.Abs(this._body.velocity.x));
    }

	public void Update () {
        if (this._body == null || CoreController.ArePlayersDead) return;

        #region Keyboard Simulation : TODO MOVE TO INPUT
        if (InputController.isRightAction() && this._body.velocity.x < player_MaximumSpeed)
            this._body.AddForce(new Vector3(player_Acceleration * this._body.mass, 0f, 0f));

        if (InputController.isLeftAction() && this._body.velocity.x > -player_MaximumSpeed)
            this._body.AddForce(new Vector3(-player_Acceleration * this._body.mass, 0f, 0f));

		if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || InputController.isUpAction())
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

    private void OnDeath(DeathMsg deathMsg)
    {
        if (CoreController.ArePlayersDead) return;

        // Set Death Location Camera
        Timer.Simple(0.5f, () =>
        {
            Camera2D.CameraLookAt(deathMsg.deathLocation, Vector3.zero);
        });

        Debug.Log("Recieved death");
    }
}
