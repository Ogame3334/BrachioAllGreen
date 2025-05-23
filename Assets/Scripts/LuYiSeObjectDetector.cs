using System.Collections;
using System.Linq;
using PassthroughCameraSamples;
using TMPro;
using Unity.Sentis;
using UnityEngine;

public class LuYiSeObjectDetector : MonoBehaviour
{
    [SerializeField] private ModelAsset _modelAsset;
    [SerializeField] private BackendType _backendType = BackendType.CPU;
    [SerializeField] private WebCamTextureManager _webCamTextureManager;
    [SerializeField] private LuYiSeObjectLabelRenderer _luYiSeObjectLabelRenderer;
    private Model _model;
    private Worker _worker;
    private WebCamTexture _webcamTexture;
    private Texture2D _texture;
    private Coroutine _inferenceCoroutine;
    private float inferenceInterval = 0.1f;
    private Camera _mainCamera;
    private Vector3 _diffPosition;
    private Quaternion _diffRotation;

    [SerializeField] private TextMeshProUGUI textMesh;
    
    void Start()
    {
        _mainCamera = Camera.main;
        _webcamTexture = _webCamTextureManager.WebCamTexture;
        if(_webcamTexture){
            _texture = new Texture2D(_webcamTexture.width, _webcamTexture.height, TextureFormat.RGBA32, false);
        }
        LoadModel();

        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        _inferenceCoroutine = StartCoroutine(CoInferenceLoop());
    }

    void OnDisable()
    {
        StopCoroutine(_inferenceCoroutine);
        _inferenceCoroutine = null;
    }

    private void OnDestroy()
    {
        if(_inferenceCoroutine is not null){
            StopCoroutine(_inferenceCoroutine);
            _inferenceCoroutine = null;
        }
        _worker?.Dispose();
        if(_texture is not null){
            Destroy(_texture);
            _texture = null;
        }
    }


    void LoadModel(){
        _model = ModelLoader.Load(_modelAsset);
        _worker = new Worker(_model, _backendType);
    }

    private IEnumerator CoInferenceLoop(){
        while(isActiveAndEnabled){
            if(_webcamTexture is null){
                _webcamTexture = _webCamTextureManager.WebCamTexture;

                if(_webcamTexture is not null){
                    _texture = new Texture2D(_webcamTexture.width, _webcamTexture.height, TextureFormat.RGBA32, false);
                }
            }

            yield return new WaitForSeconds(inferenceInterval);
            
            if(_texture is null) continue;

            _texture.SetPixels(_webcamTexture.GetPixels());
            _texture.Apply();
            _diffPosition = _mainCamera.transform.position;
            _diffRotation = _mainCamera.transform.rotation;

            yield return StartCoroutine(CoInferenceObject(_texture));
        }
    }

    private IEnumerator CoInferenceObject(Texture2D texture){
        Tensor<float> inputTensor = TextureConverter.ToTensor(texture, 640, 640, 3);

        var schedule = _worker.ScheduleIterable(inputTensor);
        if(schedule is null){
            _worker.Schedule(inputTensor);
        }
        else{
            var it = 0;
            while(schedule.MoveNext()){
                if(++it % 20 == 0){
                    yield return null;
                }
            }
        }

        Tensor<float> pullCoords = _worker.PeekOutput(0) as Tensor<float>;
        Tensor<int> pullLabelIDs = _worker.PeekOutput(1) as Tensor<int>;
        Tensor<float> coordResults = null;
        Tensor<int> labelIDResults = null;

        int downloadState = 0;
        bool isWating = false;

        while(true){
            switch(downloadState){
                case 0:
                    if(pullCoords?.dataOnBackend == null){
                        inputTensor.Dispose();
                        yield break;
                    }
                    if(!isWating){
                        pullCoords.ReadbackRequest();
                        isWating = true;
                    }
                    else if(pullCoords.IsReadbackRequestDone()){
                        coordResults = pullCoords.ReadbackAndClone();
                        isWating = false;
                        ++downloadState;
                    }
                    break;

                case 1:
                    if(pullLabelIDs?.dataOnBackend == null){
                        inputTensor.Dispose();
                        coordResults?.Dispose();
                        yield break;
                    }
                    if(!isWating){
                        pullLabelIDs.ReadbackRequest();
                        isWating = true;
                    }
                    else if(pullLabelIDs.IsReadbackRequestDone()){
                        labelIDResults = pullLabelIDs.ReadbackAndClone();
                        isWating = false;
                        ++downloadState;
                    }
                    break;
                
                case 2:
                    textMesh.text = "";

                    float[] coords = coordResults.DownloadToArray();
                    int[] labelIDs = labelIDResults.DownloadToArray();
                    
                    var renderableLabels = coords
                                            .Select((c, i) => new {c, i})
                                            .GroupBy(x => x.i / 4)
                                            .Select(g => g.Select(x => x.c)
                                                            .ToArray())
                                            .Select((ga, i) => new {v = new Vector2(ga[0], ga[1]), i})
                                            .Join(
                                                labelIDs.Select((l, i) => new {l, i}), 
                                                li => li.i, 
                                                vi => vi.i, 
                                                (vi, li) => new RenderableLabelInfo(){label = EnumToString.Enum2String<YoloClasses>(li.l), position = vi.v}
                                            )
                                            .ToArray();

                    Transform mainCameraTransform = _mainCamera.transform;
                    
                    _diffPosition = mainCameraTransform.position - _diffPosition;
                    _diffRotation = Quaternion.Inverse(_diffRotation) * mainCameraTransform.rotation;
                    _luYiSeObjectLabelRenderer.RenderLabel(renderableLabels, new HMVMoveDiff(){position = _diffPosition, rotation = _diffRotation});


                    // textMesh.text += string.Join('\n', renderableLabels.Select(rl => $"{rl.label}: {rl.position}"));
                    textMesh.text = $"HMV Position: {_mainCamera.transform.position}, Rotation: {_mainCamera.transform.rotation.eulerAngles}\n";
                    textMesh.text += $"position: {_diffPosition}, rotation: {_diffRotation.eulerAngles}\n";
                    textMesh.text += $"LabelObjectPoolLength: {LabelObjectPool.GetPoolLength()}\n";
                    textMesh.text += $"LabelObjectPoolActiveLength: {LabelObjectPool.GetPoolActiveLength()}\n";
                    textMesh.text += string.Join(", ", LabelObjectPool.GetLabelNames());

                    downloadState = 3;
                    break;

                case 3:
                    inputTensor.Dispose();
                    coordResults?.Dispose();
                    labelIDResults?.Dispose();
                    yield break;
            }
            yield return null;
        }
    }
}
