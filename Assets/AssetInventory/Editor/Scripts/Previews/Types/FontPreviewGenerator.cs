using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace AssetInventory
{
    public static class FontPreviewGenerator
    {
        private const string PREVIEW_TEXT = "ABCDEF\nGHIJKL\nMNOPQR\nSTUVWX\nYZ1230";
        private const string RENDER_LAYER = "AIRenderLayer";

        public static Texture2D Create(Font font, int textureSize = 128)
        {
            // Step 1: Set a higher rendering resolution
            int renderResolution = textureSize * 4; // Increase resolution (e.g., 512 for 128 texture size)

            // Step 2: Generate Mesh Data for the Text at higher resolution
            TextGenerator textGen = new TextGenerator();
            TextGenerationSettings textSettings = new TextGenerationSettings()
            {
                textAnchor = TextAnchor.MiddleCenter,
                generateOutOfBounds = true,
                generationExtents = new Vector2(renderResolution, renderResolution),
                pivot = new Vector2(0.5f, 0.5f),
                richText = false,
                font = font,
                fontSize = renderResolution, // Use higher font size
                fontStyle = FontStyle.Normal,
                verticalOverflow = VerticalWrapMode.Overflow,
                horizontalOverflow = HorizontalWrapMode.Overflow,
                color = Color.black,
                scaleFactor = 1.0f,
                lineSpacing = 1.0f,
            };

            textGen.Populate(PREVIEW_TEXT, textSettings);

            // Step 3: Create a Mesh from the Generated Data
            Mesh mesh = new Mesh();
            mesh.name = "TextMesh";
            IList<UIVertex> verts = textGen.verts;
            int vertCount = verts.Count;

            Vector3[] vertices = new Vector3[vertCount];
            Vector2[] uv = new Vector2[vertCount];
            int[] triangles = new int[(vertCount / 4) * 6];

            for (int i = 0; i < vertCount; i++)
            {
                vertices[i] = verts[i].position;
                uv[i] = verts[i].uv0;
            }

            for (int i = 0, t = 0; i < vertCount; i += 4, t += 6)
            {
                triangles[t + 0] = i + 0;
                triangles[t + 1] = i + 1;
                triangles[t + 2] = i + 2;
                triangles[t + 3] = i + 2;
                triangles[t + 4] = i + 3;
                triangles[t + 5] = i + 0;
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;

            // Step 4: Calculate the Mesh Bounds and Adjust Scaling
            Bounds textBounds = mesh.bounds;
            float maxDimension = Mathf.Max(textBounds.size.x, textBounds.size.y);
            float scaleFactor = (renderResolution / maxDimension) * 0.9f; // 0.9 to add some padding

            // Step 5: Create a GameObject with MeshFilter and MeshRenderer
            GameObject tempGO = new GameObject("TempTextMesh");
            MeshFilter meshFilter = tempGO.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = tempGO.AddComponent<MeshRenderer>();
            meshFilter.sharedMesh = mesh;

            // Create a Material with a high-quality shader
            Material material = new Material(Shader.Find("Unlit/Transparent"));
            material.mainTexture = font.material.mainTexture;
            meshRenderer.sharedMaterial = material;

            // Assign the layer to the GameObject
            int renderLayer = LayerMask.NameToLayer(RENDER_LAYER);
            if (renderLayer == -1)
            {
                // Create the layer if it doesn't exist
                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty layersProp = tagManager.FindProperty("layers");

                for (int i = 8; i < layersProp.arraySize; i++)
                {
                    SerializedProperty layerSP = layersProp.GetArrayElementAtIndex(i);
                    if (layerSP != null && string.IsNullOrEmpty(layerSP.stringValue))
                    {
                        layerSP.stringValue = RENDER_LAYER;
                        tagManager.ApplyModifiedProperties();
                        renderLayer = i;
                        break;
                    }
                }
            }
            tempGO.layer = renderLayer;

            // Step 6: Set Up a Temporary Camera and RenderTexture with Anti-Aliasing
            RenderTexture renderTexture = new RenderTexture(renderResolution, renderResolution, 24)
            {
                antiAliasing = 8 // Enable anti-aliasing
            };
            Camera camera = new GameObject("TempCamera").AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = renderResolution / 2;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.white;
            camera.targetTexture = renderTexture;
            camera.cullingMask = LayerMask.GetMask(RENDER_LAYER);

            // Step 7: Position the GameObject and Camera
            tempGO.transform.position = new Vector3(0, 0, 0);
            tempGO.transform.localScale = Vector3.one * scaleFactor;

            camera.transform.position = new Vector3(0, 0, -100);

            // Step 8: Render the Mesh into the RenderTexture
            camera.Render();

            // Step 9: Read the Pixels from the RenderTexture into a Texture2D
            Texture2D highResTexture = new Texture2D(renderResolution, renderResolution, TextureFormat.RGBA32, false);
            RenderTexture.active = renderTexture;
            highResTexture.ReadPixels(new Rect(0, 0, renderResolution, renderResolution), 0, 0);
            highResTexture.Apply();

            // Step 10: Downscale the Texture to the Desired Size
            Texture2D finalTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    Color color = highResTexture.GetPixelBilinear((float)x / textureSize, (float)y / textureSize);
                    finalTexture.SetPixel(x, y, color);
                }
            }
            finalTexture.Apply();

            // Step 11: Clean Up
            RenderTexture.active = null;
            camera.targetTexture = null;
            Object.DestroyImmediate(camera.gameObject);
            Object.DestroyImmediate(tempGO);
            renderTexture.Release();

            // Destroy the high-resolution texture to free memory
            Object.DestroyImmediate(highResTexture);

            return finalTexture;
        }

        public static Texture2D Create(string file, int textureSize = 128)
        {
            Font font = AssetDatabase.LoadAssetAtPath<Font>(file);
            if (font == null)
            {
                Debug.LogError($"Failed to load font from: {file}");
                return null;
            }

            return Create(font, textureSize);
        }
    }
}