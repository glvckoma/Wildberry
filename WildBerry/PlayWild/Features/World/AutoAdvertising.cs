using System;
using System.Collections.Generic;
using MelonLoader;
using PlayWild.Features.Base;
using PlayWild.Interface;
using UnityEngine;

namespace PlayWild.Features.World
{
    public class AutoAdvertising : BaseFeature
    {
        public override string Name => "Auto Advertising";

        private bool isRunning = false;
        private int messageIndex = 0;
        private float lastTickTime = 0f;
        private float nextDelaySeconds = 60f;
        private bool pendingQuickRetry = false;

        private string messagesMultiline = string.Empty;
        private float intervalSeconds = 60f;

        private Vector2 messagesScroll = Vector2.zero;

        public override void Initialize()
        {
            base.Initialize();

            messagesMultiline = GetPersistedValue("messages", string.Empty) ?? string.Empty;
            intervalSeconds = Mathf.Max(10f, GetPersistedValue("intervalSeconds", 60f));
        }

        public override void OnDisable()
        {
            base.OnDisable();
            StopAdvertising();
        }

        public override void OnUpdate()
        {
            if (!IsEnabled || !isRunning)
                return;

            if (Time.time - lastTickTime >= nextDelaySeconds)
            {
                try
                {
                    SendNextMessage();
                }
                finally
                {
                    ScheduleNext();
                }
            }
        }

        public override void OnGUI(Rect area)
        {
            DrawToggle(new Rect(area.x, area.y, area.width, WildBerryTheme.ToggleHeight), IsEnabled, Name, v => IsEnabled = v);

            if (!IsEnabled)
                return;

            float y = area.y + 25f;

            DrawStatusLabel(new Rect(area.x + 5, y, 220, 20), isRunning ? "Status: Active" : "Status: Inactive",
                isRunning ? WildBerryTheme.StatusActive : WildBerryTheme.StatusText);
            y += 22f;

            if (DrawStyledButton(new Rect(area.x + 5, y, 70, WildBerryTheme.ButtonHeight), "Start"))
                StartAdvertising();
            if (DrawStyledButton(new Rect(area.x + 80, y, 70, WildBerryTheme.ButtonHeight), "Stop"))
                StopAdvertising();
            if (DrawStyledButton(new Rect(area.x + 155, y, 85, WildBerryTheme.ButtonHeight), "Preview"))
                PreviewNext();
            y += 32f;

            DrawLabel(new Rect(area.x + 5, y, 110, 20), "Interval (s)");
            string intStr = DrawStyledTextField(new Rect(area.x + 115, y, 60, WildBerryTheme.TextFieldHeight), Mathf.RoundToInt(intervalSeconds).ToString());
            if (float.TryParse(intStr, out float intVal))
            {
                if (Mathf.Abs(intVal - intervalSeconds) > Mathf.Epsilon)
                {
                    intervalSeconds = Mathf.Max(10f, intVal);
                    SetPersistedValue("intervalSeconds", intervalSeconds);
                }
            }
            y += 28f;

            DrawLabel(new Rect(area.x + 5, y, 200, 18), "Messages (one per line)");
            y += 20f;

            float textHeight = 90f;
            Rect scrollRect = new Rect(area.x + 5, y, area.width - 10, textHeight);
            Rect innerRect = new Rect(0, 0, scrollRect.width - 20, textHeight);

            messagesScroll = GUI.BeginScrollView(scrollRect, messagesScroll, new Rect(innerRect.x, innerRect.y, innerRect.width, innerRect.height));
            string newMultiline = DrawStyledTextArea(new Rect(0, 0, innerRect.width, innerRect.height), messagesMultiline);
            GUI.EndScrollView();

            if (!string.Equals(newMultiline, messagesMultiline, StringComparison.Ordinal))
            {
                messagesMultiline = newMultiline;
                SetPersistedValue("messages", messagesMultiline);
            }
        }

        public override float GetDynamicHeight()
        {
            if (!IsEnabled)
                return WildBerryTheme.ToggleHeight;

            return 25f + 22f + 32f + 28f + 20f + 90f + 10f;
        }

