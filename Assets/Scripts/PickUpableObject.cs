using UnityEngine;

public class PickUpableObject : MonoBehaviour
{
    public Transform ParentObjectTransform {get; set;} = null;

    private bool _isFirstFrame = true;
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private Vector3 _startParentPosition;
    private Quaternion _startParentRotation;
    
    void Update()
    {
        if(ParentObjectTransform is not null){
            if(_isFirstFrame){
                _startPosition = transform.position;
                _startRotation = transform.rotation;
                _startParentPosition = ParentObjectTransform.transform.position;
                _startParentRotation = ParentObjectTransform.transform.rotation;
                _isFirstFrame = false;
            }

            var diffParentPosition = ParentObjectTransform.position - _startParentPosition;
            var diffParentRotation = ParentObjectTransform.rotation * Quaternion.Inverse(_startParentRotation);

            transform.position = diffParentRotation * (_startPosition - _startParentPosition) + _startParentPosition + diffParentPosition;
            transform.rotation = diffParentRotation * _startRotation;
        }
        else{
            if(!_isFirstFrame){                
                _startPosition = transform.position;
                _startRotation = transform.rotation;

                _isFirstFrame = true;
            }
        }
    }
}
