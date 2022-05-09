using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARAnchorManager))]
[RequireComponent(typeof(ARRaycastManager))]
public class AnchorCreator : MonoBehaviour
{
    public void RemoveAllAnchors()
    {
        foreach (var anchor in m_Anchors)
        {
            m_AnchorManager.RemoveAnchor(anchor);
        }
        m_Anchors.Clear();
    }

    private AnchorDataManager anchorDataManager;

    void Awake()
    {

        anchorDataManager = FindObjectOfType<AnchorDataManager>();
        if(anchorDataManager==null)
        {
            Debug.LogError(GetType() + "/ anchorDataManager is null!");
        }

        m_RaycastManager = GetComponent<ARRaycastManager>();
        m_AnchorManager = GetComponent<ARAnchorManager>();
        m_Anchors = new List<ARAnchor>();
    }


    void Update()
    {
        if (Input.touchCount == 0|| isTouchUI()) return;

        var touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began)
            return;

        if (m_RaycastManager.Raycast(touch.position, s_Hits, TrackableType.FeaturePoint))
        {
            // Raycast hits are sorted by distance, so the first one
            // will be the closest hit.
            var hitPose = s_Hits[0].pose;
            var anchor = m_AnchorManager.AddAnchor(hitPose);
            if (anchor == null)
            {
                Debug.Log("Error creating anchor");
            }
            else
            {
                Debug.Log("anchorName:" + anchor.name);
                AnchorDataInfo anchorData = new AnchorDataInfo();
                anchorData.anchorName = anchor.name;
                anchorData.resName = anchorDataManager.GetAnchorPrefabName();
                anchorDataManager.AddAnchorDataByCache(anchor.name, anchorData);

                m_Anchors.Add(anchor);
            }
        }
    }

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    List<ARAnchor> m_Anchors;

    ARRaycastManager m_RaycastManager;

    ARAnchorManager m_AnchorManager;


    /// <summary>判断是否点击在UI上面</summary>
    private bool isTouchUI()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null) return false;

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (Input.touchCount < 1) return false;
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return true;
        }
        else
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return true;
        }

        return false;
    }

}

