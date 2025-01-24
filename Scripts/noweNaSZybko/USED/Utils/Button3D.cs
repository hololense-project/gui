using UnityEngine;
using UnityEngine.Events;

public class Button3D : MonoBehaviour
{
    [Header("Button Settings")]
    public UnityEvent OnClick;

    private void Reset()
    {
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }
    }

    private void OnMouseDown()
    {
        OnClick?.Invoke();
    }
}