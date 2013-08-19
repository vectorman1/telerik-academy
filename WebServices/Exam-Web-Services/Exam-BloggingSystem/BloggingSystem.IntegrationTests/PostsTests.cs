﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Transactions;
using BloggingSystem.Services.Controllers;
using System.Collections.Generic;
using System.Web.Http;
using BloggingSystem.Services.Models;
using Newtonsoft.Json;
using System.Net;

namespace BloggingSystem.IntegrationTests
{
    [TestClass]
    public class PostsTests
    {
        static TransactionScope tran;
        private InMemoryHttpServer httpServer;
        private LoggedUserModel validUser;

        [TestInitialize]
        public void TestInit()
        {

            var type = typeof(PostsController);
            tran = new TransactionScope();

            var routes = new List<Route>
            {
                new Route(
                    "TagsApi", 
                    "api/tags/{tagId}/posts",
                 new { controller = "tags", action = "posts"}
                 ),
                new Route( 
                    "PostsApi", 
                    "api/posts/{postId}/comment",
                 new { controller = "posts", action = "comment"}
                ),  
                new Route(
                    "UsersApi",
                    "api/users/{action}",
                    new { controller = "users"}
                    ),          
                new Route(
                    "DefaultApi",
                    "api/{controller}/{id}",
                    new { id = RouteParameter.Optional }),
            };

            this.httpServer = new InMemoryHttpServer("http://localhost/", routes);

            var testUser = new UserModel()
            {
                Username = "validusername",
                DisplayName = "validdisplayname",
                AuthCode = new string('b', 40)
            };

            var response = this.httpServer.Post("api/users/register", testUser);
            var contentString = response.Content.ReadAsStringAsync().Result;
            var userModel = JsonConvert.DeserializeObject<LoggedUserModel>(contentString);

            this.validUser = userModel;
        }

        [TestCleanup]
        public void TearDown()
        {
            tran.Dispose();
        }

        [TestMethod]
        public void Post_No_Tags_No_SessionKey_Should_Return_Bad_Reuqest()
        {
            var postModel = new PostModel() { Title = "Test", Text = "Test text"};
            var response = this.httpServer.Post("api/posts/", postModel);
            var contentString = response.Content.ReadAsStringAsync().Result;
            var postCreatedModel = JsonConvert.DeserializeObject<PostCreatedModel>(contentString);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);            
        }

        [TestMethod]
        public void Post_No_Tags_Should_Return_Created()
        {
            var postModel = new PostModel() { Title = "Test", Text = "Test text" };
            var headers = new Dictionary<string, string>();
            headers["X-sessionKey"] = this.validUser.SessionKey;
            var response = this.httpServer.Post("api/posts/", postModel, headers);
            var contentString = response.Content.ReadAsStringAsync().Result;
            var postCreatedModel = JsonConvert.DeserializeObject<PostCreatedModel>(contentString);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.AreEqual(postModel.Title, postCreatedModel.Title);
        }

        [TestMethod]
        public void Post_Null_Title_Should_Return_BadRequest()
        {
            var postModel = new PostModel() { Title = null, Text = "Test text" };
            var headers = new Dictionary<string, string>();
            headers["X-sessionKey"] = this.validUser.SessionKey;
            var response = this.httpServer.Post("api/posts/", postModel, headers);
            var contentString = response.Content.ReadAsStringAsync().Result;
            var postCreatedModel = JsonConvert.DeserializeObject<PostCreatedModel>(contentString);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);     
        }

        [TestMethod]
        public void Post_Null_Text_Should_Return_BadRequest()
        {
            var postModel = new PostModel() { Title = "Test", Text = null };
            var headers = new Dictionary<string, string>();
            headers["X-sessionKey"] = this.validUser.SessionKey;
            var response = this.httpServer.Post("api/posts/", postModel, headers);
            var contentString = response.Content.ReadAsStringAsync().Result;
            var postCreatedModel = JsonConvert.DeserializeObject<PostCreatedModel>(contentString);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void Post_Null_Model_Return_BadRequest()
        {
            var postModel = new PostModel() {};
            var headers = new Dictionary<string, string>();
            headers["X-sessionKey"] = this.validUser.SessionKey;
            var response = this.httpServer.Post("api/posts/", postModel, headers);
            var contentString = response.Content.ReadAsStringAsync().Result;
            var postCreatedModel = JsonConvert.DeserializeObject<PostCreatedModel>(contentString);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void Post_Empty_Tag_Array_Should_Return_Created()
        {
            var postModel = new PostModel() { Title = "Test", Text = "Test text", Tags= new string[]{} };
            var headers = new Dictionary<string, string>();
            headers["X-sessionKey"] = this.validUser.SessionKey;
            var response = this.httpServer.Post("api/posts/", postModel, headers);
            var contentString = response.Content.ReadAsStringAsync().Result;
            var postCreatedModel = JsonConvert.DeserializeObject<PostCreatedModel>(contentString);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.AreEqual(postModel.Title, postCreatedModel.Title);
        }

        [TestMethod]
        public void Post_With_Tags_Array_Should_Return_Created()
        {
            var postModel = new PostModel() { Title = "Test", Text = "Test text", Tags = new string[] { "first tag", "second tag"} };
            var headers = new Dictionary<string, string>();
            headers["X-sessionKey"] = this.validUser.SessionKey;
            var response = this.httpServer.Post("api/posts/", postModel, headers);
            var contentString = response.Content.ReadAsStringAsync().Result;
            var postCreatedModel = JsonConvert.DeserializeObject<PostCreatedModel>(contentString);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.AreEqual(postModel.Title, postCreatedModel.Title);
        }
    }
}
