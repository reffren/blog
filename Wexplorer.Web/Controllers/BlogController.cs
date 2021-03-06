﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Wexplorer.Data.Abstract;
using Wexplorer.Data.Entities;
using Wexplorer.Web.Models;

namespace Wexplorer.Web.Controllers
{
    public class BlogController : Controller
    {
        IBlogRepository _repository;
        
        PostsModel postsModel;
        PostModel postModel;
        public int PageSize = 10;
        public BlogController(IBlogRepository blogRepository)
        {
            _repository = blogRepository;
        }

        public ViewResult Posts(int page = 1)
        {
            postsModel = new PostsModel();
            postsModel.Posts = _repository.Posts.ToList().OrderByDescending(p => p.PostID).Skip((page - 1) * PageSize).Take(PageSize);
            postsModel.PagingInfo = new PagingInfo { CurrentPage = page, ItemsPerPage = PageSize, TotalItems = _repository.Posts.Count() };

            return View(postsModel);
        }

        public ActionResult Post(string title)
        {
            postModel = new PostModel()
            {
                Post = _repository.Posts.FirstOrDefault(f => f.UrlPost == title)
            };

            if(postModel.Post == null)
                return HttpNotFound();
            else
                return View(postModel);
        }

        [HttpPost]
        public ActionResult Post(PostModel _postModel)
        {
            if (ModelState.IsValid)
            {
                var response = Request["g-recaptcha-response"];
                string secretKey = "6Ldg5RGRGRGukGNb_OYhRFGRFGpUYFXPWi_c7RGRGW";
                var client = new WebClient();
                var result = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secretKey, response));
                var obj = JObject.Parse(result);
                var status = (bool)obj.SelectToken("success");

                ViewBag.NameUser = _postModel.Comment.NameUser;
                ViewBag.Email = _postModel.Comment.Email;
                ViewBag.Commentary = _postModel.Comment.Commentary;

                if (status)
                {
                    _postModel.Comment.PublishedDate = DateTime.Now;
                    _repository.SaveComment(_postModel.Comment);
                }
                else
                {
                     ViewBag.Message = "Google reCaptcha validation failed";
                }

            }
            string urlPost = _repository.Posts.FirstOrDefault(p => p.PostID == _postModel.Comment.PostID).UrlPost;

            return RedirectToAction("Post", new { title = urlPost });
        }

        public ViewResult SiteMap()
        {
            var siteMap = _repository.Categories.ToList();
            return View(siteMap);
        }

        public ActionResult PostsOfCategory(string cat, int page = 1)
        {
            postsModel = new PostsModel();
            postsModel.Posts = _repository.Posts.Where(p => p.Category.UrlCategory == cat).ToList().OrderBy(p => p.PostID).Skip((page - 1) * PageSize).Take(PageSize);
            postsModel.PagingInfo = new PagingInfo { CurrentPage = page, ItemsPerPage = PageSize, TotalItems = _repository.Posts.Where(p => p.Category.UrlCategory == cat).Count() };
            
            if (postsModel.Posts.Count() == 0)
                return HttpNotFound();
            else
                return View("PostsOfCategory", postsModel);
        }

        public ActionResult AboutMe()
        {
            return View();
        }

        public ViewResult Contacts()
        {
            return View();
        }
    }
}