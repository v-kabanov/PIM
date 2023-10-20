CREATE TEXT SEARCH DICTIONARY public.russian_hunspell (
    TEMPLATE = ispell,
    DictFile = ru_RU,
    AffFile = ru_RU,
    Stopwords = russian);
    
CREATE TEXT SEARCH DICTIONARY public.english_gb_hunspell (
    TEMPLATE = ispell,
    DictFile = en_GB,
    AffFile = en_GB,
    Stopwords = english);

CREATE TEXT SEARCH DICTIONARY public.english_us_hunspell (
    TEMPLATE = ispell,
    DictFile = en_US,
    AffFile = en_US,
    Stopwords = english);
---------------------------------------------------------------
-- DROP TEXT SEARCH CONFIGURATION public.mysearch

CREATE TEXT SEARCH CONFIGURATION public.mysearch (
	PARSER = default
);
ALTER TEXT SEARCH CONFIGURATION public.mysearch alter MAPPING FOR asciihword WITH public.english_gb_hunspell, public.english_us_hunspell, english_stem;
ALTER TEXT SEARCH CONFIGURATION public.mysearch alter MAPPING FOR asciiword WITH public.english_gb_hunspell, public.english_us_hunspell, english_stem;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR email WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR file WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR float WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR host WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR hword WITH public.russian_hunspell, russian_stem;
ALTER TEXT SEARCH CONFIGURATION public.mysearch alter MAPPING FOR hword_asciipart WITH public.english_gb_hunspell, public.english_us_hunspell, english_stem;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR hword_numpart WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR hword_part WITH public.russian_hunspell, russian_stem;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR int WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR numhword WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR numword WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR sfloat WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR uint WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR url WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR url_path WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR version WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR word WITH public.russian_hunspell, russian_stem;
-------------------------------------------------------------

--drop table if exists public.note;

create table if not exists public.note (
	id int not null generated by default as identity ( increment 1 start 1 minvalue 1 maxvalue 2147483647 cache 1 ),
	text varchar not null,
	create_time timestamp without time zone not null,
	last_update_time timestamp without time zone not null,
	search_vector tsvector generated always as (to_tsvector('public.mysearch', text)) stored,
	integrity_version int not null
);

alter 		table public.note
add column  search_vector tsvector
generated always	as (to_tsvector('public.mysearch', text)) stored;

--alter table note drop column search_vector;


alter table public.note
add constraint  pk_note
primary key     (id);

create text search configuration public.mysearch (copy = <language>);

--create index "Note_SearchVector_idx" on public."Note" using gin (to_tsvector('public.mysearch', "Text"));

--drop index note_search_vector_idx;
create index note_search_vector_idx on public.note using gin (search_vector);

grant insert, select, update, delete on table public.note to pimweb;
grant all on sequence public.note_id_seq to pimweb;
--------------------------

select 	*
		, to_tsvector('public.mysearch', text)
from 	note
where	search_vector @@ websearch_to_tsquery('public.mysearch', 'главный тренер')
		and last_update_time > timestamp'2023-09-29'

select * from ts_debug('public.mysearch', 'source code')

select * from note where search_vector @@ websearch_to_tsquery('public.mysearch', 'query');
select * from note where search_vector @@ websearch_to_tsquery('query');
select * from note where search_vector @@ websearch_to_tsquery('mysearch', 'query');

select to_tsvector('public.mysearch', 'Sacrificial entry
to be deleted during test') @@ websearch_to_tsquery('public.mysearch', 'sacrifice');

select *, to_tsvector('public.mysearch', text) as recalced from note where text ilike '%sacrific%'

update note set search_vector = default