# README #

* Quick summary
A very simple Personal Information Manager as text note database with proper fulltext search. Simple web interface made to make note taking as fast as possible.
There's no explicit structure, just type some text and save. Data is stored in the database and Lucene full-text search index is maintenied side-by-side.
Fulltext search is available from the textbox at the top of the page. Multiple Lucene snowball stemmers (languages) are supported, additional stemmers can be added at any time.
At startup missing indexes are rebuilt automatically in the background.

Underlying backend is developed in separate library with an attempt to keep it reasonably generic for reuse.

### How do I get set up? ###
Website can be deployed using included local IIS deployment profile from Visual Studio 2015.
Run VS elevated, open solution, right click AspNetPim project and publish.
All data is stored under App_Data folder so read-write access is required. No database configuration is necessary.
App pool can run under default ApplicationPoolIdentity in either x86 or x64 mode.
Groups and default users are set up at startup when database does not exist. Default account 'admin' - 'password'.
