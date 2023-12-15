select 	*
		, to_tsvector('public.mysearch', text)
from 	note
where	search_vector @@ websearch_to_tsquery('public.mysearch', 'главный тренер')
		and last_update_time > timestamp'2023-09-29'

select * from ts_debug('public.mysearch', 'sacrificial animal')

select * from note where search_vector @@ websearch_to_tsquery('public.mysearch', 'query');
select * from note where search_vector @@ websearch_to_tsquery('query');
select * from note where search_vector @@ websearch_to_tsquery('mysearch', 'query');

select to_tsvector('public.mysearch', 'Sacrificial entry
to be deleted during test') @@ websearch_to_tsquery('public.mysearch', 'sacrifice');

select *, to_tsvector('public.mysearch', text) as recalced from note where text ilike '%sacrific%'

update note set search_vector = default


select * from public.search('barcode printers', true)
SELECT pg_reload_conf()
select      *
from        public.note
order by    id desc
limit       1
