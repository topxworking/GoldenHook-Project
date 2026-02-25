using UnityEngine;

public class BoatViewController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer boatSpriteRenderer;

    private void OnEnable()
    {
        EventManager.Subscribe<UpgradeEvent>(OnUpgradeEvent);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<UpgradeEvent>(OnUpgradeEvent);
    }

    private void Start()
    {
        RefreshSprite();
    }

    private void OnUpgradeEvent(UpgradeEvent e)
    {
        if (e.UpgradeType == "Boat")
            RefreshSprite();
    }

    private void RefreshSprite()
    {
        var boat = UpgradeManager.Instance?.CurrentBoat;
        if (boat == null || boatSpriteRenderer == null) return;

        if (boat.boatSprite != null)
            boatSpriteRenderer.sprite = boat.boatSprite;

        Debug.Log($"[Boat] Sprite changed to: {boat.boatName}");
    }
}
