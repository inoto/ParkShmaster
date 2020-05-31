using UnityEngine;

public class UndoButton : MonoBehaviour
{
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
    }

    void Flick(bool parkingFound)
    {
        if (parkingFound)
            return;

        LeanTween.delayedCall(1f, () =>
        {
            LeanTween.scale(gameObject, Vector3.one * 2, 1f).setLoopPingPong();
            LeanTween.moveX(gameObject, transform.position.x - 0.3f, 1f).setLoopPingPong();
        });
    }
}
