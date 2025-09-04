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

    public DataTreePage()
    {
        InitializeComponent();
        BindingContext = this;
        SetCurrentObject(QiuFuncMainSingle.QiuFuncMain.Data, "GM数据对象");
    }

    private object currentObject;
    private string _currentObjectName = "根对象";
    private Stack<object> _objectStack = new Stack<object>();
    private Stack<string> _nameStack = new Stack<string>();
    private string _statusMessage = "就绪";
    private bool _canGoBack = false;

    public RangeObservableCollection<PropertyItem> Properties { get; } = new();

    public string CurrentObjectName
    {
        get => _currentObjectName;
        set
        {
            _currentObjectName = value;
            LabelObjectName.Text = value;
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

    private void SetCurrentObject(object obj, string name,string? filter=null)
    {
        currentObject = obj;
        var previousName = _nameStack.Count <= 0 ? "" : (_nameStack.Peek() + " > ");
        _objectStack.Push(obj);
        _nameStack.Push(name);

        CurrentObjectName = previousName + name;
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
            int idx = 0;
            foreach (var item in enumerable)
            {
                string title = $"[{idx}]";
                string valuePreview = GetValuePreview(item);
                bool isExpandable = IsExpandable(item);
                if (!string.IsNullOrEmpty(filter)&&!FuzzyMatch.IsMatch(title+valuePreview,filter)) continue;
                propertiesList.Add(new PropertyItem
                {
                    Name = title,
                    Value = valuePreview,
                    OriginalValue = item,
                    IsExpandable = isExpandable,
                    IndentLevel = 0
                });
                idx++;
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
                    string valuePreview = GetValuePreview(value,type);
                    bool isExpandable = IsExpandable(value);
                    if (!string.IsNullOrEmpty(filter)&&!FuzzyMatch.IsMatch(descriptor.Name+valuePreview,filter)) continue;
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
                    if (!string.IsNullOrEmpty(filter)&&!FuzzyMatch.IsMatch(descriptor.Name,filter)) continue;
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

    private string GetValuePreview(object? value,Type? originalType=null)
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

    #region ===== 编辑 =====

    /// <summary>
    /// 供 UI 调用的入口。
    /// 在 XAML 里给显示 Value 的控件加一个 TapGestureRecognizer，
    /// CommandParameter="{Binding .}"，然后把 Command 指到这里即可。
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

    private void RefreshListUI(string? filter=null)
    {
        // 获取当前对象
        var previousObject = _objectStack.Peek();
        var previousName = _nameStack.Peek();
        // 弹出当前对象
        _objectStack.Pop();
        _nameStack.Pop();
        // 更新UI
        SetCurrentObject(previousObject, previousName,filter);
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
}