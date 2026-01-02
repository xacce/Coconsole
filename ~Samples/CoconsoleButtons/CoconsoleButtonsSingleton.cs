using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Coconsole
{
	public class CoconsoleButtonsSingleton : MonoBehaviour
	{
		[SerializeField] private UIDocument doc_s;
		[SerializeField] private InputAction escapeAction_s;
		[SerializeField] private InputAction showAction_s;
		private VisualElement _root;
		private CoconsoleRuntime _coconsole;
		private bool _show = false;
		private ICoconsoleCommandWrapper[] _compatibleCommands;

		private void Awake()
		{
			_root = doc_s.rootVisualElement.Q<VisualElement>("Coconsole");
			_coconsole = new CoconsoleRuntime(new CoconsoleRuntimeSettings
			{
				entryPointComparer = new JaroWinglerComparer(),
				partComparer = new JaroWinglerComparer()
				{
					minWeight = 0.7f,
				},
			});
			_compatibleCommands = _coconsole.Commands.Where(c => c.args.Length == 0).ToArray();

			escapeAction_s.performed += _ =>
			{
				Hide();
			};
			showAction_s.performed += _ =>
			{
				if (!_show)
				{
					Show();
				}
			};
			Hide();
		}

		private void OnEnable()
		{
			escapeAction_s.Enable();
			showAction_s.Enable();
		}


		public void Show()
		{
			_show = true;
			doc_s.rootVisualElement.style.display = DisplayStyle.Flex;
			_root.Clear();
			for (int i = 0; i < _compatibleCommands.Length; i++)
			{
				var cmd = _compatibleCommands[i];
				var btn = new Button()
				{
					text = cmd.entryPoint,
				};
				btn.clicked += () =>
				{
					cmd.Handle(Array.Empty<string>());
				};
				_root.Add(btn);
			}
		}

		public void Hide()
		{
			_show = false;
			doc_s.rootVisualElement.style.display = DisplayStyle.None;
		}


		private void OnApplicationQuit()
		{
			escapeAction_s.Disable();
			showAction_s.Disable();
			escapeAction_s.Dispose();
			showAction_s.Dispose();
		}
	}
}