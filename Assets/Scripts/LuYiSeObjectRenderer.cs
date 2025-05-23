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
    private EnvironmentRaycastManager _environmentRaycastManager;
    [SerializeField]
    private LabelObjectPool _labelObjectPool;

    private List<GameObject> _viewingObjects = new List<GameObject>();

    private const int YoloInputSize = 640;
    private Camera _mainCamera;


    void Awake()
    {
        _mainCamera = Camera.main;
    }

    public void RenderLabel(RenderableLabelInfo[] renderableLabels, HMVMoveDiff hmvMoveDiff){
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

            markerWorldPos -= _mainCamera.transform.position;
            markerWorldPos = Quaternion.Inverse(hmvMoveDiff.rotation) * markerWorldPos;
            markerWorldPos += _mainCamera.transform.position;

            markerWorldPos -= hmvMoveDiff.position;
            var depth = Vector3.Distance(_mainCamera.transform.position, markerWorldPos);
            var labelObj = LabelObjectPool.GetLabelObject();
            labelObj.transform.position = markerWorldPos;
            // var labelObj = Instantiate(_labelPrefab, markerWorldPos, Quaternion.identity);
            labelObj.transform.localScale = Vector3.one / (depth + 1);
            labelObj.GetComponent<TextMeshPro>().text = renderableLabel.label;
            _viewingObjects.Add(labelObj);
        }
    }

    private void ClearViewingObjects(){
        foreach(var obj in _viewingObjects){
            if(obj && obj.gameObject){
                obj.SetActive(false);
            }
        }
        _viewingObjects.Clear();
    }
}
