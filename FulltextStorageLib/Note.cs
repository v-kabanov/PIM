// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-04-19
// Comment		
// **********************************************************************************************/

using System;
using System.Text.RegularExpressions;

namespace FulltextStorageLib
{
    public interface INoteStorage : IDocumentStorage<Note, string>
    { }

    public class Note : IPersistentNote
    {
        private static readonly Regex NameRegex = new Regex(@"([\S]+.*[\S]+)\s*$", RegexOptions.Multiline);

        private string _text;

        public string Id { get; set; }

        public DateTime CreateTime { get; set; }
        public DateTime LastUpdateTime { get; set; }

        /// <summary>
        ///     0 for unsaved, incremented every time it's updated in the storage
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        ///     Register update
        /// </summary>
        /// <returns>
        ///     New version
        /// </returns>
        public int IncrementVersion()
        {
            return ++Version;
        }

        /// <summary>
        ///     Name is just cached first line of text; should not be persisted separately
        /// </summary>
        public string Name { get; private set; }

        public string Text
        {
            get { return _text; }
            set
            {
                Name = ExtractFirstLine(value);
                if (_text != value)
                    LastUpdateTime = DateTime.Now;
                _text = value;
            }
        }

        /// <summary>
        ///     Id is assigned after note is saved
        /// </summary>
        public bool IsTransient => Version == 0;

        public static Note Create(string text)
        {
            var result = new Note()
            {
                Id = CreateShortGuid(),
                Text = text,
                CreateTime = DateTime.Now,
                LastUpdateTime = DateTime.Now,
                Version = 0
            };

            if (string.IsNullOrEmpty(result.Name))
                return null;

            return result;
        }

        /// <returns>
        ///     null if text contains any valid text (non-blank-space)
        /// </returns>
        private static string ExtractFirstLine(string text)
        {
            var nameMatch = NameRegex.Match(text);
            return nameMatch.Success
                ? nameMatch.Groups[1].Value
                : null;
        }

        public static string CreateShortGuid()
        {
            var encoded = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            encoded = encoded
              .Replace("/", "_")
              .Replace("+", "-");

            return encoded.Substring(0, 22);
        }
    }
}