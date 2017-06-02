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

            for (var n = 0; n < _testNotes.Length; ++n)
            {
                _testNotes[n].LastUpdateTime = DateTime.Now.AddHours(n - _testNotes.Length);
                _testNotes[n].CreateTime = _testNotes[n].LastUpdateTime.AddDays(-1);
            }

            _indexedStorage.SaveOrUpdate(_testNotes);

            foreach (var note in _testNotes)
            {
                Assert.IsNotEmpty(note.Id);
                Assert.AreEqual(1, note.Version);
            }

            Assert.IsTrue(_indexedStorage.WaitForFulltextBackgroundWorkToComplete(1000));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _indexedStorage.Dispose();
        }

        [Test]
        [TestCase(SearchableDocumentTime.Creation)]
        [TestCase(SearchableDocumentTime.LastUpdate)]
        public void GetExistingByTimePeriodEnd(SearchableDocumentTime documentTime)
        {
            var lastDocTime = documentTime == SearchableDocumentTime.LastUpdate
                ? _testNotes.Last().LastUpdateTime
                : _testNotes.Last().CreateTime;

            var searchResult = _indexedStorage.GetTopInPeriod(null, lastDocTime, 5, documentTime);

            // range end is exclusive
            Assert.AreEqual(_testNotes.Reverse().Skip(1).First().Id, searchResult.First().Id);

            Assert.AreEqual(5, searchResult.Count);
        }

        [Test]
        [TestCase(SearchableDocumentTime.Creation)]
        [TestCase(SearchableDocumentTime.LastUpdate)]
        public void GetExistingByTimePeriod(SearchableDocumentTime documentTime)
        {
            var firstIndex = new Random().Next(_testNotes.Length);
            var lastIndex = firstIndex + 2;
            if (lastIndex > _testNotes.Length - 1)
                lastIndex = _testNotes.Length - 1;

            var firstNote = _testNotes[firstIndex];
            var lastNote = _testNotes[lastIndex];

            var firstDocTime = firstNote.GetSeachableTime(documentTime);
            var lastDocTime = lastNote.GetSeachableTime(documentTime).AddSeconds(1);

            var searchResult = _indexedStorage.GetTopInPeriod(firstDocTime, lastDocTime, 50, documentTime);

            Assert.AreEqual(lastIndex - firstIndex + 1, searchResult.Count);

            // range end is exclusive
            for (var i = firstIndex; i <= lastIndex; ++i)
            {
                var sourceNote = _testNotes[i];
                var searchResultNote = searchResult[lastIndex - i];

                Assert.AreEqual(sourceNote.Id, searchResultNote.Id);
                Assert.AreEqual(sourceNote.CreateTime.TrimToSecondsPrecision(), searchResultNote.CreateTime);
                Assert.AreEqual(sourceNote.LastUpdateTime.TrimToSecondsPrecision(), searchResultNote.LastUpdateTime);
            }
        }

        [Test]
        [TestCase(SearchableDocumentTime.Creation)]
        [TestCase(SearchableDocumentTime.LastUpdate)]
        public void GetNonExistentByLastUpdateTimePeriodEnd(SearchableDocumentTime documentTime)
        {
            var lastDocTime = documentTime == SearchableDocumentTime.LastUpdate
                ? _testNotes.First().LastUpdateTime
                : _testNotes.First().CreateTime;

            var searchResult = _indexedStorage.GetTopInPeriod(null, lastDocTime, 5, documentTime);

            Assert.AreEqual(0, searchResult.Count);
        }

        [Test]
        public void FindWithDifferentEnding()
        {
            var result = _indexedStorage.Search("encryPted", 20);

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void SearchForSentence()
        {
            var result = _indexedStorage.Search("free market competition", 10);

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void AlternativeLanguage()
        {
            var result = _indexedStorage.Search("подписал контракт", 10);

            Assert.AreEqual(1, result.Count);
        }
    }
}
