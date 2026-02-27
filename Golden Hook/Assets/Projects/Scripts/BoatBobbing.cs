using UnityEngine;

public class BoatBobbing : MonoBehaviour
{
    [Header("Bobbing")]
    [SerializeField] private float bobHeight = 0.05f;
    [SerializeField] private float bobSpeed = 1f;

    [Header("Rocking")]
    [SerializeField] private float rockAngle = 1.5f;
    [SerializeField] private float rockSpeed = 0.6f;

    [Header("Drift")]
    [SerializeField] private float driftAmount = 0.03f;
    [SerializeField] private float driftSpeed = 0.4f;

    private Vector3 _startPos;
    private Quaternion _startRot;

    private void Start()
    {
        _startPos = transform.localPosition;
        _startRot = transform.localRotation;
    }

    private void Update()
    {
        float t = Time.time;

        float bobY = Mathf.Sin(t * bobSpeed * Mathf.PI) * bobHeight;
        float driftX = Mathf.Sin(t * driftSpeed * Mathf.PI) * driftAmount;

        transform.localPosition = _startPos + new Vector3(driftX, bobY, 0f);

        float rock = Mathf.Sin(t * rockSpeed * Mathf.PI + 1f) * rockAngle;
        transform.localRotation = _startRot * Quaternion.Euler(0f, 0f, rock);
    }
}
