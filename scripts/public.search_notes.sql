drop function if exists public.search_notes;

create or replace function public.search_notes(searchQuery varchar(200), fuzzy boolean)
returns table (id integer, text varchar, last_update_time timestamp with time zone, create_time timestamp with time zone, headline text, rank real) as $$
begin
    if (fuzzy) then
        return query
        with fuzzy as (
            select  to_tsquery('public.mysearch', string_agg(lexeme || ':*', ' | ' order by positions)) as tsquery
                    , true as if_matched_note
            from    unnest(to_tsvector('public.mysearch', searchQuery))
        )
        , file_search as (
            select      nf.note_id
                        , first_value(sf.rank) over (partition by nf.note_id order by sf.rank desc) as rank
                        , first_value(sf.headline) over (partition by nf.note_id order by sf.rank desc) as headline
            from        public.search_files(searchQuery, fuzzy) sf
                        join public.note_file nf
                            on nf.file_id = sf.id
        )
        , files as (
            select      fs.note_id
                        , any_value(fs.rank) as rank
                        , any_value(fs.headline) as headline
            from        file_search fs
            group by    fs.note_id
        )
        select  n.id
                , n.text
                , n.last_update_time
                , n.create_time
                --, n.integrity_version
                --, n.search_vector
                , case
                    when coalesce(fn.rank, 0) > coalesce(ts_rank_cd(n.search_vector, f.tsquery), 0)
                        then fn.headline
                        else ts_headline('public.mysearch', n.text, f.tsquery)
                    end as headline
                , greatest(ts_rank_cd(n.search_vector, f.tsquery), fn.rank) as rank
        from    note n
                left join fuzzy f
                    on f.tsquery @@ n.search_vector
                left join files fn
                    on fn.note_id = n.id
        where   f.if_matched_note
                or fn.note_id is not null
        order by rank desc;
    else
        return query
        with file_search as (
            select      nf.note_id
                        , first_value(sf.rank) over (partition by nf.note_id order by sf.rank desc) as rank
                        , first_value(sf.headline) over (partition by nf.note_id order by sf.rank desc) as headline
            from        public.search_files(searchQuery, fuzzy) sf
                        join public.note_file nf
                            on nf.file_id = sf.id
        )
        , files as (
            select      fs.note_id
                        , any_value(fs.rank) as rank
                        , any_value(fs.headline) as headline
            from        file_search fs
            group by    fs.note_id
        )
        select  n.id
                , n.text
                , n.last_update_time
                , n.create_time
                --, n.integrity_version
                --, n.search_vector
                , case
                    when coalesce(fn.rank, 0) > coalesce(ts_rank_cd(n.search_vector, websearch_to_tsquery('public.mysearch', searchQuery)), 0)
                        then fn.headline
                        else ts_headline('public.mysearch', n.text, websearch_to_tsquery('public.mysearch', searchQuery))
                    end as headline
                , greatest(ts_rank_cd(n.search_vector, websearch_to_tsquery('public.mysearch', searchQuery)), fn.rank) as rank
        from    note n
                left join files fn
                    on fn.note_id = n.id
        where   n.search_vector @@ websearch_to_tsquery('public.mysearch', searchQuery)
                or fn.note_id is not null
        order by rank desc;
    end if;
end;
$$ language plpgsql;
