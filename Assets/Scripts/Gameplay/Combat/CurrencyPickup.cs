using UnityEngine;

/// <summary>
/// 敌人掉落的可收集货币（使用货币原型贴图）。
/// </summary>
public class CurrencyPickup : MonoBehaviour
{
    private const string PrefabResourcesPath = "Currency/货币原型";
    private const string PrefabEditorPath = "Assets/perfab/货币原型.prefab";
    private const string SpriteResourcesPath = "Currency/货币原型";
    private const string SpriteEditorPath = "Assets/art assets/货币原型.png";

    private const float PickupRadius = 0.75f;

    private bool collected;

    public static void Spawn(Vector3 position)
    {
        GameObject prefab = LoadPrefab();
        GameObject pickupObject;

        if (prefab != null)
        {
            pickupObject = Instantiate(prefab, position, Quaternion.identity);
            pickupObject.name = "CurrencyPickup";

            if (pickupObject.GetComponent<CurrencyPickup>() == null)
                pickupObject.AddComponent<CurrencyPickup>();
        }
        else
        {
            pickupObject = new GameObject("CurrencyPickup");
            pickupObject.transform.position = position;
            pickupObject.AddComponent<CurrencyPickup>();
        }

        pickupObject.GetComponent<CurrencyPickup>()?.EnsureSetup();
    }

    private void Awake()
    {
        EnsureSetup();
    }

    private void EnsureSetup()
    {
        if (GetComponent<SpriteRenderer>() == null)
        {
            SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = LoadSprite();
            spriteRenderer.sortingOrder = 20;
        }

        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider == null)
            collider = gameObject.AddComponent<CircleCollider2D>();

        collider.isTrigger = true;
        collider.radius = 0.4f;

        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (body == null)
            body = gameObject.AddComponent<Rigidbody2D>();

        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private void Update()
    {
        if (collected)
            return;

        transform.Rotate(0f, 0f, 120f * Time.deltaTime);
        TryCollectByDistance();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
            return;

        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerController>() != null)
            Collect();
    }

    private void TryCollectByDistance()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        float distance = Vector2.Distance(transform.position, player.transform.position);
        if (distance <= PickupRadius)
            Collect();
    }

    private void Collect()
    {
        if (collected)
            return;

        collected = true;
        RunProgression.AddCurrency(1);
        Destroy(gameObject);
    }

    private static GameObject LoadPrefab()
    {
        GameObject prefab = Resources.Load<GameObject>(PrefabResourcesPath);
#if UNITY_EDITOR
        if (prefab == null)
            prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(PrefabEditorPath);
#endif
        return prefab;
    }

    private static Sprite LoadSprite()
    {
        Sprite sprite = Resources.Load<Sprite>(SpriteResourcesPath);
#if UNITY_EDITOR
        if (sprite == null)
        {
            sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(SpriteEditorPath);
            if (sprite == null)
            {
                Texture2D texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(SpriteEditorPath);
                if (texture != null)
                {
                    sprite = Sprite.Create(
                        texture,
                        new Rect(0f, 0f, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f),
                        100f);
                }
            }
        }
#endif
        return sprite;
    }
}
