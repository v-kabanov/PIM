create table if not exists public.note_file (
    note_id     int not null,
    file_id     int not null
);

if not exists (
        select  *
        from    information_schema.key_column_usage c1
                join information_schema.key_column_usage c2
                    on c2.table_name = c1.table_name
                    and c2.constraint_name = c1.constraint_name
                    and c2.constraint_schema = c1.constraint_schema
        where   c1.table_name = 'note_file'
                and c1.ordinal_position = 1
                and c1.column_name = 'note_id'
                and c2.ordinal_position = 2
                and c2.column_name = 'file_id'
) then
    alter table public.note_file
    add constraint  pk_note_file
    primary key     (note_id, file_id);
end if;

create index if not exists  idx_note_file__file_id
on                          public.note_file (file_id);
