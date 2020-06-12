using UnityEngine;
using UnityEngine.UI;

public class UndoButton : MonoBehaviour
{
    [SerializeField] Image whiteImage;
    [SerializeField] LeanTweenType flickEase = LeanTweenType.easeInOutBack;

    Vector2 initialPosition;
    Vector3 initialScale;

    void Start()
    {
        initialPosition = transform.position;
        initialScale = transform.localScale;

        Car.EndOfTrailEvent += Flick;
        Car.BackEvent += Back;
    }

    void OnDestroy()
    {
        Car.EndOfTrailEvent -= Flick;
        Car.BackEvent -= Back;
    }

    void Back()
    {
        LeanTween.cancel(gameObject);
        transform.position = initialPosition;
        transform.localScale = initialScale;

        LeanTween.cancel(whiteImage.rectTransform);
        LeanTween.cancel(whiteImage.gameObject);
        Color c = whiteImage.color;
        c.a = 0f;
        whiteImage.color = c;
    }

    void Flick(bool parkingFound)
    {
        if (parkingFound)
            return;

        if (LeanTween.isTweening(whiteImage.gameObject) || LeanTween.isTweening(gameObject))
            Back();

        LeanTween.delayedCall(1f, () =>
        {
            LeanTween.moveX(gameObject, transform.position.x - 0.3f, 0.3f);
            LeanTween.scale(gameObject, Vector3.one * 2, 0.3f);
        });
        LeanTween.delayedCall(whiteImage.gameObject, 1f, () =>
        {
            LeanTween.alpha(whiteImage.rectTransform, 1f, 0.1f).setLoopPingPong(2).setEase(flickEase);
        }).setRepeat(-1);
    }
}
