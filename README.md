# RazorPages

Hi!

This repo has gotten some attention due to @DamianEdwards tweeting about it, so I'll answer some questions.

## About the project

### What is this?

This is a prototype/sample for https://github.com/aspnet/Mvc/issues/494 . We don't have a product name for this yet. Read the issue for detail, but basically it's a page-based programming model for ASP.NET.

I'll be provisionally using the name RazorPages in namespaces, docs, and comments, but please know that's not official.

### How far along is the development of $UnnamedProduct?

We're (ASP.NET team) are currently doing app building to validate the programming model. This is very early days, and it's only been about 40 commits since hello world. There are tons of hacks in the code and this doesn't current reflect how we'd build a shipping version. 

Once we're done iterating on the design, we'll begin work in earnest with a larger group of developers. 

### What are the plans for this repo?

Once the project is stable and closer to release we will likely merge it into the aspnet/Mvc repo.

### What are the plans for the release of $UnnamedProduct?

According to the [.Net Core roadmap](https://blogs.msdn.microsoft.com/dotnet/2016/07/15/net-core-roadmap/) this project is slated for inclusion in the Q4-2016/Q1-2017 release. That probably means 1.1.0. As with all things, release dates and plans are subject to change. We will ship it when it's ready.

### What can I do to help?

You can help right now by trying to build sites and showing us what it looks like. We (ASP.NET team) are doing app building and will be adding more samples here to help drive discussions.

## About the prototype

### Why are Razor Pages using `.razor` instead of `.cshtml`?

The primary reason is so that intellisense **won't** try to work with these files. We know that tooling will give wrong answers for Razor Pages, and would rather skip that part. Secondly we want to make sure we have an easy way to figure out which files are which! It's likely we'll solve this problem doing something smarter for the release and very likely we'll use the `.cshtml` extension.