        private void StartAdvertising()
        {
            var list = GetMessages();
            if (list.Count == 0)
            {
                MelonLogger.Warning("[WildBerry] Auto Advertising: No messages to send.");
                return;
            }

            EnsureChatKeyboardOpen();
            messageIndex = 0;
            isRunning = true;
            ScheduleNext(initialImmediate: true);
            MelonLogger.Msg("[WildBerry] Auto Advertising: Started.");
        }

        private void StopAdvertising()
        {
            if (!isRunning) return;
            isRunning = false;
            MelonLogger.Msg("[WildBerry] Auto Advertising: Stopped.");
        }

        private void PreviewNext()
        {
            var list = GetMessages();
            if (list.Count == 0)
            {
                MelonLogger.Warning("[WildBerry] Auto Advertising: No messages to preview.");
                return;
            }

            EnsureChatKeyboardOpen();
            string msg = SelectNextMessage(peekOnly: true);
            TrySendChat(msg);
            MelonLogger.Msg($"[WildBerry] Auto Advertising: Preview sent: \"{msg}\"");
        }

        private void SendNextMessage()
        {
            var list = GetMessages();
            if (list.Count == 0)
            {
                StopAdvertising();
                return;
            }

            string msg = SelectNextMessage(peekOnly: true);
            bool sent = TrySendChat(msg);
            if (sent)
            {
                messageIndex++;
                pendingQuickRetry = false;
            }
            else
            {
                pendingQuickRetry = true;
            }
        }

        private string SelectNextMessage(bool peekOnly)
        {
            var list = GetMessages();
            if (list.Count == 0) return string.Empty;

            if (messageIndex >= list.Count)
                messageIndex = 0;
            string messageToSend = list[messageIndex];
            if (!peekOnly)
                messageIndex++;
            return messageToSend;
        }

        private void ScheduleNext(bool initialImmediate = false)
        {
            if (!isRunning) return;

            if (initialImmediate)
            {
                lastTickTime = Time.time - 99999f;
                nextDelaySeconds = 0f;
                return;
            }

            float delay = pendingQuickRetry ? 0.5f : Mathf.Max(10f, intervalSeconds);

            lastTickTime = Time.time;
            nextDelaySeconds = delay;
        }

        private List<string> GetMessages()
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(messagesMultiline))
                return result;

            var lines = messagesMultiline.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var raw in lines)
            {
                string s = (raw ?? string.Empty).Trim();
                if (!string.IsNullOrEmpty(s))
                    result.Add(s);
            }
            return result;
        }

        private bool TrySendChat(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            try
            {
                var km = Il2Cpp.KeyboardManager.instance;
                if (km == null)
                    return false;

                if (!EnsureChatKeyboardOpen()) return false;

                try { km.focusedInputField = km.chatInputField; } catch { }
                try { if (km.chatInputField != null) km.chatInputField.IsSelectedOrFocused = true; } catch { }

                try { if (km.chatInputField?.inputAJ != null) km.chatInputField.inputAJ.value = text; } catch { }
                try { if (km.chatInputField?.input != null) km.chatInputField.input.value = text; } catch { }
                try { if (km.chatInputField?.inputAJ != null && km.chatInputField.inputAJ.label != null) km.chatInputField.inputAJ.label.text = text; } catch { }
                try { if (km.chatInputField?.input != null && km.chatInputField.input.label != null) km.chatInputField.input.label.text = text; } catch { }

                km.text = text;
                km.textUnformatted = text;

                km.OnChatInputChanged(Il2Cpp.CommunicationMethod.Chat);
                Il2Cpp.KeyboardManager.SimulateSpecialKeyButtonPress(Il2Cpp.KeyboardKeyCommand.Return);
                km.OnSubmitChat();
                return true;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Auto Advertising: KeyboardManager send failed: {ex.Message}");
                return false;
            }
        }

        private bool EnsureChatKeyboardOpen()
        {
            try
            {
                var km = Il2Cpp.KeyboardManager.instance;
                if (km == null) return false;

                bool isOpen = false;
                try { isOpen = km.IsOpen; } catch { }

                if (!isOpen || km.chatInputField == null)
                {
                    try { Il2Cpp.KeyboardManager.layoutType = Il2Cpp.KeyboardType.Chat; } catch { }
                    try { Il2Cpp.DragonDungeonUI.Chat("", Il2Cpp.ChatType.Safe); } catch { }
                    km.Open(null);
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
