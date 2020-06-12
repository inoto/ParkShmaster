using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StartMarker : MonoBehaviour
{
    [SerializeField] GameObject sphere;
    [SerializeField] Image image;
    [SerializeField] LeanTweenType scaleEase;

    Collider collider;
    Car car;
    public Car Car => car;

    void Awake()
    {
        collider = GetComponent<Collider>();
    }

    public void Init(Car car, Color color)
    {
        this.car = car;
        image.color = color;
        collider.enabled = false;
    }

    public void Activate()
    {
        if (LeanTween.isTweening(image.gameObject))
            Deactivate();

        collider.enabled = true;
        float time = 0.5f;
        image.transform.localScale = Vector3.zero;
        LeanTween.alpha(image.rectTransform, 0f, time);
        LeanTween.delayedCall(gameObject, 0.75f, () =>
        {
            image.gameObject.SetActive(true);
            LeanTween.scale(image.gameObject, Vector3.one * 3f, time).setEase(scaleEase).setRepeat(-1);
            LeanTween.alpha(image.rectTransform, 1f, time/2).setLoopPingPong().setRepeat(-1);
        });
    }

    public void Deactivate()
    {
        LeanTween.cancel(gameObject);
        LeanTween.cancel(image.gameObject);
        LeanTween.cancel(image.rectTransform);
        collider.enabled = false;
        image.gameObject.SetActive(false);
    }
}
