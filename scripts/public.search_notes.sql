drop function if exists public.search;

create or replace function public.search_notes(searchQuery varchar(200), fuzzy boolean)
returns table (id integer, text varchar, last_update_time timestamp with time zone, create_time timestamp with time zone, headline text, rank real) as $$
begin
    if (fuzzy) then
        return query
        with fuzzy as (
            select  to_tsquery('public.mysearch', string_agg(lexeme || ':*', ' | ' order by positions)) as tsquery
            from    unnest(to_tsvector('public.mysearch', searchQuery))
        )
        select  n.id
                , n.text
                , n.last_update_time
                , n.create_time
                --, n.integrity_version
                --, n.search_vector
                , ts_headline('public.mysearch', n.text, fuzzy.tsquery) as headline
                , ts_rank_cd(n.search_vector, fuzzy.tsquery) as rank
        from    note n
                join fuzzy
                    on n.search_vector @@ fuzzy.tsquery
        order by rank desc;
    else
        return query
        select  n.id
                , n.text
                , n.last_update_time
                , n.create_time
                --, n.integrity_version
                --, n.search_vector
                , ts_headline('public.mysearch', n.text, websearch_to_tsquery('public.mysearch', searchQuery)) as headline
                , ts_rank_cd(n.search_vector, websearch_to_tsquery('public.mysearch', searchQuery)) as rank
        from    note n
        where   n.search_vector @@ websearch_to_tsquery('public.mysearch', searchQuery)
        order by rank desc;
    end if;
end;
$$ language plpgsql;
