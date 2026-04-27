--User Story 11624709: DVT - Help Menu - development/testing
CREATE TABLE IF NOT EXISTS public.config_setting
(
    setting_id uuid NOT NULL,
    module character varying(200) COLLATE pg_catalog."default" NOT NULL,
    name character varying(200) COLLATE pg_catalog."default" NOT NULL,
    data_type character varying(200) COLLATE pg_catalog."default" NOT NULL,
    value character varying(2000) COLLATE pg_catalog."default" NOT NULL,
    updated_by character varying(200) COLLATE pg_catalog."default" NOT NULL,
    updated_date timestamp without time zone NOT NULL,
    deleted boolean NOT NULL,
    CONSTRAINT config_setting_pkey PRIMARY KEY (setting_id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.config_setting
    OWNER to dvtadmin;

REVOKE ALL ON TABLE public.config_setting FROM dvt_core_api_nonprod;

GRANT DELETE, INSERT, SELECT, UPDATE ON TABLE public.config_setting TO dvt_core_api_nonprod;

GRANT ALL ON TABLE public.config_setting TO dvtadmin;