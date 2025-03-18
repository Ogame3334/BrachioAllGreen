using UnityEngine;
using UnityEngine.Events;

public class IntaractableObject : MonoBehaviour
{
    [SerializeField]
    private UnityEvent OnIntaract = new UnityEvent();

    public void Intaract(){
        OnIntaract.Invoke();
    }
}
