﻿namespace TodoApp.Controllers
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.Description;
    using TodoApp.Models;

    [Authorize]
    public class TodoController : ApiController
    {
        private ApplicationDbContext db;

        private ApplicationUserManager userManager;

        public TodoController()
        {
            this.userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            this.db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        }

        // GET: api/Todo
        public async Task<IEnumerable<ToDo>> GetByUserId()
        {
            var currentUser = this.userManager.FindById(User.Identity.GetUserId());

            return await this.db.ToDoes.Where(todo => todo.User.Id == currentUser.Id).ToListAsync();
        }

        // GET: api/Todo/5
        [ResponseType(typeof(ToDo))]
        public async Task<IHttpActionResult> GetOne(int id)
        {
            var currentUser = this.userManager.FindById(User.Identity.GetUserId());

            var todo = await this.db.ToDoes.Where(item => item.User.Id == currentUser.Id && item.Id == id).SingleOrDefaultAsync();

            if (todo == null)
            {
                return this.NotFound();
            }

            return this.Ok(todo);
        }

        // POST: api/Todo
        [ResponseType(typeof(ToDo))]
        public async Task<IHttpActionResult> Post([FromBody] ToDo todo)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var currentUser = await this.userManager.FindByIdAsync(this.User.Identity.GetUserId());
            todo.User = currentUser;
            this.db.ToDoes.Add(todo);
            await this.db.SaveChangesAsync();

            return this.Created<ToDo>(new Uri(Request.RequestUri, todo.Id.ToString()), todo);
        }

        // PUT: api/Todo/5
        public async Task<IHttpActionResult> Put(ToDo todo)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var currentUserId = this.User.Identity.GetUserId();

            var todoToUpdate = await this.db.ToDoes.AsNoTracking().Where(item => item.Id == todo.Id).SingleOrDefaultAsync();

            if (todoToUpdate.User.Id != currentUserId)
            {
                return this.StatusCode(HttpStatusCode.Forbidden);
            }

            this.db.Entry(todo).State = EntityState.Modified;

            await db.SaveChangesAsync();

            return this.StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/Todo/5
        public async Task<IHttpActionResult> Delete(int id)
        {
            var currentUserId = this.User.Identity.GetUserId();

            var todoToDelete = await this.db.ToDoes.Where(item => item.User.Id == currentUserId && item.Id == id).FirstOrDefaultAsync();

            if (todoToDelete == null)
            {
                return this.NotFound();
            }

            if (todoToDelete.User.Id != currentUserId)
            {
                return this.StatusCode(HttpStatusCode.Forbidden);
            }

            this.db.ToDoes.Remove(todoToDelete);
            await this.db.SaveChangesAsync();

            return this.Ok();
        }
    }
}