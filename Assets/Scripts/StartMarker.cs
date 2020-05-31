using UnityEngine;
using UnityEngine.InputSystem;

public class StartMarker : MonoBehaviour
{
    [SerializeField] GameObject sphere;

    Collider collider;

    void Awake()
    {
        collider = GetComponent<Collider>();
    }

    public void Init(Car car)
    {
        collider.enabled = false;
    }

    public void Activate()
    {
        collider.enabled = true;
        sphere.SetActive(true);
        float time = 2f;
        LeanTween.scale(sphere, Vector3.one * 1.5f, time).setRepeat(-1);
        LeanTween.alpha(sphere, 0f, time).setRepeat(-1);
    }

    public void Deactivate()
    {
        LeanTween.cancel(sphere);
        collider.enabled = false;
        sphere.SetActive(false);
    }
}
