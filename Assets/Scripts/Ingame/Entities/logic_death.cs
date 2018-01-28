using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DeathType
{
    DEATH_GENERIC = 0,
    DEATH_FALL = 1,
    DEATH_STATIC = 2
}

[Serializable]
public struct DeathMsg
{
    public DeathType type;
    public Transform deathLocation;
}

public class logic_death : MonoBehaviour {

    public DeathType deathType;
    public Vector2 _deathSize;

    private GameObject _player;

	// Use this for initialization
	void Start () {
        _player = GameObject.Find("Player");
        this.name = "logic_death";
	}
	
	// Update is called once per frame
    void Update()
    {
        if (_player == null) return;
        if (CoreController.ArePlayersDead) return;

        Vector3 col = transform.position;
        Vector3 playerPos = _player.transform.position;

        float relativeX = col.x + _deathSize.x;
        float relativeZ = col.z + _deathSize.y;

        if (playerPos.x > col.x && playerPos.x < relativeX)
            if (playerPos.z > relativeZ && playerPos.z < col.z)
            {
                CoreController.TriggerPlayerDeath(new DeathMsg { type = this.deathType, deathLocation = _player.transform });
                return;
            }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(255, 0, 0, 100);

        // Draw PIT
        Vector3 col = transform.position;

        float relativeX = col.x + _deathSize.x;
        float relativeZ = col.z + _deathSize.y;

        // Draw Lines
        Gizmos.DrawLine(new Vector3(col.x, col.y, col.z), new Vector3(relativeX, col.y, col.z));
        Gizmos.DrawLine(new Vector3(col.x + _deathSize.x / 2, col.y, col.z), new Vector3(col.x + _deathSize.x / 2, col.y, relativeZ));

        // Draw Circles
        Gizmos.DrawSphere(col, 0.1f);
        Gizmos.DrawSphere(new Vector3(relativeX, col.y, col.z), 0.1f);
        Gizmos.DrawSphere(new Vector3(col.x + _deathSize.x / 2, col.y, relativeZ), 0.1f);

        // Draw Icon
        Gizmos.DrawIcon(new Vector3(col.x + _deathSize.x / 2, col.y, col.z), "logic_dead", true);
    }
}
