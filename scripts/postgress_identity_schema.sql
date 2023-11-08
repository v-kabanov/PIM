begin transaction;
--commit
-- rollback

create sequence if not exists public.aspnet_identity_id_seq
    increment 1
    start 1
    minvalue 1
    maxvalue 2147483647
    cache 1;

alter sequence public.aspnet_identity_id_seq
    owner to postgres;
    
drop sequence if exists public.snow_flake_id_seq cascade;
drop table if exists public.aspnet_roles cascade;
drop table if exists public.aspnet_users cascade;
drop table if exists public.aspnet_user_claims cascade;
drop table if exists public.aspnet_user_roles cascade;
drop table if exists public.aspnet_user_logins cascade;
drop table if exists public.aspnet_user_tokens cascade;
drop table if exists public.aspnet_role_claims cascade;

-- alter table public.aspnet_user_roles drop constraint fk_aspnet_roles_id

create table if not exists public.aspnet_roles
(
	id int not null default nextval('public.aspnet_identity_id_seq')::int,
    name character varying(64) collate pg_catalog."default" not null,
    normalized_name character varying(64) collate pg_catalog."default" not null,
    concurrency_stamp character varying(36) collate pg_catalog."default",
    constraint pk_aspnet_roles primary key (id),
    constraint u_aspnet_roles_name unique (name),
    constraint u_aspnet_roles_normalized_name unique (normalized_name)
);

alter table public.aspnet_roles
    owner to postgres;

create table if not exists public.aspnet_users
(
	id int not null default nextval('public.aspnet_identity_id_seq')::int,
    user_name character varying(64) collate pg_catalog."default" not null,
    normalized_user_name character varying(64) collate pg_catalog."default" not null,
    email character varying(256) collate pg_catalog."default" not null,
    normalized_email character varying(256) collate pg_catalog."default" not null,
    email_confirmed boolean not null,
    password_hash character varying(256) collate pg_catalog."default",
    security_stamp character varying(256) collate pg_catalog."default",
    concurrency_stamp character varying(36) collate pg_catalog."default",
    phone_number character varying(32) collate pg_catalog."default",
    phone_number_confirmed boolean not null,
    two_factor_enabled boolean not null,
    --lockout_end_unix_time_seconds bigint,
    lockout_end timestamp with time zone null,
    lockout_enabled boolean not null,
    access_failed_count integer not null,
    constraint pk_aspnet_users primary key (id),
    constraint u_aspnet_users_normalized_user_name unique (normalized_user_name),
    constraint u_aspnet_users_username unique (user_name)
);

alter table public.aspnet_users
    owner to postgres;

-- drop index public.ix_aspnet_users_email;

create index if not exists ix_aspnet_users_email
    on public.aspnet_users using btree
    (normalized_email collate pg_catalog."default")
    tablespace pg_default;

-- drop index public.ix_aspnet_users_user_name;

do
$$ begin
if not exists (
        select  *
        from    information_schema.columns
                where table_name = 'aspnet_roles'
                and column_name = 'id'
                and column_default is not null
) then
    alter table public.aspnet_roles
    alter column id set default (nextval('public.aspnet_identity_id_seq')::int);
end if;

if not exists (
        select  *
        from    information_schema.columns
                where table_name = 'aspnet_users'
                and column_name = 'id'
                and column_default is not null
) then
    alter table public.aspnet_users
    alter column id set default (nextval('public.aspnet_identity_id_seq')::int);
end if;

if exists (
        select  *
        from    information_schema.columns
                where table_name = 'aspnet_users'
                and column_name = 'lockout_end_unix_time_seconds'
) then
    alter table public.aspnet_users
    drop column lockout_end_unix_time_seconds;
    
    alter table public.aspnet_users
    add column  lockout_end timestamp with time zone;
end if;

if not exists (
        select  *
        from    information_schema.key_column_usage
                where table_name = 'aspnet_roles'
                and column_name = 'id'
) then
    create unique index ix_aspnet_users_user_name
        on public.aspnet_users using btree
        (normalized_user_name collate pg_catalog."default")
        tablespace pg_default;
end if;

end $$;

-- drop index public.ix_aspnet_roles_name;

create index ix_aspnet_roles_name
    on public.aspnet_roles using btree
    (normalized_name collate pg_catalog."default")
    tablespace pg_default;


