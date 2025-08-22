using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UTMTdrid;

namespace QiuUTMT;

public partial class DetailInfoInternative : ContentPage
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
    public DetailInfoInternative()
    {
        InitializeComponent();
        BindingContext = this;
        var sampleObject = CreateSampleObject();
        SetCurrentObject(QiuFuncMainSingle.QiuFuncMain, "GM数据对象");
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
        _objectStack.Push(obj);
        _nameStack.Push(name);

        CurrentObjectName = name;
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
            return $"[集合: {count} 项]";
        }

        return $"[对象: {type.Name}]";
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

    private object CreateSampleObject()
    {
        // 创建示例对象
        return new
        {
            Name = "示例对象",
            Value = 42,
            Created = DateTime.Now,
            Nested = new
            {
                Title = "嵌套对象",
                Items = new[] { "项目1", "项目2", "项目3" },
                Metadata = new
                {
                    Author = "管理员",
                    Version = "1.0"
                }
            },
            Collection = new List<object>
            {
                new { Id = 1, Name = "列表项1" },
                new { Id = 2, Name = "列表项2" },
                new { Id = 3, Name = "列表项3" }
            }
        };
    }
}