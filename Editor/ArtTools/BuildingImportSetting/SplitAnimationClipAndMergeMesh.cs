using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CustomAnimationData{
    //public string Path;
    public float Error = 0.0001f;
    public EditorCurveBinding[] BindScale = new EditorCurveBinding[3];

    public EditorCurveBinding[] BindPosition = new EditorCurveBinding[3];

    public EditorCurveBinding[] BindRotation = new EditorCurveBinding[4];

    public AnimationCurve[] CurveScale = new AnimationCurve[3];

    public AnimationCurve[] CurvePosition = new AnimationCurve[3];

    public AnimationCurve[] CurveRotation = new AnimationCurve[4];

    public Vector3 Scale;
    public Vector3 Position;
    public Quaternion Rotation;
    public bool SignScale;
    public bool SignPosition;
    public bool SignRotation;

    public bool CanRemove { get; set; }

    public void DoCheck()
    {
        CanRemove = true;
        
        if (CurveScale.Length != BindScale.Length || CurvePosition.Length != BindPosition.Length ||
            CurveRotation.Length != BindRotation.Length)
        {
            CanRemove = false;
            return;
        }
        for (int i = 0; i < BindScale.Length && i < CurveScale.Length; i++)
        {
            if (BindScale[i] != null && CurveScale[i] != null)
            {
                //关键帧长度不等于2
                if (CurveScale[i].length != 2)
                {
                    CanRemove = false;
                    return;
                }
                //不相等
                if (Mathf.Abs(CurveScale[i].keys[0].value - CurveScale[i].keys[1].value) > Error)
                {
                    CanRemove = false;
                    return;
                }
            }
        }
        //存在可以删除的curve，存储key值，赋值给GameObject
        if (CurveScale.Length == 3 && CurveScale[0] != null &&
            CurveScale[0].length > 0)
        {
            SignScale = true;
            Scale = new Vector3(CurveScale[0].keys[0].value, CurveScale[1].keys[0].value, CurveScale[2].keys[0].value);
        }
        //
        for (int i = 0; i < BindPosition.Length && i < CurvePosition.Length; i++)
        {
            if (BindPosition[i] != null && CurvePosition[i] != null)
            {
                //关键帧长度不等于2
                if (CurvePosition[i].length != 2)
                {
                    CanRemove = false;
                    return;
                }
                //不相等
                if (Mathf.Abs(CurvePosition[i].keys[0].value - CurvePosition[i].keys[1].value) > Error)
                {
                    CanRemove = false;
                    return;
                }
            }
        }
        //存在可以删除的curve，存储key值，赋值给GameObject
        if (CurvePosition.Length == 3 && CurvePosition[0] != null &&
            CurvePosition[0].length > 0)
        {
            SignPosition = true;
            Position = new Vector3(CurvePosition[0].keys[0].value, CurvePosition[1].keys[0].value, CurvePosition[2].keys[0].value);
        }
        //
        for (int i = 0; i < BindRotation.Length && i < CurveRotation.Length; i++)
        {
            if (BindRotation[i] != null && CurveRotation[i] != null)
            {
                //关键帧长度不等于2
                if (CurveRotation[i].length != 2)
                {
                    CanRemove = false;
                    return;
                }
                //不相等
                if (Mathf.Abs(CurveRotation[i].keys[0].value - CurveRotation[i].keys[1].value) > Error)
                {
                    CanRemove = false;
                    return;
                }
            }
        }
        //存在可以删除的curve，存储key值，赋值给GameObject
        if (CurveRotation.Length == 4 && CurveRotation[0] != null &&
            CurveRotation[0].length > 0)
        {
            SignRotation = true;
            Rotation = new Quaternion(CurveRotation[0].keys[0].value, CurveRotation[1].keys[0].value, 
                CurveRotation[2].keys[0].value, CurveRotation[3].keys[0].value);
        }
    }
};

/// <summary>
/// 将building的动画分片段导出,loop类型的动画，将模型单独
/// 分离出来，去掉不需要的mesh，合并没有关键帧或者只有两个关键帧
/// 切数值一样的mesh，去掉不需要的curve。目的是减少culling的开销、
/// flush的开销、动态合批的内存开销、loop动画的内存开销。但是会
/// 额外增加包体大小，每个建筑的每个等级的每个loop动画都会额外
/// 生成一个模型。
/// </summary>
public class SplitAnimationClipAndMergeMesh
{
    private static Dictionary<string, CustomAnimationData> m_aniData = new 
        Dictionary<string, CustomAnimationData>();
    private const string g_storeDir = "Assets/Models/Environment/Models/Environment/COL3Building/AutoGenerate/";
    private const string g_prefabDir = "Assets/Data/Prefabs/BuildingModel/";

