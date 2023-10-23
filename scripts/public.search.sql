--drop function search

create or replace function public.search(searchQuery varchar(200))
returns table (id integer, text varchar, last_update_time timestamp without time zone, create_time timestamp without time zone, integrity_version int, search_vector tsvector) as $$
begin
return query
    with fuzzy as (
        select  to_tsquery('public.mysearch', string_agg(lexeme || ':*', ' | ' order by positions)) as tsquery
        from    unnest(to_tsvector('public.mysearch', searchQuery))
    )
    select  n.id
            , n.text
            , n.last_update_time
            , n.create_time
    from    note n
            join fuzzy
                on n.search_vector @@ fuzzy.tsquery;
end;
$$ language plpgsql;
