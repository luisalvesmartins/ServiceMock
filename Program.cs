using Newtonsoft.Json;
using System.Web;

namespace SimpleCacheAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const string DATAFILE = "c:/auteput/servicecache.json";

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();


            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            Dictionary<string,DatabaseRecord> db = new Dictionary<string,DatabaseRecord>();
            load();

            app.MapGet("/missing", (HttpContext httpContext) =>
            {
                Dictionary<string, DatabaseRecord> dbMissing = new Dictionary<string, DatabaseRecord>();
                foreach (KeyValuePair<string,DatabaseRecord> item in db)
                {
                    if (item.Value.toProcess)
                    {
                        dbMissing[item.Key] = item.Value;    
                    }
                }
                return dbMissing;
            });

            app.MapPost("/retrieve",async (HttpContext httpContext) =>
            {
                string res = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
                //A rigid format of r= in the post
                string req = res.Substring(2);
                if (db.ContainsKey(req))
                {
                    if (db[req].toProcess)
                    {
                        httpContext.Response.StatusCode = 404;
                        return "";
                    }
                    else
                    {
                        db[req].DateLastAccess = DateTime.Now;
                        save();
                        res = HttpUtility.UrlDecode(db[req].response);
                        return res;
                    }
                }
                else
                {
                    DatabaseRecord dr = new DatabaseRecord()
                    {
                        DateCreated = DateTime.Now,
                        DateLastAccess = DateTime.Now,
                        response = "",
                        toProcess = true
                    };
                    db.Add(req, dr);
                    save();
                    httpContext.Response.StatusCode = 404;
                    return "";
                }
            });

            app.MapPost("/store", async (HttpContext httpContext) => 
            {
                string req = httpContext.Request.Query["r"];
                string res= await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
                string[] pars= res.Split("&");
                //A rigid format of r=&s= in the post
                req = pars[0].Substring(2);
                res = pars[1].Substring( 2);
                DatabaseRecord dr =new DatabaseRecord() { 
                    DateCreated= DateTime.Now, 
                    DateLastAccess= DateTime.Now,
                    response = res,
                    toProcess = false
                };
                if (db.ContainsKey(req))
                {
                    if (db[req].response != res)
                    {
                        db[req].response = res;
                        db[req].DateLastUpdate = DateTime.Now;
                        db[req].toProcess = false;
                        save();
                    }
                }
                else
                {
                    db.Add(req, dr);
                    save();
                }
                return "OK";
            });

            void save()
            {
                string sdb = JsonConvert.SerializeObject(db);
                File.WriteAllText(DATAFILE, sdb);
            }
            void load()
            {
                if (File.Exists(DATAFILE))
                {
                    string sdb=File.ReadAllText(DATAFILE);
                    if (!string.IsNullOrEmpty(sdb))
                        db = JsonConvert.DeserializeObject<Dictionary<string, DatabaseRecord>>(sdb);
                }
            }

            app.Run();
        }
    }
}