using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* BASED ON : https://weesals.wordpress.com/2014/03/15/unity-rts-part-2-line-of-sight/ */

[ExecuteInEditMode]
public class LOSManager : MonoBehaviour
{

    public bool PreviewInEditor = false;

    // The parameters used to determine texture size and quality
    [Serializable]
    public class SizeParameters
    {
        public Terrain Terrain;
        public int Width = -1, Height = -1;
        public float Quality = 1;
        public bool HighDetailTexture = false;
    }

    public SizeParameters Size;
    public Terrain Terrain { get { return Size.Terrain; } }
    public int Width { get { return Size.Width; } }
    public int Height { get { return Size.Height; } }
    public float Scale { get { return Size.Quality; } }
    public bool HighDetailTexture { get { return Size.HighDetailTexture; } }

    public int ExtraWidth = 300;
    public int ExtraHeigth = 100;

    // The parameters used for visual features
    [Serializable]
    public class VisualParameters
    {
        [Range(0, 1024)]
        public int InterpolationRate = 512;
        public float GrayscaleDecayDuration = 300;
        public bool RevealOnEntityDiscover = true;
    }

    public VisualParameters Visual;
    public int InterpolationRate { get { return Visual.InterpolationRate; } }
    public float GrayscaleDecayDuration { get { return Visual.GrayscaleDecayDuration; } }
    public bool RevealOnEntityDiscover { get { return Visual.RevealOnEntityDiscover; } }

    // List of entities that interact with LOS
    [HideInInspector]
    public List<LOSEntity> Entities = new List<LOSEntity>();
    // List of entities currently animating their LOS
    [HideInInspector]
    public List<LOSEntity> AnimatingEntities = new List<LOSEntity>();

    // Some internal data
    int frameId = 0;
    float timer = 0;
    Color32[] pixels;
    Color32[] lerpPixels;
    Texture2D losTexture;
    int[] jCache = new int[1024];

    // Used to determine when the user changes a field that requires
    // the texture to be recreated
    private int previewParameterHash = 0;
    private int GenerateParameterHash() { return (Width + Height * 1024) + Scale.GetHashCode() + HighDetailTexture.GetHashCode(); }

    void Start()
    {
        if (Application.isPlaying) InitializeTexture();
    }

    // Get a size from the provided properties
    private int SizeFromParams(int desired, float terrainSize, float scale)
    {
        int size = 128;
        if (desired > 0) size = Mathf.CeilToInt(desired * scale);
        else if (terrainSize > 0) size = Mathf.CeilToInt(terrainSize * scale);
        return Mathf.Clamp(size, 4, 512);
    }
    // Create a texture matching the required properties
    void InitializeTexture()
    {
        int width = SizeFromParams(Width, Terrain != null ? Terrain.terrainData.size.x : 0, Scale) + ExtraWidth;
        int height = SizeFromParams(Height, Terrain != null ? Terrain.terrainData.size.z : 0, Scale) + ExtraHeigth;

        if (losTexture != null) DestroyImmediate(losTexture);

        TextureFormat texFormat = HighDetailTexture ? TextureFormat.RGB24 : TextureFormat.RGB565;
        losTexture = new Texture2D(width, height, texFormat, false);
        pixels = losTexture.GetPixels32();

        for (int p = 0; p < pixels.Length; ++p)
        {
            pixels[p] = Color.black;
        }
        losTexture.SetPixels32(pixels);
        lerpPixels = null;
        if (Terrain != null)
        {
            Shader.SetGlobalTexture("_FOWTex", losTexture);
            Shader.SetGlobalVector("_FOWTex_ST",
                new Vector4(
                    Scale / width, Scale / height,
                    (0.5f - Scale * 0.5f) / width, (0.5f - Scale * 0.5f) / height
                )
            );
        }
        Debug.Log("FOW Texture created, " + width + " x" + height);
    }

    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (PreviewInEditor)
            {
                // Make sure we have a valid texture
                if (losTexture == null || previewParameterHash != GenerateParameterHash())
                {
                    InitializeTexture();
                    previewParameterHash = GenerateParameterHash();
                }
            }
            else
            {
                // Or just use a white texture as placeholder
                Shader.SetGlobalTexture("_FOWTex", UnityEditor.EditorGUIUtility.whiteTexture);
                if (losTexture != null) DestroyImmediate(losTexture);
                losTexture = null;
            }
        }
