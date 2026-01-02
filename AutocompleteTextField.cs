using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Coconsole
{
    public interface ISuggestProvider
    {
        public struct RSettings
        {
            public bool isExecution;
            public bool addWhitespace;
            public bool readonlyMode;
            public int2 suggestionRange;
            public Action<Coconsole.Suggestion, int> onSelected;
            public static RSettings Null => new RSettings { };
        }

        public ISuggestProvider.RSettings Update(string text, int index, ref Coconsole.Suggestion[] suggestions);
    }

    public interface ISearchResult
    {
        public double distance { get; }
    }

    public interface IComparer
    {
        public string[] Search(string value, string[] values);
        public T[] Search<T>(string value, string[] values, Func<int, string, double, T> onResult) where T : ISearchResult;
    }

    public class CoconsoleRuntimeSettings
    {
        public IComparer entryPointComparer;
        public IComparer partComparer;
    }


    public interface ICoconsoleCommandWrapper
    {
        public string entryPoint { get; }
        public ISuggestProvider.RSettings Update(string[] parts, int index, int length, ref Coconsole.Suggestion[] suggestions);

        public ICoconsolePart[] args { get; }
        public string humanReadable { get; }
        public void Handle(string[] parts);
    }

    public interface ICoconsolePart
    {
        public string humanView { get; }
        public string[] suggestions { get; }
        public bool hasSuggestions { get; }
        public bool Validate(string part);
    }


    public enum KekNum
    {
        Value,
        Notvalue,
        Vavalue,
        Kepasa,
        Kepasa2,
        Kepasa3,
    }

    public class CoconsoleCommand_Kill : CoconsoleCommandWrapper
    {
        public override ICoconsolePart[] args => new ICoconsolePart[]
        {
            new CoconsoleIntPart(),
            new CoconsoleIntPart(),
        };

        public override string humanReadable => $"kill {args[0].humanView} {args[1].humanView}";

        public override string entryPoint => "kill";

        public override void Handle(string[] parts)
        {
            throw new NotImplementedException();
        }

        public CoconsoleCommand_Kill(IComparer partComparer) : base(partComparer)
        {
        }
    }

    public class CoconsoleCommand_Revive : CoconsoleCommandWrapper
    {
        public override ICoconsolePart[] args => new ICoconsolePart[]
        {
            new CoconsoleEnumPart<KekNum>(),
            new CoconsoleIntPart(),
            new CoconsoleEnumPart<KekNum>(),
        };

        public override string humanReadable => "revive <enum> <int> <enum>";

        public override void Handle(string[] parts)
        {
            GameCommands.Kill(
                CoconsoleEnumPart<KekNum>.ToValue(parts[1]),
                CoconsoleIntPart.ToValue(parts[2]),
                CoconsoleEnumPart<KekNum>.ToValue(parts[3])
            );
        }

        public void Handle(KekNum p1, int p2, KekNum p3)
        {
            Debug.Log("Handled");
            GameCommands.Kill(p1, p2, p3);
        }

        public override string entryPoint => "revive";

        public CoconsoleCommand_Revive(IComparer partComparer) : base(partComparer)
        {
        }
    }

    public static class GameCommands
    {
        // [Cmd("kill")]
        public static void Kill(KekNum p1, int p2, KekNum p3)
        {
            Debug.Log("Handled");
        }

        // [Cmd("kill")]
        public static void Kill(KekNum p1)
        {
            Debug.Log("Handled");
        }
    }

    public class AutocompleteTextField : TextField
    {
        private readonly ISuggestProvider _provider;
        private ScrollView _suggestionsContainer;
        private VisualElement[] _suggestionsVe = Array.Empty<VisualElement>();
        private ISuggestProvider.RSettings _lastResponse = ISuggestProvider.RSettings.Null;
        private Coconsole.Suggestion[] _suggestions = Array.Empty<Coconsole.Suggestion>();
        private int _highlightIndex;

        public AutocompleteTextField(ISuggestProvider provider, ScrollView suggestionsWrapper) : base()
        {
            textEdition.placeholder = "Enter command...";
            _provider = provider;
            _suggestionsContainer = suggestionsWrapper;
            selectAllOnFocus = false;
            selectAllOnMouseUp = false;

            RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            RegisterCallback<FocusInEvent>(FocusIn, TrickleDown.TrickleDown);
            RegisterCallback<FocusOutEvent>(FocusOut, TrickleDown.TrickleDown);
            this.RegisterValueChangedCallback(OnValueChanged);
        }

        private void FocusOut(FocusOutEvent evt)
        {
            ResetSuggestions();
        }

        private void FocusIn(FocusInEvent evt)
        {
            Trigger(value, cursorIndex);
        }


        [EventInterest()]
        protected override void HandleEventBubbleUp(EventBase evt)
        {
        }

        private bool HasSuggesstions => _suggestionsVe.Length > 0;

        private void OnKeyDown(KeyDownEvent keyUpEvent)
        {
            if (keyUpEvent.character == '\n' && HasSuggesstions)
            {
                keyUpEvent.StopPropagation();
                return;
            }
            switch (keyUpEvent.keyCode)
            {
                case KeyCode.Tab when HasSuggesstions:
                    UpdateHighlight(-1);
                    keyUpEvent.StopPropagation();
                    break;
                case KeyCode.UpArrow when HasSuggesstions:
                    UpdateHighlight(1);
                    keyUpEvent.StopPropagation();
                    break;
                case KeyCode.DownArrow when HasSuggesstions:
                    UpdateHighlight(1);
                    keyUpEvent.StopPropagation();
                    break;
                case KeyCode.Escape:
                    Blur();
                    keyUpEvent.StopPropagation();
                    break;
                case KeyCode.Return when HasSuggesstions && !_lastResponse.readonlyMode:
                    if (_highlightIndex == -1) _highlightIndex = 0;
                    _lastResponse.onSelected?.Invoke(_suggestions[_highlightIndex], _highlightIndex);
                    if (_lastResponse.isExecution)
                    {
                        SetValueWithoutNotify("");
                        Trigger("", 0);
                    }
                    else
                    {
                        ApplySuggestion(_highlightIndex);
                    }

                    break;
            }
        }


        private void ApplySuggestion(int index)
        {
            var raw = _lastResponse.addWhitespace ? String.Concat(_suggestions[index].sourceText, " ") : _suggestions[index].sourceText;
            var newValue = String.Concat(value.Substring(0, _lastResponse.suggestionRange.x), raw, value.Substring(_lastResponse.suggestionRange.y));


            SetValueWithoutNotify(newValue);
            SelectRange(_lastResponse.suggestionRange.x + raw.Length, _lastResponse.suggestionRange.x + raw.Length);
            Trigger(newValue, _lastResponse.suggestionRange.x + raw.Length);
        }

        private void ResetHighlight()
        {
            if (_highlightIndex > -1)
            {
                _suggestionsVe[_highlightIndex].RemoveFromClassList("highlight");
                _highlightIndex = -1;
            }
        }

        private void UpdateHighlight(int direction)
        {
            if (_highlightIndex > -1)
            {
                _suggestionsVe[_highlightIndex].RemoveFromClassList("highlight");
            }

            _highlightIndex += direction;
            if (_highlightIndex >= _suggestionsVe.Length) _highlightIndex = 0;
            if (_highlightIndex <= -1) _highlightIndex = _suggestionsVe.Length - 1;

            _suggestionsVe[_highlightIndex].AddToClassList("highlight");
        }

        protected virtual VisualElement CreateSuggestion(Suggestion suggestion, int index)
        {
            var lb = new Label(suggestion.visualText);
            lb.AddToClassList("coconsole-suggestion");
            lb.RegisterCallback<PointerEnterEvent>((evt) => { lb.CapturePointer(evt.pointerId); });
            lb.RegisterCallback<PointerLeaveEvent>((evt) => { lb.ReleasePointer(evt.pointerId); });
            lb.RegisterCallback<PointerCaptureOutEvent>((evt) => { });
            lb.RegisterCallback<PointerDownEvent>((evt) =>
            {
                if (_lastResponse.readonlyMode) return;
                evt.StopPropagation();
                _lastResponse.onSelected?.Invoke(_suggestions[index], index);
                // Focus();
                ApplySuggestion(index);
                lb.ReleasePointer(evt.pointerId);
            });
            return lb;
        }

        private void OnValueChanged(ChangeEvent<string> evt)
        {
            var value = evt.newValue;
            Trigger(value, cursorIndex);
        }

        private void Trigger(string value, int updatedCursorIndex)
        {
            _lastResponse = _provider.Update(value, updatedCursorIndex, ref _suggestions);
            ShowSuggestions();
        }

        private void ResetSuggestions()
        {
            _lastResponse = ISuggestProvider.RSettings.Null;
            _suggestionsContainer.style.visibility = Visibility.Hidden;
        }

        private void ShowSuggestions()
        {
            _highlightIndex = -1;
            if (_suggestions.Length == 0)
            {
                _suggestionsContainer.style.visibility = Visibility.Hidden;
                return;
            }

            _suggestionsVe = new VisualElement[_suggestions.Length];
            _suggestionsContainer.style.visibility = Visibility.Visible;
            _suggestionsContainer.Clear();
            for (var i = 0; i < _suggestions.Length; i++)
            {
                _suggestionsVe[i] = CreateSuggestion(_suggestions[i], i);
                _suggestionsContainer.Add(_suggestionsVe[i]);
            }

            _suggestionsContainer.scrollOffset = new Vector2(0, _suggestionsContainer.verticalScroller.highValue);
        }
    }
}