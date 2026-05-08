using UnityEngine;
using UnityEngine.UI;

public static class UIHelpers
{
    public enum Corner { TopLeft, TopRight, TopCenter, Center }

    public static GameObject CreateUIObject(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    public static void SetAnchorCorner(RectTransform rt, Corner corner, Vector2 offset)
    {
        switch (corner)
        {
            case Corner.TopLeft:
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
                break;
            case Corner.TopRight:
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1, 1);
                break;
            case Corner.TopCenter:
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1);
                rt.pivot = new Vector2(0.5f, 1);
                break;
        }
        rt.anchoredPosition = offset;
    }

    public static void SetAnchorCenter(RectTransform rt, Vector2 offset)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = offset;
    }

    public static GameObject CreateCircleSprite(Transform parent, string name, float radius, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localScale = Vector3.one * radius * 2f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleTexture();
        sr.color  = color;
        sr.sortingOrder = 5;
        return go;
    }

    public static Sprite CreateCircleTexture()
    {
        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float r = size / 2f - 2f;
        float thickness = 6f;

        for (int x = 0; x < size; x++)
        for (int y = 0; y < size; y++)
        {
            float dist = Vector2.Distance(new Vector2(x, y), center);
            float alpha = 0f;

            // Ring: między r-thickness a r
            if (dist >= r - thickness && dist <= r)
                alpha = 1f;
            // Wewnętrzna kropka (dla InnerRing - mały kółek bez dziury)
            else if (dist < r - thickness - 2f)
                alpha = 0f;

            tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}