using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Parses text commands typed into the terminal InputField and executes
/// the corresponding action (loading a stage, printing help, clearing output).
/// This component owns only Gameplay/UI logic and never touches the Physics Layer.
/// </summary>
public class TerminalManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text outputText;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private ScrollRect outputScrollRect;

    private const string HeaderDivider = "==================================";
    private const string PromptSymbol = "> ";

    // How close to the bottom (0~1 normalized) the user must already be for
    // new content to auto-scroll further down. Prevents forcing the view
    // back down when the user has manually scrolled up to read past output.
    private const float AutoScrollThreshold = 0.05f;

    // Placeholder for future GameProgress integration.
    // Currently fixed; will be set by GameProgress once that system exists.
    private string currentStage = "Stage01";

    private readonly Dictionary<string, Action<string[]>> commandHandlers =
        new Dictionary<string, Action<string[]>>(StringComparer.OrdinalIgnoreCase);

    private void Awake()
    {
        RegisterCommands();
    }

    private void Start()
    {
        // ASSUMPTION 1: inputField.lineType must be SingleLine or MultiLineSubmit.
        // If lineType is MultiLineNewline, Enter only inserts a line break and
        // onSubmit is never invoked at all — this component would silently stop
        // executing commands. Verify this in the Inspector, not just in code.
        //
        // ASSUMPTION 2: If the project uses the new Input System UI Input Module,
        // the default "Submit" action may also be bound to a gamepad button
        // (commonly the South button), not just Keyboard Enter. Unlike the previous
        // Input.GetKeyDown(KeyCode.Return) check, onSubmit does not distinguish the
        // input source — a gamepad Submit press will also trigger command execution.
        // If keyboard-only submission is required, filter by checking the device
        // in the active InputAction callback context instead of relying on onSubmit alone.
        inputField.onSubmit.AddListener(OnInputSubmit);

        PrintWelcomeBanner();
        ScrollToBottom();
        RefocusInputField();
    }

    private void OnDestroy()
    {
        inputField.onSubmit.RemoveListener(OnInputSubmit);
    }

    private void RegisterCommands()
    {
        commandHandlers["/start"] = HandleStart;
        commandHandlers["/help"] = HandleHelp;
        commandHandlers["/clear"] = HandleClear;
        commandHandlers["/exit"] = HandleExit;
    }

    private void OnInputSubmit(string rawInput)
    {
        // onSubmit only fires when the Submit action is actually performed
        // (Enter / Numpad Enter by default, see ASSUMPTION 2 above), unlike
        // onEndEdit which also fires on focus loss from a mouse click elsewhere.
        // No manual key-state check is required here anymore.
        if (string.IsNullOrWhiteSpace(rawInput))
        {
            RefocusInputField();
            return;
        }

        bool wasNearBottom = IsScrollNearBottom();

        AppendLine(PromptSymbol + rawInput);
        ExecuteCommand(rawInput);

        inputField.text = string.Empty;
        AppendLine(PromptSymbol);

        // Only force the view down if the user was already reading recent
        // output. If they scrolled up to review earlier lines, respect that
        // and let new output accumulate below without moving their view.
        if (wasNearBottom)
        {
            ScrollToBottom();
        }

        RefocusInputField();
    }

    private void ExecuteCommand(string rawInput)
    {
        string[] tokens = rawInput.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            return;
        }

        string command = tokens[0];

        if (commandHandlers.TryGetValue(command, out Action<string[]> handler))
        {
            handler.Invoke(Array.Empty<string>());
        }
        else
        {
            AppendLine("Unknown command.");
            AppendLine("Enter '/help' to view the list of commands.");
        }
    }

    private void HandleStart(string[] args)
    { 
        GameSessionTimer.ResetTimer();
        // NOTE: "/start stage02" is not supported yet — by design (YAGNI).
        // currentStage is fixed for now and will be updated by GameProgress
        // once stage-unlock tracking exists.
        AppendLine($"Loading {currentStage}...");
        LoadStage(currentStage);
    }

    private void HandleHelp(string[] args)
    {
        AppendLine("Available Commands");
        AppendLine("/start");
        AppendLine("/help");
        AppendLine("/clear");
        AppendLine("/exit");
    }

    private void HandleClear(string[] args)
    {
        outputText.text = string.Empty;
        PrintClearedBanner();
        ScrollToBottom();
    }

    private void HandleExit(string[] args)
    {
        AppendLine("Closing terminal...");
            
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    private void LoadStage(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    private void PrintWelcomeBanner()
    {
        AppendLine(HeaderDivider);
        AppendLine("SWING-BY TERMINAL v0.1");
        AppendLine(string.Empty);
        AppendLine("Available Commands");
        AppendLine("/start");
        AppendLine("/help");
        AppendLine("/exit");
        AppendLine(HeaderDivider);
        //AppendLine(PromptSymbol);
    }

    private void PrintClearedBanner()
    {
        // NOTE: Trailing prompt is intentionally NOT printed here.
        // HandleClear is always invoked via OnInputSubmit, which appends
        // the trailing prompt itself after ExecuteCommand returns.
        // Printing it here as well caused a duplicate "> " line (fixed).
        AppendLine(HeaderDivider);
        AppendLine("SWING-BY TERMINAL");
    }

    private void AppendLine(string line)
    {
        outputText.text += $"{line}\n";
    }

    private bool IsScrollNearBottom()
    {
        // verticalNormalizedPosition: 0 = bottom, 1 = top.
        return outputScrollRect.verticalNormalizedPosition <= AutoScrollThreshold;
    }

    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();

        LayoutRebuilder.ForceRebuildLayoutImmediate(outputScrollRect.content);

        outputScrollRect.verticalNormalizedPosition = 0f;
    }

    private void RefocusInputField()
    {
        inputField.ActivateInputField();
        inputField.Select();
    }
}