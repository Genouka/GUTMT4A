﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Underanalyzer.Decompiler;
using UndertaleModLib;
using UndertaleModLib.Compiler;
using UndertaleModLib.Decompiler;
using UndertaleModLib.Models;
using UndertaleModLib.Scripting;
namespace UTMTdrid;

// Everything that gets inherited (methods, attributes) from IScriptInterface gets put here
// in order to have the inherited stuff separated from normal stuff.

public partial class CliMain : IScriptInterface
{
    #region Inherited UMTLib Properties

    /// <inheritdoc/>
    public UndertaleData Data { get; set; }

    /// <inheritdoc/>
    public string FilePath { get; set; }

    /// <inheritdoc/>
    public string ScriptPath { get; set; }

    /// <inheritdoc/>
    public object Highlighted { get; set; }

    /// <inheritdoc/>
    public object Selected { get; set; }

    /// <inheritdoc/>
    public bool CanSave { get; set; }

    /// <inheritdoc/>
    public bool ScriptExecutionSuccess { get; set; }

    /// <inheritdoc/>
    public string ScriptErrorMessage { get; set; }

    /// <inheritdoc/>
    public string ExePath { get; set; }

    /// <inheritdoc/>
    public string ScriptErrorType { get; set; }

    /// <inheritdoc/>
    public bool IsAppClosed { get; set; }

    #endregion

    #region Inherited UMTLib Methods

    /// <inheritdoc/>
    public void EnsureDataLoaded()
    {
        if (Data is null)
            throw new ScriptException("No data file is currently loaded!");
    }

    /// <inheritdoc/>
    public bool MakeNewDataFile()
    {
        Data = UndertaleData.CreateNew();
        GenoukaUI_WriteLine("New file created.");
        return true;
    }

    /// <inheritdoc/>
    public void ScriptMessage(string message)
    {
        GenoukaUI_WriteLine(message);
        if (IsInteractive) Pause();
    }

    /// <inheritdoc/>
    public void ScriptWarning(string message)
    {
        GenoukaUI_WriteLine($"WARNING: {message}");
        if (IsInteractive) Pause();
    }

    /// <inheritdoc/>
    public void SetUMTConsoleText(string message)
    {
        // Since the UMTConsole text messages are literally just messages that are shown,
        // I'm giving them the same behaviour as the normal ScriptMessage
        ScriptMessage(message);
    }

    /// <inheritdoc/>
    public bool ScriptQuestion(string message)
    {
        GenoukaUI_Write($"{message} (Y/N) ");
        bool isInputYes = MAUI_Page.DisplayAlert("Question?", "Would you like to play a game", "Yes", "No").Result;
        //bool isInputYes = Console.ReadKey(false).Key == ConsoleKey.Y;
        GenoukaUI_WriteLine("");
        return isInputYes;
    }

    /// <inheritdoc/>
    public void ScriptError(string error, string title = "Error", bool setConsoleText = true)
    {
        // No need to care about setConsoleText if we're in CLI.

        GenoukaUI_WriteLine("[err]--------------------------------------------------");
        GenoukaUI_WriteLine("[err]----------------------ERROR!----------------------");
        GenoukaUI_WriteLine("[err]--------------------------------------------------");
        GenoukaUI_WriteLine("[err]"+title);
        GenoukaUI_WriteLine("[err]--------------------------------------------------");
        GenoukaUI_WriteLine("[err]"+error);
        GenoukaUI_WriteLine("[err]--------------------------------------------------");
        GenoukaUI_WriteLine("[err]----------------------ERROR!----------------------");
        GenoukaUI_WriteLine("[err]--------------------------------------------------");
        if (IsInteractive) { Pause(); }
    }

