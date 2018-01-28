using UnityEngine;
using System.Collections;
using Assets.Scripts.General;

public enum ShakeMode {
    SHAKE_UP = 1,
    SHAKE_DOWN = 2,
    SHAKE_LEFT = 3,
    SHAKE_RIGHT = 4,
    SHAKE_ALL = 5
}

public class Camera2D : MonoBehaviour {

    public float dampTime = 0.15f;

    // SHAKE SETTINGS
    public static bool isCameraShaking;
    public static bool allowCameraFollow;

    private static ShakeMode _shakemode;
    private static float _shakePower;
    
    // OBJECTS
    private Camera _camera;
    private static GameObject _player;

    // OTHER
    private static Vector3 _cameraOffset;
    private static Transform _target;
    private Vector3 _zero;

    private static float targetBloom;

	// Use this for initialization
    void Awake()
    {
        _camera = GetComponent<Camera>();
        _player = GameObject.Find("Player"); // Player
        _zero = Vector3.zero;

        ResetLook(); // Set default look to be player
	}
	
	// Update is called once per frame
	void Update ()
    {
        #region Camera Stuff
        if (!allowCameraFollow || _target == null) return;

        Vector3 point = _camera.WorldToViewportPoint(_target.position);

        Vector3 delta = _target.position - _camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
        Vector3 destination = transform.position + _cameraOffset + delta;
        Vector3 final = Vector3.SmoothDamp(transform.position, destination, ref _zero, dampTime);

        if (isCameraShaking)
        {
            switch (_shakemode)
            {
                case ShakeMode.SHAKE_ALL:
                    final.z += Random.value * _shakePower * 2 - _shakePower;
                    final.x += Random.value * _shakePower * 2 - _shakePower;
                break;
                case ShakeMode.SHAKE_UP:
                    final.z += Random.value * _shakePower * 2 - _shakePower;
                break;
                case ShakeMode.SHAKE_DOWN:
                    final.z -= Random.value * _shakePower * 2 - _shakePower;
                break;
                case ShakeMode.SHAKE_LEFT:
                    final.x += Random.value * _shakePower * 2 - _shakePower;
                break;
                case ShakeMode.SHAKE_RIGHT:
                    final.x -= Random.value * _shakePower * 2 - _shakePower;
                break;
                default:
                break;
            }

            isCameraShaking = false;
        }

        transform.position = final;
        #endregion
    }

    public static void ResetLook()
    {
        if (_player != null)
            _target = _player.transform;

        allowCameraFollow = true;
        _cameraOffset = new Vector3(1f, 0f, 1f);
    }

    public static void CameraLookAt(Transform pos,Vector3 offset)
    {
        _target = pos;
        _cameraOffset = offset;
    }

    public static void ShakeCamera(float power = 0.3f, ShakeMode shakemode = ShakeMode.SHAKE_ALL)
    {
        isCameraShaking = true;
        _shakePower = power;
        _shakemode = shakemode;
    }

    public static void StopShake()
    {
        isCameraShaking = false;
    }

    public static void SetCameraBloom(float bloom)
    {
        targetBloom = bloom;
    }
}
