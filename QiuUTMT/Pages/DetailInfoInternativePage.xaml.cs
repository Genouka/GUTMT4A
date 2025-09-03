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

public partial class DetailInfoInternativePage : ContentPage
{
    public class PropertyItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public object OriginalValue { get; set; }
        public bool IsExpandable { get; set; }
        public int IndentLevel { get; set; }
        public bool IsExpanded { get; set; }
    }

    public DetailInfoInternativePage()
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

    public ObservableCollection<PropertyItem> Properties { get; } = new ObservableCollection<PropertyItem>();

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

    private void SetCurrentObject(object obj, string name)
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

        // 获取对象属性
        if (obj is IEnumerable enumerable && obj is not string)
        {
            int idx = 0;
            foreach (var item in enumerable)
            {
                string title = $"[{idx}]";
                string valuePreview = GetValuePreview(item);
                bool isExpandable = IsExpandable(item);

                Properties.Add(new PropertyItem
                {
                    Name = title,
                    Value = valuePreview,
                    OriginalValue = item,
                    IsExpandable = isExpandable,
                    IndentLevel = 0
                });
                idx++;
            }
        }
        else
        {
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
            {
                try
                {
                    object value = descriptor.GetValue(obj);
                    string valuePreview = GetValuePreview(value);
                    bool isExpandable = IsExpandable(value);

                    Properties.Add(new PropertyItem
                    {
                        Name = descriptor.Name,
                        Value = valuePreview,
                        OriginalValue = value,
                        IsExpandable = isExpandable,
                        IndentLevel = 0
                    });
                }
                catch (Exception ex)
                {
                    Properties.Add(new PropertyItem
                    {
                        Name = descriptor.Name,
                        Value = $"<错误: {ex.Message}>",
                        IsExpandable = false,
                        IndentLevel = 0
                    });
                }
            }
        }

        StatusMessage = $"已加载 {Properties.Count} 个属性";
    }

    private string GetValuePreview(object value)
    {
        if (value == null) return "null";

        var type = value.GetType();
        if (type.IsPrimitive || type == typeof(string))
            return value.ToString();

        if (type == typeof(DateTime))
            return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");

        if (value is IEnumerable enumerable && !(value is string))
        {
            int count = 0;
            foreach (var item in enumerable) count++;
            return $"集合[{count}]";
        }

        //如果不是集合就是对象

        var title = $"对象[{type.Name}]";

        if (value is UndertaleString)
        {
            title += ((UndertaleString)value).Content;
        }

        if (value is UndertaleNamedResource)
        {
            title += ((UndertaleNamedResource)value).Name.Content;
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

        // 集合也可以展开
        return true;
    }

    private async void OnPropertySelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is PropertyItem property && property.IsExpandable)
        {
            // 导航到选中的属性对象
            SetCurrentObject(property.OriginalValue, property.Name);
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
            maxLength: 200,
            keyboard: type == typeof(string) ? Keyboard.Text : Keyboard.Numeric);

        if (newText == null) return; // 用户取消

        // 类型转换
        object newValue = ConvertStringToTargetType(newText, type);

        try
        {
            // 把值写回到原对象
            WriteValueBackToInstance(currentObject, item.Name, newValue);
            // 刷新 UI
            SetCurrentObject(currentObject, CurrentObjectName);
        }
        catch (Exception ex)
        {
            await DisplayAlert("错误", $"写入失败：{ex.Message}", "OK");
        }
    }

    /// <summary>
    /// 把字符串转换成目标类型
    /// </summary>
    private static object ConvertStringToTargetType(string text, Type targetType)
    {
        if (targetType == typeof(string)) return text;
        if (targetType == typeof(DateTime) && DateTime.TryParse(text, out var dt)) return dt;
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
}