create table public.aspnet_role_claims
(
    id integer not null default nextval('aspnet_identity_id_seq'::regclass),
    role_id int not null,
    claim_type character varying(1024) collate pg_catalog."default" not null,
    claim_value character varying(1024) collate pg_catalog."default" not null,
    constraint pk_aspnet_role_claims primary key (id),
    constraint fk_aspnet_roles_id foreign key (role_id)
        references public.aspnet_roles (id) match simple
        on update cascade
        on delete cascade
);

alter table public.aspnet_role_claims
    owner to postgres;
    
-- drop index public.ix_aspnet_role_claims_role_id;

create index if not exists ix_aspnet_role_claims_role_id
    on public.aspnet_role_claims using btree
    (role_id)
    tablespace pg_default;

create table public.aspnet_user_claims
(
    id integer not null default nextval('aspnet_identity_id_seq'::regclass),
    user_id int not null,
    claim_type character varying(1024) collate pg_catalog."default" not null,
    claim_value character varying(1024) collate pg_catalog."default" not null,
    constraint pk_aspnet_user_claims primary key (id),
    constraint fk_aspnet_users_id foreign key (user_id)
        references public.aspnet_users (id) match simple
        on update cascade
        on delete cascade
);

alter table public.aspnet_user_claims
    owner to postgres;

-- drop index public.ix_aspnet_user_claims_user_id;

create index ix_aspnet_user_claims_user_id
    on public.aspnet_user_claims using btree
    (user_id)
    tablespace pg_default;

-- drop table public.aspnet_user_logins;

create table public.aspnet_user_logins
(
    login_provider character varying(32) collate pg_catalog."default" not null,
    provider_key character varying(1024) collate pg_catalog."default" not null,
    provider_display_name character varying(32) collate pg_catalog."default" not null,
    user_id int not null,
    constraint pk_aspnet_user_logins primary key (login_provider, provider_key),
    constraint fk_aspnet_user_logins_user_id foreign key (user_id)
        references public.aspnet_users (id) match simple
        on update cascade
        on delete cascade
);

alter table public.aspnet_user_logins
    owner to postgres;

-- drop index public.ix_aspnet_user_logins_user_id;

create index ix_aspnet_user_logins_user_id
    on public.aspnet_user_logins using btree
    (user_id)
    tablespace pg_default;


-- drop table public.aspnet_user_tokens;

create table public.aspnet_user_tokens
(
    user_id int not null,
    login_provider character varying(32) collate pg_catalog."default" not null,
    name character varying(32) collate pg_catalog."default" not null,
    value character varying(256) collate pg_catalog."default",
    constraint pk_aspnet_user_tokens primary key (user_id, login_provider, name),
    constraint fk_aspnet_users_id foreign key (user_id)
        references public.aspnet_users (id) match simple
        on update cascade
        on delete cascade
);

alter table public.aspnet_user_tokens
    owner to postgres;

-- drop index public.ix_aspnet_user_tokens_user_id;

create index if not exists ix_aspnet_user_tokens_user_id
    on public.aspnet_user_tokens using btree
    (user_id)
    tablespace pg_default;

-- drop table public.aspnet_user_roles;

create table public.aspnet_user_roles
(
    user_id int not null,
    role_id int not null,
    constraint pk_aspnet_user_roles primary key (user_id, role_id),
    constraint fk_aspnet_roles_id foreign key (role_id)
        references public.aspnet_roles (id) match simple
        on update cascade
        on delete cascade,
    constraint fk_aspnet_users_id foreign key (user_id)
        references public.aspnet_users (id) match simple
        on update cascade
        on delete cascade
);

alter table public.aspnet_user_roles
    owner to postgres;

-- drop index public.ix_aspnet_user_roles_role_id;

create index if not exists ix_aspnet_user_roles_role_id
    on public.aspnet_user_roles using btree
    (role_id)
    tablespace pg_default;

-- drop index public.ix_aspnet_user_roles_user_id;

create index ix_aspnet_user_roles_user_id
    on public.aspnet_user_roles using btree
    (user_id)
    tablespace pg_default;

grant all on sequence public.aspnet_identity_id_seq to pimweb;
grant insert, select, update, delete on table public.aspnet_users to pimweb;
grant insert, select, update, delete on table public.aspnet_roles to pimweb;
grant insert, select, update, delete on table public.aspnet_user_claims to pimweb;
grant insert, select, update, delete on table public.aspnet_user_logins to pimweb;
grant insert, select, update, delete on table public.aspnet_user_roles to pimweb;
grant insert, select, update, delete on table public.aspnet_user_tokens to pimweb;
grant insert, select, update, delete on table public.aspnet_role_claims to pimweb;
