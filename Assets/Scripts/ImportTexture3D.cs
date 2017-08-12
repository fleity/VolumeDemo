using System;
using UnityEngine;

public class ImportTexture3D : MonoBehaviour
{
    public Texture3DAsset texture3DAsset;
    [SerializeField] public Texture3D texture3D;
    
    public Material material;
    [SerializeField] public string materialTexture = "_noise3dMain";
    public int resolution = 128;
    [Space]
    [HideInInspector]public Vector3 tex3dTestPositon = Vector3.zero;
    
    /*
    public int heightSlice = 0;
    private Texture2D test2dTextureSlice;

    public enum textureChannel
    {
        r,
        g,
        b,
        a
    };
    public textureChannel currentTexChannel;
    */

    void OnEnable()
    {
        ImportTexture3dToMaterial();
    }

    [ContextMenu("ImportTexture3dToMaterial")]
    public void ImportTexture3dToMaterial()
    {
        if (texture3D != null)
            DestroyImmediate(texture3D);

        material = GetComponent<Renderer>().sharedMaterial;
        texture3D = new Texture3D(texture3DAsset.resolution, texture3DAsset.resolution, texture3DAsset.resolution,
            TextureFormat.RGBA32, true);
        texture3D.SetPixels(texture3DAsset.colors);
        texture3D.Apply();
        material.SetTexture(materialTexture, texture3D);
    }


    void OnDestroy()
    {
        if (texture3D != null)
            DestroyImmediate(texture3D);
    }

    [ContextMenu("GetTexture3dValue")]
    private void GetTexture3dValue()
    {
        Color[] tex3dValues = texture3D.GetPixels(0);
        Debug.Log(
            tex3dValues[
                (int) (resolution*resolution*tex3dTestPositon.z + resolution*tex3dTestPositon.y + tex3dTestPositon.x)]);
    }

    [ContextMenu("FindFirstTexture3dValue")]
    private void FindFirstTexture3dValue()
    {
        Color[] tex3dValues = texture3D.GetPixels(0);
        int index = -1;
        for (int i = 0; i < tex3dValues.Length; ++i)
        {
            if (tex3dValues[i].r > 0f)
            {
                index = i;
                break;
            }
        }

        if (index >= 0)
        {
            Debug.Log("First non zero value is " + index + ": " + tex3dValues[index]);
        }
        else
        {
            Debug.Log("No non zero values.");
        }
        
    }

    /*
    [ContextMenu("GetAllTex3DValues")]
    private void GetAllTex3DValues()
    {
        Color[] tex3dValues = texture3D.GetPixels(0);
        for (int i = 0; i < resolution * resolution; ++i)
        {
            Debug.Log(tex3dValues[
                (int)(resolution * resolution * i + resolution * heightSlice + i)
                ]);
        }
    }

    [ContextMenu("Make2dTestTexture")]
    private void Make2dTestTexture()
    {
        if (material == null)
        {
            material = GetComponent<Renderer>().material;
        }
        if (test2dTextureSlice != null)
        {
            DestroyImmediate(test2dTextureSlice);
        }
        test2dTextureSlice = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        Color[] texture3dValues = texture3D.GetPixels();
        float colorChannelValue;
        int z = heightSlice;
        for (int y = 0; y < resolution; ++y)
        {
            for (int x = 0; x < resolution; ++x)
            {
                switch (currentTexChannel)
                {
                    case textureChannel.r:
                    {
                        colorChannelValue = texture3dValues[(int)(z * resolution * resolution + y * resolution + x)].r;
                        break;
                    }

                    case textureChannel.g:
                    {
                        colorChannelValue = texture3dValues[(int)(z * resolution * resolution + y * resolution + x)].g;
                        break;
                    }

                    case textureChannel.b:
                    {
                        colorChannelValue = texture3dValues[(int)(z * resolution * resolution + y * resolution + x)].b;
                        break;
                    }

                    case textureChannel.a:
                    {
                        colorChannelValue = texture3dValues[(int)(z * resolution * resolution + y * resolution + x)].a;
                        break;
                    }

                    default:
                    {
                        colorChannelValue = 1.0f;
                        break;
                    }
                }
                test2dTextureSlice.SetPixel(x, y, new Color(colorChannelValue, colorChannelValue, colorChannelValue, 1.0f));
            }
        }

        test2dTextureSlice.Apply();
        material.SetTexture("_MainTex", test2dTextureSlice);
        material.mainTexture = test2dTextureSlice;
    }
    */
}
