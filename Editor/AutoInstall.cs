#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

[InitializeOnLoad]
public static class SDKSyncAutoInstaller
{
    // Khóa để chỉ chạy tự động 1 lần (mỗi project) trừ khi bạn reset.
    private const string PrefKeyRan = "SDKSyncAutoInstaller_RanOnce";

    // Tên package của bạn trong package.json ("name")
    private const string ThisPackageName = "com.yourorg.sdk-sync";

    static SDKSyncAutoInstaller()
    {
        // Chỉ chạy tự động lần đầu, tránh spam mỗi domain reload
        if (!EditorPrefs.GetBool(PrefKeyRan, false))
        {
            EditorApplication.update += Bootstrap;
        }
    }

    // ------- Menu thủ công: Window/SDK Sync/Install Dependencies -------
    [MenuItem("Window/SDK Sync/Install Dependencies")]
    public static void InstallNow()
    {
        Debug.Log("[SDK-Sync] Manual install started.");
        RunInstaller(force: true);
    }

    [MenuItem("Window/SDK Sync/Reset One-Time Flag")]
    public static void ResetFlag()
    {
        EditorPrefs.DeleteKey(PrefKeyRan);
        Debug.Log("[SDK-Sync] One-time flag cleared. Auto install will run next domain reload.");
    }
    // -------------------------------------------------------------------

    private static ListRequest _listReq;
    private static bool _started;

    private static void Bootstrap()
    {
        if (!_started)
        {
            _started = true;
            _listReq = Client.List(true); // include dependencies
            return;
        }

        if (_listReq != null && !_listReq.IsCompleted) return;
        EditorApplication.update -= Bootstrap;

        // Sau khi có danh sách package hiện có, chạy installer
        RunInstaller(force: false, installedNames: _listReq?.Result?.Select(p => p.name).ToHashSet());
    }

    private static void RunInstaller(bool force, HashSet<string> installedNames = null)
    {
        var pkgInfo = UnityEditor.PackageManager.PackageInfo.FindForPackageName("com.mgif.sdks");
        if (pkgInfo == null)
        {
            Debug.LogWarning($"[SDK-Sync] Cannot find package 'com.mgif.sdks'.");
            return;
        }

        var listPath = Path.Combine(pkgInfo.resolvedPath, "Editor/install.txt");
        if (!File.Exists(listPath))
        {
            Debug.Log($"[SDK-Sync] No install.txt found at: {listPath}");
            EditorPrefs.SetBool(PrefKeyRan, true);
            return;
        }

        var entries = File.ReadAllLines(listPath)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrEmpty(l) && !l.StartsWith("#"))
            .ToArray();

        if (entries.Length == 0)
        {
            Debug.Log("[SDK-Sync] install.txt is empty.");
            EditorPrefs.SetBool(PrefKeyRan, true);
            return;
        }

        // Nếu chạy tự động và đã từng chạy, thoát sớm
        if (!force && EditorPrefs.GetBool(PrefKeyRan, false))
            return;

        installedNames ??= GetInstalledNamesFallback();

        int queued = 0;
        foreach (var entry in entries)
        {
            // Git URL?
            if (entry.Contains("://") || entry.EndsWith(".git") || entry.Contains(".git?") || entry.Contains(".git#"))
            {
                Debug.Log($"[SDK-Sync] Installing from Git: {entry}");
                Client.Add(entry);
                queued++;
            }
            else
            {
                // ID hoặc ID@version
                var id = entry;
                var name = entry.Split('@')[0];

                if (installedNames.Contains(name))
                {
                    // Nếu đã có, bỏ qua (bạn có thể bổ sung logic kiểm tra version nếu muốn)
                    continue;
                }

                Debug.Log($"[SDK-Sync] Installing: {id}");
                Client.Add(id);
                queued++;
            }
        }

        if (queued > 0)
        {
            Debug.Log($"[SDK-Sync] Queued {queued} package(s) for installation.");
        }
        else
        {
            Debug.Log("[SDK-Sync] Nothing to install (all present).");
        }

        // Đánh dấu đã chạy 1 lần
        EditorPrefs.SetBool(PrefKeyRan, true);
    }

    private static HashSet<string> GetInstalledNamesFallback()
    {
        try
        {
            var req = Client.List(true);
            while (!req.IsCompleted) { }
            return req.Result.Select(p => p.name).ToHashSet();
        }
        catch
        {
            return new HashSet<string>();
        }
    }
}
#endif
