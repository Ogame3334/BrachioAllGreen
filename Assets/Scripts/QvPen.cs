using System.Collections.Generic;
using UnityEngine;

public class QvPen : MonoBehaviour
{
    [SerializeField]
    private Transform _penNibTransform;
    [SerializeField]
    private GameObject _trailPrefabs;

    private List<GameObject> _trailObjects = new List<GameObject>();
    private GameObject _trailObject = null;

    public void StartDraw(){
        if(_trailObject is not null) return;
        _trailObject = Instantiate(_trailPrefabs, _penNibTransform);
        _trailObjects.Add(_trailObject);
    }

    public void EndDraw(){
        _trailObject.transform.parent = null;
        _trailObject = null;
    }

    public void ClearAll(){
        foreach(var obj in _trailObjects){
            Destroy(obj);
        }
        _trailObjects.Clear();
    }

    public void Undo(){
        Destroy(_trailObjects[_trailObjects.Count - 1]);
        _trailObjects.RemoveAt(_trailObjects.Count - 1);
    }
}
