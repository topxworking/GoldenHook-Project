using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhototypeGame : MonoBehaviour
{
    int money = 0;
    int rodLevel = 1;
    int rodCost = 100;

    public bool autoMode = false;
    float autoTimer = 0f;

    enum State { Idle, Waiting, Hooked }
    State state = State.Idle;
    float timer = 0f;
    float waitTime = 5f;

    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI statusText;
    public Button castBtn;
    public Button reelBtn;
    public Button upgradeBtn;
    public Button autoFishing;
    public TextMeshProUGUI upgradeCostText;

    void Start()
    {
        castBtn.onClick.AddListener(OnCast);
        reelBtn.onClick.AddListener(OnReel);
        upgradeBtn.onClick.AddListener(OnUpgrade);
        autoFishing.onClick.AddListener(OnAuto);
    }

    void Update()
    {
        if (state == State.Waiting)
        {
            timer -= Time.deltaTime;
            statusText.text = $"Waiting... {timer:F1}";
            if (timer <= 0f)
            {
                state = State.Hooked;
                statusText.text = "Fish On! REEL IT IN!";
                castBtn.gameObject.SetActive(false);
                reelBtn.gameObject.SetActive(true);
            }
        }

        if (autoMode && state == State.Idle)
        {
            autoTimer += Time.deltaTime;
            if (autoTimer >= waitTime * 1.5f)
            {
                autoTimer = 0f;
                OnCast();
            }
        }

        if (autoMode && state == State.Hooked) OnReel();

        RefreshUI();
    }

    void OnCast()
    {
        if (state != State.Idle) return;
        state = State.Waiting;
        timer = waitTime / rodLevel;
        statusText.text = "Casting...";
    }

    void OnReel()
    {
        if (state != State.Hooked) return;

        float roll = Random.value;
        int earned;
        string fishName;

        if (roll < 0.60f)       { fishName = "Minnow"; earned = Random.Range(5, 15); }
        else if (roll < 0.90f)  { fishName = "Tuna"; earned = Random.Range(30, 80); }
        else if (roll < 0.99f)  { fishName = "Shark"; earned = Random.Range(150, 400); }
        else                    { fishName = "Sea Dragon"; earned = Random.Range(1000, 3000); }

        money += earned;
        state = State.Idle;
        statusText.text = $"Got {fishName}! +${earned}";

        reelBtn.gameObject.SetActive(false);
        castBtn.gameObject.SetActive(true);
    }

    void OnUpgrade()
    {
        if (money < rodCost) return;
        money -= rodCost;
        rodLevel++;
        rodCost = rodCost * 3;
        statusText.text = $"Rod Upgraded to Level {rodLevel}!";
    }

    void RefreshUI()
    {
        moneyText.text = $"${money:N0}";
        upgradeCostText.text = $"Upgrade Rod Lv{rodLevel + 1}: ${rodCost}";
        upgradeBtn.interactable = money >= rodCost;
    }

    void OnAuto()
    {
        autoMode = !autoMode;
    }
}
