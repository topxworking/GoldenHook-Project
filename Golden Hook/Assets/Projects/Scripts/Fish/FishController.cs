using System;
using UnityEngine;

public class FishController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private float jumpHeight = 1.5f;
    //[SerializeField] private float moveSpeed = 3f;

    public event Action<FishController> OnReturnToPool;

    private float _timer;
    private Vector3 _targetPos;
    private Vector3 _startPos;
    private FishData _data;

    public void Initialize(FishData data)
    {
        _data = data;
        spriteRenderer.sprite = data.fishSprite;
        spriteRenderer.color = data.GetRarityColor();
        _timer = 0f;
        _startPos = transform.position;
        _targetPos = _startPos + new Vector3(UnityEngine.Random.Range(-2f, 2f), 0f, 0f);
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        float t = _timer / lifetime;
        float arc = Mathf.Sin(t * Mathf.PI) * jumpHeight;

        transform.position = Vector3.Lerp(_startPos, _targetPos, t)
            + Vector3.up * arc;

        spriteRenderer.flipX = (_targetPos.x - _startPos.x) < 0f;

        if (_timer >= lifetime)
        {
            OnReturnToPool?.Invoke(this);
        }
    }
}
