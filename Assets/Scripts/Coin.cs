using System;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public static event Action PickedUpEvent;

    [SerializeField] float rotateSpeed = 2f;
    [SerializeField] ParticleSystem particlesPrefab;
    bool pickedUp = false;
    Vector3 initialScale;
    Vector3 initialPosition;

    void Start()
    {
        initialScale = transform.localScale;
        initialPosition = transform.position;

        Car.BackEvent += Back;
    }

    void OnDestroy()
    {
        Car.BackEvent -= Back;
    }

    void Back()
    {
        LeanTween.cancel(gameObject);
        transform.localScale = initialScale;
        transform.position = initialPosition;
        pickedUp = false;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!pickedUp)
            transform.RotateAround(transform.position, Vector3.down, rotateSpeed * Time.deltaTime);
        else
        {
            transform.RotateAround(transform.position, Vector3.up, rotateSpeed * 8 * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Car"))
        {
            pickedUp = true;
            LeanTween.moveY(gameObject, transform.position.y + 1.5f, 0.25f).setLoopPingPong();
            LeanTween.scale(gameObject, Vector3.zero, 0.25f).setDelay(0.25f).setOnComplete(() =>
            {
                gameObject.SetActive(false);
            });
            Instantiate(particlesPrefab, transform.position, Quaternion.identity);
            PickedUpEvent?.Invoke();
        }
    }
}
