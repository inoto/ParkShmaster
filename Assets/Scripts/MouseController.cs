using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MouseController : MonoBehaviour
{
    const float RayLimit = 20f;

    Mouse ms;
    [SerializeField] Camera cam;

    Ray ray;
    RaycastHit hit;
    Car activeCar = null;
    Car[] cars;
    LinkedList<Car> actions = new LinkedList<Car>();
    Stack<Car> backedCars = new Stack<Car>();

    void Awake()
    {
        ms = Mouse.current;
    }

    void Start()
    {
        cars = FindObjectsOfType<Car>();
    }

    void Update()
    {
        Vector2 mousePos = ms.position.ReadValue();

        if (ms.leftButton.wasPressedThisFrame)
        {
            ray = cam.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out hit, RayLimit))
            {
                if (hit.collider.CompareTag("Car"))
                {
                    var car = hit.transform.GetComponent<Car>();
                    car.Interacted(hit.point);
                    BackOtherCars(car);
                    activeCar = car;
                    return;
                }
                if (hit.collider.CompareTag("StartMarker"))
                {
                    var startMarker = hit.transform.GetComponent<StartMarker>();
                    startMarker.Car.Back();
                    return;
                }
            }
        }
        else if (ms.leftButton.wasReleasedThisFrame && activeCar != null)
        {
            ray = cam.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out hit, RayLimit))
            {
                activeCar.Trail.End(hit.point);
            }
            else
            {
                activeCar.Trail.End();
            }
            ContinueOtherCars();
            activeCar = null;
        }

        if (ms.leftButton.isPressed && activeCar != null)
        {
            ray = cam.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out hit, RayLimit))
            {
                if (hit.collider.CompareTag("Parking"))
                {
                    activeCar.Trail.ParkingReached(hit.point, hit.collider.gameObject);
                    return;
                }

                if (hit.collider.CompareTag("Plane"))
                {
                    activeCar.Trail.Modify(hit.point);
                    return;
                }
            }
        }
    }

    void BackOtherCars(Car car)
    {
        foreach (var otherCar in cars)
        {
            if (car == otherCar)
                continue;
            if (!otherCar.IsOnStartPosition)
            {
                otherCar.Back();
                backedCars.Push(otherCar);
            }
        }
    }

    void ContinueOtherCars()
    {
        while (backedCars.Count > 0)
        {
            var otherCar = backedCars.Pop();
            if (otherCar.IsOnStartPosition && otherCar.Trail.IsSplineEnded)
            {
                otherCar.Trail.Move();
            }
        }
    }

    public void UndoButtonPressed()
    {
        foreach (var car in cars)
        {
            if (!car.IsOnStartPosition)
            {
                car.Back();
            }
            else if (car.Trail.IsSplineEnded)
            {
                car.Trail.Back();
                break;
            }
        }
        activeCar = null;
    }
}
