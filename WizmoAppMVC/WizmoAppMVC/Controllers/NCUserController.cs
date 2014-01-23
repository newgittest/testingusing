using SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WizmoAppMVC.Models;

namespace WizmoAppMVC.Controllers
{
    public class NCUserController : ApiController
    {
        private NCUsersContext db = new NCUsersContext();

        // GET api/NCUser
        public IEnumerable<NCUser> GetNCUsers()
        {
            return db.NCUsers.AsEnumerable();
        }

        // GET api/NCUser/5
        public NCUser GetNCUser(int id)
        {
            NCUser ncuser = db.NCUsers.Find(id);
            if (ncuser == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return ncuser;
        }

        // PUT api/NCUser/5
        public NCUser PutNCUser(int id, NCUser ncuser)
        {
            if (ModelState.IsValid && id == ncuser.id)
            {
                db.Entry(ncuser).State = EntityState.Modified;

                try
                {
                    db.SaveChanges();

                    Task.Factory.StartNew(
                    () =>
                    {
                        var clients = Hub.GetClients<NCUserHub>();
                        clients.RecordUpdated("Rama", ncuser);
                    });

                }
                catch (DbUpdateConcurrencyException ex)
                {
                    throw new Exception("Error occured in updating user",ex);
                }

                return ncuser;
                       
            }
            else
            {
                throw new Exception("Exception occured"); 
            }
        }

        // POST api/NCUser
        public HttpResponseMessage PostNCUser(NCUser ncuser)
        {
            if (ModelState.IsValid)
            {
                db.NCUsers.Add(ncuser);
                db.SaveChanges();

                

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, ncuser);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = ncuser.id }));

                Task.Factory.StartNew(
                () =>
                {
                    var clients = Hub.GetClients<NCUserHub>();
                    clients.RecordCreated("Rama", ncuser);
                });

                return response;
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        // DELETE api/NCUser/5
        public HttpResponseMessage DeleteNCUser(int id)
        {
            NCUser ncuser = db.NCUsers.Find(id);
            if (ncuser == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.NCUsers.Remove(ncuser);

            try
            {
                db.SaveChanges();

                Task.Factory.StartNew(
                 () =>
                 {
                     var clients = Hub.GetClients<NCUserHub>();
                     clients.RecordDeleted("Rama", ncuser);
                 });

            }
            catch (DbUpdateConcurrencyException)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            return Request.CreateResponse(HttpStatusCode.OK, ncuser);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}