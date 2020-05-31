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

    void Awake()
    {
        ms = Mouse.current;
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
                    if (activeCar != null
                        && activeCar.gameObject.GetInstanceID() == hit.transform.gameObject.GetInstanceID())
                    {
                        activeCar.Back();
                        activeCar = null;
                        return;
                    }

                    activeCar = hit.transform.GetComponent<Car>();
                    activeCar.Trail.Begin();
                    return;
                }
                if (hit.collider.CompareTag("StartMarker"))
                {
                    activeCar.Back();
                    activeCar = null;
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
        }

        if (ms.leftButton.isPressed && activeCar != null)
        {
            ray = cam.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out hit, RayLimit))
            {
                if (hit.collider.CompareTag("Parking"))
                {
                    activeCar.Trail.ParkingReached(hit.point, hit.transform.position);
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

    public void UndoButtonPressed()
    {
        if (activeCar != null)
        {
            activeCar.Back();
            activeCar = null;
        }
    }
}
