using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class LetraScrolling : MonoBehaviour
{
    [SerializeField] private RawImage img;
    [SerializeField] private float x, y;

    void Awake()
    {
        if (img == null) img = GetComponent<RawImage>();
        ConfigureTextureWrap();
    }

    void Update()
    {
        if (img == null) return;

        Rect uv = img.uvRect;
        Vector2 offset = uv.position + new Vector2(x, y) * Time.unscaledDeltaTime;
        offset.x = Mathf.Repeat(offset.x, 1f);
        offset.y = Mathf.Repeat(offset.y, 1f);
        img.uvRect = new Rect(offset, uv.size);
    }

    void Reset()
    {
        img = GetComponent<RawImage>();
    }

    void ConfigureTextureWrap()
    {
        if (img == null || img.texture == null) return;
        img.texture.wrapModeU = TextureWrapMode.Repeat;
        img.texture.wrapModeV = TextureWrapMode.Clamp;
    }
}