    public static bool DealSpecialBuilding(GameObject go)
    {
        if (go.name.Equals("Con2009") || go.name.Equals("Con5203"))
        {
            GameObject lockGo = Object.Instantiate(go);
            GameObject idleGo = Object.Instantiate(go);
            PrefabUtility.SaveAsPrefabAsset(lockGo, g_prefabDir + go.name + "_lock" + ".prefab");
            PrefabUtility.SaveAsPrefabAsset(idleGo, g_prefabDir + go.name + "_idle" + ".prefab");
            Object.DestroyImmediate(lockGo);
            Object.DestroyImmediate(idleGo);
            return true;
        }
        return false;
    }

    public static void DoSplitAnimationClipAndMergeMesh(GameObject go, bool isMerge)
    {
        if (go == null)
        {
            return;
        }
        string path = AssetDatabase.GetAssetPath(go);
        if (!path.StartsWith("Assets/Models/Environment/Models/Environment/COL3Building/Models"))
        {
            return;
        }
        int index = Application.dataPath.LastIndexOf('/');
        string headDir = Application.dataPath.Remove(index + 1);
        string fullDirPath = headDir + g_storeDir;
        IGG.FileUtil.CreateDirectory(fullDirPath);

        //删除原先的AutoGenerate文件夹下面的，以及prefab。
        DeleteFiles(fullDirPath, go.name);
        DeleteFiles(headDir + g_prefabDir, go.name);

        if (DealSpecialBuilding(go))
        {
            return;
        }

        AnimationClip[] clips = AnimationUtility.GetAnimationClips(go);
        for (int i = 0; i < clips.Length; i++)
        {
            if (isMerge && clips[i].wrapMode == WrapMode.Loop)
            {
                MergeMesh(go, clips[i]);
            }
            else
            {
                AnimationClip aniClip = Object.Instantiate(clips[i]);
                AssetDatabase.CreateAsset(aniClip, g_storeDir+go.name+"_"+clips[i].name+".anim");
               
                GameObject aniGo = Object.Instantiate(go);
                Animation ani = aniGo.GetComponent<Animation>();
                if (null != ani)
                {
                    Object.DestroyImmediate(ani);
                }
                ani = aniGo.AddComponent<Animation>();
                ani.clip = aniClip;
                //AnimationUtility.SetAnimationClips(ani, new AnimationClip[] { aniClip });
                PrefabUtility.SaveAsPrefabAsset(aniGo, g_prefabDir + go.name + "_"+clips[i].name+".prefab");

                Object.DestroyImmediate(aniGo);
            }
        }
    }

    private static void DeleteFiles(string path, string name)
    {
        string[] resFiles = Directory.GetFiles(path);
        for (int i = 0; i < resFiles.Length; i++)
        {
            if (resFiles[i].StartsWith(path+name))
            {
                File.Delete(resFiles[i]);
            }
        }
    }

