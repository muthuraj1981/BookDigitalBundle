CREATE DEFINER=`devopslive`@`%` TRIGGER `journals_live`.`tg_Digitial_Bundle` AFTER INSERT ON `tb_documents` FOR EACH ROW
BEGIN
DECLARE v_docType varchar(100) DEFAULT "";
   DECLARE v_projectID varchar(100) DEFAULT "";
   DECLARE v_taskid varchar(100) DEFAULT "";
   DECLARE v_cloud_sync varchar(100) DEFAULT "";
   DECLARE v_dbtask varchar(10) DEFAULT "";
   DECLARE v_clientid varchar(100) DEFAULT "";
   DECLARE v_documentname varchar(100) DEFAULT "";
   DECLARE v_taskname varchar(100) DEFAULT "";   
set v_docType=new.`document_type`;
set v_projectID =new.`project_id`;
set v_documentname = new.`document_name`;
set v_taskid = new.task_id;
set v_cloud_sync = new.cloud_sync;
set v_taskname = new.document_description;
set v_taskname=(select LCASE(v_taskname));
set v_clientid=(SELECT company_id FROM `tb_projects` where project_id=v_projectID);
set v_dbtask = (select engine_process from tb_tasks where task_id=v_taskid and company_id in (11,18,5));
	IF ((v_docType='.zip') and (v_cloud_sync=0) and (v_documentname like '%DB_Package%') and ((v_clientid=11) or (v_clientid=18) or (v_clientid=5))) THEN
	    IF ((v_taskname='first pages to pm and for xml validation') and (v_clientid<>5)) THEN
			INSERT INTO tbl_digitalbundle_pdf(project_id, chapter_id,task_id,user_id,document_id,start_date,status,stage,xml,remarks) VALUES (New.project_id,NEW.chapter_id,NEW.task_id,NEW.user_id,New.document_id,Now(),0,1,1,'trigger');
		END IF;
		IF ((v_taskname='first pages typesetting') and (v_clientid<>5)) THEN
			INSERT INTO tbl_digitalbundle_pdf(project_id, chapter_id,task_id,user_id,document_id,start_date,status,stage,xml,remarks) VALUES (New.project_id,NEW.chapter_id,NEW.task_id,NEW.user_id,New.document_id,Now(),0,1,1,'trigger');
		END IF;
		
		IF ((v_taskname='export xml to eproduct team') and (v_clientid<>5)) THEN
			INSERT INTO tbl_digitalbundle_pdf(project_id, chapter_id,task_id,user_id,document_id,start_date,status,stage,xml,webpdf,cover,remarks) VALUES (New.project_id,NEW.chapter_id,NEW.task_id,NEW.user_id,New.document_id,Now(),0,1,1,1,1,'trigger');
		END IF;
		IF (v_clientid=5) THEN 
			IF ((v_taskname='revert xml to ep team') and (v_clientid=5)) THEN
				INSERT INTO tbl_digitalbundle_pdf(project_id, chapter_id,task_id,user_id,document_id,start_date,status,stage,xml,remarks) VALUES (New.project_id,NEW.chapter_id,NEW.task_id,NEW.user_id,New.document_id,Now(),0,1,1,'trigger');
			END IF;
			IF ((v_taskname='db processing – web pdf') and (v_clientid=5)) THEN
				INSERT INTO tbl_digitalbundle_pdf(project_id, chapter_id,task_id,user_id,document_id,start_date,status,stage,xml,POD,package,webpdf,cover,RTF,remarks) VALUES (New.project_id,NEW.chapter_id,NEW.task_id,NEW.user_id,New.document_id,Now(),0,1,1,1,1,1,1,1,'trigger');
			END IF;
		END IF;
	END IF;
END