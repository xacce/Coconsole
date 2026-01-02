using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Coconsole
{
    public abstract class CoconsoleCommandWrapper : ICoconsoleCommandWrapper
    {
        public IComparer partComparer;
        public virtual ICoconsolePart[] args { get; }
        public virtual string humanReadable { get; }


        protected CoconsoleCommandWrapper(IComparer partComparer)
        {
            this.partComparer = partComparer;
        }


        public abstract string entryPoint { get; }

        public ISuggestProvider.RSettings Update(string[] parts, int index, int length, ref Coconsole.Suggestion[] suggestions)
        {
            #region Sequence suggestions

            int cPos = parts[0].Length + 1;
            for (int i = 1; i < Mathf.Min(parts.Length, args.Length + 1); i++)
            {
                var part = parts[i];
                var handler = args[i - 1];
                if (handler.hasSuggestions && index >= cPos && index <= cPos + part.Length)
                {
                    suggestions = partComparer.Search(parts[i], handler.suggestions, (_, s, d) => new Coconsole.Suggestion()
                    {
                        distance = d,
                        sourceText = s,
                        visualText = s,
                    });
                    return new ISuggestProvider.RSettings
                    {
                        addWhitespace = true,
                        suggestionRange = new int2(cPos, cPos + part.Length),
                    };
                }

                cPos += part.Length + 1;
            }

            #endregion

            #region Execution

            var cleanedparts = parts.Where(s => !String.IsNullOrWhiteSpace(s)).ToArray();
            if (cleanedparts.Length - 1 == args.Length)
            {
                bool fitted = true;
                for (int i = 1; i < cleanedparts.Length; i++)
                {
                    var part = cleanedparts[i];
                    var handler = args[i - 1];
                    if (!handler.Validate(part))
                    {
                        fitted = false;
                        break;
                    }
                }

                if (fitted)
                {
                    return new ISuggestProvider.RSettings
                    {
                        onSelected = (_, _) =>
                        {
                            Debug.Log($"[Coconsole] Executing {parts[0]} with args: {string.Join(',', parts.Skip(1).ToArray())}");
                            Handle(parts.Where(s => !String.IsNullOrWhiteSpace(s)).ToArray());
                        },
                        isExecution = true,
                    };
                }
            }

            #endregion

            #region Show selected

            suggestions = new Coconsole.Suggestion[]
            {
                new()
                {
                    sourceText = humanReadable,
                    visualText = humanReadable,
                }
            };
            return new ISuggestProvider.RSettings
            {
                readonlyMode = true,
            };

            #endregion
        }


        public abstract void Handle(string[] parts);
    }
}