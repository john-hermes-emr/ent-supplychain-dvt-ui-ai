CREATE TABLE IF NOT EXISTS public.site_master
(
    site_master_id character varying(100) COLLATE pg_catalog."default" NOT NULL,
    division_id character varying(100) COLLATE pg_catalog."default",
    local_site_id character varying(100) COLLATE pg_catalog."default",
    description character varying(100) COLLATE pg_catalog."default",
    update_date timestamp without time zone NOT NULL,
    update_by character varying(200) COLLATE pg_catalog."default" NOT NULL,
    deleted boolean NOT NULL,
    CONSTRAINT site_master_pkey PRIMARY KEY (site_master_id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.site_master
    OWNER to dvtadmin;

REVOKE ALL ON TABLE public.site_master FROM dvt_core_api_nonprod;

GRANT DELETE, INSERT, SELECT, UPDATE ON TABLE public.site_master TO dvt_core_api_nonprod;

GRANT ALL ON TABLE public.site_master TO dvtadmin;