    /// <inheritdoc/>
    public void SimpleTextOutput(string title, string label, string defaultText, bool allowMultiline)
    {
        // In order to be similar to GUI output, we strip everything past a newline in "defaultValue" should multiline be disabled
        if (!allowMultiline)
            defaultText = defaultText.Remove(defaultText.IndexOf('\n'));

        GenoukaUI_WriteLine("----------------------OUTPUT----------------------");
        GenoukaUI_WriteLine(title);
        GenoukaUI_WriteLine(label);
        GenoukaUI_WriteLine("--------------------------------------------------");
        GenoukaUI_WriteLine(defaultText);
        GenoukaUI_WriteLine("--------------------------------------------------");
        GenoukaUI_WriteLine("----------------------OUTPUT----------------------");
        GenoukaUI_WriteLine("--------------------------------------------------");

        if (IsInteractive) Pause();
    }

    /// <inheritdoc/>
    public void ScriptOpenURL(string url)
    {
        Process p;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            //TODO: why useShellExecute on Windows, but not on the other OS?
            p = Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); // Works ok on windows
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            p = Process.Start("xdg-open", url);  // Works ok on linux, should work on FreeBSD as it's very similar.
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            p = Process.Start("open", url); // Not tested
        }
        else
        {
            throw new InvalidOperationException("Unable to open the browser on this OS: " + RuntimeInformation.OSDescription);
        }

        p?.Dispose();
    }

    /// <inheritdoc/>
    public bool RunUMTScript(string path)
    {
        try
        {
            RunCSharpFile(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public bool AreFilesIdentical(string file1, string file2)
    {
        using FileStream fs1 = new FileStream(file1, FileMode.Open, FileAccess.Read, FileShare.Read);
        using FileStream fs2 = new FileStream(file2, FileMode.Open, FileAccess.Read, FileShare.Read);

        if (fs1.Length != fs2.Length) return false; // different size, files can't be the same

        while (true)
        {
            int b1 = fs1.ReadByte();
            int b2 = fs2.ReadByte();
            if (b1 != b2) return false; // different contents, files are not the same
            if (b1 == -1) break; // here both bytes are the same. Thus we only need to check if one is at end-of-file.
        }

        // identical
        return true;
    }

    /// <inheritdoc/>
    public void SetFinishedMessage(bool isFinishedMessageEnabled)
    {
        FinishedMessageEnabled = isFinishedMessageEnabled;
    }

    /// <inheritdoc/>
    public void UpdateProgressBar(string message, string status, double currentValue, double maxValue)
    {
        string evaluatedMessage = !String.IsNullOrEmpty(message) ? $"{message}|" : "";
        GenoukaUI_WriteLine($"[{evaluatedMessage}{status}] {currentValue} out of {maxValue}");
    }

    /// <inheritdoc/>
    public void SetProgressBar(string message, string status, double currentValue, double maxValue)
    {
        savedMsg = message;
        savedStatus = status;
        savedValue = currentValue;
        savedValueMax = maxValue;

        UpdateProgressBar(message, status, currentValue, maxValue);
    }

    /// <inheritdoc/>
    public void UpdateProgressValue(double currentValue)
    {
        UpdateProgressBar(savedMsg, savedStatus, currentValue, savedValueMax);

        savedValue = currentValue;
    }

    /// <inheritdoc/>
    public void UpdateProgressStatus(string status)
    {
        UpdateProgressBar(savedMsg, status, savedValue, savedValueMax);

        savedStatus = status;
    }

    /// <inheritdoc/>
    public void AddProgress(int amount)
    {
        progressValue += amount;
    }

    /// <inheritdoc/>
    public void IncrementProgress()
    {
        progressValue++;
    }

    /// <inheritdoc/>
    public void AddProgressParallel(int amount) //P - Parallel (multi-threaded)
    {
        Interlocked.Add(ref progressValue, amount); //thread-safe add operation (not the same as "lock ()")
    }

    /// <inheritdoc/>
    public void IncrementProgressParallel()
    {
        Interlocked.Increment(ref progressValue); //thread-safe increment
    }

    /// <inheritdoc/>
    public int GetProgress()
    {
        return progressValue;
    }

    /// <inheritdoc/>
    public void SetProgress(int value)
    {
        progressValue = value;
    }


    #region Empty Inherited Methods

    /// <inheritdoc/>
    public void InitializeScriptDialog()
    {
        // CLI has no dialogs to initialize
    }

    /// <inheritdoc/>
    public void SetProgressBar()
    {
        //no progress bar that can be setup to show
    }

    /// <inheritdoc/>
    public void HideProgressBar()
    {
        // nothing to hide..
    }

    /// <inheritdoc/>
    public void EnableUI()
    {
        // nothing to enable...
    }

    /// <inheritdoc/>
    public void SyncBinding(string resourceType, bool enable)
    {
        //there is no UI with any data binding
    }

    /// <inheritdoc/>
    public void DisableAllSyncBindings()
    {
        //there is no UI with any data binding
    }

    #endregion

    /// <inheritdoc/>
    public void StartProgressBarUpdater()
    {
        if (cTokenSource is not null)
            GenoukaUI_WriteLine("Warning - there is another progress updater task running (hangs) in the background.");

        cTokenSource = new CancellationTokenSource();
        cToken = cTokenSource.Token;

        updater = Task.Run(ProgressUpdater);
    }

    /// <inheritdoc/>
    public async Task StopProgressBarUpdater() //"async" because "Wait()" blocks UI thread
    {
        if (cTokenSource is null) return;


        cTokenSource.Cancel();

        if (await Task.Run(() => !updater.Wait(2000))) //if ProgressUpdater isn't responding
            GenoukaUI_WriteLine("Error - stopping the progress updater task is failed.");
        else
        {
            cTokenSource.Dispose();
            cTokenSource = null;
        }

        updater.Dispose();
    }

    /// <inheritdoc/>
    public void ChangeSelection(object newSelection, bool inNewTab = false)
    {
        //this does *not* make sense, as CLI does not have any selections
        //however, since Selection is a public object, it could potentially be used by scripts
        Selected = newSelection;
    }

    /// <inheritdoc/>
    public string PromptChooseDirectory()
    {
        //var result = MAUIBridge.PickAndShow(PickOptions.Default).Result;
        // var dir = new FileInfo(FilePath).DirectoryName + "/worker" + DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        // try
        // {
        //     new DirectoryInfo(dir).Create();
        // }
        // catch (Exception e)
        // {
        //     
        // }
        // return dir;
        string path;
        DirectoryInfo directoryInfo;
        do
        {
            GenoukaUI_WriteLine("Please enter a path (or drag and drop) to a valid directory:");
            GenoukaUI_Write("Path: ");
            path = RemoveQuotes(MAUIBridge.PickFolder(CancellationToken.None).Result);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            directoryInfo = new DirectoryInfo(path);
        }
        while (!directoryInfo.Exists);
        
        return path;
    }

    /// <inheritdoc/>
    public string PromptLoadFile(string defaultExt, string filter)
    {
        string path;
        FileInfo fileInfo;
        do
        {
            GenoukaUI_WriteLine("Please enter a path (or drag and drop) to a valid file:");
            GenoukaUI_Write("Path: ");
            path = RemoveQuotes(MAUIBridge.PickAndShow(PickOptions.Default).Result.FullPath);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            fileInfo = new FileInfo(path);
        }
        while (!fileInfo.Exists);

        return path;
    }

    /// <inheritdoc/>
    public string PromptSaveFile(string defaultExt, string filter)
    {
        string path;
        do
        {
            GenoukaUI_WriteLine("Please enter a path (or drag and drop) to save the file:");
            GenoukaUI_Write("Path: ");
            string? dir=MAUIBridge.PickFolder(CancellationToken.None).Result;
            if (dir is null)
            {
                dir = "/sdcard/";
            }
            string result;
            if (MAUI_Page is null)
            {
                result = "save.bin";
            }
            else
            {
                result = MAUI_Page.DisplayPromptAsync("保存到文件", "选择的路径为"+dir+"\n输入文件名").Result;
            }
            path = RemoveQuotes(dir+result);

            if (Directory.Exists(path))
            {
                GenoukaUI_WriteLine("Error: Directory exists at that path.");
                path = null; // Ensuring that the loop will work correctly
                throw new Exception("输入的文件已经存在，请另选路径");
            }
        }
        while (string.IsNullOrWhiteSpace(path));

        return path;
    }

    /// <inheritdoc/>
    public string GetDecompiledText(string codeName, GlobalDecompileContext context = null, IDecompileSettings settings = null)
    {
        return GetDecompiledText(Data.Code.ByName(codeName), context, settings);
    }

    /// <inheritdoc/>
    public string GetDecompiledText(UndertaleCode code, GlobalDecompileContext context = null, IDecompileSettings settings = null)
    {
        if (code.ParentEntry is not null)
            return $"// This code entry is a reference to an anonymous function within \"{code.ParentEntry.Name.Content}\", decompile that instead.";

        GlobalDecompileContext decompileContext = context is null ? new(Data) : context;
        try
        {
            return code != null
                ? new DecompileContext(decompileContext, code, settings ?? Data.ToolInfo.DecompilerSettings).DecompileToString()
                : "";
        }
        catch (Exception e)
        {
            return "/*\nDECOMPILER FAILED!\n\n" + e + "\n*/";
        }
    }

    /// <inheritdoc/>
    public string GetDisassemblyText(string codeName)
    {
        return GetDisassemblyText(Data.Code.ByName(codeName));
    }

    /// <inheritdoc/>
    public string GetDisassemblyText(UndertaleCode code)
    {
        if (code.ParentEntry is not null)
            return $"; This code entry is a reference to an anonymous function within \"{code.ParentEntry.Name.Content}\", disassemble that instead.";

        try
        {
            return code != null ? code.Disassemble(Data.Variables, Data.CodeLocals?.For(code), Data.CodeLocals is null) : "";
        }
        catch (Exception e)
        {
            return "/*\nDISASSEMBLY FAILED!\n\n" + e + "\n*/"; // Please don't
        }
    }

    /// <inheritdoc/>
    public string ScriptInputDialog(string titleText, string labelText, string defaultInputBoxText, string cancelButtonText, string submitButtonText, bool isMultiline, bool preventClose)
    {
        // I'll ignore the cancelButtonText and submitButtonText as they don't have much use.
        return SimpleTextInput(titleText, labelText, defaultInputBoxText, isMultiline, preventClose);
    }

    /// <inheritdoc/>
    public string SimpleTextInput(string title, string label, string defaultValue, bool allowMultiline, bool showDialog = true)
    {
        if (MAUI_Page != null)
        {
            return MAUI_Page.DisplayPromptAsync(title, label,initialValue:defaultValue).Result;
        }
        // default value gets ignored, as it doesn't really have a use in CLI.
        throw new NotImplementedException("Not yet implemented");
        string result = "";

        GenoukaUI_WriteLine("-----------------------INPUT----------------------");
        GenoukaUI_WriteLine(title);
        GenoukaUI_WriteLine(label + (allowMultiline ? " (Multiline, hit SHIFT+ENTER to insert newline)" : ""));
        GenoukaUI_WriteLine("--------------------------------------------------");

        if (!allowMultiline)
        {
            result = Console.ReadLine();
        }
        else
        {
            bool isEnterWithoutShiftPressed = false;
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey();
                //result += keyInfo.KeyChar;

                // If Enter is pressed without shift
                if (((keyInfo.Modifiers & ConsoleModifiers.Shift) == 0) && (keyInfo.Key == ConsoleKey.Enter))
                    isEnterWithoutShiftPressed = true;

                else
                {
                    // If we have Enter + any other modifier pressed, append newline. Otherwise, just the content.
                    if (keyInfo.Key == ConsoleKey.Enter)
                    {
                        result += "\n";
                        GenoukaUI_WriteLine("");
                    }
                    // If backspace, display new empty char and move one back
                    // TODO: There's some weird bug with ctrl+backspace, i'll ignore it for now.
                    // Also make some of the multiline-backspace better.
                    else if ((keyInfo.Key == ConsoleKey.Backspace) && (result.Length > 0))
                    {
                        GenoukaUI_Write(" ");
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                        result = result.Remove(result.Length - 1);
                    }
                    else
                        result += keyInfo.KeyChar;
                }

            } while (!isEnterWithoutShiftPressed);
        }

        GenoukaUI_WriteLine("--------------------------------------------------");
        GenoukaUI_WriteLine("-----------------------INPUT----------------------");
        GenoukaUI_WriteLine("--------------------------------------------------");

        return result;
    }

    /// <inheritdoc/>
    public async Task ClickableSearchOutput(string title, string query, int resultsCount, IOrderedEnumerable<KeyValuePair<string, List<(int lineNum, string codeLine)>>> resultsDict, bool editorDecompile, IOrderedEnumerable<string> failedList = null)
    {
        await ClickableSearchOutput(title, query, resultsCount, resultsDict.ToDictionary(pair => pair.Key, pair => pair.Value), editorDecompile, failedList);
    }

    /// <inheritdoc/>
    public async Task ClickableSearchOutput(string title, string query, int resultsCount, IDictionary<string, List<(int lineNum, string codeLine)>> resultsDict, bool editorDecompile, IEnumerable<string> failedList = null)
    {
        await Task.Delay(1); //dummy await

        // If we have failed entries...
        if (failedList is not null)
        {
            // Convert list to array first
            string[] failedArray = failedList.ToArray();

            // ...Print them all out
            Console.ForegroundColor = ConsoleColor.Red;
            if (failedArray.Length == 1)
                GenoukaUI_WriteLine("[err]There is 1 code entry that encountered an error while searching:");
            else
                GenoukaUI_WriteLine("[err]"+$"There are {failedArray.Length} code entries that encountered an error while searching");

            foreach (string failedEntry in failedArray)
                GenoukaUI_WriteLine("[err]"+failedEntry);

            Console.ResetColor();
            GenoukaUI_WriteLine("");
        }

        GenoukaUI_WriteLine($"{resultsCount} results in {resultsDict.Count} code entries for \"{query}\".");
        GenoukaUI_WriteLine("");

        // Print in a pattern of:
        // Results in code_file
        // Line 3: line of code
        // Line 6: line of code
        //
        // Results in code_file_1
        // etc.
        foreach (var dictEntry in resultsDict)
        {
            GenoukaUI_WriteLine($"Results in {dictEntry.Key}:");
            foreach (var resultEntry in dictEntry.Value)
                GenoukaUI_WriteLine($"Line {resultEntry.lineNum}: {resultEntry.codeLine}");

            GenoukaUI_WriteLine("");
        }

        if (IsInteractive) Pause();
    }

    /// <inheritdoc/>
    public bool LintUMTScript(string path)
    {
        // By Grossley
        if (!File.Exists(path))
        {
            ScriptError(path + " does not exist!");
            return false;
        }
        try
        {
            CancellationTokenSource source = new CancellationTokenSource(100);
            CancellationToken token = source.Token;
            CSharpScript.EvaluateAsync(File.ReadAllText(path, Encoding.UTF8), CliScriptOptions.WithFilePath(path).WithFileEncoding(Encoding.UTF8), this, typeof(IScriptInterface), token);
        }
        catch (CompilationErrorException exc)
        {
            ScriptError(exc.Message, "Script compile error");
            ScriptExecutionSuccess = false;
            ScriptErrorMessage = exc.Message;
            ScriptErrorType = "CompilationErrorException";
            return false;
        }
        catch (Exception)
        {
            // Using the 100 MS timer it can time out before successfully running, compilation errors are fast enough to get through.
            ScriptExecutionSuccess = true;
            ScriptErrorMessage = "";
            ScriptErrorType = "";
            return true;
        }
        return true;
    }

    public void ReassignGUIDs(string guid, uint objectIndex)
    {
        int eventIdx = Convert.ToInt32(EventType.Collision);
        for (var i = 0; i < Data.GameObjects.Count; i++)
        {
            UndertaleGameObject obj = Data.GameObjects[i];
            try
            {
                foreach (UndertaleGameObject.Event evnt in obj.Events[eventIdx])
                {
                    foreach (UndertaleGameObject.EventAction action in evnt.Actions)
                    {
                        if (action.CodeId.Name.Content.Contains(guid))
                        {
                            evnt.EventSubtype = objectIndex;
                        }
                    }
                }
            }
            catch
            {
                // Silently ignore, some values can be null along the way
            }
        }
    }

    public uint ReduceCollisionValue(List<uint> possibleValues)
    {
        if (possibleValues.Count == 1)
        {
            if (possibleValues[0] != uint.MaxValue)
                return possibleValues[0];

            // Nothing found, pick new one
            bool objFound = false;
            uint objIndex = 0;
            while (!objFound)
            {
                string objectIndex = SimpleTextInput("Object could not be found. Please enter it below:",
                    "Object enter box.", "", false).ToLower();
                for (var i = 0; i < Data.GameObjects.Count; i++)
                {
                    if (Data.GameObjects[i].Name.Content.ToLower() == objectIndex)
                    {
                        objFound = true;
                        objIndex = (uint)i;
                    }
                }
            }
            return objIndex;
        }

        if (possibleValues.Count != 0)
        {
            // 2 or more possible values, make a list to choose from

            string gameObjectNames = "";
            foreach (uint objID in possibleValues)
                gameObjectNames += Data.GameObjects[(int)objID].Name.Content + "\n";

            bool objFound = false;
            uint objIndex = 0;
            while (!objFound)
            {
                string objectIndex = SimpleTextInput("Multiple objects were found. Select only one object below from the set, or, if none below match, some other object name:",
                    "Object enter box.", gameObjectNames, true).ToLower();
                for (var i = 0; i < Data.GameObjects.Count; i++)
                {
                    if (Data.GameObjects[i].Name.Content.ToLower() == objectIndex)
                    {
                        objFound = true;
                        objIndex = (uint)i;
                    }
                }
            }
            return objIndex;
        }

        return 0;
    }

    public List<uint> GetCollisionValueFromCodeNameGUID(string codeName)
    {
        int eventIdx = Convert.ToInt32(EventType.Collision);
        List<uint> possibleValues = new List<uint>();
        for (var i = 0; i < Data.GameObjects.Count; i++)
        {
            UndertaleGameObject obj = Data.GameObjects[i];
            try
            {
                foreach (UndertaleGameObject.Event evnt in obj.Events[eventIdx])
                {
                    foreach (UndertaleGameObject.EventAction action in evnt.Actions)
                    {
                        if (action.CodeId.Name.Content == codeName)
                        {
                            if (Data.GameObjects[(int)evnt.EventSubtype] != null)
                            {
                                possibleValues.Add(evnt.EventSubtype);
                                return possibleValues;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Silently ignore, some values can be null along the way
            }
        }
        possibleValues = GetCollisionValueFromGUID(GetGUIDFromCodeName(codeName));
        return possibleValues;
    }

    public List<uint> GetCollisionValueFromGUID(string guid)
    {
        int eventIdx = Convert.ToInt32(EventType.Collision);
        List<uint> possibleValues = new List<uint>();
        for (var i = 0; i < Data.GameObjects.Count; i++)
        {
            UndertaleGameObject obj = Data.GameObjects[i];
            try
            {
                foreach (UndertaleGameObject.Event evnt in obj.Events[eventIdx])
                {
                    foreach (UndertaleGameObject.EventAction action in evnt.Actions)
                    {
                        if (action.CodeId.Name.Content.Contains(guid))
                        {
                            if (!possibleValues.Contains(evnt.EventSubtype))
                            {
                                possibleValues.Add(evnt.EventSubtype);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Silently ignore, some values can be null along the way
            }
        }

        if (possibleValues.Count == 0)
        {
            possibleValues.Add(uint.MaxValue);
            return possibleValues;
        }
        else
        {
            return possibleValues;
        }
    }

    public string GetGUIDFromCodeName(string codeName)
    {
        string afterPrefix = codeName.Substring(11);
        if (afterPrefix.LastIndexOf("_Collision_") != -1)
        {
            string s2 = "_Collision_";
            return afterPrefix.Substring(afterPrefix.LastIndexOf("_Collision_") + s2.Length, afterPrefix.Length - (afterPrefix.LastIndexOf("_Collision_") + s2.Length));
        }
        else
            return "Invalid";
    }

    #endregion

}
