using UnityEngine;
using System;

public static class GameObjectExt
{
    #region 获取Component
    /// <summary>
    /// 找到或给GameObject添加一个本没有的Component
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="go"></param>
    /// <returns></returns>
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        var t = go.GetComponent<T>();
        if (t == null)
        {
            t = go.AddComponent<T>();
        }
        return t;
    }

    public static Component GetOrAddComponent(this GameObject go, Type componentType)
    {
        var t = go.GetComponent(componentType);
        if (t == null)
        {
            t = go.AddComponent(componentType);
        }
        return t;
    }
    #endregion

    #region 获取各种游戏对象
    /// <summary>
    /// 获取孩子的transform
    /// </summary>
    public static Transform GetChildTransform(this Transform root, string childName)
    {
        if (root.name == childName)
        {
            return root;
        }
        if (root.childCount != 0)
        {
            var childTransform = root.Find(childName);
            if (childTransform != null)
            {
                return childTransform;
            }
            for (int i = 0; i < root.childCount; i++)
            {
                childTransform = GetChildTransform(root.GetChild(i), childName);
                if (childTransform != null)
                {
                    return childTransform;
                }
            }
        }
        return null;
    }
    #endregion

    #region 新建各种游戏对象
    public static GameObject CreateEmptyGameObject(this GameObject go, string pGameObjectName)
    {
        GameObject tNew = GameObject.Instantiate(new GameObject(), go.transform, false);
        tNew.name = pGameObjectName;
        return tNew;
    }

    public static T CreateEmptyGameObject<T>(this GameObject go) where T : Component
    {
        GameObject tNew = GameObject.Instantiate(new GameObject(), go.transform, false);
        tNew.name = typeof(T).ToString();
        var t = go.GetComponent<T>();
        if (t == null)
        {
            t = go.AddComponent<T>();
        }
        return t;
    }
    #endregion
}