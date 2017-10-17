﻿using Chloe;
using Chloe.Infrastructure.Interception;
using Chloe.SqlServer;
using Entity;
using Framework.Common.Extension;

using Quartz;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.Helper;
using System.IO;
using System.Drawing;
using CrawerEnum;

namespace Crawer.Jobs
{
    class DbCommandInterceptor : IDbCommandInterceptor
    {
        public void ReaderExecuting(IDbCommand command, DbCommandInterceptionContext<IDataReader> interceptionContext)
        {
            //interceptionContext.DataBag.Add("startTime", DateTime.Now);
            Debug.WriteLine(AppendDbCommandInfo(command));
            Console.WriteLine(command.CommandText);
            AmendParameter(command);
        }
        public void ReaderExecuted(IDbCommand command, DbCommandInterceptionContext<IDataReader> interceptionContext)
        {
            //DateTime startTime = (DateTime)(interceptionContext.DataBag["startTime"]);
            //Console.WriteLine(DateTime.Now.Subtract(startTime).TotalMilliseconds);
            if (interceptionContext.Exception == null)
                Console.WriteLine(interceptionContext.Result.FieldCount);
        }

        public void NonQueryExecuting(IDbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            Debug.WriteLine(AppendDbCommandInfo(command));
            Console.WriteLine(command.CommandText);
            AmendParameter(command);
        }
        public void NonQueryExecuted(IDbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            if (interceptionContext.Exception == null)
                Console.WriteLine(interceptionContext.Result);
        }

        public void ScalarExecuting(IDbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            //interceptionContext.DataBag.Add("startTime", DateTime.Now);
            Debug.WriteLine(AppendDbCommandInfo(command));
            Console.WriteLine(command.CommandText);
            AmendParameter(command);
        }
        public void ScalarExecuted(IDbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            //DateTime startTime = (DateTime)(interceptionContext.DataBag["startTime"]);
            //Console.WriteLine(DateTime.Now.Subtract(startTime).TotalMilliseconds);
            if (interceptionContext.Exception == null)
                Console.WriteLine(interceptionContext.Result);
        }


        static void AmendParameter(IDbCommand command)
        {
            //foreach (var parameter in command.Parameters)
            //{
            //    if (parameter is OracleParameter)
            //    {
            //        OracleParameter oracleParameter = (OracleParameter)parameter;
            //        if (oracleParameter.Value is string)
            //        {
            //            /* 针对 oracle 长文本做处理 */
            //            string value = (string)oracleParameter.Value;
            //            if (value != null && value.Length > 4000)
            //            {
            //                oracleParameter.OracleDbType = OracleDbType.NClob;
            //            }
            //        }
            //    }
            //}
        }

