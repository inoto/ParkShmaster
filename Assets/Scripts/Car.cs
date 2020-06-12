using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Car : MonoBehaviour
{
    public enum State
    {
        OnStart,
        Moving,
        OnEnd
    }

    public static event Action BackEvent;
    public static event Action<bool> EndOfTrailEvent; 

    [SerializeField] float speed = 6.5f;
    [SerializeField] LeanTweenType moveEase;
    [SerializeField] Color color;
    [SerializeField] GameObject carStartPrefab;
    [SerializeField] GameObject trailPrefab;
    [SerializeField] Transform plane;
    [SerializeField] GameObject linkedParking;

    Collider collider;
    Vector3 initialPosition;
    Quaternion initialRotation;
    StartMarker startMarker = null;
    bool isOnStartPosition = true;
    public bool IsOnStartPosition => isOnStartPosition;
    Trail trail;
    public Trail Trail => trail;
    State currentState = State.OnStart;
    bool onParking = false;
    public bool IsOnParking => onParking;

    void Awake()
    {
        collider = GetComponent<Collider>();
    }

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        startMarker = Instantiate(carStartPrefab, transform.position, Quaternion.identity).GetComponent<StartMarker>();
        startMarker.transform.Rotate(new Vector3(90f, 0, 0));
        startMarker.Init(this, color);

        trail = Instantiate(trailPrefab, Vector3.zero, Quaternion.identity).GetComponent<Trail>();
        trail.Init(this, plane, linkedParking);
    }

    public void Interacted(Vector3 hitPoint)
    {
        switch (currentState)
        {
            case State.OnStart:
                Trail.Begin();
                break;
            case State.Moving:
                Back();
                break;
            case State.OnEnd:
                Trail.Continue();
                break;
        }
    }

    public void Back()
    {
        currentState = State.OnStart;

        LeanTween.cancel(gameObject);
        startMarker.Deactivate();
        isOnStartPosition = true;
        onParking = false;

        collider.enabled = false;
        float time = 0.3f;
        LeanTween.move(gameObject, initialPosition, time).setOnComplete(() =>
        {
            collider.enabled = true;
        });
        LeanTween.rotate(gameObject, initialRotation.eulerAngles, time);

        BackEvent?.Invoke();
    }

    public void MoveAlongPoints(List<Vector3> points, bool parkingFound)
    {
        currentState = State.Moving;
        startMarker.Activate();
        LeanTween.cancel(gameObject);
        transform.position = initialPosition;
        isOnStartPosition = false;

        Vector3[] array = new Vector3[points.Count + 2];
        array[0] = points[0].Y(transform.position.y);
        for (int i = 0; i < points.Count; i++)
        {
            array[i+1] = points[i].Y(transform.position.y);
        }
        array[array.Length-1] = points[points.Count-1].Y(transform.position.y);

        LeanTween.moveSpline(gameObject, array, points.Count / speed)
            .setEase(moveEase)
            .setOrientToPath(true)
            .setOnComplete(() =>
            {
                currentState = State.OnEnd;
                onParking = parkingFound;
                EndOfTrailEvent?.Invoke(parkingFound);
            });
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Car"))
        {
            LeanTween.cancel(gameObject);

            Vector3 contactPoint = other.contacts[0].point;
            Vector3 hitPoint = transform.InverseTransformPoint(contactPoint);
            float hitRadius = contactPoint.magnitude;
            Vector3 hitDir = transform.InverseTransformDirection(-contactPoint);

            GetComponent<Rigidbody>().AddExplosionForce(150f, contactPoint, 15f, 0.7f);
            isOnStartPosition = false;
        }
    }
}