    private static void MergeMesh(GameObject go, AnimationClip clip)
    {
        //AnimationClipSettings setting = AnimationUtility.GetAnimationClipSettings(clip);
        //if (!setting.loopTime)
        //{
        //    return;
        //}
        AnimationClip copyClip = Object.Instantiate(clip);
        GameObject copyGo = Object.Instantiate(go);
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(copyClip);
        m_aniData.Clear();

        //存储动画数据，为后续检查做准备
        for (int i = 0; i < bindings.Length; i++)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(copyClip, bindings[i]);
            CustomAnimationData caData;
            if (!m_aniData.TryGetValue(bindings[i].path, out caData))
            {
                caData = new CustomAnimationData();
                m_aniData.Add(bindings[i].path, caData);
            }
            if (bindings[i].propertyName.ToLower().Contains("scale.x"))
            {
                caData.BindScale[0] = bindings[i];
                caData.CurveScale[0] = curve;
            }
            else if (bindings[i].propertyName.ToLower().Contains("scale.y"))
            {
                caData.BindScale[1] = bindings[i];
                caData.CurveScale[1] = curve;
            }
            else if (bindings[i].propertyName.ToLower().Contains("scale.z"))
            {
                caData.BindScale[2] = bindings[i];
                caData.CurveScale[2] = curve;
            }
            else if (bindings[i].propertyName.ToLower().Contains("position.x"))
            {
                caData.BindPosition[0] = bindings[i];
                caData.CurvePosition[0] = curve;
            }
            else if (bindings[i].propertyName.ToLower().Contains("position.y"))
            {
                caData.BindPosition[1] = bindings[i];
                caData.CurvePosition[1] = curve;
            }
            else if (bindings[i].propertyName.ToLower().Contains("position.z"))
            {
                caData.BindPosition[2] = bindings[i];
                caData.CurvePosition[2] = curve;
            }
            else if (bindings[i].propertyName.ToLower().Contains("rotation.x"))
            {
                caData.BindRotation[0] = bindings[i];
                caData.CurveRotation[0] = curve;
            }
            else if (bindings[i].propertyName.ToLower().Contains("rotation.y"))
            {
                caData.BindRotation[1] = bindings[i];
                caData.CurveRotation[1] = curve;
            }
            else if (bindings[i].propertyName.ToLower().Contains("rotation.z"))
            {
                caData.BindRotation[2] = bindings[i];
                caData.CurveRotation[2] = curve;
            }
            else if (bindings[i].propertyName.ToLower().Contains("rotation.w"))
            {
                caData.BindRotation[3] = bindings[i];
                caData.CurveRotation[3] = curve;
            }
        }
        //检查是否可以删除curve，可以的话就删除
        foreach (KeyValuePair<string, CustomAnimationData> item in m_aniData)
        {
            item.Value.DoCheck();
        }
        //如果父节点有动画，则子节点不能合并的
        foreach (KeyValuePair<string, CustomAnimationData> item in m_aniData)
        {
            item.Value.CanRemove = IsNodeCanRemove(item.Key, item.Value.CanRemove);
        }
        //删除curve
        foreach (KeyValuePair<string, CustomAnimationData> item in m_aniData)
        {
            if (item.Value.CanRemove)
            {
                for (int i = 0; i < item.Value.BindPosition.Length; i++)
                {
                    if (null != item.Value.BindPosition[i] &&
                        !string.IsNullOrEmpty(item.Value.BindPosition[i].path))
                    {
                        AnimationUtility.SetEditorCurve(copyClip, item.Value.BindPosition[i], null);
                    }
                }
                for (int i = 0; i < item.Value.BindScale.Length; i++)
                {
                    if (null != item.Value.BindScale[i] &&
                        !string.IsNullOrEmpty(item.Value.BindScale[i].path))
                    {
                        AnimationUtility.SetEditorCurve(copyClip, item.Value.BindScale[i], null);
                    }
                }
                for (int i = 0; i < item.Value.BindRotation.Length; i++)
                {
                    if (null != item.Value.BindRotation[i] &&
                        !string.IsNullOrEmpty(item.Value.BindRotation[i].path))
                    {
                        AnimationUtility.SetEditorCurve(copyClip, item.Value.BindRotation[i], null);
                    }
                }
            }
        }

