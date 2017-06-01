// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-04-19
// Comment		
// **********************************************************************************************/

using System;
using System.IO;
using System.Linq;
using FulltextStorageLib;
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

            var notes = new[]
            {
                Note.Create("Evaluating Evernote\r\n GMBH encryption support. interface is not too bad, local supported, no symmetric encryption, no fulltext search."),
                Note.Create("Evaluating OneNote\r\n interface is not too bad, local supported, no symmetric key encryption, no fulltext search, although claimed."),
                Note.Create("Government is the great fiction\r\n through which everybody endeavors to live at the expense of everybody else"),
                Note.Create("Stuff location: router \r\n placed into a big box in the garage near the top of the pile, box marked \"electronics\""),
                Note.Create("unbalanced signal converter \r\n in a marked plastic box in laundry"),
                Note.Create("blog post \r\n publishing tomorrow "),
                Note.Create("Quote from Bastiat\r\nFor now, as formerly, every one is, more or less, for profiting by the labors of others. No one would dare to profess such a sentiment; he even hides it from himself; and then what is done? A medium is thought of; Government is applied to, and every class in its turn comes to it, and says, \"You, who can take justifiably and honestly, take from the public, and we will partake."),
                Note.Create("Egg \r\n tolerated well, but high in cholesterol if you beleive it"),
                Note.Create("Masha \r\n Maria Мария"),
                Note.Create("Quote: Mises on education\r\n​It is often asserted that the poor man's failure in the competition of the market is caused by his lack of education. Equality of opportunity, it is said, could be provided only by making education at every level accessible to all. There prevails today the tendency to reduce all differences among various peoples to their education and to deny the existence of inborn inequalities in intellect, will power, and character. It is not generally realized that education can never be more than indoctrination with theories and ideas already developed. Education, whatever benefits it may confer, is transmission of traditional doctrines and valuations; it is by necessity conservative. It produces imitation and routine, not improvement and progress. Innovators and creative geniuses cannot be reared in schools. They are precisely the men who defy what the school has taught them."),
                Note.Create("Испанская «Барселона» объявила о подписании контракта с новым главным тренером Эрнесто Вальверде. Об этом сообщается в клубном Twitter в понедельник, 29 мая."),
            };

            var storage = new CouchbaseStorage<Note>(@"c:\temp\MyNotes", new NoteCouchbaseAdapter());

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
            var searchEngine = new SearchEngine<INote, INoteHeader, string>(
                fullTextDir.FullName, adapter, new MultiIndex(adapter.DocumentKeyName));

            searchEngine.UseFuzzySearch = true;

            var indexNameEnglish = "en";
            var englishAnalyzer = LuceneIndex.CreateSnowballAnalyzer("English");
            var russianAnalyzer = LuceneIndex.CreateSnowballAnalyzer("Russian");

            searchEngine.AddOrOpenIndex(indexNameEnglish, englishAnalyzer);
            searchEngine.AddOrOpenIndex("ru", russianAnalyzer);

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
