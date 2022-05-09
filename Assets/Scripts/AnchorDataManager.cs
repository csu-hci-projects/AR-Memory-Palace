using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.XR.ARFoundation;

/// <summary>锚点数据管理</summary>
public class AnchorDataManager : MonoBehaviour
{
    /// <summary>所有的锚点预设体</summary>
    public List<GameObject> anchorPrefabsList = new List<GameObject>();
    /// <summary>所有的资源文件</summary>
    private Dictionary<string, GameObject> dicRes = new Dictionary<string, GameObject>();
    /// <summary>资源预设体（如果为空，者从资源数据库中取第一对象）</summary>
    private GameObject anchorPrefab;

    /// <summary>配置保存路径</summary>
    private string configPath;
    /// <summary>需要保存的锚点数据</summary>
    private Dictionary<string, AnchorDataInfo> dicSaveAnchorData = new Dictionary<string, AnchorDataInfo>();
    /// <summary>所有的锚点数据（包括缓存的锚点数据）</summary>
    private Dictionary<string, AnchorDataInfo> dicAllAnchorData = new Dictionary<string, AnchorDataInfo>();

    private ARAnchorManager anchorManager;

    private void Awake()
    {
        configPath = Application.persistentDataPath + "/AnchorData.json";
        loadCacheAnchorData();

        anchorManager = FindObjectOfType<ARAnchorManager>();
        if (anchorManager == null)
        {
            Debug.LogError(GetType() + "/ anchorManager is null!");
        }
        else
        {
            anchorManager.anchorPrefab = null;
            anchorManager.anchorsChanged += onAnchorsChanged;
        }

        addAnchorPrefabs();
    }

    private void addAnchorPrefabs()
    {
        if(anchorPrefabsList==null|| anchorPrefabsList.Count==0)
        {
            Debug.LogError("anchorPrefabsList could not be NULL!");
            return;
        }

        for (int i = 0; i < anchorPrefabsList.Count; i++)
        {
            string key = anchorPrefabsList[i].name;
            if (!dicRes.ContainsKey(key)) dicRes.Add(key, anchorPrefabsList[i]);
            else Debug.LogWarning("Duplicate source name! name:" + key);
        }

        anchorPrefab = anchorPrefabsList[0];
      
    }

    /// <summary>
    /// 通过资源名称获取锚点预设体
    /// </summary>
    /// <param name="resName"></param>
    /// <returns></returns>
    private GameObject GetAnchorPrefabByResName(string resName)
    {
        GameObject obj = null;
        dicRes.TryGetValue(resName, out obj);
        return obj;
    }

    /// <summary>加载缓存中的锚点数据</summary>
    private void loadCacheAnchorData()
    {
        AnchorDataInfoList list = readTextData<AnchorDataInfoList>(configPath);
        if (list == null || list.anchorDataInfos == null || list.anchorDataInfos.Length == 0) return;

        AnchorDataInfo[] anchorDataInfos =list.anchorDataInfos;

        for(int i=0;i< anchorDataInfos.Length;i++)
        {
            AnchorDataInfo info = anchorDataInfos[i];
            if(info!=null) AddAnchorDataByCache(info.anchorName, info);
        }
    }

    /// <summary>
    /// 添加数据到缓存中
    /// </summary>
    /// <param name="key">秘钥</param>
    /// <param name="anchorData">锚点数据</param>
    public void AddAnchorDataByCache(string key, AnchorDataInfo anchorData)
    {
        if (!dicAllAnchorData.ContainsKey(key))
        {
            dicAllAnchorData.Add(key, anchorData);
            Debug.Log("Save data to cache successfully! key:" + key);
        }
    }

    /// <summary>获取当前锚点预设体的名字</summary>
    public string GetAnchorPrefabName()
    {
        return anchorPrefab.name;
    }

    /// <summary>
    /// 通过资源名字切换锚点预设体的
    /// </summary>
    /// <param name="resName"></param>
    public void ChageAnchorPrefabByResName(string resName)
    {
        if (!dicRes.ContainsKey(resName)) return;

        GameObject obj = null;

        dicRes.TryGetValue(resName, out obj);

        anchorPrefab = obj;
    }

