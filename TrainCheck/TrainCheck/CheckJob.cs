﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Data;


namespace TrainCheck
{
    public class JobMain
    {
        public Int32 ID { get; set; }
        public DateTime JobDate { get; set; }
        public Int32 UserID { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public String IpAddress { get; set; }
        public Boolean IsUploaded { get; set; }
        public DateTime UploadDateTime { get; set; }
        public Int32 NeedCheckPosition { get; set; }
        public Int32 CheckPosition { get; set; }
        public Int32 PassPosition { get; set; }
        public List<JobDetail> _Items;
        public List<JobDetail> Items
        {
            get
            {
                if (_Items == null)
                    _Items = new List<JobDetail>();
                return _Items;
            }
        }
        public JobDetail FindBySpecsID(int specsid)
        {
            foreach (JobDetail detail in Items)
            {
                if (detail.SpecsID == specsid)
                    return detail;
            }
            return null;
        }
        public void SetCheckToSpecs(Specs spec)
        {
            JobDetail detail = FindBySpecsID(spec.ID);
            if (detail != null)
            {
                foreach (string kv in detail.CheckDetailList.Split(','))
                {

                    String[] checkresults = kv.Split('=');
                    if (checkresults.Length == 2)
                    {
                        int spid = Int32.Parse(checkresults[0]);
                        bool ispass = checkresults[1] == "1" ? true : false;
                        SpecsDetail spdetail = spec.FindByID(spid);
                        if (spdetail != null)
                        {
                            spdetail.isChecked = ispass;
                            spdetail.isDone = true;
                        }
                    }
                }
                spec.IsCheckAll = true;
            }
        }
        
    }
    public class JobDetail
    {
        public Int32 ID { get; set; }
        public Int32 JobID { get; set; }
        public Int32 SpecsID { get; set; }
        public DateTime CheckTime { get; set; }
        public Boolean isChecked { get; set; }
        public String CheckDetailList { get; set; }
    }
    public class SpecsDetail
    {
        public Int32 ID { get; set; }
        public Int32 SpecsID { get; set; }
        public String CheckDetail { get; set; }
        public String CheckMethod { get; set; }
        public String SpecifiedSizeHeight { get; set; }
        public String KnockPosition { get; set; }
        public bool isChecked { get; set; }
        public bool isDone { get; set; }
        public SpecsDetail()
        { }
        public SpecsDetail(int id, int specsID, string checkdetail, string checkmethod, string sh, string knock, bool ischeck)
        {
            ID = id;
            SpecsID = specsID;
            CheckDetail = checkdetail;
            SpecifiedSizeHeight = sh;
            CheckMethod = checkmethod;
            KnockPosition = knock;
            isChecked= ischeck;
        }
    }
    public class Specs
    {
        public Int32 ID { get; set; }
        public String Seciton { get; set; }
        public String CheckPosition { get; set; }
        public String CheckMethod { get; set; }
        public String BarCode { get; set; }
        public Int32 Sequence { get; set; }
        public Boolean IsCheckAll { get; set; }
        private List<SpecsDetail> _items ;
        public List<SpecsDetail> Items 
        {
            get
            {
                if (_items == null)
                    _items = new List<SpecsDetail>();
                return _items;
            }
            
        }
        public SpecsDetail FindByID(int detailID)
        {
            return Items.Single(detail => detail.ID == detailID);    
        }
        public String CheckDetailList
        {
            get
            {
                return String.Join(",", Items.Select<SpecsDetail, String>(detail => string.Format("{0}={1}", detail.ID, detail.isChecked ? 1 : 0)).ToArray());
            }
        }
    }
    public class DbFactory
    {
        public static Specs FindBySequence(int sequ)
        {
            return FindByFilter(String.Format("sequence={0}",sequ));
        }
    
