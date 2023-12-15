create sequence if not exists public.file_id_seq;

alter sequence public.file_id_seq owner to postgres;
grant all on sequence public.file_id_seq to pimweb;
grant all on sequence public.file_id_seq to postgres;

--drop table if exists public.file cascade
create table if not exists public.file (
	id int not null,
    relative_path varchar(8000) not null,
    content_hash bytea not null,
    title varchar(100) not null,
	description varchar null,
    extracted_text varchar,
	create_time timestamp with time zone not null,
	last_update_time timestamp with time zone not null,
    search_vector tsvector generated always as (to_tsvector('public.mysearch', relative_path || ' ' || coalesce(title, '') || ' ' || coalesce(description, '') || ' ' || coalesce(extracted_text, ''))) stored,
	integrity_version int not null
);

do
$$ begin

create index if not exists  idx_file_create_time
on                          public.file (create_time);

create index if not exists  idx_file_last_update_time
on                          public.file (last_update_time);

create index if not exists  idx_file_hash
on                          public.file (content_hash);

if not exists (
        select  *
        from    information_schema.key_column_usage
                where table_name = 'file'
                and column_name = 'id'
) then
    alter table public.file
    add constraint  pk_file
    primary key     (id);
end if;

if not exists (
        select  *
        from    information_schema.columns
                where table_name = 'file'
                and column_name = 'id'
                and column_default is not null
) then
    alter table public.file
    alter column id set default (nextval('public.file_id_seq')::int);
end if;

create index if not exists file_search_vector_idx on public.file using gin (search_vector);

end $$;

grant insert, select, update, delete on table public.file to pimweb;
