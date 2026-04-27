CREATE TABLE IF NOT EXISTS public.user_info
(
    user_info_id uuid NOT NULL,
    first_name character varying(100) COLLATE pg_catalog."default" NOT NULL,
    last_name character varying(100) COLLATE pg_catalog."default" NOT NULL,
    email_address character varying(200) COLLATE pg_catalog."default" NOT NULL,
    load_directory character varying(1000) COLLATE pg_catalog."default",
    log_directory character varying(1000) COLLATE pg_catalog."default",
	output_directory character varying(1000) COLLATE pg_catalog."default",
    update_date timestamp without time zone NOT NULL,
    update_by character varying(200) COLLATE pg_catalog."default" NOT NULL,
    deleted boolean NOT NULL,
    CONSTRAINT "user_PK" PRIMARY KEY (user_info_id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.user_info
    OWNER to dvtadmin;

REVOKE ALL ON TABLE public.user_info FROM dvt_core_api_nonprod;

GRANT DELETE, INSERT, SELECT, UPDATE ON TABLE public.user_info TO dvt_core_api_nonprod;

GRANT ALL ON TABLE public.user_info TO dvtadmin;