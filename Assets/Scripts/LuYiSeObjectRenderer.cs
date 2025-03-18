using System.Collections.Generic;
using PassthroughCameraSamples;
using UnityEngine;

public class LuYiSeObjectLabelRenderer : MonoBehaviour
{
    [SerializeField]
    private WebCamTextureManager _webCamTextureManager;
    [SerializeField]
    private GameObject _labelPrefab;

    private List<GameObject> _viewingObjects = new List<GameObject>();


    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void RenderLabel(RenderableLabelInfo[] renderableLabels){
        
    }
}
