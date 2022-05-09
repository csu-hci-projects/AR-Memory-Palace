using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#if UNITY_IOS
using UnityEngine.XR.ARKit;
#endif

public class ARWorldMapController : MonoBehaviour
{
    private ARSession m_ARSession;
    private List<string> m_LogMessages = new List<string>();

    [SerializeField]
    private Text m_LogText;
    [SerializeField]
    private Text m_ErrorText;
    [SerializeField]
    private Text m_MappingStatusText;
    [SerializeField]
    private Button m_SaveButton;
    [SerializeField]
    private Button m_LoadButton;
    [SerializeField]
    private Button m_ResetButton;

    private void Awake()
    {
        m_ARSession = FindObjectOfType<ARSession>();
        if (m_ARSession == null)
        {
            Debug.LogError(GetType() + "/Awake()/ m_ARSession is null!");
        }

        m_SaveButton.onClick.AddListener(onSaveButton);
        m_LoadButton.onClick.AddListener(onLoadButton);
        m_ResetButton.onClick.AddListener(onResetButton);
    }

    private void Update()
    {
        if (supported)
        {
            SetActive(m_ErrorText, false);
            SetActive(m_SaveButton, true);
            SetActive(m_LogText, true);
            SetActive(m_MappingStatusText, true);
        }
        else
        {
            SetActive(m_ErrorText, true);
            SetActive(m_SaveButton, false);
            SetActive(m_LogText, false);
            SetActive(m_MappingStatusText, false);
        }

        #if UNITY_IOS
        var sessionSubsystem = (ARKitSessionSubsystem)m_ARSession.subsystem;
        #else
        XRSessionSubsystem sessionSubsystem = null;
        #endif
        if (sessionSubsystem == null) return;

        var numLogsToShow = 20;
        string msg = "";
        for (int i = Mathf.Max(0, m_LogMessages.Count - numLogsToShow); i < m_LogMessages.Count; ++i)
        {
            msg += m_LogMessages[i];
            msg += "\n";
        }
        SetText(m_LogText, msg);

        #if UNITY_IOS
        SetText(m_MappingStatusText, string.Format("Mapping Status: {0}", sessionSubsystem.worldMappingStatus));
        #endif
    }

    /// <summary>保存ARWorldMap按钮事件</summary>
    private void onSaveButton()
    {
        #if UNITY_IOS
        StartCoroutine(save());
        #endif
    }

    /// <summary>加载ARWorldMap按钮事件</summary>
    private void onLoadButton()
    {
        #if UNITY_IOS
        StartCoroutine(load());
        #endif
    }

    /// <summary>重置ARSession按钮事件</summary>
    private void onResetButton()
    {
        m_ARSession.Reset();
    }

    #if UNITY_IOS

    /// <summary>保存ARWorldMap</summary>
    private IEnumerator save()
    {
        FindObjectOfType<AnchorDataManager>().Save();//保存数据到本地缓存

        var sessionSubsystem = (ARKitSessionSubsystem)m_ARSession.subsystem;
        if (sessionSubsystem == null)
        {
            log("No session subsystem available. Could not save.");
            yield break;
        }

        var request = sessionSubsystem.GetARWorldMapAsync();

        while (!request.status.IsDone())
            yield return null;

        if (request.status.IsError())
        {
            log(string.Format("Session serialization failed with status {0}", request.status));
            yield break;
        }

        var worldMap = request.GetWorldMap();
        request.Dispose();

        saveAndDisposeWorldMap(worldMap);
    }

    /// <summary>加载ARWorldMap</summary>
    private IEnumerator load()
    {
        var sessionSubsystem = (ARKitSessionSubsystem)m_ARSession.subsystem;
        if (sessionSubsystem == null)
        {
            log("No session subsystem available. Could not load.");
            yield break;
        }

        var file = File.Open(path, FileMode.Open);
        if (file == null)
        {
            log(string.Format("File {0} does not exist.", path));
            yield break;
        }

        log(string.Format("Reading {0}...", path));

        int bytesPerFrame = 1024 * 10;
        var bytesRemaining = file.Length;
        var binaryReader = new BinaryReader(file);
        var allBytes = new List<byte>();
        while (bytesRemaining > 0)
        {
            var bytes = binaryReader.ReadBytes(bytesPerFrame);
            allBytes.AddRange(bytes);
            bytesRemaining -= bytesPerFrame;
            yield return null;
        }

        var data = new NativeArray<byte>(allBytes.Count, Allocator.Temp);
        data.CopyFrom(allBytes.ToArray());

        log(string.Format("Deserializing to ARWorldMap...", path));
        ARWorldMap worldMap;
        if (ARWorldMap.TryDeserialize(data, out worldMap))
        data.Dispose();

        if (worldMap.valid)
        {
            log("Deserialized successfully.");
        }
        else
        {
            Debug.LogError("Data is not a valid ARWorldMap.");
            yield break;
        }

        log("Apply ARWorldMap to current session.");
        sessionSubsystem.ApplyWorldMap(worldMap);
    }

    /// <summary>保存和释放ARWorldMap</summary>
    private void saveAndDisposeWorldMap(ARWorldMap worldMap)
    {
        log("Serializing ARWorldMap to byte array...");
        var data = worldMap.Serialize(Allocator.Temp);
        log(string.Format("ARWorldMap has {0} bytes.", data.Length));

        var file = File.Open(path, FileMode.Create);
        var writer = new BinaryWriter(file);
        writer.Write(data.ToArray());
        writer.Close();
        data.Dispose();
        worldMap.Dispose();
        log(string.Format("ARWorldMap written to {0}", path));
    }

    #endif

    /// <summary>ARWorldMap保存路径</summary>
    private string path
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, "my_session.worldmap");
        }
    }

    /// <summary>判断设备是否支持ARWorldMap功能</summary>
    private bool supported
    {
        get
        {
            #if UNITY_IOS
            return m_ARSession.subsystem is ARKitSessionSubsystem && ARKitSessionSubsystem.worldMapSupported;
            #else
            return false;
            #endif
        }
    }

    static void SetActive(Button button, bool active)
    {
        if (button != null) button.gameObject.SetActive(active);
    }

    static void SetActive(Text text, bool active)
    {
        if (text != null) text.gameObject.SetActive(active);
    }

    static void SetText(Text text, string value)
    {
        if (text != null) text.text = value;
    }

    private void log(string logMessage)
    {
        m_LogMessages.Add(logMessage);
    }

}
