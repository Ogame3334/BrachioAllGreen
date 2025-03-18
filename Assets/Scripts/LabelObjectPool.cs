using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;

public class LabelObjectPool : MonoBehaviour
{
    public static LabelObjectPool Instance {get; private set;}

    [SerializeField]
    private GameObject _labelPrefab;

    [SerializeField]
    private int poolSize = 4;
    private static List<GameObject> _pool;

    void Awake()
    {
        if(Instance == null){
            if(!_labelPrefab.TryGetComponent<TextMeshPro>(out var _)) throw new NullReferenceException();

            Instance = this;
        }
        else{
            Destroy(this);
            return;
        }

        _pool = new List<GameObject>(poolSize);
        for(int i = 0; i < poolSize; ++i){
            var labelObj = Instantiate(_labelPrefab, this.transform);
            labelObj.SetActive(false);
            _pool.Add(labelObj);
        }
    }

    public static GameObject GetLabelObject(){
        if(Instance is null) throw new NullReferenceException();
        
        var labels = _pool.Where(p => !p.activeSelf);
        if(labels.ToArray().Length > 0){
            var labelObj = labels.First();
            labelObj.SetActive(true);

            return labelObj;
        }
        else{
            GameObject labelObj = Instantiate(Instance._labelPrefab);
            _pool.Add(labelObj);
            
            return labelObj;
        }
    }

    public static int GetPoolLength(){
        if(Instance == null) return -1;
        return _pool.ToArray().Length;
    }
    public static int GetPoolActiveLength(){
        if(Instance == null) return -1;
        return _pool.Where(p => p.activeSelf).ToArray().Length;
    }
    public static string[] GetLabelNames(){
        if(Instance == null) return new string[0];
        return _pool.Where(p => p.activeSelf).Select(p => p.GetComponent<TextMeshPro>().text).ToArray();
    }
}
