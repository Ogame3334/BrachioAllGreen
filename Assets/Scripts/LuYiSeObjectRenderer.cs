using System;
using System.Collections.Generic;
using System.Linq;
using Meta.XR;
using PassthroughCameraSamples;
using TMPro;
using UnityEngine;

public class LuYiSeObjectLabelRenderer : MonoBehaviour
{
    [SerializeField]
    private WebCamTextureManager _webCamTextureManager;
    [SerializeField]
    private GameObject _labelPrefab;
    [SerializeField]
    private EnvironmentRaycastManager _environmentRaycastManager;

    private List<GameObject> _viewingObjects = new List<GameObject>();

    private const int YoloInputSize = 640;
    private Camera _mainCamera;


    void Awake()
    {
        if(!_labelPrefab.TryGetComponent<TextMeshPro>(out var _)) throw new NullReferenceException();
        _mainCamera = Camera.main;
    }

    public void RenderLabel(RenderableLabelInfo[] renderableLabels){
        ClearViewingObjects();

        var intrinsics = PassthroughCameraUtils.GetCameraIntrinsics(_webCamTextureManager.eye);
        var camResolution = intrinsics.Resolution;

        foreach(var renderableLabel in renderableLabels){
            var perPosition = renderableLabel.position / YoloInputSize;

            var centerPixel = new Vector2(
                perPosition.x * camResolution.x,
                (1f - perPosition.y) * camResolution.y
            );
    
            var centerRay = PassthroughCameraUtils.ScreenPointToRayInWorld(
                _webCamTextureManager.eye, 
                new Vector2Int(
                    Mathf.RoundToInt(centerPixel.x),
                    Mathf.RoundToInt(centerPixel.y)
                )
            );

            if(!_environmentRaycastManager.Raycast(centerRay, out var centerHit)){
                continue;
            }

            var markerWorldPos = centerHit.point;
            var depth = Vector3.Distance(_mainCamera.transform.position, markerWorldPos);
            var labelObj = Instantiate(_labelPrefab, markerWorldPos, Quaternion.identity);
            labelObj.transform.localScale /= depth + 1;
            labelObj.GetComponent<TextMeshPro>().text = renderableLabel.label;
            _viewingObjects.Add(labelObj);
        }
    }

    private void ClearViewingObjects(){
        foreach(var obj in _viewingObjects){
            if(obj && obj.gameObject){
                Destroy(obj);
            }
        }
        _viewingObjects.Clear();
    }
}
