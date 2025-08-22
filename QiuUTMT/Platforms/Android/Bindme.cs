using Com.Kongzue.Dialogx.Dialogs;
using Com.Kongzue.Dialogx.Interfaces;
using MauiBinding.Android.DialogX.Additions;
using Object = Java.Lang.Object;
using View = Android.Views.View;

namespace QiuUTMT;

public class Bindme
{
    public static async Task<bool> dAskDialog(string title, string message)
    {
        var tcs = new TaskCompletionSource<bool>();
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var msgbox = MessageDialog.Show(title,message, "是", "否")
                .SetCancelable(false)
                .SetOkButton(new ProxyOnDialogButtonClickListener(view =>
                {
                    tcs.SetResult(true);
                }))
                .SetCancelButton(new ProxyOnDialogButtonClickListener(View =>
                {
                    tcs.SetResult(false);
                }));
        });
        return await tcs.Task;
    }
    public static async Task<string> dInputDialog(string title, string message)
    {
        var tcs = new TaskCompletionSource<string>();
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var msgbox = InputDialog.Show(title, message, "是")
                .SetOkButton(new ProxyInputOkClickListener((view, result) =>
                {
                    tcs.SetResult(result);
                }));
            msgbox.SetCancelable(false);
        });
        return await tcs.Task;
    }
    
    public static async Task<string> dMsgDialog(string title, string message)
    {
        var tcs = new TaskCompletionSource<string>();
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var msgbox = MessageDialog.Show(title, message, "好喵");
        });
        return await tcs.Task;
    }
    public class ProxyOnDialogButtonClickListener : Java.Lang.Object, IOnDialogButtonClickListener
    {
        public delegate void OnDialogButtonClick(View view);
        public  OnDialogButtonClick Callback { get; set; }

        public ProxyOnDialogButtonClickListener(OnDialogButtonClick  callback)
        {
            this.Callback=callback;
        }
        public bool OnClick(Object? p0, View? p1)
        {
            Callback(p1);
            return false;
        }
    }
    public class ProxyInputOkClickListener : Java.Lang.Object, IOnInputDialogButtonClickListener
    {
        public delegate void OnDialogButtonClick(View view,string result);
        public  OnDialogButtonClick Callback { get; set; }

        public ProxyInputOkClickListener(OnDialogButtonClick  callback)
        {
            this.Callback=callback;
        }

        public bool OnClick(Object? p0, View? p1, string? p2)
        {
            Callback(p1, p2);
            return false;
        }
    }
}