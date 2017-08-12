// first Texture3DAssetCreator by the amazing Paul Nasda
// multi channel import by Julian Oberbeck
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class Texture3DAssetCreator : MonoBehaviour {

    [MenuItem("Assets/LoadTexture3DValidate", true)]
    public static bool LoadTexture3DValidate()
    {
        Texture2D texture = Selection.activeObject as Texture2D;
        if (texture == null)
            return false;
        if (texture.width != texture.height)
            return false;
        if (AssetDatabase.GetAssetPath(texture).Contains(".0001"))
        {
            Debug.Log("texture looks like a valid 3d noise texture candidate.");
            return true;
        }
        return false;
    }

    [MenuItem("Assets/ConvertToTexture3D")]
    public static void LoadTexture3D()
    {
        
        string imagesPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        imagesPath = imagesPath.ToLower();
        // assets/textures/render/main/r/main_r.0001.png

        // Setup texture object
        Texture2D texture = (Selection.activeObject as Texture2D);
        int resolution = texture.width;
        Texture3DAsset asset = ScriptableObject.CreateInstance<Texture3DAsset>();
        asset.colors = new Color[resolution * resolution * resolution];
        asset.resolution = resolution;
        Texture2D sliceTexture = new Texture2D(resolution, resolution, texture.format, false);
        string fileString;

        // check if texture sequence is split into multiple channels
        string selectedFileTextureChannel = imagesPath.Substring(imagesPath.LastIndexOf("_") + 1, imagesPath.Length - imagesPath.LastIndexOf("_") - 10);
        List<string> textureChannelList = new List<string>();
        bool splitChannels = false;
        
        if (selectedFileTextureChannel == "rgba" || selectedFileTextureChannel == "rgb")
        {
            splitChannels = false;
            textureChannelList.Add(selectedFileTextureChannel);
        }
        else
        {
            string[] textureChannels = {"r", "g", "b", "a"};
            foreach (string textureChannel in textureChannels)
            {
                string checkTexFile = imagesPath;
                checkTexFile = checkTexFile.Replace("/" + selectedFileTextureChannel + "/", "/" + textureChannel + "/");
                checkTexFile = checkTexFile.Replace("_" + selectedFileTextureChannel + ".", "_" + textureChannel + ".");
                if (File.Exists(checkTexFile))
                {
                    splitChannels = true;
                    textureChannelList.Add(textureChannel);
                }
            }
        }

        string[] fileNameSplit = Path.GetFileNameWithoutExtension(imagesPath).Split('.');
        string selected_index = fileNameSplit[fileNameSplit.Length - 1];

        //textureChannelList.ForEach(Debug.Log);
        //Debug.Log(splitChannels);
        /*
        print( String.Join( ",",  listOfThings ) )
        */

        // read slice texture channels into volume 
        for (var z = 0; z < resolution; ++z)
        {
            foreach (var textureChannel in textureChannelList)
            {
                fileString = imagesPath;
                fileString = fileString.Replace("_" + selectedFileTextureChannel + ".", "_" +textureChannel + ".");
                fileString = fileString.Replace("/" + selectedFileTextureChannel + "/", "/" +textureChannel+ "/");
                
                fileString = fileString.Replace(selected_index, (z).ToString("D4"));
                if (Load2dFileTexture(fileString, sliceTexture))
                {
                    // Debug.Log("reading " + textureChannel + " channel");
                    for (var y = 0; y < resolution; ++y)
                        for (var x = 0; x < resolution; ++x)
                        {
                            if (splitChannels)
                            {
                                // construct naming convention from AssetPath for r, g, b, a component textures
                                // asset.colors.r[...
                                if (textureChannel == "r")
                                {
                                    asset.colors[z*resolution*resolution + y*resolution + x].r =
                                        sliceTexture.GetPixel(x, y).r;
                                }
                                if (textureChannel == "g")
                                {
                                    asset.colors[z*resolution*resolution + y*resolution + x].g =
                                        sliceTexture.GetPixel(x, y).g;
                                }
                                if (textureChannel == "b")
                                {
                                    asset.colors[z*resolution*resolution + y*resolution + x].b =
                                        sliceTexture.GetPixel(x, y).b;
                                }
                                if (textureChannel == "a")
                                {
                                    asset.colors[z*resolution*resolution + y*resolution + x].a =
                                        sliceTexture.GetPixel(x, y).r;
                                }
                                else
                                {
                                    asset.colors[z*resolution*resolution + y*resolution + x].a = 1.0f;
                                }
                            }
                            else
                            {
                                asset.colors[z * resolution * resolution + y * resolution + x] = sliceTexture.GetPixel(x, y);
                            }
                        }
                }
            }
        }
        DestroyImmediate(sliceTexture);

        
        string assetpath = Path.GetDirectoryName(Path.GetDirectoryName(imagesPath)); // get grandparent folder
        string fn = Path.GetFileNameWithoutExtension(imagesPath).Split('.')[0];      // get file name
        assetpath = Path.Combine(assetpath, fn + "_3dtexture.asset");
        AssetDatabase.CreateAsset(asset, assetpath);

        if (splitChannels)
            Debug.Log("Created 3dTextureAsset from split channel textures as " + assetpath);
        else
            Debug.Log("Created 3dTextureAsset from combined textures as " + assetpath);
        //material.SetTexture(materialTexture, texture3D);
    }

    public static bool Load2dFileTexture(string filePath, Texture2D tex)
    {
        byte[] fileData;

        if (File.Exists(filePath))
        {
            FileStream stream = File.OpenRead(filePath);
            fileData = new byte[stream.Length];
            stream.Read(fileData, 0, (int)stream.Length);
            stream.Close();
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            return true;
        }
        return false;
    }

    public static string GetUniqueAssetPathNameOrFallback(string filename)
    {
        string path;
        try
        {
            // Private implementation of a filenaming function which puts the file at the selected path.
            System.Type assetdatabase = typeof(UnityEditor.AssetDatabase);
            path = (string)assetdatabase.GetMethod("GetUniquePathNameAtSelectedPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(assetdatabase, new object[] { filename });
        }
        catch
        {
            // Protection against implementation changes.
            path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/" + filename);
        }
        return path;
    }

}