        public static string AppendDbCommandInfo(IDbCommand command)
        {
            StringBuilder sb = new StringBuilder();

            foreach (IDbDataParameter param in command.Parameters)
            {
                if (param == null)
                    continue;

                object value = null;
                if (param.Value == null || param.Value == DBNull.Value)
                {
                    value = "NULL";
                }
                else
                {
                    value = param.Value;

                    if (param.DbType == DbType.String || param.DbType == DbType.AnsiString || param.DbType == DbType.DateTime)
                        value = "'" + value + "'";
                }

                sb.AppendFormat("{3} {0} {1} = {2};", Enum.GetName(typeof(DbType), param.DbType), param.ParameterName, value, Enum.GetName(typeof(ParameterDirection), param.Direction));
                sb.AppendLine();
            }

            sb.AppendLine(command.CommandText);

            return sb.ToString();
        }
    }
    /// <summary>
    /// 下载并且上传
    /// </summary>
    [DisallowConcurrentExecution]
    public class DownAndUpPage_Job : IJob
    {
        MsSqlContext dbcontext;
        public DownAndUpPage_Job()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());
        }

        public void Execute(IJobExecutionContext context)
        {
            //IDbCommandInterceptor interceptor = new DbCommandInterceptor();
            //dbcontext.Session.AddInterceptor(interceptor);
            DateTime dt = DateTime.Now;
            string shortdate = dt.ToString("yyyy-MM-dd");           
            IQuery<Page> cpq = dbcontext.Query<Page>();          
            List<Page> plst = cpq.Where(a => a.pagelocal.Length==0).TakePage(1, 20).ToList();
            HttpWebHelper web = new HttpWebHelper();
            foreach (var p in plst)
            {

                try
                {
                    Stream stream = web.GetStream(p.pagesource);
                    Image img = Image.FromStream(stream);
                    stream.Close();
                    string filePath = AppDomain.CurrentDomain.BaseDirectory +"DownLoadImgs/"+ p.Id +".jpg";
                    img.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    string localimg = UcHelper.uploadFile("Page/"+p.Id + ".jpg", filePath);
                    p.pagelocal = localimg;
                    p.modify = dt;
                    dbcontext.Update(p);
                }
                catch (Exception ex)
                {
                    Err_PageJob err = new Err_PageJob();
                    err.imgurl = p.pagesource;
                    err.source = p.source;
                    err.errtype = ErrPage.限制访问;
                    err.modify = dt;
                    err.shortdate = shortdate;
                    err.message = ex.Message;
                    err = dbcontext.Insert(err);
                    continue;
                }
              
               
            }
        }
    }
    /// <summary>
    /// 下载并且上传
    /// </summary>
    [DisallowConcurrentExecution]
    public class DownAndUpComic_Job : IJob
    {
        MsSqlContext dbcontext;
        public DownAndUpComic_Job()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());
        }

        public void Execute(IJobExecutionContext context)
        {
            //IDbCommandInterceptor interceptor = new DbCommandInterceptor();
            //dbcontext.Session.AddInterceptor(interceptor);
            DateTime dt = DateTime.Now;
            string shortdate = dt.ToString("yyyy-MM-dd");
            IQuery<Comic> cpq = dbcontext.Query<Comic>();
            List<Comic> plst = cpq.Where(a => a.comiccoverlocal.Length == 0).TakePage(1, 20).ToList();
            HttpWebHelper web = new HttpWebHelper();
            foreach (var p in plst)
            {

                try
                {
                    Stream stream = web.GetStream(p.comiccoversource);
                    Image img = Image.FromStream(stream);
                    stream.Close();
                    string filePath = AppDomain.CurrentDomain.BaseDirectory + "DownLoadImgs/" + p.Id + ".jpg";
                    img.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    string localimg = UcHelper.uploadFile("Comic/"+p.Id + ".jpg", filePath);
                    p.comiccoverlocal = localimg;
                    p.modify = dt;
                    dbcontext.Update(p);
                }
                catch (Exception ex)
                {
                    Err_ComicJob err = new Err_ComicJob();
                    err.bookurl = p.bookurl;                   
                    err.errtype = ErrComic.图片出错;
                    err.modify = dt;
                    err.shortdate = shortdate;
                    err.message = ex.Message;
                    err = dbcontext.Insert(err);
                    continue;
                }


            }
        }
    }
    /// <summary>
    /// 下载并且上传
    /// </summary>
    [DisallowConcurrentExecution]
    public class DownAndUpChapter_Job : IJob
    {
        MsSqlContext dbcontext;
        public DownAndUpChapter_Job()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());
        }

        public void Execute(IJobExecutionContext context)
        {
            //IDbCommandInterceptor interceptor = new DbCommandInterceptor();
            //dbcontext.Session.AddInterceptor(interceptor);
            DateTime dt = DateTime.Now;
            string shortdate = dt.ToString("yyyy-MM-dd");
            IQuery<Chapter> cpq = dbcontext.Query<Chapter>();
            List<Chapter> plst = cpq.Where(a => a.chaptersource.Length == 0).TakePage(1, 20).ToList();
            HttpWebHelper web = new HttpWebHelper();
            foreach (var p in plst)
            {

                try
                {
                    Stream stream = web.GetStream(p.chaptersource);
                    Image img = Image.FromStream(stream);
                    stream.Close();
                    string filePath = AppDomain.CurrentDomain.BaseDirectory + "DownLoadImgs/" + p.Id + ".jpg";
                    img.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    string localimg = UcHelper.uploadFile("Chapter/" + p.Id + ".jpg", filePath);
                    p.chapterlocal = localimg;
                    p.modify = dt;
                    dbcontext.Update(p);
                }
                catch (Exception ex)
                {
                    Err_ChapterJob err = new Err_ChapterJob();
                    err.bookurl = p.chapterurl;
                    err.errtype = ErrChapter.图片出错;
                    err.modify = dt;
                    err.shortdate = shortdate;
                    err.message = ex.Message;
                    err = dbcontext.Insert(err);
                    continue;
                }


            }
        }
    }
}
