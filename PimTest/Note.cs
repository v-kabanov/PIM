// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-04-19
// Comment		
// **********************************************************************************************/

using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace PimTest
{
    public class Note
    {
        private static Regex _nameRegex = new Regex(@"[\S]+.*[\S]+\s*$", RegexOptions.Multiline);
        private static int _id;

        public int Id { get; set; }

        public DateTime CreateTime { get; set; }

        public string Name { get; set; }

        public string Text { get; set; }

        public static Note Create(string text)
        {
            var nameMatch = _nameRegex.Match(text);
            if (!nameMatch.Success)
            {
                return null;
            }
            return new Note()
            {
                Id = Interlocked.Increment(ref _id),
                Name = nameMatch.Value,
                Text = text,
                CreateTime = DateTime.Now
            };
        }
    }
}