    /// <summary>
    /// 锚点发生改变事件
    /// </summary>
    /// <param name="eventArgs"></param>
    private void onAnchorsChanged(ARAnchorsChangedEventArgs eventArgs)
    {
        //Debug.Log("addedCount:" + eventArgs.added.Count + "  updatedCount:" + eventArgs.updated.Count + "  removedCount:" + eventArgs.removed.Count);

        for (int i = 0; i < eventArgs.added.Count; i++)
        {
            onAddedListChanged(eventArgs.added[i]);
        }

        for (int i = 0; i < eventArgs.updated.Count; i++)
        {
            onUpdatedListChanged(eventArgs.updated[i]);
        }

        for (int i = 0; i < eventArgs.removed.Count; i++)
        {
            onRemovedListChanged(eventArgs.removed[i]);
        }
    }

    private void onAddedListChanged(ARAnchor aRAnchor)
    {
        Debug.Log("onAddedListChanged:--- anchorName:" + aRAnchor.name);

        AnchorDataInfo info = null;
        dicAllAnchorData.TryGetValue(aRAnchor.name,out info);
        if (info == null) return;

        GameObject anchorObj = Instantiate(GetAnchorPrefabByResName(info.resName), aRAnchor.transform);
        anchorObj.transform.localPosition = Vector3.zero;
        anchorObj.transform.localEulerAngles = Vector3.zero;

        Add(info.anchorName, info);
    }

    private void onUpdatedListChanged(ARAnchor aRAnchor)
    {
        //Debug.Log("onUpdatedListChanged:--- anchorName:" + aRAnchor.name);
    }

    private void onRemovedListChanged(ARAnchor aRAnchor)
    {
        Debug.Log("onRemovedListChanged--- anchorName:" + aRAnchor.name);

        Remove(aRAnchor.name);
    }

    /// <summary>
    /// 添加一条锚点信息
    /// </summary>
    /// <param name="key">秘钥</param>
    /// <param name="anchorData">添加的锚点数据</param>
    public void Add(string key, AnchorDataInfo anchorData)
    {
        //说明秘钥或者锚点数据为空
        if (string.IsNullOrEmpty(key) || anchorData == null) return;
        //说明这条数据已经添加过
        if (dicSaveAnchorData.ContainsKey(key)) return;

        dicSaveAnchorData.Add(key, anchorData);
    }

    /// <summary>
    /// 移除一条锚点数据
    /// </summary>
    /// <param name="key">秘钥</param>
    public void Remove(string key)
    {
        if (dicSaveAnchorData.ContainsKey(key)) dicSaveAnchorData.Remove(key);
    }

    /// <summary>将锚点数据保存在本地配置表中</summary>
    public void Save()
    {
        //如果已经存在配置表，先将当前配置表删除（目的是允许玩家保存空数据）
        if (File.Exists(configPath)) File.Delete(configPath);

        if (dicSaveAnchorData == null || dicSaveAnchorData.Count == 0) return;

        AnchorDataInfo[] anchorDataInfos = dicSaveAnchorData.Values.ToArray();
        AnchorDataInfoList anchorDataInfoList = new AnchorDataInfoList();
        anchorDataInfoList.anchorDataInfos = anchorDataInfos;

        string jsonData = JsonUtility.ToJson(anchorDataInfoList);
        writeData(configPath, jsonData);
    }

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="textPath"></param>
    /// <param name="data"></param>
    private void writeData(string textPath, string data)
    {
        if (string.IsNullOrEmpty(textPath) || string.IsNullOrEmpty(data)) return;

        StreamWriter sw = null;
        sw = File.CreateText(textPath); //如果配置表之前存在者覆盖

        sw.WriteLine(data + '\n');

        sw.Close();
        sw.Dispose();

        Debug.Log(GetType() + "/数据保存成功！ path:" + textPath);
    }

    /// <summary>
    /// 读取txt类型配置表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="textPath"></param>
    /// <returns></returns>
    private static T readTextData<T>(string textPath)
    {
        T temp;
        StreamReader streamReader = null;

        if (File.Exists(textPath)) streamReader = File.OpenText(textPath);
        else { return default(T); }

        string str = streamReader.ReadToEnd();
        temp = JsonUtility.FromJson<T>(str);

        streamReader.Close();
        streamReader.Dispose();

        return temp;
    }
}

[System.Serializable]
public class AnchorDataInfoList
{
    public AnchorDataInfo[] anchorDataInfos;
}

[System.Serializable]
public class AnchorDataInfo
{
    /// <summary>锚点的名字</summary>
    public string anchorName;
    /// <summary>资源的名字</summary>
    public string resName;
}
