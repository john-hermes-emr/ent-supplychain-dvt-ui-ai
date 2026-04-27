CREATE TABLE IF NOT EXISTS public.option_list_item
(
    option_id uuid NOT NULL,
    option_name character varying(50) COLLATE pg_catalog."default" NOT NULL,
    category_name character varying(50) COLLATE pg_catalog."default" NOT NULL,
    help_text character varying(50) COLLATE pg_catalog."default",
    sort_field integer,
    update_by character varying(200) COLLATE pg_catalog."default" NOT NULL,
    update_date timestamp without time zone NOT NULL,
    deleted boolean NOT NULL,
    CONSTRAINT option_list_item_pkey PRIMARY KEY (option_id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.option_list_item
    OWNER to dvtadmin;

REVOKE ALL ON TABLE public.option_list_item FROM dvt_core_api_nonprod;

GRANT DELETE, INSERT, SELECT, UPDATE ON TABLE public.option_list_item TO dvt_core_api_nonprod;

GRANT ALL ON TABLE public.option_list_item TO dvtadmin;