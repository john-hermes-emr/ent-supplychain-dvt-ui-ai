CREATE TABLE IF NOT EXISTS public.file_template
(
    file_template_id uuid NOT NULL,
    depends_on_template_id uuid[],
    file_type character varying(20) COLLATE pg_catalog."default",
    file_name_format character varying(100) COLLATE pg_catalog."default",
    column_info jsonb,
    sort_order integer,
    optional boolean,
    update_date timestamp without time zone NOT NULL,
    update_by character varying(200) COLLATE pg_catalog."default" NOT NULL,
    deleted boolean NOT NULL,
    CONSTRAINT file_template_pkey PRIMARY KEY (file_template_id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.file_template
    OWNER to dvtadmin;

REVOKE ALL ON TABLE public.file_template FROM dvt_core_api_nonprod;

GRANT DELETE, INSERT, SELECT, UPDATE ON TABLE public.file_template TO dvt_core_api_nonprod;

GRANT ALL ON TABLE public.file_template TO dvtadmin;