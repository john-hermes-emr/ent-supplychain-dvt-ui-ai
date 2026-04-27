CREATE TABLE IF NOT EXISTS public.activity_log
(
    log_id uuid NOT NULL,
    entity_id uuid NOT NULL,
    entity character varying(100) COLLATE pg_catalog."default" NOT NULL,
    message_type character varying(50) COLLATE pg_catalog."default" NOT NULL,
    message character varying(1000) COLLATE pg_catalog."default" NOT NULL,
    create_by character varying(200) COLLATE pg_catalog."default" NOT NULL,
    create_date timestamp without time zone NOT NULL,
    deleted boolean NOT NULL,
    CONSTRAINT log_pkey PRIMARY KEY (log_id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.activity_log
    OWNER to dvtadmin;

REVOKE ALL ON TABLE public.activity_log FROM dvt_core_api_nonprod;

GRANT DELETE, INSERT, SELECT, UPDATE ON TABLE public.activity_log TO dvt_core_api_nonprod;

GRANT ALL ON TABLE public.activity_log TO dvtadmin;

-- Create Index on entity_id for faster queries
CREATE INDEX IF NOT EXISTS idx_activity_log_entity_id ON public.activity_log (entity_id);