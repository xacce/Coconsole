using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Coconsole
{
	public class CoconsoleSingleton : MonoBehaviour, IPerfomantScrollViewDataSource<string>
	{
		[SerializeField] private UIDocument doc_s;
		[SerializeField] private bool collectLogsInactive_s = true;
		[SerializeField] private bool showOnStart_s = true;
		[SerializeField] private bool lazyInitialize_s;
		[SerializeField] private InputAction escapeAction_s;
		[SerializeField] private InputAction showAction_s;
		[SerializeField] private int recordHeight_s = 20;
		[SerializeField] private float updateInterval_s = 0.1f;
		private VisualElement _root;
		private bool _collecting = false;

		private List<string> _buffer;
		private CoconsoleRuntime _coconsole;
		private bool _initialized;
		private bool _show;
		private VisualElement _input;
		private AutocompleteTextField _field;
		private StreamScrollView<string, Label> _history;
		private Coroutine _coro;
		public static CoconsoleSingleton instance { get; private set; }


		private void Awake()
		{
			if (instance != null) GameObject.Destroy(this);
			instance = this;
			_buffer = new List<string>();
			_history = new StreamScrollView<string, Label>(this, recordHeight_s,
				() => new Label(),
				(s, e) => { e.text = s; },
				(items, hierarchy, range) =>
				{
					for (int i = range.x, ii = 0; i < range.x + range.y; i++, ii++)
					{
						var item = items[i];
						var el = hierarchy.ElementAt(ii) as Label;
						el.text = item;
					}
				}
			);
			_root = doc_s.rootVisualElement.Q<VisualElement>("Coconsole");
			_root.Q<VisualElement>("HistoryWrapper").Add(_history);
			_coconsole = new CoconsoleRuntime(new CoconsoleRuntimeSettings
			{
				entryPointComparer = new JaroWinglerComparer(),
				partComparer = new JaroWinglerComparer()
				{
					minWeight = 0.7f,
				},
			});
			if (!lazyInitialize_s)
			{
				Bind();
			}

			escapeAction_s.performed += _ =>
			{
				if (_initialized && _show && _root.panel.focusController.focusedElement != _field)
				{
					Hide();
				}
			};
			showAction_s.performed += _ =>
			{
				if (!_show)
				{
					Show();
				}
			};
		}

		private void OnEnable()
		{
			escapeAction_s.Enable();
			showAction_s.Enable();
			if (collectLogsInactive_s)
			{
				Application.logMessageReceived += OnLog;
				_collecting = true;
			}
		}

		private IEnumerator _Update()
		{
			while (true)
			{
				_history.ThrottledUpdate();
				yield return new WaitForSeconds(updateInterval_s);
			}
		}

		private void OnLog(string condition, string stacktrace, LogType type)
		{
			if (type == LogType.Warning) return;
			_buffer.Add($"[{DateTime.Now:HH:mm:ss.ff}][{type}] {condition}");
		}


		private void OnDisable()
		{
			if (_collecting)
			{
				_collecting = false;
				Application.logMessageReceived -= OnLog;
			}

			if (_coro != null)
				StopCoroutine(_coro);
		}

		private void Bind()
		{
			if (_initialized) return;
			_initialized = true;
			_field = new AutocompleteTextField(_coconsole, _root.Q<ScrollView>("Suggestions"));
			_input = _root.Q<VisualElement>("Input");
			_input.Add(_field);
		}

		void Start()
		{
			if (showOnStart_s)
			{
				Show();
			}
			else
			{
				_root.style.display = DisplayStyle.None; //do not trigger;
			}
		}

		public void Flush()
		{
			_buffer.Clear();
			_history.ThrottledUpdate();
		}

		public void Show()
		{
			if (!collectLogsInactive_s)
			{
				Application.logMessageReceived += OnLog;
				_collecting = true;
			}

			_coro = StartCoroutine(_Update());
			_show = true;
			Bind();
			_root.style.display = DisplayStyle.Flex;
		}

		public void Hide()
		{
			if (!_show) return;
			if (!collectLogsInactive_s)
			{
				Application.logMessageReceived -= OnLog;
				_collecting = false;
			}

			StopCoroutine(_coro);
			_show = false;
			_root.style.display = DisplayStyle.None;
		}

		public IReadOnlyList<string> GetItemsRo()
		{
			return _buffer;
		}


		private void OnApplicationQuit()
		{
			escapeAction_s.Disable();
			showAction_s.Disable();
			escapeAction_s.Dispose();
			showAction_s.Dispose();
			instance = null;
		}
	}
}