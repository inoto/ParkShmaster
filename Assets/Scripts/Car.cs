using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Configuration;
using UnityEngine;
using UnityEngine.InputSystem;

public class Car : MonoBehaviour
{
    public static event Action BackEvent;
    public static event Action<bool> EndOfTrailEvent; 

    [SerializeField] float speed = 6.5f;
    [SerializeField] LeanTweenType moveEase;
    [SerializeField] GameObject carStartPrefab;
    [SerializeField] GameObject trailPrefab;
    [SerializeField] Transform plane;

    Collider collider;
    Vector3 initialPosition;
    Quaternion initialRotation;
    StartMarker startMarker = null;
    Trail trail;
    public Trail Trail => trail;

    void Awake()
    {
        collider = GetComponent<Collider>();
    }

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        startMarker = Instantiate(carStartPrefab, transform.position, Quaternion.identity).GetComponent<StartMarker>();
        startMarker.Init(this);

        trail = Instantiate(trailPrefab, Vector3.zero, Quaternion.identity).GetComponent<Trail>();
        trail.Init(this, plane);
    }

    public void Back()
    {
        LeanTween.cancel(gameObject);
        startMarker.Deactivate();
        Trail.Back();

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
        startMarker.Activate();
        LeanTween.cancel(gameObject);
        transform.position = initialPosition;

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
                EndOfTrailEvent?.Invoke(parkingFound);
            });
    }
}