        //遍历可以删除的path，在go上找出来，设置key的值，并将这些网格合并
        //TODO:暂时不处理材质不同的情况，默认go下所有材质都是同一个。
        Material sharedMat = null;
        List<MeshFilter> filterList = new List<MeshFilter>();   //用于合并的
        List<Transform> childrenList = new List<Transform>(); //待删除的
        //所有被删除了curve的，加入到待删除列表和合并列表
        foreach (KeyValuePair<string, CustomAnimationData> item in m_aniData)
        {
            Transform child = copyGo.transform.Find(item.Key);
            if (null == child)
            {
                Debug.LogError("can not find "+item.Key+" in "+ copyGo.name);
                continue;
            }
            if (!item.Value.CanRemove)
            {
                continue;
            }
           
            MeshFilter childFilter = child.GetComponent<MeshFilter>();
            if (null == childFilter)
            {
                continue;
            }
            if (item.Value.SignScale)
            {
                child.localScale = item.Value.Scale;
            }
            if (item.Value.SignPosition)
            {
                child.localPosition = item.Value.Position;
            }
            if (item.Value.SignRotation)
            {
                child.localRotation = item.Value.Rotation;
            }

            if (null == sharedMat)
            {
                MeshRenderer render = child.GetComponent<MeshRenderer>();
                if (null != render)
                {
                    sharedMat = render.sharedMaterial;
                }
            }
            else
            {
                MeshRenderer render = child.GetComponent<MeshRenderer>();
                if (null != render && sharedMat != render.sharedMaterial)
                {
                    Debug.LogError(copyGo.name + "存在两个以上的材质球，会导致合并错误。");
                    continue;
                }
            }

            childrenList.Add(child);
            filterList.Add(childFilter);
        }
        //找出所有原来就没有k帧的child
        List<Transform> tempList = new List<Transform>();
        FindNoAnimationChildren(copyGo.transform, copyGo.transform, tempList);
        for (int i = 0; i < tempList.Count; i++)
        {
            MeshRenderer mr = tempList[i].GetComponent<MeshRenderer>();
            if (mr == null)
            {
                continue;
            }
            if (sharedMat == null)
            {
                sharedMat = mr.sharedMaterial;
            }
            if (mr.sharedMaterial == sharedMat)
            {
                childrenList.Add(tempList[i]);
                filterList.Add(tempList[i].GetComponent<MeshFilter>());
            }
            else
            {
                Debug.LogError(tempList[i].name+"有不一样的材质");
                //
                if (tempList[i].transform.lossyScale.x <= 0.012f &&
                tempList[i].transform.lossyScale.y <= 0.012f &&
                tempList[i].transform.lossyScale.z <= 0.012f)
                {
                    childrenList.Add(tempList[i]);
                }
            }
        }
        //去掉scale小于0.01的
        for (int i = filterList.Count - 1; i >= 0; i--)
        {
            if (filterList[i].transform.lossyScale.x <= 0.012f &&
                filterList[i].transform.lossyScale.y <= 0.012f &&
                filterList[i].transform.lossyScale.z <= 0.012f)
            {
                filterList.RemoveAt(i);
            }
        }
        //combine
        GameObject combineGo = null;
        if (filterList.Count > 0)
        {
            CombineInstance[] combines = new CombineInstance[filterList.Count];
            for (int i = 0; i < combines.Length; i++)
            {
                combines[i].mesh = filterList[i].sharedMesh;
                combines[i].transform = filterList[i].transform.localToWorldMatrix;
            }
            combineGo = new GameObject("combine");
            combineGo.AddComponent<MeshRenderer>().sharedMaterial = sharedMat;
            MeshFilter combineFilter = combineGo.AddComponent<MeshFilter>();
            combineFilter.sharedMesh = new Mesh();
            combineFilter.sharedMesh.name = copyGo.name + "_combine";
            combineFilter.sharedMesh.CombineMeshes(combines);
        }

        //移除已经被combine过的GameObject和Mesh(如果有子物体，则只移除MeshRenderer和MeshFilter)
        for (int i = 0; i < childrenList.Count; i++)
        {
            MeshRenderer childRenderer = childrenList[i].GetComponent<MeshRenderer>();
            MeshFilter childFilter = childrenList[i].GetComponent<MeshFilter>();
            if (null != childRenderer)
            {
                Object.DestroyImmediate(childRenderer);
            }
            if (null != childFilter)
            {
                Object.DestroyImmediate(childFilter);
            }
        }
        RemoveMeshRendererMeshFilter(copyGo.transform, copyGo.transform);
        if (null != combineGo)
        {
            combineGo.transform.SetParent(copyGo.transform);
        }

        //优化浮点数，主要是减少包体占用，内存不会变
        OptmizeAnimationFloat(copyClip, 3);
        AssetDatabase.CreateAsset(copyClip, g_storeDir+go.name+"_"+ clip.name + ".anim");
        //删掉原来的Animation,用新的
        Animation ani = copyGo.GetComponent<Animation>();
        if (null != ani)
        {
            Object.DestroyImmediate(ani);
        }
        ani = copyGo.AddComponent<Animation>();
        ani.clip = copyClip;
        //AnimationUtility.SetAnimationClips(ani, new AnimationClip[] { copyClip });

