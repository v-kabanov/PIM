create table if not exists public.note (
	id int not null,
	text varchar not null,
	create_time timestamp with time zone not null,
    -- with time zone it takes same size (8 bytes) and allows to use it for incremental non-realtime external FT index update or other replication
	last_update_time timestamp with time zone not null,
	search_vector tsvector generated always as (to_tsvector('public.mysearch', text)) stored,
	integrity_version int not null
);

do
$$ begin

create sequence if not exists public.note_id_seq;

ALTER SEQUENCE public.note_id_seq
    OWNER TO postgres;
GRANT ALL ON SEQUENCE public.note_id_seq TO pimweb;
GRANT ALL ON SEQUENCE public.note_id_seq TO postgres;

if exists (
        select  *
        from    information_schema.columns
                where table_name = 'note'
                and column_name = 'create_time'
                and data_type = 'timestamp without time zone'
    ) then
    alter table     public.note
    alter column    create_time type timestamp with time zone;
end if;

if exists (
        select  *
        from    information_schema.columns
                where table_name = 'note'
                and column_name = 'last_update_time'
                and data_type = 'timestamp without time zone'
    ) then
    alter table     public.note
    alter column    last_update_time type timestamp with time zone;
end if;

create index if not exists  idx_note_create_time
on                          public.note (create_time);

create index if not exists  idx_note_last_update_time
on                          public.note (last_update_time);

if not exists (
        select  *
        from    information_schema.columns
                where table_name = 'note'
                and column_name = 'search_vector'
                and data_type = 'tsvector'
    ) then
    alter 		table public.note
    add column  search_vector tsvector
    generated always	as (to_tsvector('public.mysearch', text)) stored;
end if;

if not exists (
        select  *
        from    information_schema.key_column_usage
                where table_name = 'note'
                and column_name = 'id'
                and ordinal_position = 1
    ) then
    alter table public.note
    add constraint  pk_note
    primary key     (id);
end if;

create index if not exists note_search_vector_idx on public.note using gin (search_vector);

grant insert, select, update, delete on table public.note to pimweb;
grant all on sequence public.note_id_seq to pimweb;

end $$;