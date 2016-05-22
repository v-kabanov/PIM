// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-04-19
// Comment		
// **********************************************************************************************/

using System;
using System.IO;
using System.Linq;
using AuNoteLib;
using Newtonsoft.Json.Serialization;

namespace PimTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Available stemmers:");

            foreach (var n in LuceneIndex.GetAvailableSnowballStemmers())
            {
                Console.WriteLine(n);
            }

            var notes = new Note[]
            {
                Note.Create("Evaluating Evernote\r\n GMBH encryption support. interface is not too bad, local supported, no symmetric encryption, no fulltext search."),
                Note.Create("Evaluating OneNote\r\n interface is not too bad, local supported, no symmetric key encryption, no fulltext search, although claimed."),
                Note.Create("Meditations\r\n Marcus Aurelius. erasing imaginations"),
                Note.Create("Stuff location: router \r\n placed into a big box in the garage near the top of the pile, box marked \"electronics\""),
                Note.Create("unbalanced signal converter \r\n in a marked plastic box in laundry"),
                Note.Create("blog post \r\n publishing tomorrow "),
                Note.Create("Apple \r\n not tolerated well"),
                Note.Create("Egg \r\n tolerated well, but high in cholesterol if you beleive it"),
                Note.Create("Masha \r\n Maria Мария"),
                Note.Create("Mama \r\n mum mother Tanya"),
            };

            var storage = new NoteStorage(@"c:\temp\MyNotes");

            if (storage.CountAll() > 0)
            {
                notes = storage.GetAll().ToArray();
            }
            else
            {
                // filling database
                foreach (var note in notes)
                {
                    storage.SaveOrUpdate(note);
                }
            }

            var fullTextDir = new DirectoryInfo(@"c:\temp\AuNotes\ft");
            if (fullTextDir.Exists)
            {
                fullTextDir.Delete(true);
            }
            fullTextDir.Create();

            var adapter = new LuceneNoteAdapter();
            var searchEngine = new SearchEngine<INote, INoteHeader>(
                fullTextDir.FullName, adapter, new MultiIndex(adapter.DocumentKeyName));

            searchEngine.UseFuzzySearch = true;

            var indexNameEnglish = "en";
            var englishAnalyzer = LuceneIndex.CreateSnowballAnalyzer("English");
            var russianAnalyzer = LuceneIndex.CreateSnowballAnalyzer("Russian");

            searchEngine.AddIndex(indexNameEnglish, englishAnalyzer);
            searchEngine.AddIndex("ru", russianAnalyzer);

            searchEngine.Add(notes);

            var result1 = searchEngine.GetTopInPeriod(null, DateTime.Now.AddSeconds(-1), 4);

            var result2 = searchEngine.GetTopInPeriod(DateTime.Now.AddSeconds(-10), null, 4);

            // problem in the past: did not find 'erasing' by 'erase'; but 'erasure' works

            bool finish;
            do
            {
                Console.WriteLine();
                Console.Write("Your query: ");
                var queryText = Console.ReadLine();
                finish = string.IsNullOrEmpty(queryText);
                if (!finish)
                {
                    var result = searchEngine.Search(queryText, 100);
                    Console.WriteLine($"Found {result.Count} items");
                    foreach (var hit in result)
                    {
                        Console.WriteLine($"\t {hit.Id} - {hit.Name};");
                        var loaded = storage.GetExisting(hit.Id);

                        if (loaded == null)
                        {
                            Console.WriteLine($"Note {hit.Id} not found in database");
                        }
                    }
                }
            }
            while (!finish);
        }
    }
}
