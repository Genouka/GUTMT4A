using System.Collections;

namespace QiuUTMT;

using System;
using System.Collections.Generic;
using System.Linq;

public class DynamicListHelper
{
    public static void AddDynamicItem<T>(IList<T> list)
    {
        // 获取泛型类型 T
        Type itemType = typeof(T);

        // 使用反射创建 T 的实例（假设有无参构造函数）
        T newItem = (T)Activator.CreateInstance(itemType);

        // 添加到集合
        list.Add(newItem);
    }
    public static void AddDynamicItem(IList list)
    {
        Type listType = list.GetType();

        // 检查是否是泛型集合（例如 IList<T>）
        if (listType.IsGenericType)
        {
            // 获取泛型参数 T
            Type itemType = listType.GetGenericArguments()[0];

            // 创建实例
            object newItem = Activator.CreateInstance(itemType);

            // 通过非泛型接口添加
            list.Add(newItem);
        }
        else
        {
            throw new ArgumentException("列表不是泛型集合 IList<T>");
        }
    }
}