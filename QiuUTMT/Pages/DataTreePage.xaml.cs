using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UndertaleModLib;
using UndertaleModLib.Models;
using UTMTdrid;

namespace QiuUTMT;

public partial class DataTreePage : ContentPage
{
    public DataTreePage()
    {
        InitializeComponent();
        BindingContext = this;
        SetCurrentObject(QiuFuncMainSingle.QiuFuncMain.Data, "区块表");
        TreeOrder.IsEnabled = false;
        ViewableEditor.IsEnabled = false;
    }

    public class PropertyItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public object OriginalValue { get; set; }
        public object OriginalType { get; set; }
        public bool IsExpandable { get; set; }
        public int IndentLevel { get; set; }
        public bool IsExpanded { get; set; }
    }


    private object currentObject;
    private string _currentPathName = "根对象";
    private Stack<object> _objectStack = new Stack<object>();
    private Stack<string> _nameStack = new Stack<string>();
    private string _statusMessage = "就绪";
    private bool _canGoBack = false;

    #region ===== 树状导航实现 =====

    public RangeObservableCollection<PropertyItem> Properties { get; } = new();

    public string CurrentPathName
    {
        get => _currentPathName;
        set
        {
            _currentPathName = value;
            LabelPathName.Text = value;
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            LabelStatus.Text = value;
        }
    }

    public bool CanGoBack
    {
        get => _canGoBack;
        set
        {
            _canGoBack = value;
            ButtonBack.IsEnabled = value;
        }
    }

    /// <summary>
    /// 设定树状导航中当前层级的所有列表子项，并将数据和路径堆栈
    /// </summary>
    /// <param name="obj">要堆栈的数据</param>
    /// <param name="name">堆栈数据的名称</param>
    /// <param name="filter">过滤器(目前的实现是模糊搜索)</param>
    private void SetCurrentObject(object obj, string name, string? filter = null)
    {
        currentObject = obj;
        var previousName = _nameStack.Count <= 0 ? "" : (_nameStack.Peek() + " > ");
        _objectStack.Push(obj);
        _nameStack.Push(name);

        CurrentPathName = previousName + name;
        CanGoBack = _objectStack.Count > 1;

        Properties.Clear();

        if (obj == null)
        {
            Properties.Add(new PropertyItem
            {
                Name = "信息",
                Value = "对象为null",
                IsExpandable = false,
                IndentLevel = 0
            });
            return;
        }

        List<PropertyItem> propertiesList = new();
        // 获取对象属性
        if (obj is IEnumerable enumerable && obj is not string)
        {
            int titleIdx = 0;
            foreach (var item in enumerable)
            {
                string title = $"[{titleIdx}]";
                titleIdx++;
                string valuePreview = GetValuePreview(item);
                bool isExpandable = IsExpandable(item);
                if (!string.IsNullOrEmpty(filter) && !FuzzyMatch.IsMatch(title + valuePreview, filter)) continue;
                propertiesList.Add(new PropertyItem
                {
                    Name = title,
                    Value = valuePreview,
                    OriginalValue = item,
                    IsExpandable = isExpandable,
                    IndentLevel = 0
                });
            }

            Properties.AddRange(propertiesList);
        }
        else
        {
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
            {
                try
                {
                    Type type = descriptor.PropertyType;
                    object? value = descriptor.GetValue(obj);
                    string valuePreview = GetValuePreview(value, type);
                    bool isExpandable = IsExpandable(value);
                    if (!string.IsNullOrEmpty(filter) &&
                        !FuzzyMatch.IsMatch(descriptor.Name + valuePreview, filter)) continue;
                    propertiesList.Add(new PropertyItem
                    {
                        Name = descriptor.Name,
                        Value = valuePreview,
                        OriginalValue = value,
                        OriginalType = type,
                        IsExpandable = isExpandable,
                        IndentLevel = 0
                    });
                }
                catch (Exception ex)
                {
                    if (!string.IsNullOrEmpty(filter) && !FuzzyMatch.IsMatch(descriptor.Name, filter)) continue;
                    propertiesList.Add(new PropertyItem
                    {
                        Name = descriptor.Name,
                        Value = $"<错误: {ex.Message}>",
                        IsExpandable = false,
                        IndentLevel = 0
                    });
                }
            }

            Properties.AddRange(propertiesList);
        }

        if (string.IsNullOrEmpty(filter))
        {
            StatusMessage = $"已加载 {Properties.Count} 个属性";
        }
        else
        {
            StatusMessage = $"当前层级有 {Properties.Count} 个搜索结果";
        }
    }

    private string GetValuePreview(object? value, Type? originalType = null)
    {
        if (value == null)
        {
            if (originalType != null)
            {
                return $"[{originalType.Name}]null";
            }

            return "null";
        }

        var type = value.GetType();
        if (type.IsEnum)
            return $"枚举[{type.Name}]{(ulong)value}({value})";

        if (type.IsPrimitive || type == typeof(string))
            return value.ToString();

        if (type == typeof(DateTime))
            return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");

        if (value is IEnumerable enumerable && !(value is string))
        {
            int count = 0;
            if (value is ICollection list)
            {
                count = list.Count;
            }
            else
            {
                foreach (var item in enumerable) count++;
            }

            return $"集合[{type.Name}][{count}]";
        }

        //如果不是集合就是对象

        var title = $"对象[{type.Name}]";

        if (value is UndertaleString undertaleString)
        {
            title += undertaleString.Content;
        }

        if (value is UndertaleNamedResource undertaleNamedResource)
        {
            title += undertaleNamedResource.Name.Content;
        }

        return title;
    }

    /// <summary>
    /// 判断项目是否是属于不可展开类型的，从而判断是直接编辑还是跳到下一级树状图
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private bool IsExpandable(object value)
    {
        if (value == null) return false;

        var type = value.GetType();
        // 基本类型、字符串、日期等不可展开
        if (type.IsPrimitive || type == typeof(string) || type == typeof(DateTime))
            return false;
        // 枚举不可展开
        if (type.IsEnum)
            return false;
        // 集合也可以展开
        return true;
    }

    private async void OnPropertySelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is PropertyItem property)
        {
            if (property.IsExpandable)
            {
                // 导航到选中的属性对象
                SetCurrentObject(property.OriginalValue, property.Name);
            }
            else
            {
                EditPropertyAsync(property);
            }
        }

        // 清除选择
        ((ListView)sender).SelectedItem = null;
    }

    private void OnBackButtonClicked(object sender, EventArgs e)
    {
        if (_objectStack.Count > 1)
        {
            // 弹出当前对象
            _objectStack.Pop();
            _nameStack.Pop();

            // 获取上一个对象
            var previousObject = _objectStack.Peek();
            var previousName = _nameStack.Peek();

            _objectStack.Pop();
            _nameStack.Pop();

            // 更新UI
            SetCurrentObject(previousObject, previousName);
        }
    }

    #endregion

    #region ===== 不可展开项目编辑视窗 =====

    /// <summary>
    /// 弹出编辑不可展开项目的UI
    /// </summary>
    private async Task EditPropertyAsync(PropertyItem item)
    {
        if (item == null) return;

        // 基本类型、字符串、DateTime 才允许编辑，可自行扩展
        var type = item.OriginalValue?.GetType();
        if (type == null ||
            (!type.IsPrimitive && type != typeof(string) && type != typeof(DateTime)))
        {
            await DisplayAlert("提示", "该属性不支持编辑", "OK");
            return;
        }

        // 弹窗输入
        string newText = await DisplayPromptAsync(
            $"编辑 {item.Name}",
            "请输入新的值：",
            initialValue: item.OriginalValue?.ToString() ?? string.Empty,
            keyboard: Keyboard.Text);

        if (newText == null) return; // 用户取消

        // 类型转换
        object newValue = ConvertStringToTargetType(newText, type);

        try
        {
            // 把值写回到原对象
            WriteValueBackToInstance(currentObject, item.Name, newValue);
            // 刷新 UI
            RefreshListUI();
        }
        catch (Exception ex)
        {
            await DisplayAlert("错误", $"写入失败：{ex.Message}", "OK");
        }
    }

    /// <summary>
    /// 刷新当前层级列表的UI
    /// </summary>
    /// <param name="filter">过滤器(模糊搜索的关键词,显示全部设为null)</param>
    private void RefreshListUI(string? filter = null)
    {
        // 获取当前对象
        var previousObject = _objectStack.Peek();
        var previousName = _nameStack.Peek();
        // 弹出当前对象
        _objectStack.Pop();
        _nameStack.Pop();
        // 更新UI
        SetCurrentObject(previousObject, previousName, filter);
    }

    /// <summary>
    /// 把字符串转换成目标类型
    /// </summary>
    private static object ConvertStringToTargetType(string text, Type targetType)
    {
        if (targetType == typeof(string)) return text;
        if (targetType == typeof(DateTime) && DateTime.TryParse(text, out var dt)) return dt;
        if (targetType == typeof(bool)) return bool.Parse(text);
        return Convert.ChangeType(text, targetType);
    }

    /// <summary>
    /// 把值写回到实例的属性/索引器
    /// </summary>
    private static void WriteValueBackToInstance(object instance, string propertyOrIndex, object value)
    {
        if (instance is IDictionary dict && propertyOrIndex.StartsWith("["))
        {
            // 处理字典 / ExpandoObject 之类
            var key = propertyOrIndex.Trim('[', ']');
            dict[key] = value;
            return;
        }

        if (instance is IList list && int.TryParse(propertyOrIndex.Trim('[', ']'), out int index))
        {
            // 处理 IList
            list[index] = value;
            return;
        }

        // 普通对象属性
        var pd = TypeDescriptor.GetProperties(instance).Find(propertyOrIndex, false);
        if (pd == null) throw new InvalidOperationException($"找不到属性：{propertyOrIndex}");
        pd.SetValue(instance, value);
    }

    #endregion

    private void SearchBar_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        RefreshListUI(e.NewTextValue);
    }

    private async void TreeOrder_OnClicked(object? sender, EventArgs e)
    {
        var currentObject = _objectStack.Peek();
        var currentName = _nameStack.Peek();
        const String optionCreateObject = "新增对象";
        var list = new[]
        {
            optionCreateObject
        };
        string action = await DisplayActionSheet($"操作 {currentName}", "取消", null, list);
        if (action == null) return;
        switch (action)
        {
            case optionCreateObject:
                if (currentObject is IList currentList)
                {
                    try
                    {
                        DynamicListHelper.AddDynamicItem(currentList);
                        RefreshListUI();
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("添加对象错误", ex.Message, "OK");
                    }
                }

                break;
        }
    }

    private void ViewableEditor_OnClicked(object? sender, EventArgs e)
    {
        var currentObject = _objectStack.Peek();
        var currentName = _nameStack.Peek();
    }

    private async void MenuItem_AsUnExpandedObjectEdit_OnClicked(object? sender, EventArgs e)
    {
        MenuItem item = sender as MenuItem;
        var contextItem = item.BindingContext;
        if (contextItem is PropertyItem property)
        {
            await EditPropertyAsync(property);
        }
        else
        {
            await DisplayAlert("无法作为非展开对象编辑", "contextItem is not PropertyItem", "OK");
        }
    }
}