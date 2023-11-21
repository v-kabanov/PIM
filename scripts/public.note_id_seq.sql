-- SEQUENCE: public.note_id_seq

-- DROP SEQUENCE IF EXISTS public.note_id_seq;

CREATE SEQUENCE IF NOT EXISTS public.note_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 2147483647
    CACHE 1;

ALTER SEQUENCE public.note_id_seq
    OWNER TO postgres;

GRANT ALL ON SEQUENCE public.note_id_seq TO pimweb;

GRANT ALL ON SEQUENCE public.note_id_seq TO postgres;