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
    Collider _other = null;
    private PickUpableObject _pickUpableObject = null;
    private bool _isGripping = false;
    private QvPen _qvPen = null;

    private void Update()
    {
        bool isGripDowned = _hand == Hand.Left ? OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger) : OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger);
        bool isGripReleased = _hand == Hand.Left ? OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger) : OVRInput.GetUp(OVRInput.Button.SecondaryHandTrigger);
        bool isTriggerDowned = _hand == Hand.Left ? OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) : OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger);
        bool isTriggerReleased = _hand == Hand.Left ? OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger) : OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger);

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

        if(OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch) && _hand == Hand.Right){
            bool toCond = !_qvPenObject.activeSelf;
            _qvPenObject.SetActive(toCond);
            if(toCond){
                _qvPenObject.transform.SetPositionAndRotation(
                    Camera.main.transform.forward / 3 + new Vector3(0, 0.9f, 0),
                    Quaternion.identity
                );
            }
        }
        if(OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch) && _hand == Hand.Left){
            _objectDetector.SetActive(!_objectDetector.activeSelf);
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
    }

    void OnTriggerEnter(Collider other)
    {
        _other = other;
    }
    void OnTriggerStay(Collider other)
    {
        if(_other is null) _other = other;
    }
    void OnTriggerExit(Collider other)
    {
        _other = null;
    }
}
