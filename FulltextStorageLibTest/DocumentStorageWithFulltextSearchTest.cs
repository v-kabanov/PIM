// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-05-28
// Comment  
// **********************************************************************************************/
// 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FulltextStorageLib;
using NUnit.Framework;

namespace FulltextStorageLibTest
{
    [TestFixture]
    public class DocumentStorageWithFulltextSearchTest
    {
        private string _rootStorageDirectory;
        private DocumentStorageWithFulltextSearch<Note, string, INoteHeader> _indexedStorage;
        private Note[] _testNotes;

        [OneTimeSetUp]
        public void SetUp()
        {
            _rootStorageDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "IndexedStorage");

            if (Directory.Exists(_rootStorageDirectory))
                Directory.Delete(_rootStorageDirectory, true);

            _indexedStorage = DocumentStorageWithFulltextSearch<Note, string, INoteHeader>.CreateStandard(_rootStorageDirectory, new NoteCouchbaseAdapter(), new LuceneNoteAdapter());
            _indexedStorage.Open();

            _indexedStorage.OpenOrCreateIndexes(new [] { "English", "Russian" });

            _testNotes = new[]
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
                Note.Create("Mama \r\n mum mother Tanya")
            };

            for (var n = 0; n < _testNotes.Length; ++n)
            {
                _testNotes[n].LastUpdateTime = DateTime.Now.AddHours(n - _testNotes.Length);
                _testNotes[n].CreateTime = _testNotes[n].LastUpdateTime;
            }

            _indexedStorage.SaveOrUpdate(_testNotes);

            foreach (var note in _testNotes)
            {
                Assert.IsNotEmpty(note.Id);
                Assert.AreEqual(1, note.Version);
            }
        }

        [Test]
        public void GetTopInPeriodSortsByLastUpdateTimeDescending()
        {
            var searchResult = _indexedStorage.GetTopInPeriod(null, DateTime.Now, 5);

            Assert.AreEqual(_testNotes.Last().Id, searchResult.First().Id);

            Assert.AreEqual(5, searchResult.Count);
        }

        [Test]
        public void FindWithDifferentEnding()
        {
            var result = _indexedStorage.Search("encryPted", 20);

            Assert.AreEqual(2, result.Count);
        }
    }
}
