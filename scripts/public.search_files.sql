--drop function if exists search_files

create or replace function public.search_files(searchQuery varchar(200), fuzzy boolean)
returns table (id integer, relative_path varchar(8000), title varchar(100), description varchar, last_update_time timestamp with time zone, create_time timestamp with time zone, headline text, rank real) as $$
begin
    if (fuzzy) then
        return query
        with fuzzy as (
            select  to_tsquery('public.mysearch', string_agg(lexeme || ':*', ' | ' order by positions)) as tsquery
            from    unnest(to_tsvector('public.mysearch', searchQuery))
        )
        select  x.id
                , x.relative_path
                , x.title
                , x.description
                , x.last_update_time
                , x.create_time
                , ts_headline('public.mysearch', x.relative_path || ' ' || coalesce(x.title, '') || ' ' || coalesce(x.description, '') || ' ' || coalesce(x.extracted_text, ''), fuzzy.tsquery) as headline
                , ts_rank_cd(x.search_vector, fuzzy.tsquery) as rank
        from    file x
                join fuzzy
                    on x.search_vector @@ fuzzy.tsquery
        order by rank desc;
    else
        return query
        select  x.id
                , x.relative_path
                , x.title
                , x.description
                , x.last_update_time
                , x.create_time
                , ts_headline('public.mysearch', x.relative_path || ' ' || coalesce(x.title, '') || ' ' || coalesce(x.description, '') || ' ' || coalesce(x.extracted_text, ''), websearch_to_tsquery('public.mysearch', searchQuery)) as headline
                , ts_rank_cd(x.search_vector, websearch_to_tsquery('public.mysearch', searchQuery)) as rank
        from    file x
        where   x.search_vector @@ websearch_to_tsquery('public.mysearch', searchQuery)
        order by rank desc;
    end if;
end;
$$ language plpgsql;
