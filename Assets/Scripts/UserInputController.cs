using System;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum Hand
{
    Left,
    Right
}

public class UserInputController : MonoBehaviour
{
    [SerializeField]
    private Hand _hand = Hand.Left;
    [SerializeField]
    private GameObject _qvPenObject;
    [SerializeField]
    private GameObject _objectDetector;
    [SerializeField]
    public PassthroughLayerEffectManager plem;
    Collider _other = null;
    private PickUpableObject _pickUpableObject = null;
    private bool _isGripping = false;
    private QvPen _qvPen = null;
    private float _triggerPressDuration = 0f;
    private static bool IsInverseChromaKey = false;

    private static int FirstStartUpState = 0;

    void Start()
    {
        TextDisplayManager.WriteTextCenter("Loading Systems . . .", 0.2f);
    }

    private void Update()
    {
        bool isGripDowned = _hand == Hand.Left ? OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger) : OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger);
        bool isGripReleased = _hand == Hand.Left ? OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger) : OVRInput.GetUp(OVRInput.Button.SecondaryHandTrigger);
        bool isTriggerDowned = _hand == Hand.Left ? OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) : OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger);
        bool isTriggerReleased = _hand == Hand.Left ? OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger) : OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger);
        bool isTriggerPressed = _hand == Hand.Left ? OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) : OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger);
        float prevTriggerPressDuration = _triggerPressDuration;

        if(FirstStartUpState == 0){
            if(TextDisplayManager.IsCanWrite()){
                FirstStartUpState = 1;
                TextDisplayManager.WriteTextCenter("System All Green.", 0.2f, false);                
            }
            
            return;
        }
        else if(FirstStartUpState == 1){
            if(TextDisplayManager.IsCanWrite()){
                FirstStartUpState = 2;
                TextDisplayManager.WriteTextCenter("Activate by holding trigger for 1 second.", 0, false);                
            }
        }
        else if(FirstStartUpState == 3){
            if(TextDisplayManager.IsCanWrite()){
                FirstStartUpState = 4;
                TextDisplayManager.ClearTextCenter();
            }
        }

        if(isGripDowned){
            if(_other && _other.gameObject.TryGetComponent<PickUpableObject>(out _pickUpableObject)){
                _pickUpableObject.ParentObjectTransform = transform;
            }
            _isGripping = true;
        }
        else if(isGripReleased){
            if(_pickUpableObject is not null) _pickUpableObject.ParentObjectTransform = null;
            _pickUpableObject = null;
            _isGripping = false;
        }

        if(isTriggerPressed && _other is null && !_isGripping){
            _triggerPressDuration += Time.deltaTime;
        }
        else{
            _triggerPressDuration = 0f;
        }

        if(OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch) && _hand == Hand.Right){
            if(TextDisplayManager.IsCanWrite() && IsInverseChromaKey){
                bool toCond = !_qvPenObject.activeSelf;
                _qvPenObject.GetComponent<QvPen>().SetQvPenActive(toCond);
                if(toCond){
                    _qvPenObject.transform.SetPositionAndRotation(
                        Camera.main.transform.forward / 3 + new Vector3(0, 0.9f, 0),
                        Quaternion.identity
                    );
                }
                TextDisplayManager.WriteText($"VirtualPencilSystem {GetActiveString(toCond)}.", 0.05f, false);
            }
        }
        if(OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch) && _hand == Hand.Left){
            if(TextDisplayManager.IsCanWrite() && IsInverseChromaKey){
                var temp = _objectDetector.GetComponent<LuYiSeObjectDetector>();
                temp.IsActive = !temp.IsActive;
                if(!temp.IsActive) temp.ClearLabels();
                TextDisplayManager.WriteText($"ObjectDetectionSystem {GetActiveString(temp.IsActive)}.", 0.05f, false);
            }
        }

        // なにかを掴んでいる時
        if(_isGripping){
            if(isTriggerDowned){
                if(!_other.gameObject.TryGetComponent<QvPen>(out _qvPen)){
                    throw new NullReferenceException("QvPenねーーから！！");
                }

                _qvPen.StartDraw();
            }
            else if(isTriggerReleased){
                _qvPen.EndDraw();
            }
        }
        // なにも掴んでいない時
        else{
            _qvPen = null;

            // 何も掴んでいない && オブジェクトと重なっているとき && トリガーを引く && IntaractableObjectであるとき
            if(_other && isTriggerDowned && _other.gameObject.TryGetComponent<IntaractableObject>(out var intaractableObject)){
                intaractableObject.Intaract();
            }
        }

        if(FirstStartUpState >= 2){
            if(prevTriggerPressDuration < 1f && 1f <= _triggerPressDuration && _other is null && !_isGripping){
                // 緑一色モード，{発動/停止}ッ！！！
                var toCond = !plem.isEffectActive;
                plem.isEffectActive = toCond;
                IsInverseChromaKey = toCond;
                if(toCond == false){
                    _objectDetector.GetComponent<LuYiSeObjectDetector>().IsActive = false;
                    _qvPenObject.GetComponent<QvPen>().SetQvPenActive(false);
                    TextDisplayManager.ClearText();
                    // TextDisplayManager.WriteTextCenter("All System shut down . . .", 0);
                    FirstStartUpState = 2;
                }
                else{
                    TextDisplayManager.WriteTextCenter("Boot Process . . .", 0.05f);
                    FirstStartUpState = 3;
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(gameObject.activeSelf)
            _other = other;
    }
    void OnTriggerStay(Collider other)
    {
        if(gameObject.activeSelf && _other is null)
            _other = other;
    }
    void OnTriggerExit(Collider other)
    {
        if(gameObject.activeSelf)
            _other = null;
    }

    private string GetActiveString(bool cond){
        return cond ? "Activated" : "Deactivated";
    } 
}
