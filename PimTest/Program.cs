// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-04-19
// Comment		
// **********************************************************************************************/

using System;

namespace PimTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var index = new LuceneIndex();

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

            index.Add(notes);

            // problem: does not find 'erasing' by 'erase'; but 'erasure' works

            bool finish;
            do
            {
                Console.WriteLine();
                Console.Write("Your query: ");
                var query = Console.ReadLine();
                finish = string.IsNullOrEmpty(query);
                if (!finish)
                {
                    var result = index.Search(query);
                    Console.WriteLine("Found {0} items", result.Count);
                    foreach (var note in result)
                    {
                        Console.WriteLine("\t {0} - {1}", note.Id, note.Name);
                    }
                }
            }
            while (!finish);
        }
    }
}
