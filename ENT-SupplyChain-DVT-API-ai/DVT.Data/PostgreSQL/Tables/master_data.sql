CREATE TABLE IF NOT EXISTS public.master_data
(
    item_id uuid NOT NULL,
    table_name character varying(50) COLLATE pg_catalog."default" NOT NULL,
    text_id character varying(50) COLLATE pg_catalog."default" NOT NULL,
    item_name character varying(200) COLLATE pg_catalog."default" NOT NULL,
    item_name_abbrev character varying(100) COLLATE pg_catalog."default" NOT NULL,
    text1 character varying(100) COLLATE pg_catalog."default",
    text2 character varying(100) COLLATE pg_catalog."default",
    text3 character varying(100) COLLATE pg_catalog."default",
    text4 character varying(100) COLLATE pg_catalog."default",
    text5 character varying(100) COLLATE pg_catalog."default",
    text6 character varying(100) COLLATE pg_catalog."default",
    update_date timestamp without time zone NOT NULL,
    update_by character varying(200) COLLATE pg_catalog."default" NOT NULL,
    deleted boolean NOT NULL,
    CONSTRAINT master_data_pkey PRIMARY KEY (item_id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.master_data
    OWNER to dvtadmin;

REVOKE ALL ON TABLE public.master_data FROM dvt_core_api_nonprod;

GRANT DELETE, INSERT, SELECT, UPDATE ON TABLE public.master_data TO dvt_core_api_nonprod;

GRANT ALL ON TABLE public.master_data TO dvtadmin;

-- Create index on item_id and deleted for faster queries
CREATE INDEX IF NOT EXISTS idx_master_data_item_id_deleted ON public.master_data (item_id, deleted);

-- Crate an index on deleted flag, table_name and text_id for faster queries
CREATE INDEX IF NOT EXISTS idx_master_data_unique_table_names ON public.master_data (deleted, table_name, text_id);

