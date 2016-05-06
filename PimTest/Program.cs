// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-04-19
// Comment		
// **********************************************************************************************/

using System;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace PimTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var index = new LuceneIndex(LuceneIndex.CreateDefaultAnalyzer("English"), LuceneIndex.CreateTransientDirectory());

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

            var adapter = new LuceneNoteAdapter();

            index.Add(adapter.GetIndexedDocuments(notes));

            var result1 = adapter.Search(
                index
                , adapter.CreateQueryFromFilter(
                    adapter.CreateTimeRangeFilter(null, DateTime.Now.AddSeconds(-1))));

            var result2 = adapter.Search(
                index
                , adapter.CreateQueryFromFilter(
                    adapter.CreateTimeRangeFilter(DateTime.Now.AddSeconds(-10), null)));

            // problem: does not find 'erasing' by 'erase'; but 'erasure' works

            bool finish;
            do
            {
                Console.WriteLine();
                Console.Write("Your query: ");
                var queryText = Console.ReadLine();
                finish = string.IsNullOrEmpty(queryText);
                if (!finish)
                {
                    var query = adapter.CreateQuery(index, queryText);
                    var result = index.Search(query, 100);
                    Console.WriteLine($"Found {result.Count} items");
                    foreach (var hit in result.Select(h => new { NoteHeader = adapter.GetNoteHeader(h.Document), Score = h.Score }))
                    {
                        Console.WriteLine($"\t {hit.NoteHeader.Id} - {hit.NoteHeader.Name}; Score = {hit.Score}");
                        var loaded = storage.GetExisting(hit.NoteHeader.Id);

                        if (loaded == null)
                        {
                            Console.WriteLine($"Note {hit.NoteHeader.Id} not found in database");
                        }
                    }
                }
            }
            while (!finish);
        }
    }
}
