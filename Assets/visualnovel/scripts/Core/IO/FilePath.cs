using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilePath 
{
    public static readonly string root = $"{Application.dataPath}/visualnovel/resources/TextData/";

    //Resources Paths
    public static readonly string resources_graphics = "Graphics/";
    public static readonly string resources_backgroundImages = $"{resources_graphics}BG Images/";
    public static readonly string resources_backgroundVideos = $"{resources_graphics}BG Videos/";
    public static readonly string resources_blendTextures = $"{resources_graphics}Transition Effects/";

}