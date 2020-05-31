using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreen : MonoBehaviour
{
    [SerializeField] GameObject objectToShow;
    [SerializeField] GameObject particles;

    void Start()
    {
        Car.EndOfTrailEvent += Win;
    }

    void OnDestroy()
    {
        Car.EndOfTrailEvent -= Win;
    }

    void Win(bool parkingFound)
    {
        if (!parkingFound)
            return;

        objectToShow.gameObject.SetActive(true);
        particles.gameObject.SetActive(true);
        LeanTween.delayedCall(3f, () => { SceneManager.LoadScene(0); });
    }
}
