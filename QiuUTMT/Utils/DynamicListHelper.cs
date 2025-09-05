namespace QiuUTMT;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class DynamicListHelper
{
    public static void AddDynamicItem<T>(IList<T> list)
    {
        // 获取泛型类型 T
        Type itemType = typeof(T);

        // 使用反射创建 T 的实例（假设有无参构造函数）
        T newItem = (T)Activator.CreateInstance(itemType);
        if (newItem != null) list.Add(newItem);
    }

    public static void AddDynamicItem(IList list)
    {
        Type listType = list.GetType();

        // 检查是否是泛型集合（例如 IList<T>）
        if (listType.IsGenericType)
        {
            // 获取泛型参数 T
            //Type itemType = listType.GetGenericArguments()[0];
            Type itemType = list.Count >= 1 && list[0] != null ? list[0].GetType() : listType.GetGenericArguments()[0];

            // 创建实例
            object newItem = Activator.CreateInstance(itemType);
            if (newItem != null) list.Add(newItem);
        }
        else
        {
            throw new ArgumentException("列表不是泛型集合 IList<T>");
        }
    }

    /// <summary>
    /// 把 obj.name 设为 null。
    /// name 可以是属性或字段。
    /// 成功返回 true，找不到或无法写入返回 false。
    /// NOTICE: 先查找属性，再查找字段
    /// </summary>
    public static bool SetMemberToNull(object obj, string name)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        Type t = obj.GetType();
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        PropertyInfo pi = t.GetProperty(name, flags);
        if (pi != null)
        {
            if (!pi.CanWrite)
            {
                return false;
            }

            pi.SetValue(obj, null);
            return true;
        }

        FieldInfo fi = t.GetField(name, flags);
        if (fi != null)
        {
            fi.SetValue(obj, null);
            return true;
        }

        return false;
    }
}