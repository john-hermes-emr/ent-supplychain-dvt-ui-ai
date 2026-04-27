CREATE TABLE IF NOT EXISTS public.job
(
    job_id uuid NOT NULL,
    division_id uuid NOT NULL,	
    feed_number integer,
	status character varying(20) COLLATE pg_catalog."default" NOT NULL,
	user_info_id uuid NOT NULL,
    archive_file_path character varying(500) COLLATE pg_catalog."default",
    create_by character varying(200) COLLATE pg_catalog."default" NOT NULL,
    create_date timestamp without time zone NOT NULL,
    update_by character varying(200) COLLATE pg_catalog."default" NOT NULL,
    update_date timestamp without time zone NOT NULL,
    deleted boolean NOT NULL,
    CONSTRAINT job_pkey PRIMARY KEY (job_id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.job
    OWNER to dvtadmin;

REVOKE ALL ON TABLE public.job FROM dvt_core_api_nonprod;

GRANT DELETE, INSERT, SELECT, UPDATE ON TABLE public.job TO dvt_core_api_nonprod;

GRANT ALL ON TABLE public.job TO dvtadmin;

-- Create an index on job_id and deleted for faster queries
CREATE INDEX IF NOT EXISTS idx_job_get_by_id ON public.job (job_id, deleted);

-- Create an index on user_info_id, job_status and deleted for faster queries
CREATE INDEX IF NOT EXISTS idx_job_get_active_job ON public.job (user_info_id, status, deleted);