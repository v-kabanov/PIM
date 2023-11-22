# README #

* Quick summary

A very simple Personal Information Manager as text note database with proper fulltext search. Simple web interface made to make note taking as fast as possible.
There's no explicit structure, just type some text and save. Data is stored in the Postgres database.
Fulltext search is available from the textbox at the top of the page. It is implemented in Postgres and thus can be tuned.

### How do I get set up? ###
The PimWeb project is ASP.NET Core 7.0 application. Best hosting is under reverse proxy on linux or under IIS on Windows.
Groups and default users are set up at startup when no users are found in the database. The default account is 'admin' - 'password'.
