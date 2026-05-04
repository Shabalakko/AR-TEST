#if UNITY_EDITOR && UNITY_ANDROID
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;

/// <summary>
/// Fixes the namespace conflict between :arcore_client: and :unityandroidpermissions:
/// which both declare package="com.google.ar.core" in their AndroidManifest.xml.
/// This is a known bug in com.unity.xr.arcore 6.x with Android Gradle Plugin 8.x.
///
/// This script runs automatically after Unity generates the Gradle project,
/// before Gradle compiles it.
/// </summary>
public class FixARCoreNamespaceConflict : IPostGenerateGradleAndroidProject
{
    private const string TargetAarName = "unityandroidpermissions.aar";
    private const string ConflictingPackage = "package=\"com.google.ar.core\"";
    private const string FixedPackage = "package=\"com.unity.androidpermissions\"";

    public int callbackOrder => 1;

    public void OnPostGenerateGradleAndroidProject(string gradleProjectPath)
    {
        Debug.Log("[FixARCoreNamespace] Running post-generate Gradle fix...");

        // Search from parent directory to find the AAR regardless of Unity version structure
        string searchRoot = Path.GetFullPath(Path.Combine(gradleProjectPath, ".."));
        string[] aarFiles = Directory.GetFiles(searchRoot, TargetAarName, SearchOption.AllDirectories);

        if (aarFiles.Length == 0)
        {
            // Try the gradle project path itself
            aarFiles = Directory.GetFiles(gradleProjectPath, TargetAarName, SearchOption.AllDirectories);
        }

        if (aarFiles.Length == 0)
        {
            Debug.LogWarning($"[FixARCoreNamespace] {TargetAarName} not found in {searchRoot}. Skipping patch.");
            return;
        }

        foreach (string aarPath in aarFiles)
        {
            Debug.Log($"[FixARCoreNamespace] Patching: {aarPath}");
            PatchAar(aarPath);
        }
    }

    private static void PatchAar(string aarPath)
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "aar_patch_" + Path.GetFileNameWithoutExtension(aarPath));

        try
        {
            // Clean and recreate temp dir
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
            Directory.CreateDirectory(tempDir);

            // Extract AAR (AAR is a ZIP file)
            ZipFile.ExtractToDirectory(aarPath, tempDir);

            // Find and patch the AndroidManifest.xml inside the AAR
            string manifestPath = Path.Combine(tempDir, "AndroidManifest.xml");
            if (!File.Exists(manifestPath))
            {
                Debug.LogWarning($"[FixARCoreNamespace] No AndroidManifest.xml found inside {aarPath}. Skipping.");
                return;
            }

            string manifestContent = File.ReadAllText(manifestPath, Encoding.UTF8);

            if (!manifestContent.Contains(ConflictingPackage))
            {
                Debug.Log($"[FixARCoreNamespace] {aarPath} does not contain conflicting package. Already patched or not needed.");
                return;
            }

            // Replace the conflicting package name
            string patchedContent = manifestContent.Replace(ConflictingPackage, FixedPackage);
            File.WriteAllText(manifestPath, patchedContent, Encoding.UTF8);
            Debug.Log($"[FixARCoreNamespace] Manifest patched: '{ConflictingPackage}' -> '{FixedPackage}'");

            // Repack as AAR (ZIP)
            string tempAarPath = aarPath + ".patched.tmp";
            if (File.Exists(tempAarPath))
                File.Delete(tempAarPath);

            ZipFile.CreateFromDirectory(tempDir, tempAarPath, System.IO.Compression.CompressionLevel.Optimal, false);

            // Replace original AAR with patched version
            File.Delete(aarPath);
            File.Move(tempAarPath, aarPath);

            Debug.Log($"[FixARCoreNamespace] Successfully patched {Path.GetFileName(aarPath)}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FixARCoreNamespace] Failed to patch {aarPath}: {e.Message}\n{e.StackTrace}");
        }
        finally
        {
            // Cleanup temp directory
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, true); }
                catch { /* ignore cleanup errors */ }
            }
        }
    }
}
#endif
