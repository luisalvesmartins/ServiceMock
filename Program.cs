using Newtonsoft.Json;

namespace ServiceMock
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const string DATAFILE = "c:/temp/data.json";

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

            app.MapGet("/retrieve", (HttpContext httpContext) =>
            {
                string req = httpContext.Request.Query["r"];
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
                        return db[req].response;
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
                DatabaseRecord dr=new DatabaseRecord() { 
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