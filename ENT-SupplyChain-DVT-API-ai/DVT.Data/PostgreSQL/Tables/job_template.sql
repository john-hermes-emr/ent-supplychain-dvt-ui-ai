CREATE TABLE IF NOT EXISTS public.job_template
(
    job_template_id uuid NOT NULL,
    template_name character varying(50) COLLATE pg_catalog."default",
    file_template_ids uuid[],
    create_by character varying(200) COLLATE pg_catalog."default" NOT NULL,
    create_date timestamp without time zone NOT NULL,
    update_date timestamp without time zone NOT NULL,
    update_by character varying(200) COLLATE pg_catalog."default" NOT NULL,
    deleted boolean NOT NULL,
    CONSTRAINT job_template_pkey PRIMARY KEY (job_template_id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.job_template
    OWNER to dvtadmin;

REVOKE ALL ON TABLE public.job_template FROM dvt_core_api_nonprod;

GRANT DELETE, INSERT, SELECT, UPDATE ON TABLE public.job_template TO dvt_core_api_nonprod;

GRANT ALL ON TABLE public.job_template TO dvtadmin;