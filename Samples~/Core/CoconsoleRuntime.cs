using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Coconsole
{
	public partial class CoconsoleRuntime : ISuggestProvider
	{
		private string[] _entryPoints;
		private List<ICoconsoleCommandWrapper> _commands;
		private readonly CoconsoleRuntimeSettings _settings;
		private int _currentCommandIndex = -1;

		// {
		//     _settings = settings;
		//     _commands = new List<ICoconsoleCommandWrapper>();
		//     AddCommand(new CoconsoleCommand_Kill(_settings.partComparer));
		//     AddCommand(new CoconsoleCommand_Revive(_settings.partComparer));
		//     _entryPoints = new string[_commands.Count];
		//     for (int i = 0; i < _commands.Count; i++)
		//     {
		//         _entryPoints[i] = _commands[i].entryPoint;
		//     }
		// }

		public IEnumerable<ICoconsoleCommandWrapper> Commands => _commands;

		private void AddCommand(ICoconsoleCommandWrapper command)
		{
			_commands.Add(command);
		}


		public ISuggestProvider.RSettings Update(string text, int index, ref Suggestion[] suggestions)
		{
			// text = text.Trim();
			var parts = text.Split(' ');
			if (String.IsNullOrEmpty(parts[0]))
			{
				suggestions = Array.Empty<Suggestion>();
				return new ISuggestProvider.RSettings() { };
			}

			if (parts.Length == 0 || String.IsNullOrEmpty(parts[0]))
			{
				_currentCommandIndex = -1;
			}

			if (_currentCommandIndex > -1)
			{
				var currentCommand = _commands[_currentCommandIndex];
				if (!currentCommand.entryPoint.Equals(parts[0]) || parts.Length == 1) //entry changed or user remove backspace
				{
					_currentCommandIndex = -1;
				}
				else
				{
					return currentCommand.Update(parts, index, text.Length, ref suggestions);
				}
			}


			suggestions = _settings.entryPointComparer.Search(parts[0], _entryPoints, (index, s, d) => new Suggestion()
			{
				sourceText = s,
				distance = d,
				visualText = s,
				index = index,
			});

			if (suggestions.Length == 1 || (suggestions.Length > 1 && Math.Abs(suggestions[0].distance - 1f) < Double.Epsilon &&
			                                Math.Abs(suggestions[1].distance - 1f) > Double.Epsilon))
			{
				_currentCommandIndex = suggestions[0].index;
				return _commands[_currentCommandIndex].Update(parts, index, text.Length, ref suggestions);
			}

			return new ISuggestProvider.RSettings()
			{
				readonlyMode = false,
				suggestionRange = new int2(0, text.Length),
				addWhitespace = true,
			};
		}
	}
}