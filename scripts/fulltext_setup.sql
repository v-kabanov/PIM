CREATE TEXT SEARCH DICTIONARY public.russian_hunspell (
    TEMPLATE = ispell,
    DictFile = ru_ru,
    AffFile = ru_ru,
    Stopwords = russian);
    
CREATE TEXT SEARCH DICTIONARY public.english_gb_hunspell (
    TEMPLATE = ispell,
    DictFile = en_gb,
    AffFile = en_gb,
    Stopwords = english);

CREATE TEXT SEARCH DICTIONARY public.english_us_hunspell (
    TEMPLATE = ispell,
    DictFile = en_us,
    AffFile = en_us,
    Stopwords = english);
---------------------------------------------------------------
-- DROP TEXT SEARCH CONFIGURATION public.mysearch

CREATE TEXT SEARCH CONFIGURATION public.mysearch (
	PARSER = default
);
ALTER TEXT SEARCH CONFIGURATION public.mysearch alter MAPPING FOR asciihword WITH public.english_gb_hunspell, public.english_us_hunspell, english_stem;
ALTER TEXT SEARCH CONFIGURATION public.mysearch alter MAPPING FOR asciiword WITH public.english_gb_hunspell, public.english_us_hunspell, english_stem;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR email WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR file WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR float WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR host WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR hword WITH public.russian_hunspell, russian_stem;
ALTER TEXT SEARCH CONFIGURATION public.mysearch alter MAPPING FOR hword_asciipart WITH public.english_gb_hunspell, public.english_us_hunspell, english_stem;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR hword_numpart WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR hword_part WITH public.russian_hunspell, russian_stem;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR int WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR numhword WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR numword WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR sfloat WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR uint WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR url WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR url_path WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR version WITH simple;
ALTER TEXT SEARCH CONFIGURATION public.mysearch ADD MAPPING FOR word WITH public.russian_hunspell, russian_stem;
