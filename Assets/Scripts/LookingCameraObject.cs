using UnityEngine;

public class LookingCameraObject : MonoBehaviour
{
    private Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;   
    }

    void Update()
    {
        this.gameObject.transform.rotation = Quaternion.LookRotation(this.gameObject.transform.position - _mainCamera.transform.position);   
    }
}
