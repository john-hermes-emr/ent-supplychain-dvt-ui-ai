-- Table: public.job_file

-- DROP TABLE IF EXISTS public.job_file;

CREATE TABLE IF NOT EXISTS public.job_file
(
    job_file_id uuid NOT NULL,
    job_id uuid NOT NULL,
    file_name character varying(200) COLLATE pg_catalog."default",
    file_path character varying(500) COLLATE pg_catalog."default",
	table_name character varying(100) COLLATE pg_catalog."default",
    file_type character varying(20) COLLATE pg_catalog."default",
    sort_order integer,
    depends_on_file_type character varying(100) COLLATE pg_catalog."default",   
    status character varying(20) COLLATE pg_catalog."default",
    file_creation_timestamp timestamp without time zone,
    file_last_modified_timestamp timestamp without time zone,
    record_count integer,
    load_date timestamp without time zone,
    validation_message jsonb,
    validation_stats jsonb,
    update_date timestamp without time zone NOT NULL,
    update_by character varying(200) COLLATE pg_catalog."default" NOT NULL,
    deleted boolean NOT NULL,
    CONSTRAINT job_file_pkey PRIMARY KEY (job_file_id),
    CONSTRAINT "JobFile_Job_FK" FOREIGN KEY (job_id)
        REFERENCES public.job (job_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.job_file
    OWNER to dvtadmin;

REVOKE ALL ON TABLE public.job_file FROM dvt_core_api_nonprod;

GRANT INSERT, DELETE, SELECT, UPDATE ON TABLE public.job_file TO dvt_core_api_nonprod;

GRANT ALL ON TABLE public.job_file TO dvtadmin;

-- Create an index on job_id for faster lookups
CREATE INDEX IF NOT EXISTS idx_job_file_job_id ON public.job_file (job_id);