DELIMITER //
CREATE TRIGGER  tg_Digitial_Bundle
AFTER INSERT 
ON tb_documents FOR EACH ROW 
BEGIN
   DECLARE v_docType varchar(100) DEFAULT "";
   DECLARE v_projectID varchar(100) DEFAULT "";
   DECLARE v_taskid varchar(100) DEFAULT "";
   DECLARE v_dbtask varchar(10) DEFAULT "";
   DECLARE v_clientid varchar(100) DEFAULT "";
set v_docType=new.`document_type`;
set v_projectID =new.`project_id`;
set v_taskid = new.task_id;

set v_clientid=(SELECT company_id FROM `tb_projects` where project_id=v_projectID);
set v_dbtask = (select engine_process from tb_tasks where task_id=v_taskid and company_id in (11,18));

IF (v_dbtask='4') THEN
	IF ((v_docType='.zip') and ((v_clientid=11) or (v_clientid=18))) THEN
		INSERT INTO tbl_digitalbundle_pdf(project_id, chapter_id,task_id,user_id,document_id,start_date,status,xml,remarks) VALUES (New.project_id,NEW.chapter_id,NEW.task_id,NEW.user_id,New.document_id,Now(),0,1,'trigger');
	END IF;
END IF;
END; //
DELIMITER ;