        public static Specs Find(String barcode)
        {
            return FindByFilter(String.Format("barcode='{0}'",barcode));
        }
        public static Specs FindByFilter(String sFilter)
        {
            Specs result = new Specs();
            using (IDataReader dr = DataAccess.ExecuteReader(String.Format("select * from DictSpecs where {0}", sFilter)))
            {
                if (dr.Read())
                {
                    result.BarCode = (dr["barcode"]==DBNull.Value)?"":dr["barcode"].ToString();
                    result.ID = Int32.Parse(dr["ID"].ToString());
                    result.Sequence = Int32.Parse(dr["Sequence"].ToString());
                    result.Seciton = dr["Section"].ToString();
                    result.CheckPosition = dr["CheckPosition"].ToString();
                    result.CheckMethod = dr["CheckMethod"].ToString();
                    dr.Close();
                }
                else
                {
                    
                    return null;
                }
            }
            using (IDataReader dr = DataAccess.ExecuteReader(String.Format("select * from DictSpecsItems where DictSpecsID={0}", result.ID)))
            {
                while (dr.Read())
                {
                    SpecsDetail detail = new SpecsDetail();
                    detail.ID=Int32.Parse(dr["id"].ToString());
                    detail.SpecsID=result.ID;
                    detail.SpecifiedSizeHeight=(dr["SpecifiedSizeHeight"] == DBNull.Value) ? "" : dr["SpecifiedSizeHeight"].ToString();
                    detail.CheckDetail=dr["CheckDetail"].ToString();
                    detail.CheckMethod=(dr["CheckMethod"] == DBNull.Value) ? result.CheckMethod : dr["CheckMethod"].ToString();
                    detail.KnockPosition =(dr["KnockPosition"] == DBNull.Value) ? "" : dr["KnockPosition"].ToString();
                    result.Items.Add(detail);
                }
            }
            return result;
        }
        public static bool SaveSpecs(Specs spec)
        {
            return false;
        }
        public static JobMain Create(Int32 id)
        { 
            JobMain result;
            if (id == 0)
            {
                result = new JobMain();                
                result.UserID = AppHelper.UserID;               
                result.ID= SaveJob(result);
            }
            else
                result = FindJobMain(id);
            return result;
    
        }
        public static JobMain FindJobMainBySQL(String sqlString)
        {
            JobMain result = new JobMain();
            using (IDataReader dr = DataAccess.ExecuteReader(sqlString))
            {
                if (dr.Read())
                {
                    result.ID = Int32.Parse(dr["ID"].ToString());
                    result.JobDate = DateTime.Parse(dr["JobDate"].ToString());
                    result.UserID = Int32.Parse(dr["UserID"].ToString());
                    result.BeginTime = DateTime.Parse(dr["beginTime"].ToString());
                    result.EndTime = (dr["EndTime"] == DBNull.Value) ? default(DateTime) : DateTime.Parse(dr["EndTime"].ToString());
                    result.IsUploaded = (dr["IsUploaded"] == DBNull.Value) ? false : Boolean.Parse(dr["isUploaded"].ToString());
                    result.NeedCheckPosition = (dr["NeedCheckPosition"] == DBNull.Value) ? 0 : Int32.Parse(dr["NeedCheckPosition"].ToString());
                    result.CheckPosition = (dr["CheckPosition"] == DBNull.Value) ? 0 : Int32.Parse(dr["CheckPosition"].ToString());
                    result.PassPosition = (dr["PassPosition"] == DBNull.Value) ? 0 : Int32.Parse(dr["PassPosition"].ToString());
                    dr.Close();
                }
                else
                    return null;

            }
            using (IDataReader dr = DataAccess.ExecuteReader(String.Format("Select * from JobDetail where Jobid={0}", result.ID)))
            {
                while (dr.Read())
                {
                    JobDetail item = new JobDetail();
                    item.ID = Int32.Parse(dr["ID"].ToString());
                    item.JobID = result.ID;
                    item.SpecsID = Int32.Parse(dr["SpecsID"].ToString());
                    item.CheckTime = (dr["CheckTime"] == DBNull.Value) ? default(DateTime) : DateTime.Parse(dr["CheckTime"].ToString());
                    item.isChecked = (dr["isChecked"] == DBNull.Value) ? false : Boolean.Parse(dr["isChecked"].ToString());
                    item.CheckDetailList = (dr["CheckDetailList"] == DBNull.Value) ? "" : dr["checkDetailList"].ToString();
                    result.Items.Add(item);
                }
                dr.Close();
            }
            return result;

        }
        public static JobMain FindJobMain(Int32 id)
        {
            return FindJobMainBySQL(String.Format("select * from jobMain where ID={0}", id));
        }
        public static int JobInsert(JobMain job)
        {
            int needCheckcount = Int32.Parse(DataAccess.ExecuteScalar("select count(*) from DictSpecs").ToString());
            String sqlstring = String.Format(@"Insert into JobMain(jobdate,userid,begintime,isUploaded,NeedCheckPosition,CheckPosition,PassPosition)
                                            Values(getdate(),{0},getdate(),0,{1},0,0)", job.UserID,needCheckcount);
            DataAccess.ExecuteNonQuery(sqlstring);
            int result = Int32.Parse(DataAccess.ExecuteScalar("Select max(id) from jobmain").ToString());
            job.ID = result;
            job.NeedCheckPosition = needCheckcount;
            return result;
        }
        public static int JobDetailUpdate(JobDetail detail)
        {
            return DataAccess.ExecuteNonQuery(String.Format(@"Update jobDetail set ischecked={0},checkTime='{1}',checkDetailList=
                                                                '{2}' where jobid={3} and specsID={4}    ",
                                                                  detail.isChecked ? 1 : 0,
                                                                  detail.CheckTime,
                                                                  detail.CheckDetailList,
                                                                  detail.JobID,
                                                                  detail.SpecsID));
        }
        public static int JobDetailInsert(JobDetail detail)
        {
            String sqlstring = string.Format(@"Insert into jobDetail (jobid,specsID,checkTime,ischecked,checkdetailList)
                                               Values({0},{1},'{2}',{3},'{4}')      ",
                                               detail.JobID,
                                               detail.SpecsID,
                                               detail.CheckTime,
                                               detail.isChecked ? 1 : 0,
                                               detail.CheckDetailList);
            DataAccess.ExecuteNonQuery(sqlstring);
            int result = Int32.Parse(DataAccess.ExecuteScalar("select max(id) from jobdetail").ToString());
            int checkP = Int32.Parse(DataAccess.ExecuteScalar(String.Format("select count(*) from jobdetail where jobid={0}",detail.JobID)).ToString());
            DataAccess.ExecuteNonQuery(String.Format("Update JobMain set CheckPosition={1} where id={0}", detail.JobID,checkP));
            return result;


        }
        public static int SaveJob(JobMain job)
        {
            StringBuilder sqlList = new StringBuilder();
            if (job.ID == 0)
            {
                int result = JobInsert(job);
                return result;

            }
            else
            {
                foreach (JobDetail detail in job.Items)
                {
                    if (detail.ID > 0)
                        sqlList.AppendFormat(@"Update jobDetail set ischecked={0},checkTime='{1}',checkDetailList=
                                                                '{2}' where jobid={3} and specsID={4}    ",
                                                                  detail.isChecked ? 1 : 0,
                                                                  detail.CheckTime,
                                                                  detail.CheckDetailList,
                                                                  detail.JobID,
                                                                  detail.SpecsID);
                    else
                        sqlList.AppendFormat(@"Insert into jobDetail (jobid,specsID,checkTime,ischecked,checkdetailList)
                                               Values({0},{1},'{2}',{3},'{4}')      ",
                                               job.ID,
                                               detail.SpecsID,
                                               detail.CheckTime,
                                               detail.isChecked ? 1 : 0,
                                               detail.CheckDetailList);

                    DataAccess.ExecuteNonQuery(sqlList.ToString());


                }

                return 1;
            }
          
        }
        public static JobMain FindToDayJob()
        {
            JobMain result = FindJobMainBySQL(String.Format("select * from jobmain where NeedCheckPosition>CheckPosition and isuploaded=0 and Userid={0} and dateDiff(Day,jobDate,getdate())=0",AppHelper.UserID));
            return result;
        }
        public static bool DeleteJobMain(JobMain job)
        {
            int result = DataAccess.ExecuteNonQuery(String.Format("delete from jobdetail where jobid={0}", job.ID));
            result = DataAccess.ExecuteNonQuery(String.Format("delete from jobmain where id={0}", job.ID));
            return result > 0;
        }
    }
}
