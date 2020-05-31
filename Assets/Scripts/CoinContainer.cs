using TMPro;
using UnityEngine;

public class CoinContainer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI cointCountText;

    int coinCount = 0;

    void Start()
    {
        Coin.PickedUpEvent += UpdateCountText;
        Car.BackEvent += BackCount;
    }

    void OnDestroy()
    {
        Coin.PickedUpEvent -= UpdateCountText;
        Car.BackEvent -= BackCount;
    }

    void UpdateCountText()
    {
        coinCount += 1;
        cointCountText.text = coinCount.ToString();
    }

    void BackCount()
    {
        coinCount = 0;
        cointCountText.text = coinCount.ToString();
    }
}