#endif
        if (losTexture != null)
        {
            // Update any animating entities (update their FOW color)
            for (int e = 0; e < AnimatingEntities.Count; ++e)
            {
                if (AnimatingEntities[e].UpdateFOWColor())
                    AnimatingEntities.RemoveAt(e--);
            }
            // If in editor mode
            if (!Application.isPlaying)
            {
                // Refresh the map each frame
                for (int p = 0; p < pixels.Length; ++p)
                {
                    pixels[p] = new Color32(0, 255, 0, 255);
                }

                // Add LOS and AO for all entities
                foreach (var entity in Entities)
                {
                    RevealLOS(entity, entity.IsRevealer ? 255 : 0, 255, 255);
                }
            }
            else
            {
                bool forceFullUpdate = Time.frameCount == 1;
                // Reset all entities to be invisible
                if (forceFullUpdate)
                {
                    int revealerCount = 0;
                    foreach (var entity in Entities)
                    {
                        entity.RevealState = RevealStates.Hidden;
                        if (entity.IsRevealer) revealerCount++;
                    }
                    if (revealerCount == 0)
                    {
                        Debug.LogError("No LOSEntity items were marked as revealers! Tick the 'Is Revealed' checkbox for at least 1 item.");
                    }
                }

                // Decay grayscale
                if (GrayscaleDecayDuration > 0)
                {
                    const int GrayscaleGranularity = 4;
                    int oldGrayDecay = (int)(256 / GrayscaleGranularity * timer / GrayscaleDecayDuration) * GrayscaleGranularity;
                    timer += Time.deltaTime;
                    int newGrayDecay = (int)(256 / GrayscaleGranularity * timer / GrayscaleDecayDuration) * GrayscaleGranularity;
                    int grayDecayCount = newGrayDecay - oldGrayDecay;
                    if (grayDecayCount != 0)
                    {
                        for (int p = 0; p < pixels.Length; ++p)
                        {
                            pixels[p].r = (byte)Mathf.Max(pixels[p].r - grayDecayCount, 0);
                            pixels[p].g = (byte)Mathf.Max(pixels[p].g - grayDecayCount, 0);
                            pixels[p].b = (byte)Mathf.Max(pixels[p].b - grayDecayCount, 0);
                        }
                    }
                }

                ++frameId;
                // Reset AO and LOS
                bool updateAo = (frameId % 2) == 0;
                if (updateAo || forceFullUpdate)
                {
                    for (int p = 0; p < pixels.Length; ++p)
                    {
                        pixels[p].r = 0;
                        pixels[p].a = 255;
                    }
                }

                // Reveal LOS from all entities
                foreach (var entity in Entities)
                {
                    if (entity.IsRevealer) RevealLOS(entity, 255, 255, 330);
                }

                int count = 0;
                foreach (var entity in Entities)
                {
                    ++count;

                    var rect = entity.Bounds;
                    var fowColor = GetFOWColor(rect);
                    var visible = GetRevealFromFOW(fowColor);
                    if (entity.RevealState != visible && !(entity.RevealState == RevealStates.Hidden && visible == RevealStates.Fogged))
                    {
                        entity.RevealState = visible;
                        if (visible == RevealStates.Unfogged && RevealOnEntityDiscover)
                        {
                            RevealLOS(rect, 0, entity.Height + entity.transform.position.y, 0, 255, 255);
                        }
                    }
                    if (visible != RevealStates.Hidden || forceFullUpdate)
                    {
                        entity.SetFOWColor(GetQuantizedFOW(fowColor), !forceFullUpdate);
                        // Queue the item for FOW animation
                        if (entity.RequiresFOWUpdate && !AnimatingEntities.Contains(entity))
                            AnimatingEntities.Add(entity);
                    }
                }
            }
            bool isChanged = true;
            if (InterpolationRate > 0 && Application.isPlaying)
            {
                if (lerpPixels == null) lerpPixels = pixels.ToArray();
                else
                {
                    int rate = Mathf.Max(Mathf.RoundToInt(InterpolationRate * Time.deltaTime), 1);
                    for (int p = 0; p < lerpPixels.Length; ++p)
                    {
                        byte r = EaseToward(lerpPixels[p].r, pixels[p].r, rate),
                            g = EaseToward(lerpPixels[p].g, pixels[p].g, rate),
                            b = EaseToward(lerpPixels[p].b, pixels[p].b, rate),
                            a = EaseToward(lerpPixels[p].a, pixels[p].a, rate);
                        if (isChanged || lerpPixels[p].a != a || lerpPixels[p].r != r || lerpPixels[p].g != g || lerpPixels[p].b != b)
                        {
                            isChanged = true;
                            lerpPixels[p] = new Color32(r, g, b, a);
                        }
                    }
                }
            }
            else lerpPixels = null;

            if (isChanged)
            {
                losTexture.SetPixels32(lerpPixels ?? pixels);
                losTexture.Apply();
            }
        }
    }
    private byte EaseToward(byte from, byte to, int amount)
    {
        if (Mathf.Abs(from - to) < amount) return to;
        return (byte)(from + (to > from ? amount : -amount));
    }

    // Get the extents of a point/rectangle
    private void GetExtents(Vector2 pos, int inflateRange, out int xMin, out int yMin, out int xMax, out int yMax)
    {
        xMin = Mathf.RoundToInt(pos.x - inflateRange);
        xMax = Mathf.RoundToInt(pos.x + inflateRange);
        yMin = Mathf.RoundToInt(pos.y - inflateRange);
        yMax = Mathf.RoundToInt(pos.y + inflateRange);
        if (xMin < 0) xMin = 0; else if (xMax >= losTexture.width) xMax = losTexture.width - 1;
        if (yMin < 0) yMin = 0; else if (yMax >= losTexture.height) yMax = losTexture.height - 1;
    }
    private void GetExtents(Rect rect, int inflateRange, out int xMin, out int yMin, out int xMax, out int yMax)
    {
        xMin = Mathf.RoundToInt(rect.xMin * Scale) - inflateRange;
        xMax = Mathf.RoundToInt(rect.xMax * Scale) + inflateRange;
        yMin = Mathf.RoundToInt(rect.yMin * Scale - 1) - inflateRange;
        yMax = Mathf.RoundToInt(rect.yMax * Scale - 1) + inflateRange;
        if (xMin < 0) xMin = 0; else if (xMax >= losTexture.width) xMax = losTexture.width - 1;
        if (yMin < 0) yMin = 0; else if (yMax >= losTexture.height) yMax = losTexture.height - 1;
        if (xMax < xMin) xMax = xMin;
        if (yMax < yMin) yMax = yMin;
    }

    // Reveal an area
    private void RevealLOS(LOSEntity sight, float los, float fow, float grayscale)
    {
        Rect rect = sight.Bounds;
        RevealLOS(rect, sight.Range, sight.Height + sight.transform.position.y, los, fow, grayscale);
    }

    private void RevealLOS(Rect rect, float range, float height, float los, float fow, float grayscale)
    {
        int xMin, yMin, xMax, yMax;
        int rangeI = Mathf.RoundToInt(range * Scale);
        int xiMin, yiMin, xiMax, yiMax;

        GetExtents(rect, 0, out xiMin, out yiMin, out xiMax, out yiMax);
        GetExtents(rect, rangeI, out xMin, out yMin, out xMax, out yMax);

        for (int y = yMin; y <= yMax; ++y)
        {
            float yIntl = Mathf.Clamp(y, rect.yMin, rect.yMax - 1);
            for (int x = xMin; x <= xMax; ++x)
            {
                var nodePos = new Vector2(x, y) / Scale;
                var intlPos = new Vector2(Mathf.Clamp(nodePos.x, rect.xMin, rect.xMax - 1), yIntl);
                float dist2 = (intlPos - nodePos).sqrMagnitude;
                float range2 = (range * range);
                if (dist2 > range2) continue;
                const float FadeStart = 2;
                float innerRange = Mathf.Max(range - FadeStart, 0);
                float innerRange2 = innerRange * innerRange;
                float bright = 1;
                if (dist2 > innerRange2)
                {
                    bright = Mathf.Clamp01((range - Mathf.Sqrt(dist2)) / (range - innerRange));
                }
                int p = x + y * losTexture.width;
                pixels[p].r = (byte)Mathf.Max(pixels[p].r, bright * los);
                pixels[p].g = (byte)Mathf.Max(pixels[p].g, bright * fow);
                pixels[p].b = (byte)Mathf.Max(pixels[p].b, Mathf.Clamp(bright * grayscale, 0, 255));
            }
        }

    }

    // Get states and colours for entities
    public Color32 GetFOWColor(Vector2 pos)
    {
        int x = Mathf.RoundToInt(pos.x * Scale),
            y = Mathf.RoundToInt(pos.x * Scale);
        int p = x + y * losTexture.width;
        return pixels[p];
    }

    public Color32 GetFOWColor(Rect rect)
    {
        int xMin, yMin, xMax, yMax;
        GetExtents(rect, 0, out xMin, out yMin, out xMax, out yMax);

        Color32 color = new Color32(0, 0, 0, 0);
        for (int y = yMin; y <= yMax; ++y)
        {
            for (int x = xMin; x <= xMax; ++x)
            {
                int p = x + y * losTexture.width;
                color.r = (byte)Mathf.Max(color.r, pixels[p].r);
                color.g = (byte)Mathf.Max(color.g, pixels[p].g);
                color.b = (byte)Mathf.Max(color.b, pixels[p].b);
                color.a = (byte)Mathf.Max(color.a, pixels[p].a);
            }
        }
        return color;
    }
    public RevealStates GetRevealFromFOW(Color32 px)
    {
        if (px.r >= 128) return RevealStates.Unfogged;
        if (px.g >= 128) return RevealStates.Fogged;
        return RevealStates.Hidden;
    }
    public RevealStates IsVisible(Vector2 pos)
    {
        return GetRevealFromFOW(GetFOWColor(pos));
    }
    public RevealStates IsVisible(Rect rect)
    {
        return GetRevealFromFOW(GetFOWColor(rect));
    }
    public Color32 GetQuantizedFOW(Color32 px)
    {
        if (px.r >= 128)
        {
            px.r = px.g = px.b = 255;
        }
        else
        {
            px.r = 0;
            px.g = px.g < 128 ? (byte)1 : (byte)255;
        }
        return px;
    }


    // Allow entities to tell us when theyre added
    public static void AddEntity(LOSEntity entity)
    {
        if (Instance != null && !Instance.Entities.Contains(entity)) Instance.Entities.Add(entity);
    }
    public static void RemoveEntity(LOSEntity entity)
    {
        if (Instance != null) Instance.Entities.Remove(entity);
    }


    // A singleton instance of this class
    private static LOSManager _instance;
    public static LOSManager Instance
    {
        get
        {
            if (_instance == null) _instance = GameObject.FindObjectOfType<LOSManager>();
            return _instance;
        }
    }

}
