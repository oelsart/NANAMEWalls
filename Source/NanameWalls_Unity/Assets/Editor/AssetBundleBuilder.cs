using System;
using System.IO;
using OELSMods;
using UnityEditor;
using UnityEngine;

namespace OELSMods
{
  public static class ModIds
  {
    public const string NanameWalls = "OELS.NanameWalls";
  }
}

namespace SmashTools
{
  public class AssetBundleBuilder : MonoBehaviour
  {
    private const string TextureFolderName = "Textures";
    private const string SoundFolderName = "Sounds";

    private const string ShaderFileName = "Shaders";

    // RimWorld stores Shaders in Materials/ so asset bundle paths have to match it for their
    // loader to be able to find the content.
    private const string ShaderFolderName = "Materials";

    private const string OutputPath = "../../Common/AssetBundles";

    private static readonly BuildTarget[] BuildTargets =
      { BuildTarget.StandaloneWindows64, BuildTarget.StandaloneOSX, BuildTarget.StandaloneLinux64 };

    private static string PlatformSuffix(BuildTarget buildTarget)
    {
      return buildTarget switch
      {
        BuildTarget.StandaloneWindows64 => "_win",
        BuildTarget.StandaloneOSX => "_mac",
        BuildTarget.StandaloneLinux64 => "_linux",
        _ => throw new NotSupportedException(buildTarget.ToString())
      };
    }

    private static string[] GetAssetPaths<T>(string packageId)
    {
      string folderName = FolderName();

      string[] guids =
        AssetDatabase.FindAssets($"t:{typeof(T).Name}",
          new[] { $"Assets/Data/{packageId}/{folderName}" });

      string[] paths = new string[guids.Length];
      for (int i = 0; i < guids.Length; i++)
      {
        string guid = guids[i];
        string path = AssetDatabase.GUIDToAssetPath(guid);
        paths[i] = path;
      }
      return paths;

      string FolderName()
      {
        if (typeof(T) == typeof(Texture2D))
          return TextureFolderName;
        if (typeof(T) == typeof(AudioClip))
          return SoundFolderName;
        if (typeof(T) == typeof(Shader))
          return ShaderFolderName;

        throw new NotImplementedException();
      }
    }

    [MenuItem("Assets/Build AssetBundles/Naname Walls")]
    private static void BuildAssetBundles()
    {
      if (!Directory.Exists(OutputPath))
        throw new DirectoryNotFoundException(OutputPath);

      BuildForMod(ModIds.NanameWalls);
    }

    public static void BuildForMod(string packageId)
    {
      const string TextureBundleName = "oels_nanamewalls_textures";
      const string ShaderBundleName = "oels_nanamewalls_shaders";

      // Start fresh for build folder
      if (!Directory.Exists(OutputPath))
        throw new DirectoryNotFoundException(OutputPath);

      Directory.Delete(OutputPath, true);
      Directory.CreateDirectory(OutputPath);

      // Platform independent
      AssetBundleBuild[] bundles = new AssetBundleBuild[1];
      bundles[0].assetBundleName = TextureBundleName;
      bundles[0].assetNames = GetAssetPaths<Texture2D>(packageId);

      BuildPipeline.BuildAssetBundles(OutputPath, bundles,
        BuildAssetBundleOptions.ChunkBasedCompression,
        BuildTarget.StandaloneWindows64);


      // Platform dependent
      AssetBundleBuild[] platformBundles = new AssetBundleBuild[1];
      platformBundles[0].assetBundleName = ShaderBundleName;
      platformBundles[0].assetNames = GetAssetPaths<Shader>(packageId);

      BuildForPlatform(OutputPath, platformBundles,
        BuildAssetBundleOptions.ChunkBasedCompression);
    }

    private static void BuildForPlatform(string directoryPath, AssetBundleBuild[] bundles,
      BuildAssetBundleOptions bundleOptions)
    {
      foreach (BuildTarget buildTarget in BuildTargets)
      {
        AssetBundleBuild[] platformBundles =
          new AssetBundleBuild[bundles.Length];
        for (int i = 0; i < bundles.Length; i++)
        {
          AssetBundleBuild bundle = bundles[i];
          AssetBundleBuild platformBundle = bundle;
          platformBundle.assetBundleName = bundle.assetBundleName + PlatformSuffix(buildTarget);
          platformBundles[i] = platformBundle;
        }
        BuildPipeline.BuildAssetBundles(directoryPath, platformBundles, bundleOptions, buildTarget);
      }
    }
  }
}