        MeshFilter[] filters = copyGo.GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < filters.Length; i++)
        {
            Mesh newMesh = Object.Instantiate(filters[i].sharedMesh);
            filters[i].sharedMesh = newMesh;
            if (i == 0)
            {
                AssetDatabase.CreateAsset(newMesh, g_storeDir + go.name+"_"+clip.name+"_"+ "mesh.asset");
            }
            else
            {
                AssetDatabase.AddObjectToAsset(filters[i].sharedMesh, g_storeDir + go.name + "_" + clip.name + "_" + "mesh.asset");
            }
        }

        PrefabUtility.SaveAsPrefabAsset(copyGo, g_prefabDir + go.name+"_"+ clip.name + ".prefab");
        AssetDatabase.SaveAssets();
        Object.DestroyImmediate(copyGo);
    }

    private static void OptmizeAnimationFloat(AnimationClip clip, uint x)
    {
        if (clip != null && x > 0)
        {
            //浮点数精度压缩到fx
            AnimationClipCurveData[] curves = null;
            curves = AnimationUtility.GetAllCurves(clip);
            Keyframe key;
            Keyframe[] keyFrames;
            string floatFormat = "f"+x.ToString();

            if (curves != null && curves.Length > 0)
            {
                for (int ii = 0; ii < curves.Length; ++ii)
                {
                    AnimationClipCurveData curveDate = curves[ii];
                    if (curveDate.curve == null || curveDate.curve.keys == null)
                    {
                        //Debug.LogWarning(string.Format("AnimationClipCurveData {0} don't have curve; Animation name {1} ", curveDate, animationPath));
                        continue;
                    }
                    keyFrames = curveDate.curve.keys;
                    for (int i = 0; i < keyFrames.Length; i++)
                    {
                        key = keyFrames[i];
                        key.value = float.Parse(key.value.ToString(floatFormat));
                        key.inTangent = float.Parse(key.inTangent.ToString(floatFormat));
                        key.outTangent = float.Parse(key.outTangent.ToString(floatFormat));
                        keyFrames[i] = key;
                    }
                    curveDate.curve.keys = keyFrames;
                    clip.SetCurve(curveDate.path, curveDate.type, curveDate.propertyName, curveDate.curve);
                }
            }
        }
    }

    //这一串都必须是可以移除的才返回true，否则返回false
    private static bool IsNodeCanRemove(string key, bool canRemove)
    {
        if (!key.Contains("/"))
        {
            return canRemove;
        }
        int index = key.LastIndexOf('/');
        string parentKey = key.Remove(index);
        //Debug.Log(key+","+parentKey);
        CustomAnimationData parentData;
        if (m_aniData.TryGetValue(parentKey, out parentData))
        {
            return IsNodeCanRemove(parentKey, parentData.CanRemove && canRemove);
        }
        else
        {
            return canRemove;
        }
    }

    private static void RemoveMeshRendererMeshFilter(Transform root, Transform go)
    {
        if (null == go)
        {
            return;
        }
        if (go.childCount <= 0)
        {
            if (go.GetComponent<MeshFilter>() == null &&
                go.GetComponent<MeshRenderer>() == null &&
                go != root)
            {
                Object.DestroyImmediate(go.gameObject);
            }
        }
        else
        {
            for (int i = go.childCount-1; i >= 0; i--)
            {
                RemoveMeshRendererMeshFilter(root, go.GetChild(i));
            }
            MeshFilter[] filters = go.GetComponentsInChildren<MeshFilter>();
            MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>();
            if ((filters != null && filters.Length > 0) ||
                (renderers != null && renderers.Length > 0))
            {
                return;
            }
            else
            {
                if (go.GetComponent<MeshFilter>() == null &&
                go.GetComponent<MeshRenderer>() == null && 
                root != go)
                {
                    Object.DestroyImmediate(go.gameObject);
                }
            }
        }
    }

    private static void FindNoAnimationChildren(Transform root, Transform go, List<Transform> list)
    {
        for (int i = 0; i < go.transform.childCount; i++)
        {
            Transform child = go.GetChild(i);
            if (child == null)
            {
                continue;
            }
            string path = GetChildPath(root, child);
            CustomAnimationData caData;
            if (!m_aniData.TryGetValue(path, out caData))
            {
                if (child.GetComponent<MeshRenderer>() != null &&
                    child.GetComponent<MeshFilter>() != null)
                {
                    //并且要检查父节点的动画是否都是可以移除的
                    if (IsNodeCanRemove(path, true))
                    {
                        list.Add(child);
                    }
                }
            }
            FindNoAnimationChildren(root, child, list);
        }
    }

    private static string GetChildPath(Transform root, Transform child)
    {
        if (root == null || child == null)
        {
            return "";
        }
        string path = child.name;
        while (child.parent != root)
        {
            path = child.parent.name + "/" + path;
            child = child.parent;
        }
        return path;
    }

}
