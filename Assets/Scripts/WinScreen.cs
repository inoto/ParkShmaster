using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreen : MonoBehaviour
{
    [SerializeField] GameObject objectToShow;
    [SerializeField] GameObject particles;
    List<Car> cars = new List<Car>(2);

    void Start()
    {
        cars = FindObjectsOfType<Car>().ToList();
        Car.EndOfTrailEvent += OnCarEndOfTrail;
    }

    void OnDestroy()
    {
        Car.EndOfTrailEvent -= OnCarEndOfTrail;
    }

    void OnCarEndOfTrail(bool parkingFound)
    {
        if (!parkingFound)
            return;

        foreach (var car in cars)
        {
            if (!car.IsOnParking)
                return;
        }

        objectToShow.gameObject.SetActive(true);
        particles.gameObject.SetActive(true);
        LeanTween.delayedCall(3f, () => { SceneManager.LoadScene(0); });
    }
}
